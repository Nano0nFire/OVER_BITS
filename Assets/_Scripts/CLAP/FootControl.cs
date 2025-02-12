using UnityEngine;
using CLAPlus.Extension;
using Unity.Mathematics;

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
        [SerializeField] Transform rToe;
        [SerializeField] Transform lToe;

        [Header("Parameter")]
        [SerializeField] string rIKWeightName;
        [SerializeField] string lIKWeightName;
        [SerializeField] Vector3 rIKHintOffset, lIKHintOffset;
        [SerializeField] float UpperLegToGroundLength;
        [SerializeField] float heelHeight = 0.085f;
        [SerializeField] float rayRange = 0;

        [Header("Others")]
        public bool UseIKCon;
        bool rIsGrounded, lIsGrounded;
        Vector3 rPosCash, lPosCash;
        quaternion rRotCash, lRotCash;
        float rWeightCash, lWeightCash;

        RaycastHit hit1, hit2;
        float RIKWeight
        {
            get
            {
                return rWeightCash = math.lerp(rWeightCash, anim.GetFloat(rIKWeightName), 0.3f);
            }
        }
        float LIKWeight
        {
            get
            {
                return lWeightCash = math.lerp(lWeightCash, anim.GetFloat(lIKWeightName), 0.3f);
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
            // switch (State)
            // {
            //     case States.walk:
            //     case States.dash:
            //     case States.crouch:
            //     case States.falling:
            //     case States.Jump:
            //     case States.AirJump:
                    
            //         break;
            // }
            LocomotionIKControl();
        }

        void LocomotionIKControl()
        {
            // if (!UseIKCon || !clap_m._IsGrounded)
            // {
            //     anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            //     anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
            //     anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            //     anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            //     anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1);
            //     anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1);
            //     return;
            // }

            FootIKControl(RIKWeight, true);
            FootIKControl(LIKWeight, false);

            var rootCashed = root.transform;

            anim.SetIKHintPosition(AvatarIKHint.RightKnee, root.position + rootCashed.right * rIKHintOffset.x + rootCashed.up * rIKHintOffset.y + rootCashed.forward * rIKHintOffset.z);
            anim.SetIKHintPosition(AvatarIKHint.LeftKnee, root.position + rootCashed.right * -rIKHintOffset.x + rootCashed.up * rIKHintOffset.y + rootCashed.forward * rIKHintOffset.z);
            anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1);
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1);
        }
        void FootIKControl(float w, bool IsRight)
        {
            Transform foot = IsRight ? rFoot : lFoot;
            Transform UpperLeg = IsRight ? rUpperLeg : lUpperLeg;
            Transform toe = IsRight ? rToe : lToe;
            AvatarIKGoal avatarIKGoal = IsRight ? AvatarIKGoal.RightFoot : AvatarIKGoal.LeftFoot;

            anim.SetIKPositionWeight(avatarIKGoal, w);
            anim.SetIKRotationWeight(avatarIKGoal, w);

            if (w < 0.1f) // weightが0.1未満ならIKを制御しない
            {
                if (IsRight)
                {
                    rIsGrounded = false;
                }
                else
                {
                    lIsGrounded = false;
                }
                return;
            }

            if (Physics.Raycast(new Vector3(foot.position.x, UpperLeg.position.y, foot.position.z), -transform.up, out hit1, UpperLegToGroundLength + rayRange, groundLayer))
            {
                Vector3 normal;
                if (Physics.Raycast(new Vector3(toe.position.x, UpperLeg.position.y, toe.position.z), -transform.up, out hit2, UpperLegToGroundLength + rayRange, groundLayer))
                    normal = (hit2.normal + hit1.normal) / 2;
                else
                    normal = hit1.normal;

                Quaternion rotCash = foot.rotation;
                if (IsRight)
                    rRotCash = Quaternion.Lerp(rRotCash, Quaternion.FromToRotation(transform.up, normal) * new Quaternion(0, rotCash.y, 0, rotCash.w), 0.2f);
                else
                    lRotCash = Quaternion.Lerp(lRotCash, Quaternion.FromToRotation(transform.up, normal) * new Quaternion(0, rotCash.y, 0, rotCash.w), 0.2f);

                anim.SetIKPosition(avatarIKGoal, new Vector3(foot.position.x, hit1.point.y + heelHeight, foot.position.z));
                anim.SetIKRotation(avatarIKGoal, IsRight ? rRotCash : lRotCash);
            }
        }
    }
}