using Unity.Mathematics;
using UnityEngine;

public class CMTest : MonoBehaviour
{
    [SerializeField] float RayRange1 = 5f, RayRange2 = 5f;
    [SerializeField] float SecondRayRange = 5f;
    [SerializeField] float MaxAngle = 45f;
    [SerializeField] float Angle = 0f;
    [SerializeField] Transform Target;

    RaycastHit hit1, hit2;
    Vector3 StartPos1, StartPos2;
    Vector3 PlaneDir;

    CapsuleCollider Collider;

    void Start() {
        Collider = GetComponent<CapsuleCollider>();
        RayRange2 = 0.5f * math.sin(math.radians(MaxAngle));
    }

    void Update()
    {
        StartPos1 = new(transform.position.x, Collider.bounds.min.y, transform.position.z);
        StartPos2 = new(transform.position.x, Collider.bounds.min.y + RayRange2, transform.position.z);
        StartPos2 += transform.forward * SecondRayRange;
        if (Physics.Raycast(StartPos1, Vector3.down, out hit1, RayRange1))
        {
            if (Physics.Raycast(StartPos2, Vector3.down, out hit2, RayRange2 * 2f))
            {
                PlaneDir = hit2.point - hit1.point;
            }
            else
            {
                PlaneDir = Vector3.ProjectOnPlane(transform.forward, hit1.normal).normalized;
            }

            PlaneDir = Vector3.ProjectOnPlane(transform.forward, hit1.normal).normalized;
        }
        else
        {
            PlaneDir = Vector3.zero;
        }

        Draw();
        Angle = Vector3.Angle(transform.forward, PlaneDir);
        Quaternion yRotation = new(0, transform.rotation.y, 0, transform.rotation.w);

        Target.rotation = Quaternion.Lerp(Target.rotation, Quaternion.FromToRotation(transform.up, hit1.normal) * yRotation, Time.deltaTime * 5f);

    }

    void Draw()
    {
        Debug.DrawRay(StartPos1, hit1.distance * Vector3.down, Color.red);
        Debug.DrawRay(StartPos2, hit2.distance * Vector3.down, Color.red);
        Debug.DrawRay(hit1.point, PlaneDir, Color.green);
    }

    /// <summary>
    /// </summary>
    /// <param name="A">投射先</param>
    /// <param name="B">投射させたいベクトル</param>
    /// <returns></returns>
    Vector3 ProjectVector(Vector3 A, Vector3 B)
    {
        return Vector3.Dot(B, A) / Vector3.Dot(A, A) * A;
    }

}
