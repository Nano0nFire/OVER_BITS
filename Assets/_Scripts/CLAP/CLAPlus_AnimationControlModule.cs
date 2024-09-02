using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

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
            return clap_m.State;
        }
    }
    [SerializeField] string parameterName_vSpeed, parameterName_hzSpeed, parameterName_ySpeed, parameterName_IsGrounded;
    [SerializeField] float vSpeed, hzSpeed, ySpeed, SpeedAdjust;

    public Vector3 MoveDir;

    void Update()
    {
        MoveSpeedCalculate();
        anim.SetFloat(parameterName_vSpeed, vSpeed, 0.1f, Time.deltaTime);
        anim.SetFloat(parameterName_hzSpeed, hzSpeed, 0.1f, Time.deltaTime);
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

    void MoveSpeedCalculate()
    {
        vSpeed = Mathf.Round(Vector3.Dot(rb.velocity, transform.forward) * 100) / 100;
        hzSpeed = Mathf.Round(Vector3.Dot(rb.velocity, transform.right) * 100) / 100;
        ySpeed = Mathf.Round(Vector3.Dot(rb.velocity, transform.up) * 100) / 100;
    }
}
