using UnityEngine;
using System.Collections;

public class SocketTest : MonoBehaviour
{
    /************************************************属性与变量命名************************************************/
    [SerializeField]
    private string message;
    private bool ServerRunning { get; set; }
    private bool ClientRunning { get; set; }
    private int port = 8080;
    /************************************************Unity方法与事件***********************************************/
    private void Start()
    {
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && !ServerRunning)
        {
            //print("SocketServer is running...");
            ServerRunning = true;
            SocketServer.StartServer(NetHelper.GetLocalIPv4(), this.port);
        }
        else if (Input.GetKeyDown(KeyCode.T) && ServerRunning)
        {
            //print("SocketServer is stopped.");
            ServerRunning = false;
            SocketServer.StopServer();
        }

        if (Input.GetKeyDown(KeyCode.Y) && !ClientRunning)
        {
            //print("SocketClient is running...");
            ClientRunning = true;
            SocketClient.Connect(NetHelper.GetLocalIPv4(), this.port);
        }
        else if (Input.GetKeyDown(KeyCode.U) && ClientRunning)
        {
            //print("SocketClient is stopped.");
            ClientRunning = false;
            SocketClient.Disconnect();
        }
        else if (Input.GetKeyDown(KeyCode.Space) && ClientRunning)
        {
            SocketClient.Send(this.message);
        }
    }
    /************************************************自 定 义 方 法************************************************/

}
