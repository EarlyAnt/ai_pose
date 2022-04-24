using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BoneControllByNet : MonoBehaviour
{
    /************************************************属性与变量命名************************************************/
    [SerializeField]
    private Transform boneRoot;
    [SerializeField, Range(0.1f, 100f)]
    private float boneScale = 1f;
    [SerializeField]
    private Vector3 rate = Vector3.one * -0.25f;
    [SerializeField]
    private List<BodyBone> bones;
    [SerializeField]
    private string message;
    [SerializeField]
    private int port = 8080;
    [SerializeField]
    private GizmosData gizmosDatas;
    private bool serverRunning { get; set; }
    private bool clientRunning { get; set; }
    private Queue<Action> taskList = new Queue<Action>();
    /************************************************Unity方法与事件***********************************************/
    private void Start()
    {
        SocketServer.RecevieMessage += this.OnServerReceiveMessage;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && !serverRunning)
        {
            serverRunning = true;
            SocketServer.StartServer(NetHelper.GetLocalIPv4(), this.port);
        }
        else if (Input.GetKeyDown(KeyCode.T) && serverRunning)
        {
            serverRunning = false;
            SocketServer.StopServer();
        }

        if (Input.GetKeyDown(KeyCode.Y) && !clientRunning)
        {
            clientRunning = true;
            SocketClient.Connect(NetHelper.GetLocalIPv4(), this.port);
        }
        else if (Input.GetKeyDown(KeyCode.U) && clientRunning)
        {
            clientRunning = false;
            SocketClient.Disconnect();
        }
        else if (Input.GetKeyDown(KeyCode.Space) && clientRunning)
        {
            SocketClient.Send(this.message);
        }

        while (this.taskList.Count > 0)
        {
            try
            {
                this.taskList.Dequeue().Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("BoneControllByNet.Update->set bone position error: {0}", ex);
            }
        }
    }
    private void OnGUI()
    {
        //GUI.Box(new Rect(Screen.width - 260, 10, 250, 150), "Interaction");
        //GUI.Label(new Rect(Screen.width - 245, 30, 250, 30), "Up/Down Arrow : Go Forwald/Go Back");
        //GUI.Label(new Rect(Screen.width - 245, 50, 250, 30), "Left/Right Arrow : Turn Left/Turn Right");
        //GUI.Label(new Rect(Screen.width - 245, 70, 250, 30), "Hit Space key while Running : Jump");
        //GUI.Label(new Rect(Screen.width - 245, 90, 250, 30), "Hit Spase key while Stopping : Rest");
        //GUI.Label(new Rect(Screen.width - 245, 110, 250, 30), "Left Control : Front Camera");
        //GUI.Label(new Rect(Screen.width - 245, 130, 250, 30), "Alt : LookAt Camera");
    }
    private void OnDrawGizmos()
    {
        if (this.gizmosDatas == null || !this.gizmosDatas.Enable || this.gizmosDatas.Datas.Count == 0)
            return;
        
        foreach (var data in this.gizmosDatas.Datas)
        {
            for (int i = 0; i < data.Points.Count; i++)
            {
                Gizmos.color = data.LineColor;
                if (i + 1 < data.Points.Count)
                    Gizmos.DrawLine(data.Points[i].position, data.Points[i + 1].position);
            }
        }
    }
    private void OnApplicationQuit()
    {
        Debug.Log("exit application");
    }
    /************************************************自 定 义 方 法************************************************/
    [ContextMenu("创建骨骼点")]
    private void AutoFillBoneName()
    {
        if (this.boneRoot == null)
            this.boneRoot = this.transform;

        while (this.boneRoot.childCount > 0)
            GameObject.DestroyImmediate(this.boneRoot.GetChild(0).gameObject);

        TextAsset ta = Resources.Load<TextAsset>(string.Format("CSV/data"));
        Debug.LogFormat("LoadCsvData->data file name: {0}", ta.name);
        List<string> lines = new List<string>(ta.text.Split('\n'));

        string line = lines[0];
        string[] columnHeaders = line.Split('#');
        for (int i = 0; i < columnHeaders.Length; i++)
            this.bones[i].CreateBone(this.boneRoot, columnHeaders[i], this.boneScale, false);
    }
    private void OnServerReceiveMessage(string message)
    {
        //Debug.LogFormat("->server receive data: {0}", message);

        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("BoneControllByNet.OnServerReceiveMessage->parameter 'message' is null or empty");
            return;
        }

        string[] positions = message.Split('#');
        if (positions.Length == 0)
        {
            Debug.LogWarning("BoneControllByNet.OnServerReceiveMessage->none position data");
            return;
        }

        foreach (string position in positions)
        {
            string[] datas = position.Split(',');
            if (datas.Length != 4)
            {
                Debug.LogWarning("BoneControllByNet.OnServerReceiveMessage->invalid position data" + (datas.Length > 0 ? string.Format(", bone name: {0}", datas[0]) : ""));
                continue;
            }

            string bone = datas[0];
            float x = float.Parse(datas[1]);
            float y = float.Parse(datas[2]);
            float z = float.Parse(datas[3]);

            BodyBone bodyBone = this.bones.Find(t => t.Name == bone);
            if (bodyBone == null)
            {
                Debug.LogErrorFormat("BoneControllByNet.OnServerReceiveMessage->can not find the bone[{0}]", bone);
                continue;
            }

            this.taskList.Enqueue(() => bodyBone.SetPosition(new Vector3(x * this.rate.x, y * this.rate.y, z * this.rate.z)));
        }
    }
}
