using UnityEngine;

namespace CLAPlus.AnimationControl
{
    public class BodyControl : MonoBehaviour
    {
        [SerializeField] CharactorMovement clap_m;
        [SerializeField] Animator anim;

        [Header("Parameter")]
        [SerializeField] float bodyPosLerpSpeed = 1, bodyRotLerpSpeed = 1;

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
                    transform.rotation = clap_m.transform.rotation;
                    LocomotionIKControl();
                    break;

                case States.slide:
                    SlideBodyControl();
                    break;

                case States.wallRun:
                    WallrunBodyControl();
                    break;

                case States.climb:
                    ClimbBodyControl();
                    break;


                case States.rush:
                    break;
            }
        }

        void LocomotionIKControl()
        {
            if (clap_m.OnSlope) BodyPositionAdjust();
        }

        void SlideBodyControl()
        {
            Quaternion rotCashed = clap_m.transform.rotation;  // 元のQuaternion

            Quaternion yRotation = new(0, rotCashed.y, 0, rotCashed.w); // Y成分のみを持つQuaternionを作成

            transform.rotation = Quaternion.FromToRotation(clap_m.transform.up, clap_m.GroundDir) * yRotation; //  * yRotation

        }

        void WallrunBodyControl()
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, -(clap_m.WallAngle - 90), 0f), bodyRotLerpSpeed * Time.deltaTime);
        }

        void ClimbBodyControl()
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, -clap_m.WallAngle, 0f), bodyRotLerpSpeed * Time.deltaTime);
        }

        void BodyPositionAdjust()
        {
            float distance = Mathf.Abs(anim.GetIKPosition(AvatarIKGoal.RightFoot).y - anim.GetIKPosition(AvatarIKGoal.LeftFoot).y);
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0, -distance / 2, 0), bodyPosLerpSpeed * Time.deltaTime);
        }
    }
}