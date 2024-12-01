using UnityEngine;

public class testSpring : MonoBehaviour
{
    public float tiltAmount = 15f; // 最大の傾き角度
    public float tiltSpeed = 5f;  // 傾きのスムーズさ
    public Transform restrictedDirectionReference; // 傾かない方向を指すオブジェクト
    public float reducePower = 1;

    private Vector3 previousPosition; // 前回の位置
    private Quaternion initialRotation; // 初期状態の回転

    void Start()
    {
        // 初期位置と初期回転を記録
        previousPosition = transform.position;
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // 移動方向を計算
        Vector3 direction = transform.position - previousPosition;
        previousPosition = transform.position;

        // 傾かない方向（北方向など）のベクトルを取得
        Vector3 restrictedDirection = (restrictedDirectionReference.position - transform.position).normalized;

        // restrictedDirection に平行な成分を除去
        direction -= Vector3.Project(direction, restrictedDirection) * reducePower;

        // X軸の傾き（前後方向に基づく）
        float tiltX = Mathf.Clamp(direction.z * tiltAmount, -tiltAmount, tiltAmount);

        // Z軸の傾き（左右方向に基づく）
        float tiltZ = Mathf.Clamp(-direction.x * tiltAmount, -tiltAmount, tiltAmount);

        // 初期回転に対する傾きを計算
        Quaternion tiltRotation = Quaternion.Euler(tiltX * 3, 0, tiltZ * 3);
        Quaternion targetRotation = initialRotation * tiltRotation;

        // 傾きをスムーズに適用
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * tiltSpeed);
    }
}
