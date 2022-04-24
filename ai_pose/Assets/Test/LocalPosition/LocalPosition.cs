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
        Debug.LogFormat("position->club1: {0}, cube2: {1}, cube3: {2}", cube1.position, cube2.position, cube3.position);
        Debug.LogFormat("localPosition->club1: {0}, cube2: {1}, cube3: {2}", cube1.localPosition, cube2.localPosition, cube3.localPosition);
        Debug.LogFormat("transfer->club1: {0}, {1}", cube1.localPosition, cube1.InverseTransformPoint(cube1.position));
        Debug.LogFormat("transfer->club2: {0}, {1}", cube2.localPosition, cube1.InverseTransformPoint(cube2.position));
        Debug.LogFormat("transfer->club3: {0}, {1}", cube3.localPosition, cube1.InverseTransformPoint(cube3.position));
    }

    void Update()
    {
        
    }
}
