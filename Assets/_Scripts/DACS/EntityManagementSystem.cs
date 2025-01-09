using System.Collections.Generic;
using DACS.Projectile;
using Unity.Burst;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Jobs;

namespace DACS.Entities
{
    public class EntityManagementSystem : NetworkBehaviour
    {
        [SerializeField] EntitiesSO EntitiesSO;
        PlayerDataManager pdManager;
        [SerializeField] BulletControl_Basic bcNormal;
        Transform PlayerTransform; // ClientGeneralManagerから設定
        public ulong nwID; // ClientGeneralManagerから設定
        public float MaxSimulationDistance = 300;
        Queue<GameObject>[][] DisabledEntities; // 待機中のEntityをキューで把握
        [SerializeField] List<Transform> SpawnerTransformList = new(); // 読み込み範囲外のEntityを無効化する用のTransformList
        NativeArray<bool> IsActiveArray;
        TransformAccessArray transformAccessArray;
        NativeQueue<int> DeactivateEntitiesIDQueue;
        NativeArray<int> DeactivateEntitiesIDArray;
        NativeQueue<int> ActivateEntitiesIDQueue;
        NativeArray<int> ActivateEntitiesIDArray;
        bool isReady = false;

        void Start()
        {
            if (!IsOwner)
                return;

            pdManager = PlayerDataManager.Instance;
            DisabledEntities = new Queue<GameObject>[EntitiesSO.EntityTypeNum][];
            for (int i = 0; i < EntitiesSO.EntityTypeNum; i++)
            {
                DisabledEntities[i] = new Queue<GameObject>[EntitiesSO.GetList(i).Count];
                for (int j = 0; j < DisabledEntities[i].Length; j++)
                    DisabledEntities[i][j] = new();
            }

            foreach (var spawner in SpawnerTransformList)
            {
                var component = spawner.GetComponent<EntitySpawner>();
                component.bcNormal = bcNormal;
                component.emSystem = this;
                component.EntitiesSO = EntitiesSO;
                component.pdManager = pdManager;
                component.Setup();
            }
        }
        public void Setup(ClientGeneralManager cgManager)
        {
            if (IsOwner && !IsHost)
                return;

            PlayerTransform = cgManager.transform;
            nwID = cgManager.nwID;
            DeactivateEntitiesIDQueue = new(Allocator.Persistent);
            ActivateEntitiesIDQueue = new(Allocator.Persistent);
            IsActiveArray = new(SpawnerTransformList.Count, Allocator.Persistent);

            isReady = true;
        }
        void Update()
        {
            if (!isReady)
                return;

            transformAccessArray = new(SpawnerTransformList.ToArray());
            var checkJob = new EntityDistanceCheckJob()
            {
                playerPos = PlayerTransform.position,
                maxDistance = MaxSimulationDistance,
                DisableQueue = DeactivateEntitiesIDQueue.AsParallelWriter(),
                ActiveQueue = ActivateEntitiesIDQueue.AsParallelWriter(),
                IsActive = IsActiveArray
            }.Schedule(transformAccessArray);
            checkJob.Complete();
            ActivateEntitiesIDArray = ActivateEntitiesIDQueue.ToArray(Allocator.TempJob);
            DeactivateEntitiesIDArray = DeactivateEntitiesIDQueue.ToArray(Allocator.TempJob);
            foreach (var index in ActivateEntitiesIDArray) // スポナーに読み込み範囲内であることを通知
            {
                if (IsActiveArray[index])
                    continue;

                SpawnerTransformList[index].GetComponent<EntitySpawner>().LoadSpawnerServerRpc(nwID);
                IsActiveArray[index] = true;
            }
            foreach (var index in DeactivateEntitiesIDArray) // スポナーに読み込み範囲外であることを通知
            {
                if (!IsActiveArray[index])
                    continue;

                SpawnerTransformList[index].GetComponent<EntitySpawner>().UnLoadSpawnerServerRpc(nwID);;
                IsActiveArray[index] = false;
            }

            DeactivateEntitiesIDArray.Dispose();
            DeactivateEntitiesIDQueue.Clear();
            ActivateEntitiesIDArray.Dispose();
            ActivateEntitiesIDQueue.Clear();
            transformAccessArray.Dispose();
        }
        public GameObject GetEntity(int FirstIndex, int SecondIndex)
        {
            GameObject entity;
            if (DisabledEntities[FirstIndex][SecondIndex].Count == 0)
                entity = SpawnEntity(FirstIndex, SecondIndex);
            else
                entity = DisabledEntities[FirstIndex][SecondIndex].Dequeue();
            entity.SetActive(true);
            return entity;
        }
        public GameObject SpawnEntity(int FirstIndex, int SecondIndex)
        {
            var data = EntitiesSO.GetEntity(FirstIndex, SecondIndex);
            GameObject spawnedObject = Instantiate(data.EntityPrefab, Vector3.zero, Quaternion.identity);
            spawnedObject.GetComponent<NetworkObject>().Spawn();
            return spawnedObject;
        }
        public void ReturnEntity(GameObject entity)
        {
            var component = entity.GetComponent<EnemyComponent>();
            DisabledEntities[component.entityID_FirstIndex][component.entityID_SecondIndex].Enqueue(entity);
            entity.SetActive(false);
        }
        public override void OnDestroy()
        {
            DeactivateEntitiesIDQueue.Dispose();
            IsActiveArray.Dispose();
            ActivateEntitiesIDQueue.Dispose();
        }


        [BurstCompile]
        struct EntityDistanceCheckJob : IJobParallelForTransform
        {
            public Vector3 playerPos;
            public float maxDistance; // Entityがアクティブでいられる最大距離
            public NativeQueue<int>.ParallelWriter DisableQueue;
            public NativeQueue<int>.ParallelWriter ActiveQueue;
            public NativeArray<bool> IsActive;
            public void Execute(int index, TransformAccess xform)
            {
                if (Vector3.Distance(xform.position, playerPos) > maxDistance && IsActive[index])
                    DisableQueue.Enqueue(index);
                else if (!IsActive[index])
                    ActiveQueue.Enqueue(index);
            }
        }
    }
}