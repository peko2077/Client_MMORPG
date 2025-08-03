using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Google.Protobuf;
using Mymmorpg;
using Mymmorpg.tools;
using UnityEngine;

// 用于管理客户端 TCP 长连接的 Unity 单例类
public class NetworkClient : MonoBehaviour
{
    // 保存连接参数以便断线重连
    private string lastIp;
    private int lastPort;

    private TcpClient client;                      // 客户端 TCP 连接
    private NetworkStream stream;                  // 网络流，用于读写数据
    private Thread receiveThread;                  // 接收线程
    private Thread heartbeatThread;                // 心跳线程
    private Thread reconnectThread;                // 重连线程

    private readonly object disconnectLock = new object();

    private int reconnectDelaySeconds = 5;         // 重连间隔秒数
    private bool autoReconnect = true;             // 是否允许自动重连
    private volatile bool isConnected = false;     // 当前连接状态
    private volatile bool isReceiving = false;     // 是否正在接收数据
    private volatile bool isDisconnected = false; // 是否已断开，防止重复关闭

    private QueuedSender queuedSender;             // 消息排队发送器

    public static NetworkClient Instance;          // 单例引用

    public event Action<ApiResponse> OnApiResponse; // 收到 ApiResponse 的回调
    private List<byte> recvBuffer = new List<byte>(); // 缓存未处理的字节数据

