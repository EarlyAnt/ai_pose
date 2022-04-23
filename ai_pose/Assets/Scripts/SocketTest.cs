using UnityEngine;
using System.Collections;

public class SocketTest : MonoBehaviour
{
    private bool ServerRunning { get; set; }
    private bool ClientRunning { get; set; }
    private string port = "8080";

    // Use this for initialization
    void Start()
    {
        //SocketServer.OutputLog = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && !ServerRunning)
        {
            //print("SocketServer is running...");
            ServerRunning = true;
            SocketServer.OpenServer(NetHelper.GetLocalIPv4(), this.port);
        }
        else if (Input.GetKeyDown(KeyCode.T) && ServerRunning)
        {
            //print("SocketServer is stopped.");
            ServerRunning = false;
            SocketServer.StopSocket();
        }

        if (Input.GetKeyDown(KeyCode.Y) && !ClientRunning)
        {
            //print("SocketClient is running...");
            ClientRunning = true;
            SocketClient.ConnectServercer(NetHelper.GetLocalIPv4(), this.port);
        }
        else if (Input.GetKeyDown(KeyCode.U) && ClientRunning)
        {
            //print("SocketClient is stopped.");
            ClientRunning = false;
            SocketClient.StopSocket();
        }
        else if (Input.GetKeyDown(KeyCode.Space) && ClientRunning)
        {
            SocketClient.Send("hahaha");
        }
    }
}
