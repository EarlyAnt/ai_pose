using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BoneControllByIK : MonoBehaviour
{
    /************************************************属性与变量命名************************************************/
    [SerializeField]
    private int port = 8080;
    [SerializeField]
    private string message;
    [SerializeField]
    private GizmosData gizmosDatas;
    [SerializeField, Range(0.1f, 100f)]
    private float boneScale = 1f;
    [SerializeField]
    private Vector3 rate = Vector3.one * -0.25f;
    [SerializeField]
    private Vector3 offset = Vector3.zero;
    [SerializeField, Range(0f, 1f)]
    private float lerpDuration = 0.5f;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Transform boneRoot;
    [SerializeField]
    private List<BodyBone> bones;
    private Transform leftTarget;
    private Transform rightTarget;
    private bool serverRunning { get; set; }
    private bool clientRunning { get; set; }
    private Queue<Action> taskList = new Queue<Action>();
    private const string BODY = "BODY";
    /************************************************Unity方法与事件***********************************************/
    private void Start()
    {
        SocketServer.RecevieMessage += this.OnServerReceiveMessage;

        if (this.animator == null)
            this.animator = this.GetComponent<Animator>();

        this.leftTarget = this.bones.Find(t => t.Name == "LEFT_WRIST").Bone;
        this.rightTarget = this.bones.Find(t => t.Name == "RIGHT_WRIST").Bone;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && !this.serverRunning)
        {
            this.serverRunning = true;
            SocketServer.StartServer(NetHelper.GetLocalIPv4(), this.port);
        }
        else if (Input.GetKeyDown(KeyCode.T) && this.serverRunning)
        {
            this.serverRunning = false;
            SocketServer.StopServer();
        }

        if (Input.GetKeyDown(KeyCode.Y) && !this.clientRunning)
        {
            this.clientRunning = true;
            SocketClient.Connect(NetHelper.GetLocalIPv4(), this.port);
        }
        else if (Input.GetKeyDown(KeyCode.U) && this.clientRunning)
        {
            this.clientRunning = false;
            SocketClient.Disconnect();
        }
        else if (Input.GetKeyDown(KeyCode.Space) && this.clientRunning)
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
                //Debug.LogErrorFormat("BoneControllByNet.Update->set bone position error: {0}", ex);
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
    private void OnAnimatorIK(int layerIndex)
    {
        if (this.leftTarget != null)
        {
            this.animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            this.animator.SetIKPosition(AvatarIKGoal.LeftHand, this.leftTarget.position);
            //this.animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
            //this.animator.SetIKRotation(AvatarIKGoal.LeftHand, this.leftTarget.Bone.rotation);
        }

        if (this.rightTarget != null)
        {
            this.animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            this.animator.SetIKPosition(AvatarIKGoal.RightHand, this.rightTarget.position);
            //this.animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
            //this.animator.SetIKRotation(AvatarIKGoal.RightHand, this.leftTarget.Bone.rotation);
        }
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
        if (this.serverRunning)
        {
            this.serverRunning = false;
            SocketServer.StopServer();
        }
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

        this.bones[0].CreateBone(this.boneRoot, BODY, this.boneScale, false);

        TextAsset ta = Resources.Load<TextAsset>(string.Format("CSV/data"));
        Debug.LogFormat("LoadCsvData->data file name: {0}", ta.name);
        List<string> lines = new List<string>(ta.text.Split('\n'));

        string line = lines[0];
        string[] columnHeaders = line.Split('#');
        for (int i = 0; i < columnHeaders.Length; i++)
            this.bones[i + 1].CreateBone(this.boneRoot, columnHeaders[i], this.boneScale, false);
    }
    private void OnServerReceiveMessage(string message)
    {
        //Debug.LogFormat("->server receive data: {0}", message);

        if (string.IsNullOrEmpty(message))
        {
            //Debug.LogWarning("BoneControllByNet.OnServerReceiveMessage->parameter 'message' is null or empty");
            return;
        }

        List<string> positions = new List<string>(message.Split('#'));
        if (positions.Count == 0)
        {
            //Debug.LogWarning("BoneControllByNet.OnServerReceiveMessage->none position data");
            return;
        }

        try
        {
            foreach (string position in positions)
            {
                BoneData boneData = this.GetBoneData(position);
                BodyBone bodyBone = this.bones.Find(t => t.Name == boneData.BoneName);
                if (bodyBone == null)
                {
                    //Debug.LogErrorFormat("BoneControllByNet.OnServerReceiveMessage->can not find the bone[{0}]", bodyBone.Name);
                    continue;
                }

                this.taskList.Enqueue(() =>
                {
                    Debug.LogFormat("->bone: {0}, position: {1}", boneData.BoneName, boneData.Position);
                    Vector3 pos = boneData.Position;
                    pos.x *= this.rate.x;
                    pos.y *= this.rate.y;
                    pos.z *= this.rate.z;
                    pos += this.offset;
                    bodyBone.SetPosition(pos, this.lerpDuration);
                });
            }
        }
        catch (Exception ex)
        {
            //Debug.LogWarning(ex);
        }
    }
    private BoneData GetBoneData(string positionData)
    {
        try
        {
            string[] datas = positionData.Split(',');
            if (datas.Length != 4)
                throw new Exception("BoneControllByNet.GetBoneData->invalid position data" + (datas.Length > 0 ? string.Format(", bone name: {0}", datas[0]) : ""));

            string bone = datas[0];
            float x = float.Parse(datas[1]);
            float y = float.Parse(datas[2]);
            float z = float.Parse(datas[3]);
            return new BoneData(bone, x, y, z);
        }
        catch (Exception ex)
        {
            //Debug.LogErrorFormat("BoneControllByNet.GetBoneData->error: {0}", ex);
            return new BoneData("", 0, 0, 0);
        }
    }
}
