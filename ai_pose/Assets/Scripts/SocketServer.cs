using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class SocketServer
{
    /************************************************属性与变量命名************************************************/
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 22000;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    private static Socket server = null;
    private static Dictionary<string, Socket> clients = new Dictionary<string, Socket> { };
    /************************************************Unity方法与事件***********************************************/

    /************************************************自 定 义 方 法************************************************/
    //打开服务器
    public static bool StartServer(string ip, int port)
    {
        try
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(serverEndPoint);
            server.Listen(10);
            server.BeginAccept(new AsyncCallback(AcceptCallback), server);
            Debug.LogFormat("->server running: {0}", serverEndPoint);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("SocketServer.StartServer->error: {0}", ex.Message);
            return false;
        }
    }
    //停止服务器
    public static void StopServer()
    {
        try
        {
            if (clients != null && clients.Count > 0)
            {
                foreach (var kvp in clients)
                {
                    kvp.Value.Shutdown(SocketShutdown.Both);
                    kvp.Value.Disconnect(false);
                    kvp.Value.Close();
                }
                clients.Clear();
            }

            if (server != null)
            {
                server.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("SocketServer.StopServer->error: {0}", ex.Message);
        }
        Debug.Log("->server stopped");
    }

    //连接回调
    private static void AcceptCallback(IAsyncResult ar)
    {
        try
        {
            Socket listener = ar.AsyncState as Socket;
            if (listener != null)
            {
                Socket client = listener.EndAccept(ar);
                StateObject state = new StateObject();
                state.workSocket = client;
                IPEndPoint clientEndPoint = (IPEndPoint)client.RemoteEndPoint;
                Debug.LogFormat("->a new client connected: {0}", clientEndPoint);
                clients.Add(clientEndPoint.ToString(), client);
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                Send(client, string.Format("server say, welcome [{0}]", clientEndPoint.ToString()));
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            }
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("SocketServer.AcceptCallback->error: {0}", ex);
        }
    }
    //接收回调
    private static void ReceiveCallback(IAsyncResult ar)
    {
        StateObject state = (StateObject)ar.AsyncState;
        Socket client = state.workSocket;
        if (client != null)
        {
            try
            {
                // Read data from the client socket.
                int byteLength = client.EndReceive(ar);
                if (byteLength > 0)
                {
                    byte[] bytes = new byte[byteLength];
                    Buffer.BlockCopy(state.buffer, 0, bytes, 0, byteLength);
                    string content = Encoding.Default.GetString(bytes);
                    Debug.LogFormat("server received datas: {0}", content);
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("SocketServer.ReceiveCallback->client[{0}] error: {1}", ((IPEndPoint)client.RemoteEndPoint).ToString(), ex.Message);
            }
        }
    }
    //发送消息(回复客户端)
    private static void Send(Socket client, string message)
    {
        if (client != null && !string.IsNullOrEmpty(message))//确保发送的字节长度不为0
        {
            byte[] bytes = Encoding.Default.GetBytes(message);
            client.BeginSend(bytes, 0, bytes.Length, 0, SendCallback, client);
        }
    }
    //发送回调
    private static void SendCallback(IAsyncResult ar)
    {
        Socket handler = (Socket)ar.AsyncState;
        int bytesSent = handler.EndSend(ar);
        //handler.Shutdown(SocketShutdown.Both);
        //handler.Close();
    }
}
