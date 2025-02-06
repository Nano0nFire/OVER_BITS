using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using DACS.Projectile;
using DACS.Entities;

namespace DACS.Entities
{
    public class EntitySpawner : NetworkBehaviour // スポナーにアタッチ
    {
        [HideInInspector] public EntityManagementSystem emSystem; // EntityManagerが設定
        [HideInInspector] public BulletControl_Basic bcNormal; // EntityManagerが設定
        [HideInInspector] public EntitiesSO EntitiesSO; // EntityManagerが設定
        [HideInInspector] public PlayerDataManager pdManager; // EntityManagerが設定

        [Header("Entity Settings")]
        [SerializeField] int SpawnTargetFirstIndex, SpawnTargetSecondIndex;
        [SerializeField,Tooltip("0の場合はデフォルト値に基づいてステータスが決まる")] int EntityLevel = 0; // スポーンするEntityのLevel(0の場合はデフォルト値が使われる)

        [Space(1), Header("Spawner Settings")]
        [SerializeField] int MaxSpawnAmount = 5; // 最大同時スポーン数
        [SerializeField] int SpawnSpan = 10;

        [SerializeField, Tooltip("スポーン範囲を設定\nランダムスポーンの場合は最低3つを指定する/n固定スポーンの場合は一つだけ指定する")]
        Transform[] SpawnMarker = new Transform[3];
        [SerializeField] List<GameObject> Entities = new(); // アクティブなEntityをキューで把握
        public List<Transform> NearbyPalyersTransformList = new(); // スポナーを読み込んでいるプレイヤーのTransform
        // List<ulong> loadedPlayerNWIDs = new();
        EntityConfigs entityData;
        bool isSpawning = false;

        // 座標計算
        Vector3 center;
        float rad;

        public void Setup()
        {
            center = CalculateCenter();
            rad = CalculateRadius();
            entityData ??= EntitiesSO.GetEntity(SpawnTargetFirstIndex, SpawnTargetSecondIndex);

        }

        async void Update()
        {
            if (isSpawning)
                return;

            isSpawning = true;

            if (!IsServer) // Serverのみ呼び出し可能
                return;

            if (Entities.Count == MaxSpawnAmount)
                return;

            await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range((SpawnSpan - 5) > 0 ? SpawnSpan - 5 : 0, (SpawnSpan + 5) > 0 ? SpawnSpan + 5 : 5)), cancellationToken : destroyCancellationToken); // スポーンの間隔を設定から+-5秒にする
            Spawn();

