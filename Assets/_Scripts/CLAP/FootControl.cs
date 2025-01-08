using UnityEngine;
using CLAPlus.Extension;

namespace CLAPlus.AnimationControl
{
    public class FootControl : MonoBehaviour
    {
        [SerializeField] CharactorMovement clap_m;
        [SerializeField] Animator anim;
        [SerializeField] LayerMask groundLayer;

        [Header("Root")]
        [SerializeField] Transform root;

        [Header("Foot")]
        [SerializeField] Transform rUpperLeg;
        [SerializeField] Transform lUpperLeg;
        [SerializeField] Transform rFoot;
        [SerializeField] Transform lFoot;

        [Header("Parameter")]
        [SerializeField] string rIKWeightName;
        [SerializeField] string lIKWeightName;
        [SerializeField] Vector3 rIKHintOffset, lIKHintOffset;
        [SerializeField] float UpperLegToHeelLength;
        [SerializeField] float heelHight;
        [SerializeField] float rayRange = 0;
        [SerializeField] float lerpSpeed = 5;

        [Header("Others")]
        public bool UseIKCon;
        RaycastHit hit;
        float RIKWeight
        {
            get
            {
                return anim.GetFloat(rIKWeightName);
            }
        }
        float LIKWeight
        {
            get
            {
                return anim.GetFloat(lIKWeightName);
            }
        }
        States State
        {
            get
            {
                return clap_m.PublicState;
            }
        }

        void OnAnimatorIK(int layerIndex)
        {
            switch (State)
            {
                case States.walk:
                case States.dash:
                case States.crouch:
                case States.falling:
                case States.Jump:
                case States.AirJump:
                    LocomotionIKControl();
                    break;
            }
        }

        void LocomotionIKControl()
        {
            if (!UseIKCon || !clap_m._IsGrounded)
            {
                anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1);
                anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1);
                return;
            }

            FootIKControl(RIKWeight, AvatarIKGoal.RightFoot, rUpperLeg, rFoot);
            FootIKControl(LIKWeight, AvatarIKGoal.LeftFoot, lUpperLeg, lFoot);

            var rootCashed = root.transform;

            anim.SetIKHintPosition(AvatarIKHint.RightKnee, root.position + rootCashed.right * rIKHintOffset.x + rootCashed.up * rIKHintOffset.y + rootCashed.forward * rIKHintOffset.z);
            anim.SetIKHintPosition(AvatarIKHint.LeftKnee, root.position + rootCashed.right * -rIKHintOffset.x + rootCashed.up * rIKHintOffset.y + rootCashed.forward * rIKHintOffset.z);
            anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1);
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1);
        }
        void FootIKControl(float w, AvatarIKGoal avatarIKGoal, Transform UpperLeg, Transform foot)
        {
            anim.SetIKPositionWeight(avatarIKGoal, w);
            anim.SetIKRotationWeight(avatarIKGoal, w);

            if (w < 0.1f) // weightが0.1以上ならIKをスクリプト制御にする
            {
                return;
            }

            Vector3 footIKPos = anim.GetIKPosition(avatarIKGoal);

            if (Physics.Raycast(new Vector3(footIKPos.x, UpperLeg.position.y, footIKPos.z), -transform.up, out hit, UpperLegToHeelLength + rayRange, groundLayer))
            {
                anim.SetIKPosition(avatarIKGoal, hit.point + new Vector3(0, heelHight, 0));

                Quaternion hipCashed = foot.transform.rotation;  // 元のQuaternion

                Quaternion yRotation = new(0, hipCashed.y, 0, hipCashed.w); // Y成分のみを持つQuaternionを作成

                anim.SetIKRotation(avatarIKGoal, Quaternion.Lerp(anim.GetIKRotation(avatarIKGoal), Quaternion.FromToRotation(transform.up, hit.normal) * yRotation, Time.deltaTime * lerpSpeed)); //  * yRotation
            }
        }
    }
}