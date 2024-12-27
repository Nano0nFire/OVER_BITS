using UnityEngine;

public class IKTEST : MonoBehaviour
{
    public Animator animator;       // Animatorコンポーネント
    public Transform handTarget;    // 手の目標位置（Transform）
    public Transform elbowHint;     // 肘のHint位置（Transform）

    [Range(0, 1)]
    public float ikWeight = 1.0f;   // IKの影響度（0～1）

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // 手のIK制御
        if (handTarget != null)
        {
            // 手の位置と回転の重みを設定
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);

            // 手の目標位置と回転を設定
            animator.SetIKPosition(AvatarIKGoal.RightHand, handTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, handTarget.rotation);
        }

        // 肘のHint制御
        if (elbowHint != null)
        {
            // 肘のHintの重みを設定
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, ikWeight);

            // 肘のHintの位置を設定
            animator.SetIKHintPosition(AvatarIKHint.RightElbow, elbowHint.position);
        }
    }
}
