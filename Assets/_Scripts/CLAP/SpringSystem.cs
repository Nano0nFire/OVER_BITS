using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace CLAPlus
{
    public class SpringSystem : MonoBehaviour
    {
        public List<Transform> ParentsList = new(); // 揺れものの最初のボーン。このリスト内のボーンとその子達が揺れる
        [SerializeField] float smooth = 5f;
        public float tiltAmount = 15f; // 傾きの最大角度
        TransformAccessArray accessArray;
        NativeArray<Vector3> previousPositionsArray;
        NativeArray<Quaternion> initialLocalRotationArray;
        JobHandle SpringJobHandle;
        bool isReady;

        public void Setup()
        {
            isReady = false;
            Clearing();
            List<Transform> tempTransform = new(); // 全ての制御対象のリスト
            foreach (var parent in ParentsList)
            {
                var parentBone = parent.transform;
                bool isFirst = true;
                foreach (var bone in parentBone.GetComponentsInChildren<Transform>())
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        tempTransform.Add(bone);
                }
            }

            var Max = tempTransform.Count;

            accessArray = new(tempTransform.ToArray());
            previousPositionsArray = new(Max, Allocator.Persistent);
            initialLocalRotationArray = new(Max, Allocator.Persistent);

            for (int i = 0; i < Max; i++)
            {
                previousPositionsArray[i] = tempTransform[i].position;
                initialLocalRotationArray[i] = tempTransform[i].localRotation;
            }

            isReady = true;
        }
        // void OnEnable()
        // {
        //     isReady = false;
        //     Clearing();
        //     List<Transform> tempTransform = new(); // 全ての制御対象のリスト
        //     foreach (var parent in ParentsList)
        //     {
        //         var parentBone = parent.transform;
        //         bool isFirst = true;
        //         foreach (var bone in parentBone.GetComponentsInChildren<Transform>())
        //         {
        //             if (isFirst)
        //                 isFirst = false;
        //             else
        //                 tempTransform.Add(bone);
        //         }
        //     }

        //     var Max = tempTransform.Count;

        //     accessArray = new(tempTransform.ToArray());
        //     previousPositionsArray = new(Max, Allocator.Persistent);
        //     initialLocalRotationArray = new(Max, Allocator.Persistent);

        //     for (int i = 0; i < Max; i++)
        //     {
        //         previousPositionsArray[i] = tempTransform[i].position;
        //         initialLocalRotationArray[i] = tempTransform[i].localRotation;
        //     }

        //     isReady = true;
        // }

        void OnDestroy()
        {
            SpringJobHandle.Complete();

            Clearing();
        }

        void Clearing()
        {
            if (accessArray.isCreated)
                accessArray.Dispose();
            if (previousPositionsArray.IsCreated)
                previousPositionsArray.Dispose();
            if (initialLocalRotationArray.IsCreated)
                initialLocalRotationArray.Dispose();
        }

        void Update()
        {
            if (!isReady)
                return;

            SpringJobHandle = new SpringJob()
            {
                t = Time.deltaTime,
                smooth = smooth,
                tiltAmount = tiltAmount,
                previousPositionsArray = previousPositionsArray,
                initialLocalRotationArray = initialLocalRotationArray
            }.Schedule(accessArray);

            SpringJobHandle.Complete();
        }

        [BurstCompile]
        struct SpringJob : IJobParallelForTransform
        {
            [ReadOnly] public float t;
            [ReadOnly] public float smooth;
            [ReadOnly] public float tiltAmount;
            public NativeArray<Vector3> previousPositionsArray;
            [ReadOnly] public NativeArray<Quaternion> initialLocalRotationArray;

            public void Execute(int index, TransformAccess xform)
            {
                Vector3 currentPosition = xform.position;
                Vector3 worldMovementDir = currentPosition - previousPositionsArray[index];
                previousPositionsArray[index] = currentPosition;

                if (worldMovementDir.magnitude > 0.001f) // 移動している場合
                {
                    // ワールド空間の移動方向をローカル空間に変換
                    Vector3 localMovementDir = xform.localToWorldMatrix.inverse.MultiplyVector(worldMovementDir);

                    // ローカル空間での傾き計算
                    float tiltX = Mathf.Clamp(-localMovementDir.z * tiltAmount, -tiltAmount, tiltAmount);
                    float tiltZ = Mathf.Clamp(-localMovementDir.x * tiltAmount, -tiltAmount, tiltAmount);

                    if (tiltX < 0)
                        tiltX *= 0.0001f;

                    Quaternion tiltRotation = Quaternion.Euler(tiltX * 3, 0, -tiltZ * 3);
                    Quaternion targetRotation = initialLocalRotationArray[index] * tiltRotation;

                    // 傾きをスムーズに適用
                    xform.localRotation = Quaternion.Lerp(xform.localRotation, targetRotation, t * smooth);
                }
                else
                {
                    // 移動がない場合は初期角度に戻る
                    xform.localRotation = Quaternion.Lerp(xform.localRotation, initialLocalRotationArray[index], t * smooth / 2);
                }
            }
        }
    }
}