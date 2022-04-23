using UnityEngine;
using System.Collections;

public class SocketTest : MonoBehaviour
{
    /************************************************属性与变量命名************************************************/
    [SerializeField]
    private string message;
    [SerializeField]
    private int port = 8080;
    private bool serverRunning { get; set; }
    private bool clientRunning { get; set; }
    /************************************************Unity方法与事件***********************************************/
    private void Start()
    {
        SocketServer.RecevieMessage += this.OnServerReceiveMessage;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && !serverRunning)
        {
            //print("SocketServer is running...");
            serverRunning = true;
            SocketServer.StartServer(NetHelper.GetLocalIPv4(), this.port);
        }
        else if (Input.GetKeyDown(KeyCode.T) && serverRunning)
        {
            //print("SocketServer is stopped.");
            serverRunning = false;
            SocketServer.StopServer();
        }

        if (Input.GetKeyDown(KeyCode.Y) && !clientRunning)
        {
            //print("SocketClient is running...");
            clientRunning = true;
            SocketClient.Connect(NetHelper.GetLocalIPv4(), this.port);
        }
        else if (Input.GetKeyDown(KeyCode.U) && clientRunning)
        {
            //print("SocketClient is stopped.");
            clientRunning = false;
            SocketClient.Disconnect();
        }
        else if (Input.GetKeyDown(KeyCode.Space) && clientRunning)
        {
            SocketClient.Send(this.message);
        }
    }
    /************************************************自 定 义 方 法************************************************/
    private void OnServerReceiveMessage(string message)
    {
        SocketServer.Send(string.Format("服务器已收到: {0}", message));
    }
}
