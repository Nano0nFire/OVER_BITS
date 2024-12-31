// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.Threading;
// using System.Threading.Tasks;
// using Cysharp.Threading.Tasks;
// using Unity.Mathematics;

// public class CLAPlus_ProceduralAnimationModuel1 : MonoBehaviour
// {
//     [SerializeField] CharactorMovement clap_m;
//     [SerializeField] ClientGeneralManager generalManager;
//     [SerializeField] Animator anim;

//     public float HzInput, VInput;
//     public Vector3 MoveDir
//     {
//         get
//         {
//             return (PlayerTransform.right * HzInput + PlayerTransform.forward * VInput).normalized;
//         }
//         set
//         {
//             HzInput = value.x;
//             VInput = value.z;
//             if (HzInput == 0 && VInput == 0)
//             {
//                 IsWalking = false;
//             }
//             else
//             {
//                 IsWalking = true;
//             }
//         }
//     }
//     [Space(1), Header("Base & Bones")]
//     [SerializeField] Transform PlayerTransform;
//     [SerializeField] Transform baseTransform;
//     [SerializeField] Transform bone_Hip;
//     [SerializeField] Transform bone_Spine;

//     [Space(1), Header("RightLeg")]
//     [SerializeField] Transform r_foot_target;
//     [SerializeField] Transform r_foot_hint;
//     [SerializeField] Transform r_UpperLeg;
//     [SerializeField] Transform r_leg_def;

//     [Space(1), Header("LeftLeg")]
//     [SerializeField] Transform l_foot_target;
//     [SerializeField] Transform l_foot_hint;
//     [SerializeField] Transform l_UpperLeg;
//     [SerializeField] Transform l_leg_def;

//     [Space(1), Header("RightLeg")]
//     [SerializeField] Transform r_hand_target;
//     [SerializeField] Transform r_hand_hint;
//     [SerializeField] Transform r_UpperArm;
//     [SerializeField] Transform r_arm_def;

//     [Space(1), Header("LeftLeg")]
//     [SerializeField] Transform l_hand_target;
//     [SerializeField] Transform l_hand_hint;
//     [SerializeField] Transform l_UpperArm;
//     [SerializeField] Transform l_arm_def;

//     [Space(1), Header("Parameter")]
//     [SerializeField] float walkStepSpan, dashStepSpan, stepHight;
//     [SerializeField] float legLength, heelHight;
//     [SerializeField] float walkSpeed, stepDistanceAdjust;
//     [SerializeField] float maxBodyRotation = 45;
//     [SerializeField] float bodyRotationSpeed;
//     bool isRLeg;
//     [SerializeField] bool UseSpineControll;
//     bool UseAnimation;
//     float inputDir;
//     float r_FootLerp, l_FootLerp;
//     Vector3 r_nextFootPoint, r_currentFootPoint, l_nextFootPoint, l_currentFootPoint, lerpFootPos;
//     Transform FootTarget
//     {
//         get
//         {
//             return isRLeg ? r_foot_target : l_foot_target;
//         }
//     }
//     Transform DefFootPos
//     {
//         get
//         {
//             return isRLeg ? r_leg_def : l_leg_def;
//         }
//     }
//     Vector3 CurrentFootPos
//     {
//         get
//         {
//             return isRLeg ? r_currentFootPoint : l_currentFootPoint;
//         }
//         set
//         {
//             if (isRLeg)
//                 r_currentFootPoint = value;
//             else
//                 l_currentFootPoint = value;
//         }
//     }
//     Vector3 NextFootPos
//     {
//         get
//         {
//             return isRLeg ? r_nextFootPoint : l_nextFootPoint;
//         }
//         set
//         {
//             if (isRLeg)
//                 r_nextFootPoint = value;
//             else
//                 l_nextFootPoint = value;
//         }
//     }
//     Transform UpperLeg
//     {
//         get
//         {
//             return isRLeg ? r_UpperLeg : l_UpperLeg;
//         }
//     }
//     float FootLerp
//     {
//         get
//         {
//             return isRLeg ? r_FootLerp : l_FootLerp;
//         }
//         set
//         {
//             if (isRLeg)
//                 r_FootLerp = value;
//             else
//                 l_FootLerp = value;
//         }
//     }
//     bool IsWalking
//     {
//         get
//         {
//             return _isWalking;
//         }
//         set
//         {
//             if (_isWalking != value && value)
//             {
//                 FootPointCalculate();
//             }
//             _isWalking = value;
//         }
//     }
//     [SerializeField] bool _isWalking, footChanging = false;
//     [SerializeField] Vector3 defSpineRot;
//     RaycastHit legRayHit;

//     // void Start()
//     // {
//     //     // Stay();

//     //     // r_currentFootPoint = r_foot_target.position = r_leg_def.position;
//     //     // l_currentFootPoint = l_foot_target.position = l_leg_def.position;

//     // }

//     void OnAnimatorIK(int layerIndex)
//     {
//         if (Vector3.Distance(FootTarget.position, DefFootPos.position) > walkStepSpan * stepDistanceAdjust && !footChanging)
//         {
//             FootPointCalculate();
//         }

//         FootPointUpdate();

//         SetIKWeight(1);

//         SetIKTargetAndHint(0, r_foot_target, r_foot_hint);
//         SetIKTargetAndHint(1, l_foot_target, l_foot_hint);
//         SetIKTargetAndHint(2, r_hand_target, r_hand_hint);
//         SetIKTargetAndHint(3, l_hand_target, l_hand_hint);
//     }

