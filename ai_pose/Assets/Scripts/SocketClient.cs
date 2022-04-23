using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class SocketClient
{
    public class StateObject
    {
        // 当前客户端的Socket
        public Socket workSocket = null;
        // 可接收的最大字节数
        public const int BufferSize = 20200;
        // 接收的数据存储
        public byte[] buffer = new byte[BufferSize];
    }

    public static bool _BoolRevContent = false;
    public static bool BoolRevContent
    {
        get { return _BoolRevContent; }
        set { _BoolRevContent = value; }
    }
    public static Socket clientT;
    public static bool ConnectServercer(string ip, string port)
    {
        try
        {
            IPAddress IP = IPAddress.Parse(ip);
            IPEndPoint Point = new IPEndPoint(IP, int.Parse(port));
            clientT = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientT.BeginConnect(Point, new AsyncCallback(ConnectCallback), clientT);
            Receive(clientT);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static void StopSocket()
    {
        if (clientT != null && clientT.Connected)
        {
            clientT.Shutdown(SocketShutdown.Both);
            clientT.Disconnect(false);
            clientT.Close();
        }
    }


    private static void ConnectCallback(IAsyncResult ar)
    {
        Socket client = (Socket)ar.AsyncState;
        client.EndConnect(ar);
    }
    private static void Receive(Socket client)
    {
        StateObject state = new StateObject();
        state.workSocket = client;
        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
    }

    public static void ReceiveCallback(IAsyncResult ar)
    {
        StateObject state = (StateObject)ar.AsyncState;
        Socket client = state.workSocket;
        int byteLength = client.EndReceive(ar);
        if (byteLength > 0)
        {
            BoolRevContent = true;
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            byte[] bytes = new byte[byteLength];
            Buffer.BlockCopy(state.buffer, 0, bytes, 0, byteLength);
            string content = Encoding.Default.GetString(bytes);
            Debug.LogFormat("Client received datas: {0}", content);
            BoolRevContent = false;
        }
        else
        {

        }
    }

    public static bool Send(string message)
    {
        try
        {
            byte[] bytes = Encoding.Default.GetBytes(message);
            clientT.BeginSend(bytes, 0, bytes.Length, 0, new AsyncCallback(SendCallback), clientT);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

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
        catch (Exception e)
        {

        }
    }
}
