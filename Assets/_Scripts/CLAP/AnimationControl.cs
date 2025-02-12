using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace CLAPlus.AnimationControl
{
    public class AnimationControl : NetworkBehaviour
    {
        [SerializeField] ClientGeneralManager generalManager;
        [SerializeField] CharactorMovement clap_m;
        [SerializeField] Animator anim;
        [SerializeField] Rigidbody rb;
        States State
        {
            get
            {
                return clap_m.PublicState;
            }
        }
        [SerializeField] string parameterName_vSpeed, parameterName_hzSpeed, parameterName_ySpeed, parameterName_IsGrounded;
        [SerializeField] string triggerName_Jump, triggerName_Rush, triggerName_Dodge, triggerName_RWallrun, triggerName_LWallrun, triggerName_Climb, triggerName_Slide, triggerName_AnimationCancel;
        [SerializeField] float vSpeed, hzSpeed, ySpeed, tempVSpeed, tempHzSpeed, tempYSpeed;
        int time;
        bool tempIsGrounded;
        public bool isOwner = false;

        public Vector3 MoveDir;

        void Update()
        {
            if (!isOwner)
                return;

            time = (int)Time.time;

            MoveSpeedCalculate();
            SetFloat(parameterName_vSpeed, vSpeed, 0.1f, true, true, ref tempVSpeed);
            SetFloat(parameterName_hzSpeed, hzSpeed, 0.1f, true, true, ref tempHzSpeed);
            SetFloat(parameterName_ySpeed, ySpeed, 0.5f, false, true, ref tempYSpeed);

            if (tempIsGrounded != clap_m._IsGrounded)
            {
                tempIsGrounded = clap_m._IsGrounded;
                anim.SetBool(parameterName_IsGrounded, tempIsGrounded);
                SetAnimationParameter<bool>(parameterName_IsGrounded, tempIsGrounded);
            }
        }

        public void Rush()
        {
            anim.SetTrigger(triggerName_Rush);
            SetAnimationParameter<string>(triggerName_Rush, "");
        }
        public void Dodge()
        {
            anim.SetTrigger(triggerName_Dodge);
            SetAnimationParameter<string>(triggerName_Dodge, "");
        }
        public void Jump()
        {
            anim.SetTrigger(triggerName_Jump);
            SetAnimationParameter<string>(triggerName_Jump, "");
        }
        public void WallRun(bool IsWallRight)
        {
            if (IsWallRight)
            {
                anim.SetTrigger(triggerName_RWallrun);
                SetAnimationParameter<string>(triggerName_RWallrun, "");
            }
            else
            {
                anim.SetTrigger(triggerName_LWallrun);
                SetAnimationParameter<string>(triggerName_LWallrun, "");
            }
        }
        public void Climb()
        {
            anim.SetTrigger(triggerName_Climb);
            SetAnimationParameter<string>(triggerName_Climb, "");
        }
        public void Slide()
        {
            anim.SetTrigger(triggerName_Slide);
            SetAnimationParameter<string>(triggerName_Slide, "");
        }
        public void AnimationCancel()
        {
            anim.SetTrigger(triggerName_AnimationCancel);
            SetAnimationParameter<string>(triggerName_AnimationCancel, "");
        }
        void MoveSpeedCalculate()
        {
            vSpeed = Mathf.Round(Vector3.Dot(rb.linearVelocity, transform.forward) * 100) / 100;
            hzSpeed = Mathf.Round(Vector3.Dot(rb.linearVelocity, transform.right) * 100) / 100;
            ySpeed = Mathf.Round(Vector3.Dot(rb.linearVelocity, transform.up) * 100) / 100;
        }

        void SetFloat(string name, float value, float damp, bool UseAdjust, bool UseNetwork, ref float tempValue)
        {
            if (UseAdjust)
            {
                // BlendTreeの推移に合わせて式を各自で変える必要がある
                anim.SetFloat(name,
                              value > 0 ?
                                0.6f * math.sqrt(10*math.abs(value)) :
                                -0.6f * math.sqrt(10*math.abs(value)),
                              damp,
                              Time.deltaTime);
            }
            else
            {
                anim.SetFloat(name, value, damp, Time.deltaTime);
            }

            if (Mathf.Abs(anim.GetFloat(name)) <= 0.005) // 絶対値が0.01以下なら0にする
                anim.SetFloat(name, 0);

            if (!UseNetwork)
                return;

            float currentValue = anim.GetFloat(name);

            if (tempValue == currentValue && time % 2 == 0)
                return;

            tempValue = currentValue;

            SetAnimationParameter<float>(name, currentValue);
        }

        // アニメーションパラメーターを設定する汎用関数
        public void SetAnimationParameter<T>(string parameterName, T value)
        {
            if (!IsOwner) return;

            if (typeof(T) == typeof(float))
            {
                anim.SetFloat(parameterName, (float)(object)value);
                SetFloatParameterServerRpc(parameterName, (float)(object)value);
            }
            else if (typeof(T) == typeof(int))
            {
                anim.SetInteger(parameterName, (int)(object)value);
                SetIntParameterServerRpc(parameterName, (int)(object)value);
            }
            else if (typeof(T) == typeof(bool))
            {
                anim.SetBool(parameterName, (bool)(object)value);
                SetBoolParameterServerRpc(parameterName, (bool)(object)value);
            }
            else if (typeof(T) == typeof(string))
            {
                anim.SetTrigger(parameterName);
                SetTriggerParameterServerRpc(parameterName);
            }
        }

        // パラメーターの同期をサーバーに通知するRPC (float)
        [ServerRpc]
        private void SetFloatParameterServerRpc(string parameterName, float value)
        {
            SetFloatParameterClientRpc(parameterName, value);
        }

        // パラメーターの同期をサーバーに通知するRPC (int)
        [ServerRpc]
        private void SetIntParameterServerRpc(string parameterName, int value)
        {
            SetIntParameterClientRpc(parameterName, value);
        }

        // パラメーターの同期をサーバーに通知するRPC (bool)
        [ServerRpc]
        private void SetBoolParameterServerRpc(string parameterName, bool value)
        {
            SetBoolParameterClientRpc(parameterName, value);
        }

        // パラメーターの同期をサーバーに通知するRPC (trigger)
        [ServerRpc]
        private void SetTriggerParameterServerRpc(string parameterName)
        {
            SetTriggerParameterClientRpc(parameterName);
        }

        // クライアントに同期するRPC (float)
        [ClientRpc]
        private void SetFloatParameterClientRpc(string parameterName, float value)
        {
            if (IsOwner) return;  // 自分自身の処理は無視する
            anim.SetFloat(parameterName, value);
        }

        // クライアントに同期するRPC (int)
        [ClientRpc]
        private void SetIntParameterClientRpc(string parameterName, int value)
        {
            if (IsOwner) return;
            anim.SetInteger(parameterName, value);
        }

        // クライアントに同期するRPC (bool)
        [ClientRpc]
        private void SetBoolParameterClientRpc(string parameterName, bool value)
        {
            if (IsOwner) return;
            anim.SetBool(parameterName, value);
        }

        // クライアントに同期するRPC (trigger)
        [ClientRpc]
        private void SetTriggerParameterClientRpc(string parameterName)
        {
            if (IsOwner) return;
            anim.SetTrigger(parameterName);
        }
    }
}