    // Unity 生命周期：初始化实例
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;                    // 设置单例
            DontDestroyOnLoad(gameObject);      // 切换场景时保留该对象
        }
        else
        {
            Destroy(gameObject);                // 若已有实例，则销毁重复对象
        }
    }

    // Unity 生命周期：启动时自动连接服务器
    void Start()
    {
        // 初始化 MainThreadUtil
        MainThreadUtil.Init();

        lastIp = "127.0.0.1";
        lastPort = 12345;
        Connect(lastIp, lastPort);            // 默认连接本地服务器端口 12345

        GameObject.DontDestroyOnLoad(gameObject); // 保留当前脚本所在的物体
    }

    // Unity 生命周期：退出应用时断开连接
    void OnApplicationQuit()
    {
        Disconnect();                           // 程序退出时释放连接
    }

    // 建立 TCP 连接
    public void Connect(string ip, int port)
    {

        lastIp = ip;
        lastPort = port;

        try
        {
            client = new TcpClient();           // 创建 TcpClient 对象
            client.Connect(ip, port);           // 连接指定 IP 和端口
            stream = client.GetStream();        // 获取网络流

            isConnected = true;                 // 设置连接标志
            isReceiving = true;

            // 启动消息排队器
            queuedSender = new QueuedSender(stream);
            queuedSender.Start();
            queuedSender.OnSendError += ex =>
            {
                Debug.LogError($"发送线程异常: {ex.Message}");
                TryReconnect();
            };

            // 启动接收线程
            receiveThread = new Thread(ReceiveLoop);
            receiveThread.IsBackground = true;          // 设置为后台线程（程序退出自动结束）
            receiveThread.Start();

            Debug.Log("已连接到服务器");
        }
        catch (Exception e)
        {
            Debug.LogError($"连接服务器失败: {e.Message}"); // 打印连接错误信息
        }

        // 启动心跳线程
        StartHeartbeatLoop();
    }

    // 接收服务器消息的循环
    private void ReceiveLoop()
    {
        byte[] buffer = new byte[4096]; // 接收缓冲区

        while (isConnected && isReceiving)
        {
            try
            {
                // 阻塞读取服务器发送的数据
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    // 服务器断开连接
                    isConnected = false;
                    break;
                }

                // 新数据加入缓存
                recvBuffer.AddRange(new ArraySegment<byte>(buffer, 0, bytesRead));

                // 循环处理缓冲区中完整的消息
                while (true)
                {
                    // 缓冲区至少要有4字节长度前缀才能解析
                    if (recvBuffer.Count < 4)
                    {
                        break; // 不足以读取长度，等待更多数据
                    }

                    // 读取长度（4字节大端）
                    int bodyLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvBuffer.ToArray(), 0));

                    // 计算消息完整长度 = 4字节长度前缀 + bodyLength字节消息体
                    int fullMsgLength = 4 + bodyLength;

                    // 如果缓冲区数据不足一条完整消息，等待更多数据
                    if (recvBuffer.Count < fullMsgLength)
                    {
                        break;
                    }

                    // 读取消息类型（body中的第一个字节）
                    byte messageType = recvBuffer[4];

                    // protobuf数据体 = bodyLength - 1（减去消息类型1字节）
                    byte[] protobufData = recvBuffer.Skip(5).Take(bodyLength - 1).ToArray();

                    // 处理消息
                    HandleMessage(messageType, protobufData);

                    // 从缓冲区移除已处理的消息
                    recvBuffer.RemoveRange(0, fullMsgLength);
                }

            }
            catch (Exception e)
            {
                Debug.LogError($"接收线程异常: {e.Message}");
                // 连接异常，标记断开，跳出循环
                isConnected = false;
                isReceiving = false;
                break;
            }
        }

        // 退出循环后调用 Disconnect 来关闭资源
        Disconnect();
    }

    private void HandleMessage(byte messageType, byte[] protobufData)
    {
        switch (messageType)
        {
            case 0x01: // 心跳
                var heartbeat = Heartbeat.Parser.ParseFrom(protobufData);
                if (heartbeat.Type == "ping")
                {
                    Debug.Log("收到服务器 ping，回复 pong");
                    SendHeartbeat("pong");
                }
                else if (heartbeat.Type == "pong")
                {
                    // Debug.Log("收到服务器 pong");
                }
                break;

            case 0x02: // API 响应
                HandleResponse(protobufData);
                break;

            default:
                Debug.LogWarning($"未知消息类型: {messageType}, 数据长度: {protobufData.Length}");
                Debug.LogWarning($"数据内容: {BitConverter.ToString(protobufData)}");
                break;
        }
    }

    // 处理服务器发送的 Protobuf 响应
    private void HandleResponse(byte[] protobufData)
    {
        try
        {
            ApiResponse response = ApiResponse.Parser.ParseFrom(protobufData);
            Debug.Log($"服务器响应：{response.Message}");
            MainThreadUtil.RunOnMainThread(() =>
            {
                OnApiResponse?.Invoke(response);
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"解析服务器响应失败: {e.Message}");
        }
    }

    // 发送 ApiRequest 请求
    public void SendRequest(ApiRequest request)
    {
        byte[] body = request.ToByteArray();
        // Debug.Log($"[NetworkClient] protobuf长度={body.Length}, protobuf前5字节={BitConverter.ToString(body, 0, Math.Min(5, body.Length))}");

        byte messageType = 0x02; // 业务请求类型

        int totalLength = 1 + body.Length; // 1 字节类型 + protobuf 数据体
        byte[] lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(totalLength));

        byte[] fullData = new byte[4 + totalLength];
        Buffer.BlockCopy(lengthPrefix, 0, fullData, 0, 4);
        fullData[4] = messageType; // 设置消息类型
        Buffer.BlockCopy(body, 0, fullData, 5, body.Length);

        // Debug.Log($"[NetworkClient] 发送请求，总长度={fullData.Length}, 长度字段={totalLength}, 前4字节={BitConverter.ToString(fullData, 0, 4)}");

        Enqueue(fullData);
    }

    // 封装消息入队（延迟发送）
    private void Enqueue(byte[] data)
    {
        if (queuedSender != null && isConnected)
        {
            queuedSender.Enqueue(data);
        }
        else
        {
            Debug.LogWarning("排队器未启动或连接中断，无法发送");
        }
    }

    // 构造并发送心跳包（含类型）
    private void SendHeartbeat(string type)
    {
        if (!isConnected) return;

        try
        {
            var heartbeat = new Heartbeat { Type = type };
            byte[] body = heartbeat.ToByteArray();

            byte messageType = 0x01;
            int totalLength = 1 + body.Length; // 包含类型

            byte[] lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(totalLength));

            byte[] full = new byte[4 + totalLength];
            Buffer.BlockCopy(lengthPrefix, 0, full, 0, 4);   // 写入长度前缀
            full[4] = messageType;                           // 写入类型
            Buffer.BlockCopy(body, 0, full, 5, body.Length); // 写入 Protobuf 数据

            Enqueue(full); // 通过 QueuedSender 异步发送
        }
        catch (Exception e)
        {
            Debug.LogError($"发送心跳失败: {e.Message}");
        }
    }

    // 重连线程，支持失败重试
    private void TryReconnect()
    {
        if (reconnectThread != null && reconnectThread.IsAlive) return;

        reconnectThread = new Thread(() =>
        {
            while (!isConnected && autoReconnect)
            {
                try
                {
                    Debug.Log("尝试重连...");

                    stream?.Close();
                    client?.Close();

                    client = new TcpClient();
                    client.Connect(lastIp, lastPort);
                    stream = client.GetStream();

                    isConnected = true;
                    isReceiving = true;

                    // 重启消息排队器
                    queuedSender = new QueuedSender(stream);
                    queuedSender.Start();
                    queuedSender.OnSendError += ex =>
                    {
                        Debug.LogError($"发送线程异常: {ex.Message}");
                        TryReconnect();
                    };

                    // 重启接收线程
                    receiveThread = new Thread(ReceiveLoop);
                    receiveThread.IsBackground = true;
                    receiveThread.Start();

                    Debug.Log("重连成功！");
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"重连失败: {e.Message}，等待 {reconnectDelaySeconds}s");
                    Thread.Sleep(reconnectDelaySeconds * 1000);
                }
            }
        });

        reconnectThread.IsBackground = true;
        reconnectThread.Start();
    }

    private void StartHeartbeatLoop()
    {
        if (heartbeatThread != null && heartbeatThread.IsAlive) return;

        heartbeatThread = new Thread(() =>
        {
            while (isConnected)
            {
                try
                {
                    SendHeartbeat("ping");
                    // Debug.Log("发送 ping");
                    Thread.Sleep(10000); // 每 10 秒 ping 一次
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"心跳异常: {e.Message}");
                }
            }
        });

        heartbeatThread.IsBackground = true;
        heartbeatThread.Start();
    }

    // 主动断开连接，释放资源
    public void Disconnect()
    {
        lock (disconnectLock)
        {
            if (isDisconnected) return;
            isDisconnected = true;

            try
            {
                autoReconnect = false;
                isConnected = false;        // 标记为断开
                isReceiving = false;        // 让接收线程自然退出循环

                stream?.Close();            // 关闭网络流
                stream = null;

                client?.Close();            // 关闭 TCP 连接
                client = null;

                // 等待接收线程结束，避免僵尸线程
                if (receiveThread != null && receiveThread.IsAlive)
                {
                    receiveThread.Join(2000); // 等待最多2秒
                }

                if (reconnectThread != null && reconnectThread.IsAlive)
                {
                    reconnectThread.Join(2000);
                }

                Debug.Log("客户端已断开连接");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"断开连接时异常: {ex.Message}");
            }
        }

    }

}