using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class SocketServer
{
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

    public static string ErrorMsg = string.Empty;
    public static Action<string> StateChanged { get; set; }

    private static Socket server = null;

    /// <summary>
    /// 当前发送数据的客户端
    /// </summary>
    public static IPEndPoint _CurrentClient;
    public static IPEndPoint CurrentClient
    {
        get { return _CurrentClient; }
        set { _CurrentClient = value; }
    }

    /// <summary>
    /// 触发接收消息的委托
    /// </summary>
    public static bool _RevBool = false;
    public static event EventHandler RevBoolChanged = null;
    public static bool RevBool
    {
        get { return _RevBool; }
        set
        {
            if (_RevBool != value)
            {
                _RevBool = value;
                if (_RevBool)
                {
                    RevBoolChanged?.Invoke(0, EventArgs.Empty);
                }
            }
        }
    }
    /// <summary>
    /// 存储客户端连接Socket
    /// </summary>
    public static Dictionary<string, Socket> clientList = new Dictionary<string, Socket> { };
    /// <summary>
    /// 打开服务器
    /// </summary>
    /// <returns></returns>
    public static bool OpenServer(string Ip, string Port)
    {
        try
        {
            IPAddress IP = IPAddress.Parse(Ip);
            IPEndPoint Point = new IPEndPoint(IP, int.Parse(Port));
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(Point);
            server.Listen(10);
            server.BeginAccept(new AsyncCallback(AcceptCallback), server);
            OnStateChanged("服务器打开成功");
            Debug.LogFormat("->>server running: {0}", Point);
            return true;
        }
        catch (Exception ex)
        {
            ErrorMsg = ex.Message;
            OnStateChanged("服务器打开失败:" + ex.Message);
            return false;
        }
    }

    public static void StopSocket()
    {
        if (server != null)
        {
            server.Close();
        }

        if (clientList != null && clientList.Count > 0)
        {
            foreach (var kvp in clientList)
            {
                kvp.Value.Shutdown(SocketShutdown.Both);
                kvp.Value.Disconnect(false);
                kvp.Value.Close();
            }
            clientList.Clear();
        }
    }

    /// <summary>
    /// 连接回调
    /// </summary>
    /// <param name="ar"></param>
    public static void AcceptCallback(IAsyncResult ar)
    {
        try
        {
            Socket listener = ar.AsyncState as Socket;
            if (listener != null)
            {
                Socket client = listener.EndAccept(ar);
                StateObject state = new StateObject();
                state.workSocket = client;
                IPEndPoint clientipe = (IPEndPoint)client.RemoteEndPoint;
                Debug.LogFormat("->>a new client connected: {0}", clientipe);
                clientList.Add(clientipe.ToString(), client);
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RevCallback), state);
                Send(client, "->>server iniitalized");
                OnStateChanged(clientipe.ToString() + "----已连上服务器");
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            }
        }
        catch (Exception ex)
        {
            ErrorMsg = ex.Message;
            OnStateChanged(ErrorMsg);
        }
    }
    /// <summary>
    /// 接收回调
    /// </summary>
    /// <param name="ar"></param>
    public static void RevCallback(IAsyncResult ar)
    {
        StateObject state = (StateObject)ar.AsyncState;
        Socket client = state.workSocket;
        if (client != null)
        {
            IPEndPoint clientipe = (IPEndPoint)client.RemoteEndPoint;
            try
            {
                // Read data from the client socket.
                int byteLength = client.EndReceive(ar);
                if (byteLength > 0)
                {
                    byte[] bytes = new byte[byteLength];
                    Buffer.BlockCopy(state.buffer, 0, bytes, 0, byteLength);
                    string content = Encoding.Default.GetString(bytes);
                    Debug.LogFormat("Server received datas: {0}", content);
                    CurrentClient = clientipe;
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RevCallback), state);
                    RevBool = true;
                    RevBool = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = clientipe.ToString() + "退出";
                OnStateChanged(clientipe.ToString() + "----退出" + ex.Message);
            }
        }

    }
    /// <summary>
    /// 发送回复客户端
    /// </summary>
    /// <param name="handle">客户端的Socket</param>
    public static void Send(Socket client, string message)
    {
        if (client != null && !string.IsNullOrEmpty(message))//确保发送的字节长度不为0
        {
            byte[] bytes = Encoding.Default.GetBytes(message);
            client.BeginSend(bytes, 0, bytes.Length, 0, new AsyncCallback(SendCallback), client);
        }
    }
    /// <summary>
    /// 发送回调
    /// </summary>
    /// <param name="ar"></param>
    private static void SendCallback(IAsyncResult ar)
    {
        Socket handler = (Socket)ar.AsyncState;
        int bytesSent = handler.EndSend(ar);
        //handler.Shutdown(SocketShutdown.Both);
        //handler.Close();
    }

    private static void OnStateChanged(string state)
    {
        if (StateChanged != null)
            StateChanged(state);
    }
}
