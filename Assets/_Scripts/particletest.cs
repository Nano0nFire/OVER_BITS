using UnityEngine;

public class particletest : MonoBehaviour
{
    public Transform target; // 目標オブジェクト
    public float speed;
    public float springConstant = 50.0f; // バネ定数
    public float damping = 5.0f; // 減衰定数
    private Vector3 angularVelocity = Vector3.zero; // 角速度



    private void Update()
    {


        // 目標の方向を計算
        Vector3 direction = target.position - transform.position;

        // 現在の向きと目標の向きの間の角度を計算
        Quaternion deltaRotation = Quaternion.LookRotation(direction.normalized) * Quaternion.Inverse(transform.rotation);

        // バネトルクを計算
        Vector3 torque = springConstant * deltaRotation.eulerAngles - damping * angularVelocity;

        // 角速度を更新
        angularVelocity += torque * Time.deltaTime;

        // 回転を更新
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + angularVelocity * Time.deltaTime);

        // 目標の方向に移動
        Vector3 moveDirection = transform.rotation * Vector3.forward;
        transform.position += moveDirection * speed * Time.deltaTime;

    }
}