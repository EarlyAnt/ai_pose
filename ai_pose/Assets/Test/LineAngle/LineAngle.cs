using UnityEngine;

public class LineAngle : MonoBehaviour
{
    [SerializeField]
    private Transform ball1;
    [SerializeField]
    private Transform ball2;
    [SerializeField]
    private Transform body;
    [SerializeField]
    private Color lineColor;
    [SerializeField, Range(0f, 100f)]
    private float rotateSpeed = 0f;

    private void Start()
    {
    }

    private void Update()
    {
        Vector3 targetDir = this.ball2.position - this.ball1.position;
        float angleX = Vector3.Angle(Vector3.right, targetDir);
        float angleY = Vector3.Angle(Vector3.up, targetDir);
        float angleZ = Vector3.Angle(Vector3.forward, targetDir);
        Debug.LogFormat("position->angleX: {0}, angleY: {1}, angleZ: {2}, ball1: {3}, ball2: {4}",
                        angleX, angleY, angleZ, this.ball1.position, this.ball2.position);

        //this.body.rotation = Quaternion.LookRotation(Vector3.RotateTowards(this.body.forward, -targetDir,
        //                                                                   this.rotateSpeed * Time.deltaTime, 0.0f));

        Vector3 eulerAngles = this.body.eulerAngles;
        eulerAngles.y = targetDir.z <= 0 ? angleX : (180 - angleX) + 180;
        this.body.eulerAngles = eulerAngles;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = this.lineColor;
        Gizmos.DrawLine(this.ball1.position, this.ball2.position);
    }
}
