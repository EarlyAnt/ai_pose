using UnityEngine;

[System.Serializable]
class BodyBone
{
    [SerializeField]
    private string name;
    [SerializeField]
    private Transform bone;
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private float rate = 1;
    public string Name { get { return this.name; } }
    public Transform Bone { get { return this.bone; } }

    public void CreateBone(Transform root, string name, float scale = 1f, bool active = true)
    {
        if (this.bone == null)
        {
            this.name = name;
            this.bone = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            this.bone.localScale *= scale;
            this.bone.name = name;
            this.bone.SetParent(root);
            this.bone.gameObject.SetActive(active);
        }
    }

    public void SetPosition(Vector3 position)
    {
        if (this.bone != null && this.bone.gameObject.activeInHierarchy)
        {
            this.bone.position = position;
        }
    }
}
