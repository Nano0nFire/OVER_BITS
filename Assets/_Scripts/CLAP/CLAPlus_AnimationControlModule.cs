using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting.Dependencies.NCalc;

public class CLAPlus_AnimationControlModuel : MonoBehaviour
{
    [SerializeField] GeneralManager generalManager;
    [SerializeField] CLAPlus_MovementModule clap_m;
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
    [SerializeField] float vSpeed, hzSpeed, ySpeed, SpeedAdjust;

    public Vector3 MoveDir;

    void Update()
    {
        MoveSpeedCalculate();
        SetFloat(parameterName_vSpeed, vSpeed, 0.1f, true);
        SetFloat(parameterName_hzSpeed, hzSpeed, 0.1f, true);
        SetFloat(parameterName_ySpeed, ySpeed, 0.5f);

        // if (Mathf.Abs(hzSpeed) > 0.5 && Mathf.Abs(hzSpeed) < 1.5)
        //     anim.speed = 1 + Mathf.Abs(Mathf.Cos(hzSpeed * Mathf.Rad2Deg) / 10);
        // else
        //     anim.speed = 1;

        // switch (State)
        // {
        //     case States.walk:
        //     case States.dash:
        //     case States.crouch:
        //         // MoveSpeedCalculate();
        //         anim.SetFloat(parameterName_vSpeed, vSpeed);
        //         anim.SetFloat(parameterName_hzSpeed, hzSpeed);
        //         break;
        // }

        anim.SetBool(parameterName_IsGrounded, clap_m._IsGrounded);
    }

    public void Rush()
    {
        anim.SetTrigger(triggerName_Rush);
    }
    public void Dodge()
    {
        anim.SetTrigger(triggerName_Dodge);
    }
    public void Jump()
    {
        anim.SetTrigger(triggerName_Jump);
    }
    public void WallRun(bool IsWallRight)
    {
        if (IsWallRight)
            anim.SetTrigger(triggerName_RWallrun);
        else
            anim.SetTrigger(triggerName_LWallrun);
    }
    public void Climb()
    {
        anim.SetTrigger(triggerName_Climb);
    }
    public void Slide()
    {
        anim.SetTrigger(triggerName_Slide);
    }
    public void AnimationCancel()
    {
        anim.SetTrigger(triggerName_AnimationCancel);
    }
    void MoveSpeedCalculate()
    {
        vSpeed = Mathf.Round(Vector3.Dot(rb.linearVelocity, transform.forward) * 100) / 100;
        hzSpeed = Mathf.Round(Vector3.Dot(rb.linearVelocity, transform.right) * 100) / 100;
        ySpeed = Mathf.Round(Vector3.Dot(rb.linearVelocity, transform.up) * 100) / 100;
    }

    void SetFloat(string name, float value, float damp, bool UseAdjust = false)
    {
        if (UseAdjust)
        {
            // 以下動作環境によって調整する必要あり
            if (Mathf.Abs(value) <= 1.5)
                anim.SetFloat(name, 3 * value, damp, Time.deltaTime);
            else if (value > 0)
                anim.SetFloat(name, 0.33333f * value + 2.33333f, damp, Time.deltaTime);
            else
                anim.SetFloat(name, 0.33333f * value - 2.33333f, damp, Time.deltaTime);
        }
        else
        {
            anim.SetFloat(name, value, damp, Time.deltaTime);
        }

        if (Mathf.Abs(anim.GetFloat(name)) <= 0.005) // 絶対値が0.01以下なら0にする
            anim.SetFloat(name, 0);
    }
}