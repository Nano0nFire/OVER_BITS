using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting;

public class CLAPlus_ProceduralAnimationModuel : MonoBehaviour
{
    [SerializeField] CLAPlus_MovementModule clap_m;
    [SerializeField] GeneralManager generalManager;
    [SerializeField] Animator anim;

    [Header("Bones")]
    [SerializeField] Transform rUpperLeg;
    [SerializeField] Transform lUpperLeg;

    [Header("Debug")]
    public bool UseIKPos;
    public bool UseIKRot;

    [Header("Parameter")]
    [SerializeField] string rIKWeightName;
    [SerializeField] string lIKWeightName;
    [SerializeField] float UpperLegToHeelLength;
    [SerializeField] float heelHight;
    [SerializeField] float rayRange = 0;

    [Header("Others")]
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
            return clap_m.State;
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!UseIKPos)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
            return;
        }

        IKPosControl(RIKWeight, AvatarIKGoal.RightFoot, rUpperLeg);
        IKPosControl(LIKWeight, AvatarIKGoal.LeftFoot, lUpperLeg);

        BodyPositionAdjust();
    }

    void IKPosControl(float w, AvatarIKGoal avatarIKGoal, Transform UpperLeg)
    {
        anim.SetIKPositionWeight(avatarIKGoal, w);
        anim.SetIKRotationWeight(avatarIKGoal, w);

        if (w < 0.9f) // weightが0.9以上ならIKをスクリプト制御にする
            return;

        Vector3 footIKPos = anim.GetIKPosition(avatarIKGoal);

        if (Physics.Raycast(new Vector3(footIKPos.x, UpperLeg.position.y, footIKPos.z), -transform.up, out hit, UpperLegToHeelLength + rayRange))
        {
            anim.SetIKPosition(avatarIKGoal, hit.point + new Vector3(0, heelHight, 0)); // - new Vector3(0, heelHight + rayRange, 0)

            Debug.DrawRay(new Vector3(footIKPos.x, UpperLeg.position.y, footIKPos.z), -transform.up * (UpperLegToHeelLength + rayRange), Color.blue, 0.1f);

            if (UseIKRot)
            {
                anim.SetIKRotation(avatarIKGoal, Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation);
            }
        }
    }

    void BodyPositionAdjust()
    {
        float distance = Mathf.Abs(anim.GetIKPosition(AvatarIKGoal.RightFoot).y - anim.GetIKPosition(AvatarIKGoal.LeftFoot).y);
        transform.localPosition = new Vector3(0, -distance / 2, 0);
    }
}
