using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using DACS.Inventory;
using CLAPlus.Extension;

namespace CLAPlus.AnimationControl
{
    public class HandControl : MonoBehaviour
    {
        [SerializeField] Animator anim;
        [HideInInspector] public Transform ItemXform;
        Vector3 itemTargetPos, itemTargetRot;
        [Header("IK")]
        // [HideInInspector]
        public Transform rHandIKTarget;
        // [HideInInspector]
        public Transform lHandIKTarget;
        [SerializeField] Transform rArmIKHint;
        [SerializeField] Transform lArmIKHint;

        [Header("Finger")]
        [Tooltip("0 - thumb\n1 - index\n2 - middle\n3 - ring\n4 - little")]
        [SerializeField] Transform[] R_FingersParentXforms = new Transform[0];
        [Tooltip("0 - thumb\n1 - index\n2 - middle\n3 - ring\n4 - little")]
        [SerializeField] Transform[] L_FingersParentXforms =  new Transform[0];
        [SerializeField] HandStatePreset[] preset = new HandStatePreset[0];
        [SerializeField] ThumbPreset[] R_ThumbPresets = new ThumbPreset[1];
        [SerializeField] ThumbPreset[] L_ThumbPresets = new ThumbPreset[1];

        [HideInInspector] public ItemComponent.Preset[] StatePreset;

        [SerializeField] float Speed = 5;

        bool UseRFControl, UseRTControl, UseLFControl, UseLTControl;
        [SerializeField] bool UseRIK, UseLIK;
        [SerializeField] float rIKLvl, lIKLvl;

        public float R_FingersRotation
        {
            get => r_FingersRotation;
            set
            {
                if (value <= 0.1f)
                    r_FingersRotation = 0.1f;
                else if (value > 1.25)
                    r_FingersRotation = 1.25f;
                else
                    r_FingersRotation = value;
            }
        }
        float r_FingersRotation = 0;
        public int R_ThumbPreset
        {
            get => r_ThumbPreset;
            set
            {
                if (value <= 0)
                    r_ThumbPreset = 0;
                else if (value > R_ThumbEuler.Length)
                    r_ThumbPreset = R_ThumbEuler.Length;
                else
                    r_ThumbPreset = value;
            }
        }
        int r_ThumbPreset = 0;
        public float L_FingersRotation
            {
            get => l_FingersRotation;
            set
            {
                if (value <= 0.1f)
                    l_FingersRotation = 0.1f;
                else if (value > 1.25)
                    l_FingersRotation = 1.25f;
                else
                    l_FingersRotation = value;
            }
        }
        float l_FingersRotation = 0;
        public int L_ThumbPreset
            {
            get => l_ThumbPreset;
            set
            {
                if (value <= 0)
                    l_ThumbPreset = 0;
                else if (value > L_ThumbEuler.Length)
                    l_ThumbPreset = L_ThumbEuler.Length;
                else
                    l_ThumbPreset = value;
            }
        }
        int l_ThumbPreset = 0;

        Transform[] R_FingersArray;
        Transform[] L_FingersArray;
        Transform[] R_ThumbArray;
        Transform[] L_ThumbArray;
        Quaternion[] R_FingersQuaternions;
        Quaternion[] L_FingersQuaternions;
        Vector3[] R_ThumbEuler;
        Vector3[] L_ThumbEuler;

        void Start()
        {
            int i = 0;
            List<Transform> tempListF = new();
            List<Transform> tempListT = new();
            foreach (var p in R_FingersParentXforms)
            {
                if (i == 0)
                    foreach (var f in p.GetComponentsInChildren<Transform>())
                        tempListT.Add(f);
                else
                    foreach (var f in p.GetComponentsInChildren<Transform>())
                        tempListF.Add(f);
                i ++;
            }
            R_FingersArray = tempListF.ToArray();
            R_ThumbArray = tempListT.ToArray();
            R_FingersQuaternions = new Quaternion[tempListF.Count];
            R_ThumbEuler = new Vector3[tempListT.Count];
            tempListT = new();
            tempListF = new();
            i = 0;
            foreach (var p in L_FingersParentXforms)
            {
                if (i == 0)
                    foreach (var f in p.GetComponentsInChildren<Transform>())
                        tempListT.Add(f);
                else
                    foreach (var f in p.GetComponentsInChildren<Transform>())
                        tempListF.Add(f);
                i ++;
            }
            L_FingersArray = tempListF.ToArray();
            L_ThumbArray = tempListT.ToArray();
            L_FingersQuaternions = new Quaternion[tempListF.Count];
            L_ThumbEuler = new Vector3[tempListT.Count];
        }

        void LateUpdate()
        {
            var t = Time.deltaTime;

            if (UseRFControl) FingerControl(R_FingersArray, R_FingersQuaternions, R_FingersRotation, t);
            if (UseLFControl) FingerControl(L_FingersArray, L_FingersQuaternions, L_FingersRotation, t);
            if (UseRTControl) ThumbControl(R_ThumbArray, R_ThumbEuler, R_ThumbPresets[R_ThumbPreset].Rotations, t);
            if (UseLTControl) ThumbControl(L_ThumbArray, L_ThumbEuler, L_ThumbPresets[L_ThumbPreset].Rotations, t);
        }