//     void LateUpdate()
//     {
//         if (UseAnimation) // Animation使用時はスクリプトでの姿勢の制御はしない
//             return;

//         LegDirectionControll();
//     }

//     void FootPointCalculate()
//     {
//         if (!footChanging) // 脚の位置を変更中はLerpをリセットしない
//             FootLerp = 0;

//         footChanging = true;

//         for (float i = 1; !Physics.Raycast(UpperLeg.position + i * walkStepSpan * MoveDir, Vector3.down, out legRayHit, legLength) && i >= 0; i -= 0.05f);

//         NextFootPos = new Vector3(legRayHit.point.x, legRayHit.point.y - heelHight, legRayHit.point.z);
//     }

//     void FootPointUpdate()
//     {
//         if (FootLerp < 1)
//         {
//             FootPointCalculate();
//             lerpFootPos = Vector3.Lerp(CurrentFootPos, NextFootPos, FootLerp);
//             lerpFootPos.y += Mathf.Sin(FootLerp * Mathf.PI) * stepHight;

//             CurrentFootPos = lerpFootPos;
//             FootLerp += Time.deltaTime * walkSpeed;

//             if (FootLerp > 1)
//             {
//                 isRLeg ^= true;

//                 footChanging = false;
//             }
//         }

//         r_foot_target.position = r_currentFootPoint;
//         l_foot_target.position = l_currentFootPoint;
//     }
//     void Stay()
//     {
//         if (Physics.Raycast(r_UpperLeg.position, Vector3.down, out legRayHit, legLength))
//             r_foot_target.position = new Vector3(legRayHit.point.x, legRayHit.point.y - heelHight, legRayHit.point.z);
//         if (Physics.Raycast(l_UpperLeg.position, Vector3.down, out legRayHit, legLength))
//             l_foot_target.position = new Vector3(legRayHit.point.x, legRayHit.point.y - heelHight, legRayHit.point.z);
//     }

//     void SetIKWeight(float i)
//     {
//         // 右手のIKターゲットを設定
//         anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, i);
//         anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, i);
//         anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, i);

//         // 左手のIKターゲットを設定
//         anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, i);
//         anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, i);
//         anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, i);

//         // 右手のIKターゲットを設定
//         anim.SetIKPositionWeight(AvatarIKGoal.RightHand, i);
//         anim.SetIKRotationWeight(AvatarIKGoal.RightHand, i);
//         anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, i);

//         // 左手のIKターゲットを設定
//         anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, i);
//         anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, i);
//         anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, i);

//     }
//     void SetIKTargetAndHint(int i, Transform target, Transform hint)
//     {
//         AvatarIKGoal IKtarget = new();
//         AvatarIKHint IKhint = new();

//         switch (i)
//         {
//             case 0:
//                 IKtarget = AvatarIKGoal.RightFoot;
//                 IKhint = AvatarIKHint.RightKnee;
//                 break;

//             case 1:
//                 IKtarget = AvatarIKGoal.LeftFoot;
//                 IKhint = AvatarIKHint.LeftKnee;
//                 break;

//             case 2:
//                 IKtarget = AvatarIKGoal.RightHand;
//                 IKhint = AvatarIKHint.RightElbow;
//                 break;

//             case 3:
//                 IKtarget = AvatarIKGoal.LeftHand;
//                 IKhint = AvatarIKHint.LeftKnee;
//                 break;
//         }

//         anim.SetIKPosition(IKtarget, target.position);
//         anim.SetIKRotation(IKtarget, target.rotation);
//         anim.SetIKHintPosition(IKhint, hint.position);
//     }

//     void LegDirectionControll()
//     {
//         float bodyDir = 0;
//         inputDir =  - Vector2.SignedAngle(new Vector2(0,1), new Vector2(HzInput, VInput)); // 正面への入力が0, 右への入力が90, 左への入力が-90, 後ろへの入力が-180

//         if (inputDir == 0 || inputDir == -180)
//             bodyDir = 0;
//         else if (inputDir >= -90 && inputDir <= 90)
//             bodyDir = inputDir * maxBodyRotation / 90;
//         else if (inputDir > 90)
//             bodyDir = (inputDir - 180) * maxBodyRotation / 90;
//         else if (inputDir < -90)
//             bodyDir = (inputDir + 180) * maxBodyRotation / 90;

//         baseTransform.localRotation = Quaternion.AngleAxis(bodyDir, Vector3.up);
//         // Debug.Log(defSpineRot.x * Mathf.Cos(Mathf.Deg2Rad * bodyDir) + " : " +defSpineRot.x *  Mathf.Sin(Mathf.Deg2Rad * bodyDir));
//         // bone_Spine.localEulerAngles = new Vector3(defSpineRot.x * Mathf.Cos(Mathf.Deg2Rad * bodyDir), - bodyDir, defSpineRot.x * Mathf.Sin(Mathf.Deg2Rad * bodyDir));

//         // 現在の回転をQuaternionとして取得
//         Quaternion currentRotation = bone_Spine.rotation;

//         // Y軸の回転を指定した角度に設定し、新しい回転を計算
//         Quaternion targetRotation = Quaternion.Euler(0, -bodyDir, 0) * currentRotation;

//         // 新しい回転をオブジェクトに適用
//         bone_Spine.rotation = targetRotation;
//     }
// }
