using UnityEngine;

using System.IO;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using System.Net;
using Mymmorpg;

public class TestConnectionToServer : MonoBehaviour
{

    void Start()
    {
        // 创建并启动线程来进行服务器连接
        Thread thread = new Thread(ConnectToServer);
        thread.IsBackground = true;
        thread.Start();
    }


    void ConnectToServer()
    {
        try
        {
            // 创建TCP连接
            TcpClient client = new TcpClient("127.0.0.1", 12345); // 连接服务器的本地IP和端口
            NetworkStream stream = client.GetStream(); // 获取网络流

            // 1. 发送请求给服务器
            int userId = 1; // 假设查询ID为1的用户
            string req = $"GetUser:{userId}"; // 构造请求字符串
            byte[] data2 = Encoding.UTF8.GetBytes(req); // 将请求字符串转为字节
            stream.Write(data2, 0, data2.Length); // 发送请求数据

            // 2. 读取服务器回发的响应消息
            byte[] buffer = new byte[1024];
            int len = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, len); // 解析为字符串
            Debug.Log("收到服务器回应：" + response); // 打印回应

            // 3. 读取数据长度前缀（4字节）
            byte[] lengthBytes = new byte[4];
            stream.Read(lengthBytes, 0, 4); // 读取4字节长度前缀
            int dataLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBytes, 0)); // 转为数据长度

            // 4. 读取 Protobuf 数据
            byte[] protobufBytes = new byte[dataLength];
            int readTotal = 0;
            while (readTotal < dataLength)
            {
                int read = stream.Read(protobufBytes, readTotal, dataLength - readTotal); // 读取 Protobuf 数据
                if (read == 0) break; // 防止死循环
                readTotal += read; // 累积读取的字节数
            }

            // 5. 使用 Protobuf 反序列化
            using (MemoryStream ms = new MemoryStream(protobufBytes))
            {
                var user = User.Parser.ParseFrom(ms); // 反序列化为 User 对象
                Debug.Log($"收到用户ID={user.UserId}, Name={user.UserName}"); // 输出用户信息
            }

            // 关闭流和连接
            stream.Close();
            client.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("连接服务器失败: " + e.Message); // 打印连接错误
        }
    }
}