        void OnAnimatorIK(int layerIndex)
        {
            var t = Time.deltaTime * Speed;

            if (UseRIK)
            {
                rIKLvl = Mathf.Lerp(rIKLvl, 1, t);
                SetIK(true, rIKLvl);

                anim.SetIKPosition(AvatarIKGoal.RightHand, rHandIKTarget.position);
                anim.SetIKRotation(AvatarIKGoal.RightHand, rHandIKTarget.rotation);
                anim.SetIKHintPosition(AvatarIKHint.RightElbow, rArmIKHint.position);
            }
            else
            {
                if (rIKLvl > 0.01f)
                    rIKLvl = Mathf.Lerp(rIKLvl, 0, t);
                SetIK(true, rIKLvl);
            }

            if (UseLIK)
            {
                lIKLvl = Mathf.Lerp(lIKLvl, 1, t);
                SetIK(false, lIKLvl);

                anim.SetIKPosition(AvatarIKGoal.LeftHand, lHandIKTarget.position);
                anim.SetIKRotation(AvatarIKGoal.LeftHand, lHandIKTarget.rotation);
                anim.SetIKHintPosition(AvatarIKHint.LeftElbow, lArmIKHint.position);
            }
            else
            {
                if (lIKLvl > 0.01f)
                    lIKLvl = Mathf.Lerp(lIKLvl, 0, t);
                SetIK(false, lIKLvl);
            }

            if (ItemXform != null)
            {
                ItemXform.localPosition = Vector3.Lerp(ItemXform.localPosition, itemTargetPos, t * Speed);
                ItemXform.localEulerAngles = Vector3.Lerp(Extensions.SimplifyRotation(ItemXform.localEulerAngles), Extensions.SimplifyRotation(itemTargetRot), t * Speed);
            }
        }

        /// <summary>
        /// 第一引数が0以上の場合は指定されたプリセットを元に手の状態を変更する<br />
        /// 引数が-1を指定するとコントロールをオフにします。<br />
        ///
        /// 指定したプリセットがIKのコントロールを前提としている場合は、呼び出し前に、rHandIKTargetとlHandIKTargetを設定してください
        /// </summary>
        /// <param name="PresetTndex"></param>
        public void ChangeHandState(int index)
        {
            if(index == -1)
            {
                UseRFControl = false;
                UseRTControl = false;
                UseLFControl = false;
                UseLTControl = false;

                UseRIK = false;
                UseLIK = false;
            }
            else
            {
                int PresetTndex = StatePreset[index].HandPresetNumber;
                int mode = StatePreset[index].IsRightHandItem;
                itemTargetPos = StatePreset[index].Pos;
                itemTargetRot = StatePreset[index].Rot;

                // 以下指のコントロール
                UseRFControl = true;
                UseRTControl = true;
                UseLFControl = true;
                UseLTControl = true;
                if (mode == 1)
                {
                    R_ThumbPreset = preset[PresetTndex].thumb;
                    R_FingersRotation = preset[PresetTndex].finger;
                }
                else if (mode == 2)
                {
                    L_ThumbPreset = preset[PresetTndex].thumb;
                    L_FingersRotation = preset[PresetTndex].finger;
                }
                else
                {
                    R_ThumbPreset = preset[PresetTndex].thumb;
                    R_FingersRotation = preset[PresetTndex].finger;
                    L_ThumbPreset = preset[PresetTndex].thumb;
                    L_FingersRotation = preset[PresetTndex].finger;
                }

                // 以下IK

                if (preset[PresetTndex].rHintPos != Vector3.zero)
                {
                    UseRIK = true;
                    rArmIKHint.localPosition = preset[PresetTndex].rHintPos;
                }
                else
                    UseRIK = false;

                if (preset[PresetTndex].lHintPos != Vector3.zero)
                {
                    UseLIK = true;
                    lArmIKHint.localPosition = preset[PresetTndex].lHintPos;
                }
                else
                    UseLIK = false;
            }
        }

        void FingerControl(Span<Transform> transforms, Span<Quaternion> rotation, float targetRotation, float t)
        {
            int i = 0;
            foreach (var xform in transforms)
            {
                xform.localRotation = Quaternion.Lerp(rotation[i], quaternion.Euler(-targetRotation, 0, 0), t * Speed);
                rotation[i] = xform.localRotation;
                i ++;
            }
        }

        void ThumbControl(Span<Transform> transforms, Span<Vector3> rotation, Span<Vector3> targetRotations, float t)
        {
            int i = 0;
            Vector3 target;
            foreach (var xform in transforms)
            {
                target = targetRotations[i];
                // xform.localEulerAngles = Vector3.Lerp(rotation[i], targetRotations[i], t * Speed);
                xform.localEulerAngles = Vector3.Lerp(rotation[i], Extensions.SimplifyRotation(target), t * Speed);
                rotation[i] = Extensions.SimplifyRotation(xform.localEulerAngles);
                i ++;
            }
        }

        void SetIK(bool isRight, float volume)
        {
            anim.SetIKPositionWeight
            (
                isRight ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand,
                volume
            );
            anim.SetIKRotationWeight
            (
                isRight ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand,
                volume
            );
            anim.SetIKHintPositionWeight
            (
                isRight ? AvatarIKHint.RightElbow : AvatarIKHint.LeftElbow,
                volume
            );
        }

        [Serializable]
        class ThumbPreset
        {
            public Vector3[] Rotations = new Vector3[3];
        }

        [Serializable]
        class HandStatePreset
        {
            public float finger;
            public int thumb;
            public Vector3 rHintPos;
            public Vector3 lHintPos;
        }
    }
}