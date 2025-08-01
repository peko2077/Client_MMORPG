using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using Mymmorpg;
using UnityEngine;

public static class NetworkHelper
{
    private static string serverIp = "127.0.0.1";  // 服务器地址
    private static int serverPort = 12345;         // 端口号

    public static ApiResponse SendRequest(string req)
    {
        try
        {
            // 1. 构造 ApiRequest 消息对象
            ApiRequest request = new ApiRequest { Command = req };

            // 2. 序列化成字节数组（Protobuf格式）
            byte[] requestBytes = request.ToByteArray();

            // 3. 建立 TCP 连接
            using (TcpClient client = new TcpClient(serverIp, serverPort))
            using (NetworkStream stream = client.GetStream())
            {
                // 4. 先发送请求长度前缀（4字节，网络字节序）
                byte[] lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(requestBytes.Length));
                stream.Write(lengthPrefix, 0, lengthPrefix.Length);

                // 5. 发送请求消息体
                stream.Write(requestBytes, 0, requestBytes.Length);
                stream.Flush();

                // 6. 读取响应长度前缀
                byte[] lengthBytes = new byte[4];
                int lenRead = stream.Read(lengthBytes, 0, 4);
                if (lenRead < 4) throw new Exception("读取响应长度失败");

                int dataLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBytes, 0));

                // 7. 读取响应数据
                byte[] responseBytes = new byte[dataLength];
                int readTotal = 0;
                while (readTotal < dataLength)
                {
                    int read = stream.Read(responseBytes, readTotal, dataLength - readTotal);
                    if (read == 0)
                    {
                        throw new Exception("服务器关闭连接");
                    }
                    readTotal += read;
                }

                // 8. 反序列化成 ApiResponse 对象
                return ApiResponse.Parser.ParseFrom(responseBytes);
            }
            ;
        }
        catch (Exception e)
        {
            Debug.LogError($"网络请求异常：{e.Message}");
            return new ApiResponse
            {
                Success = false,
                Error = $"网络异常：{e.Message}"
            };
        }
    }
}