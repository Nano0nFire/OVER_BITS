using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class DACS_Entities_EnemySpawner : NetworkBehaviour // スポナーにアタッチ
{
    [HideInInspector] public DACS_EntityManagementSystem emSystem; // EntityManagerが設定
    [HideInInspector] public DACS_P_BulletControl_Normal bcNormal; // EntityManagerが設定
    [HideInInspector] public DACS_Entities_ScriptableObject EntitiesSO; // EntityManagerが設定
    [HideInInspector] public PlayerDataManager pdManager; // EntityManagerが設定
    [SerializeField] int SpawnTargetFirstIndex, SpawnTargetSecondIndex;
    [SerializeField] List<GameObject> Entities = new(); // アクティブなEntityをキューで把握
    [SerializeField] int MaxSpawnAmount = 5; // 最大同時スポーン数
    [SerializeField] int SpawnSpan = 10;
    [SerializeField] int EntityLevel = 0; // スポーンするEntityのLevel(0の場合はデフォルト値が使われる)
    public List<Transform> NearbyPalyersTransformList = new(); // スポナーを読み込んでいるプレイヤーのTransform
    List<ulong> loadedPlayerNWIDs = new();
    EntityConfigs entityData;
    [SerializeField] bool isSpawning = false;
    [SerializeField] Vector3 spawnPoint;
    [SerializeField] ItemData itemDataa;

    async void Update()
    {
        if (isSpawning)
            return;

        isSpawning = true;

        if (!IsServer) // Serverのみ呼び出し可能
            return;

        entityData ??= EntitiesSO.GetEntity(SpawnTargetFirstIndex, SpawnTargetSecondIndex);

        if (Entities.Count >= MaxSpawnAmount)
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
        var nwObject = entity.GetComponent<NetworkObject>();
        Debug.Log($"IsServer: {IsServer}, NetworkObject: {nwObject}");
        foreach (var id in loadedPlayerNWIDs)
            nwObject.NetworkShow(id);
        entity.transform.position = spawnPoint;
        var data = entity.GetComponent<DACS_Entities_Enemy_Component>();
        Entities.Add(entity);
        data.Spawner = this;
        data.bcNormal = bcNormal;
        data.lvl = EntityLevel != 0 ? EntityLevel : entityData.EntityStatus.BaseLevel;
        data.OnActivate(entityData);
    }
    public void OnDead(GameObject entity, ulong attackerID)
    {
        Entities.Remove(entity);
        emSystem.ReturnEntity(entity);
        foreach (var id in loadedPlayerNWIDs)
            entity.GetComponent<NetworkObject>().NetworkHide(id);
    }
    public void OnDrop(ItemData itemData, ulong attackerID)
    {
        pdManager.AddItem(itemData, attackerID);
        itemDataa = itemData;
    }

    [ServerRpc]
    public void LoadSpawnerServerRpc(ulong nwID)
    {
        if (!IsServer)
            return;

        NearbyPalyersTransformList.Add(NetworkManager.SpawnManager.SpawnedObjects[nwID].transform);
        loadedPlayerNWIDs.Add(nwID);
        foreach (var entity in Entities)
        {
            entity.GetComponent<NetworkObject>().NetworkShow(nwID);
        }
    }
    [ServerRpc]
    public void UnLoadSpawnerServerRpc(ulong nwID)
    {
        if (!IsServer)
            return;

        NearbyPalyersTransformList.Remove(NetworkManager.SpawnManager.SpawnedObjects[nwID].transform);
        loadedPlayerNWIDs.Remove(nwID);
        foreach (var entity in Entities)
        {
            entity.GetComponent<NetworkObject>().NetworkHide(nwID);
        }
    }
    // [ServerRpc]
    // public void DespawnOrderServerRpc(ulong nwID)
    // {
    //     if (!IsServer)
    //         return;
    //     DespawnForClient(Entities.ToArray(), nwID);
    //     NearbyPalyersTransformList.Remove(NetworkManager.SpawnManager.SpawnedObjects[nwID].transform);
    //     NearbyPalyersNWIDsList.Remove(nwID);
    // }
    // public void SpawnForClient(GameObject[] entities, ulong targetClientId) // このスポナーをクライアントが読み込んだ場合にServerRPCを通して呼び出される
    // {
    //     if (IsServer)
    //         return;

    //     foreach (GameObject entity in entities)
    //     {
    //         NetworkObject networkObject = entity.GetComponent<NetworkObject>();

    //         if (networkObject != null && IsServer) // サーバー側でのみ実行
    //         {
    //             // 特定のクライアントのみに通知する
    //             networkObject.CheckObjectVisibility = (clientId) => clientId == targetClientId;
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
    //         networkObject.CheckObjectVisibility = (clientId) => allowedClients.Contains(clientId);
    //         networkObject.Spawn();
    //     }
    // }
    // public void DespawnForClient(GameObject[] entities, ulong targetClientId) // このスポナーがクライアントの読み込み範囲外になった場合にServerRPCを通して呼び出される
    // {
    //     foreach (GameObject entity in entities)
    //     {
    //         NetworkObject networkObject = entity.GetComponent<NetworkObject>();

    //         if (networkObject != null && IsServer) // サーバー側でのみ実行
    //         {
    //             // 特定のクライアントのみに通知する
    //             networkObject.CheckObjectVisibility = (clientId) => clientId == targetClientId;
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
    //         networkObject.CheckObjectVisibility = (clientId) => allowedClients.Contains(clientId);
    //         networkObject.Despawn();
    //     }
    // }
}