using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class PlayerStatus : CommonEntityStatus
{
    [SerializeField] CLAPlus_MovementModule clap_m;
    public States SelectedActionSkill;
    public float ExWalkSpeed
    {
        get
        {
            return clap_m.ExWalkSpeed;
        }
        set
        {
            clap_m.ExWalkSpeed = value;
        }
    }
    public float ExDashSpeed
    {
        get
        {
            return clap_m.ExDashSpeed;
        }
        set
        {
            clap_m.ExDashSpeed = value;
        }
    }
    public float ExCrouchSpeed
    {
        get
        {
            return clap_m.ExCrouchSpeed;
        }
        set
        {
            clap_m.ExCrouchSpeed = value;
        }
    }
    public float ExJumpPower
    {
        get
        {
            return clap_m.ExJumpPower;
        }
        set
        {
            clap_m.ExJumpPower = value;
        }
    }

}
