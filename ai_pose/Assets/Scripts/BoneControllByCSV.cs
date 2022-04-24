using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BoneControllByCSV : MonoBehaviour
{
    [SerializeField]
    private string dataFileName;
    [SerializeField]
    private Transform boneRoot;
    [SerializeField]
    private List<BodyBone> bones;
    [SerializeField, Range(0.02f, 1f)]
    private float interval = 0.06f;
    private Dictionary<string, List<Vector3>> boneDatas = new Dictionary<string, List<Vector3>>();
    private int index = 0;

    private void Start()
    {
        Application.targetFrameRate = 30;

        this.LoadData();
        this.StartCoroutine(this.ReadData());
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

    private void OnApplicationQuit()
    {
        Debug.Log("exit application");
    }

    private void LoadData()
    {
        TextAsset ta = Resources.Load<TextAsset>(string.Format("CSV/{0}", this.dataFileName));
        Debug.LogFormat("LoadCsvData->data file name: {0}", ta.name);
        List<string> lines = new List<string>(ta.text.Split('\n'));

        this.bones = new List<BodyBone>();
        string line = lines[0];
        string[] columnHeaders = line.Split('#');
        foreach (string header in columnHeaders)
        {
            BodyBone bone = new BodyBone();
            bone.CreateBone(this.boneRoot, header);
            this.bones.Add(bone);
            this.boneDatas.Add(header, new List<Vector3>());
        }

        lines.RemoveAt(0);
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i] == "")
                break;

            string[] positions = lines[i].Split('#');
            if (positions.Length != this.bones.Count)
                continue;

            for (int j = 0; j < positions.Length; j++)
            {
                string[] datas = positions[j].Split(',');
                if (datas.Length != 3)
                    continue;

                float x = float.Parse(datas[0]);
                float y = float.Parse(datas[1]);
                float z = float.Parse(datas[2]);

                this.boneDatas.ToList()[j].Value.Add(new Vector3(x, y, z));
            }
        }
    }

    private IEnumerator ReadData()
    {
        while (this.gameObject.activeInHierarchy)
        {
            for (int i = 0; i < this.bones.Count; i++)
            {
                string boneName = this.bones[i].Name;
                Vector3 position = this.boneDatas[boneName][this.index];
                Debug.LogFormat("bone: {0}, position: {1}", boneName, position);
                this.bones[i].SetPosition(position, 0);
            }
            this.index = (this.index + 1) % this.boneDatas.ToList()[0].Value.Count;
            yield return new WaitForSeconds(this.interval);
        }
    }
}
