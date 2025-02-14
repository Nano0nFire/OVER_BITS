using Unity.Mathematics;
using UnityEngine;

public class CMTest : MonoBehaviour
{
    [SerializeField] float RayRange1 = 5f, RayRange2 = 5f;
    [SerializeField] float SecondRayRange = 5f;
    [SerializeField] float MaxAngle = 45f;
    [Range(-1, 1)]
    [SerializeField] float HzInput, VInput;
    [SerializeField] float Angle1, Angle2;
    [SerializeField] Vector3 InputDir, MoveDir;
    [SerializeField] Transform Target;

    RaycastHit hit1, hit2;
    Vector3 StartPos1, StartPos2;
    Vector3 PlaneDir;

    CapsuleCollider Collider;

    void Start() {
        Collider = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        RayRange2 = SecondRayRange * math.sin(math.radians(MaxAngle));
        StartPos1 = new(transform.position.x, Collider.bounds.min.y, transform.position.z);
        StartPos2 = new(transform.position.x, Collider.bounds.min.y + RayRange2, transform.position.z);
        StartPos2 += transform.forward * SecondRayRange;
        InputDir = transform.right * HzInput + transform.forward * VInput;

        if (Physics.Raycast(StartPos1, Vector3.down, out hit1, RayRange1))
        {
            if (Physics.Raycast(StartPos2, Vector3.down, out hit2, RayRange2 * 2f))
            {
                PlaneDir = (hit2.point + hit1.point).normalized;
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

        // 徐々に回転させる (スムーズにする場合)
        Target.rotation = Quaternion.Lerp(Target.rotation, Quaternion.LookRotation((hit2.point + hit1.point).normalized, (hit1.normal + hit2.normal).normalized), Time.deltaTime * 5f);

        MoveDir = Vector3.ProjectOnPlane(InputDir, (hit1.normal + hit2.normal).normalized).normalized;

        Angle1 = Vector3.Angle(transform.forward, (hit1.normal + hit2.normal).normalized);
        Angle2 = Vector3.Angle(transform.forward, (hit2.point - hit1.point).normalized);


    }

    void Draw()
    {
        Debug.DrawRay(StartPos1, hit1.distance * Vector3.down, Color.red);
        Debug.DrawRay(StartPos2, hit2.distance * Vector3.down, Color.red);
        Debug.DrawRay(hit1.point, PlaneDir, Color.green);
        Debug.DrawRay(transform.position, InputDir * 2, Color.blue);
        Debug.DrawRay(transform.position, MoveDir * 2, Color.yellow);
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