            isSpawning = false;
        }

        async void Spawn()
        {
            if (!IsServer)
                return;

            var entity = emSystem.GetEntity(SpawnTargetFirstIndex, SpawnTargetSecondIndex);
            await UniTask.Delay(TimeSpan.FromSeconds(1)); // NetworkObject.Spawn()を待つ
            // var nwObject = entity.GetComponent<NetworkObject>();
            // Debug.Log($"IsServer: {IsServer}, NetworkObject: {nwObject}");
            // foreach (var id in loadedPlayerNWIDs)
            //     nwObject.NetworkShow(id);
            entity.transform.position = GetSpawnPoint();
            var data = entity.GetComponent<EnemyComponent>();
            Entities.Add(entity);
            data.Spawner = this;
            data.bcNormal = bcNormal;
            data.lvl = EntityLevel != 0 ? EntityLevel : entityData.EntityStatus.BaseLevel;
            data.OnActivate(entityData);
        }

        Vector3 GetSpawnPoint()
        {
            if (SpawnMarker.Length < 3 && SpawnMarker.Length != 1)
                return Vector3.zero;

            for (int i = 0; i < 10; i++) // 10回試行して見つからなかった場合は中心地点を返す
            {
                // ランダム座標を生成
                Vector3 randomPoint = GenerateRandomPoint(center);

                // ポリゴン内チェック
                if (!IsPointInPolygon(randomPoint))
                    continue;

                // NavMesh上チェック
                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                    return hit.position;
            }

            return center;
        }

        Vector3 CalculateCenter()
        {
            Vector3 center = Vector3.zero;
            foreach (var obj in SpawnMarker)
                center += obj.position;
            return center / SpawnMarker.Length;
        }

        float CalculateRadius()
        {
            float x = 0;
            foreach (var obj in SpawnMarker)
                x += Vector3.Distance(center, obj.position);

            return x / SpawnMarker.Length;
        }

        // 指定したオブジェクトで囲まれた範囲内のランダムな点を生成
        Vector3 GenerateRandomPoint(Vector3 center)
        {
            // 囲まれた範囲の適当な点（中央を基準に小さな範囲を作成）
            float randomX = UnityEngine.Random.Range(-rad, rad);
            float randomZ = UnityEngine.Random.Range(-rad, rad);
            return center + new Vector3(randomX, 0, randomZ);
        }

        // 点がポリゴン内にあるかチェック（2D平面とみなして計算）
        bool IsPointInPolygon(Vector3 point)
        {
            int intersections = 0;
            for (int i = 0; i < SpawnMarker.Length; i++)
            {
                Vector3 p1 = SpawnMarker[i].position;
                Vector3 p2 = SpawnMarker[(i + 1) % SpawnMarker.Length].position;

                if (IsIntersecting(point, p1, p2))
                    intersections++;
            }

            // 奇数ならポリゴン内
            return intersections % 2 != 0;
        }

        // 線分と点が交差しているかを判定
        bool IsIntersecting(Vector3 point, Vector3 p1, Vector3 p2)
        {
            // Y座標を無視してXZ平面で計算
            if ((p1.z > point.z) != (p2.z > point.z))
            {
                float slope = (point.z - p1.z) / (p2.z - p1.z);
                float intersectX = p1.x + slope * (p2.x - p1.x);
                return point.x < intersectX;
            }
            return false;
        }

        public void OnDead(GameObject entity, ulong attackerID)
        {
            Entities.Remove(entity);
            emSystem.ReturnEntity(entity);
            // foreach (var id in loadedPlayerNWIDs)
            //     entity.GetComponent<NetworkObject>().NetworkHide(id);
        }
        public void OnDrop(ItemData itemData, ulong attackerID)
        {
            pdManager.AddItem(itemData, attackerID);
        }

        [ServerRpc(RequireOwnership = false)]
        public void LoadSpawnerServerRpc(ulong nwID)
        {
            if (!IsServer)
                return;

            NearbyPalyersTransformList.Add(NetworkManager.SpawnManager.SpawnedObjects[nwID].transform);
            Debug.Log("Loaded : " + nwID);
            // loadedPlayerNWIDs.Add(nwID);
            // foreach (var entity in Entities)
            // {
            //     entity.GetComponent<NetworkObject>().NetworkShow(nwID);
            // }
        }
        [ServerRpc(RequireOwnership = false)]
        public void UnLoadSpawnerServerRpc(ulong nwID)
        {
            if (!IsServer)
                return;

            NearbyPalyersTransformList.Remove(NetworkManager.SpawnManager.SpawnedObjects[nwID].transform);
            Debug.Log("Unloaded : " + nwID);
            // loadedPlayerNWIDs.Remove(nwID);
            // foreach (var entity in Entities)
            // {
            //     entity.GetComponent<NetworkObject>().NetworkHide(nwID);
            // }
        }
        // [ServerRpc]
        // public void DespawnOrderServerRpc(ulong nwID)
        // {
        //     if (!IsServer)
        //         return;
        //     DespawnForClient(Entities.ToArray(), nwID);
        //     NearbyPalyersTransformList.Remove(NetworkManager.SpawnManager.SpawnedSpawnMarker[nwID].transform);
        //     NearbyPalyersNWIDsList.Remove(nwID);
        // }
        // public void SpawnForClient(GameObject[] entities, ulong targetnwID) // このスポナーをクライアントが読み込んだ場合にServerRPCを通して呼び出される
        // {
        //     if (IsServer)
        //         return;

        //     foreach (GameObject entity in entities)
        //     {
        //         NetworkObject networkObject = entity.GetComponent<NetworkObject>();

        //         if (networkObject != null && IsServer) // サーバー側でのみ実行
        //         {
        //             // 特定のクライアントのみに通知する
        //             networkObject.CheckObjectVisibility = (nwID) => nwID == targetnwID;
        //             networkObject.Spawn();
        //         }
        //     }
        // }
        // HashSet<ulong> allowedClients = new HashSet<ulong>();
        // public void SpawnForClients(GameObject entity) // このスポナーを読み込んでいるプレイヤーに対して新しく生成されたEntityをSpawnさせる
        // {
        //     // 許可するクライアントIDを設定
        //     allowedClients.Clear();
        //     allowedClients.UnionWith(NearbyPalyersNWIDsList);

        //     NetworkObject networkObject = entity.GetComponent<NetworkObject>();

        //     if (networkObject != null && IsServer) // サーバー側でのみ実行
        //     {
        //         // CheckObjectVisibilityデリゲートで指定クライアントのみ許可
        //         networkObject.CheckObjectVisibility = (nwID) => allowedClients.Contains(nwID);
        //         networkObject.Spawn();
        //     }
        // }
        // public void DespawnForClient(GameObject[] entities, ulong targetnwID) // このスポナーがクライアントの読み込み範囲外になった場合にServerRPCを通して呼び出される
        // {
        //     foreach (GameObject entity in entities)
        //     {
        //         NetworkObject networkObject = entity.GetComponent<NetworkObject>();

        //         if (networkObject != null && IsServer) // サーバー側でのみ実行
        //         {
        //             // 特定のクライアントのみに通知する
        //             networkObject.CheckObjectVisibility = (nwID) => nwID == targetnwID;
        //             networkObject.Despawn();
        //         }
        //     }
        // }
        // public void DespawnForClients(GameObject entity) // このスポナーを読み込んだいるプレイヤーに対してEntityをDespawnさせる
        // {
        //     if (IsServer)
        //         return;
        //     Entities.Remove(entity);
        //     // 許可するクライアントIDを設定
        //     allowedClients.Clear();
        //     allowedClients.UnionWith(NearbyPalyersNWIDsList);

        //     NetworkObject networkObject = entity.GetComponent<NetworkObject>();

        //     if (networkObject != null && IsServer) // サーバー側でのみ実行
        //     {
        //         // CheckObjectVisibilityデリゲートで指定クライアントのみ許可
        //         networkObject.CheckObjectVisibility = (nwID) => allowedClients.Contains(nwID);
        //         networkObject.Despawn();
        //     }
        // }
    }
}