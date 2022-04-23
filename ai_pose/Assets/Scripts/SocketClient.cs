using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class SocketClient
{
    /************************************************属性与变量命名************************************************/
    public class StateObject
    {
        // 当前客户端的Socket
        public Socket workSocket = null;
        // 可接收的最大字节数
        public const int BufferSize = 20200;
        // 接收的数据存储
        public byte[] buffer = new byte[BufferSize];
    }

    private static Socket client;
    /************************************************Unity方法与事件***********************************************/

    /************************************************自 定 义 方 法************************************************/
    //连接服务器
    public static bool Connect(string ip, int port)
    {
        try
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.BeginConnect(serverEndPoint, new AsyncCallback(ConnectCallback), client);
            Receive(client);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("SocketClient.Connect->error: {0}", ex.Message);
            return false;
        }
    }
    //断开服务器连接
    public static void Disconnect()
    {
        if (client != null && client.Connected)
        {
            client.Shutdown(SocketShutdown.Both);
            client.Disconnect(false);
            client.Close();
        }
        Debug.Log("->client disconnected");
    }
    //发送数据
    public static bool Send(string message)
    {
        try
        {
            byte[] bytes = Encoding.Default.GetBytes(message);
            client.BeginSend(bytes, 0, bytes.Length, 0, new AsyncCallback(SendCallback), client);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("SocketClient.Send->error: {0}", ex.Message);
            return false;
        }
    }
    //发送回调
    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;
            // Complete sending the data to the remote device.
            int bytesSent = client.EndSend(ar);
            // Signal that all bytes have been sent.
            //sendDone.Set();
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("SocketClient.SendCallback->error: {0}", ex.Message);
        }
    }
    //连接回调
    private static void ConnectCallback(IAsyncResult ar)
    {
        Socket client = (Socket)ar.AsyncState;
        client.EndConnect(ar);
        Debug.LogFormat("->client[{0}] connected to server", (IPEndPoint)client.LocalEndPoint);
    }
    //接收数据
    private static void Receive(Socket client)
    {
        StateObject state = new StateObject();
        state.workSocket = client;
        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
    }
    //接收回调
    private static void ReceiveCallback(IAsyncResult ar)
    {
        StateObject state = (StateObject)ar.AsyncState;
        Socket client = state.workSocket;
        int byteLength = client.EndReceive(ar);
        if (byteLength > 0)
        {
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            byte[] bytes = new byte[byteLength];
            Buffer.BlockCopy(state.buffer, 0, bytes, 0, byteLength);
            string content = Encoding.Default.GetString(bytes);
            Debug.LogFormat("->client received datas: {0}", content);
        }
    }
}
