using UnityEngine;

public class LocalPosition : MonoBehaviour
{
    [SerializeField]
    private Transform cube1;
    [SerializeField]
    private Transform cube2;
    [SerializeField]
    private Transform cube3;

    void Start()
    {
        Debug.LogFormat("position->club1: {0}, cube2: {1}, cube3: {2}", this.cube1.position, this.cube2.position, this.cube3.position);
        Debug.LogFormat("localPosition->club1: {0}, cube2: {1}, cube3: {2}", this.cube1.localPosition, this.cube2.localPosition, this.cube3.localPosition);
        Debug.LogFormat("transfer->club1: {0}, {1}", this.cube1.localPosition, this.cube1.InverseTransformPoint(this.cube1.position));
        Debug.LogFormat("transfer->club2: {0}, {1}", this.cube2.localPosition, this.cube1.InverseTransformPoint(this.cube2.position));
        Debug.LogFormat("transfer->club3: {0}, {1}", this.cube3.localPosition, this.cube1.InverseTransformPoint(this.cube3.position));
    }

    void Update()
    {
        this.cube1.position = this.cube2.position - new Vector3(10, 0, 0);
        this.cube3.localPosition = this.cube1.InverseTransformPoint(this.cube3.position);
    }
}
