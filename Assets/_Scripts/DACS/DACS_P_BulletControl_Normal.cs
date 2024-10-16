using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Jobs;

public class DACS_P_BulletControl_Normal : NetworkBehaviour
{
    [SerializeField] DACS_P_ScriptableObject configsSO; // データベース
    [SerializeField] GameObject bulletPrefab;
    public Transform PlayerTransform; // ClientGeneralManagerから設定
    NetworkSpawnManager nwSpawnManager;
    public NetworkObject nwObject; // ClientまたはHostの時はClientGeneralManagerが設定する(PlayerObjectのnwObjectが設定される)
    ulong ownID; // ClientOnly
    [SerializeField] bool hasServerAuthority = false;
    [SerializeField] bool isHost = false;
    NativeList<ulong> nwIDsList; // ServerOnly
    [SerializeField] List<Transform> transformsList = new(); // 生成された弾を全て登録
    NativeList<BulletControl_Config> bulletConfigsList; // 弾のデータを格納(データの内容は変更可能)
    NativeList<float> bulletsElapsedList; // 各弾の着弾予想時間を格納
    NativeList<bool> bulletsIsActiveList; // 弾の状態を登録
    NativeQueue<int> bulletIDQueue_FirstHalf; // 利用可能な弾のIDを格納(番号が全体の半数以下ならこっちに格納される)
    NativeQueue<int> bulletIDQueue_SecondHalf; // 利用可能な弾のIDを格納(番号が全体の半数より大きいならこっちに格納される)
    NativeQueue<int> DisableBulletIDQueue; // 非アクティブにする予定の弾(競合防止のため、弾の状態の管理は最後にまとめて行う)
    NativeArray<int> DisableBulletIDArray; // DisableQueueを処理する用(メモリアクセスがNativeArrayのほうが高速なため)
    QueryParameters queryParameters; // Raycast用(レイヤーマスクなどの情報が格納される)
    NativeArray<RaycastHit> sphereCastResults; // ヒット情報を格納
    NativeArray<SpherecastCommand> sphereCastCommands;
    NativeList<float> TargetDistanceList; // Rayのヒットしたポイントとの距離(ヒットしなかった場合は0)
    TransformAccessArray transformAccessArray;
    int currentSpawnedBulletsAmount = 0; // 処理対象の増減を感知する
    float AccessArrayUpdateTiming = 0; // 定期的にAccessArrayの更新をする

    public void Setup()
    {
        nwSpawnManager = NetworkManager.Singleton.SpawnManager;

        if (nwObject == null) // server
            OnlyServer();
        else if (nwObject.IsOwnedByServer) // host
        {
            OnlyServer();
            OnlyClient();
            isHost = true;
        }
        else // client
            OnlyClient();

        bulletConfigsList = new(Allocator.Persistent);
        bulletsElapsedList = new(Allocator.Persistent);
        bulletIDQueue_FirstHalf = new(Allocator.Persistent); // 利用可能な弾(非アクティブな弾)の番号を確保しておく
        bulletIDQueue_SecondHalf = new(Allocator.Persistent);
        bulletsIsActiveList = new(Allocator.Persistent);
        DisableBulletIDQueue = new(Allocator.Persistent);
        DisableBulletIDArray = new(50, Allocator.Persistent);
        transformAccessArray = new TransformAccessArray(50);
        sphereCastResults = new(1, Allocator.Persistent);
        sphereCastCommands = new(1, Allocator.Persistent);
        TargetDistanceList = new(Allocator.Persistent);

        for(int i = 0; i < 50; i++)
        {
            var index = bulletsIsActiveList.Length;
            var obj = Instantiate(bulletPrefab);
            transformsList.Add(obj.transform);
            bulletsIsActiveList.Add(false);
            bulletConfigsList.Add(new BulletControl_Config());
            bulletsElapsedList.Add(0);
            bulletIDQueue_SecondHalf.Enqueue(index);
            TargetDistanceList.Add(0);
            if (hasServerAuthority) nwIDsList.Add(0);
        }

        queryParameters = new QueryParameters
        {
            layerMask = -1,
            hitMultipleFaces = false,
            hitTriggers = QueryTriggerInteraction.UseGlobal,
            hitBackfaces = false
        };
    }
    void OnlyServer()
    {
        nwIDsList = new(Allocator.Persistent);
        hasServerAuthority = true;
    }
    void OnlyClient()
    {
        ownID = nwObject.NetworkObjectId;
    }

    void Update()
    {
        float t = Time.deltaTime;
        AccessArrayUpdateTiming += t;

        SetArrays();

        var controlJobHandle = new ControlJob() // 弾を移動させるJobを設定
        {
            t = t,
            bulletsIsActiveArray = bulletsIsActiveList.AsArray(),
            bulletConfigsArray = bulletConfigsList.AsArray(),
            elapsedArray = bulletsElapsedList.AsArray(),
            DisableBulletIDQueue = DisableBulletIDQueue.AsParallelWriter(),
            sphereCastCommands = sphereCastCommands,
            queryParameters = queryParameters,
            TargetDistance = TargetDistanceList.AsArray()
        }.Schedule(transformAccessArray); // 並列で処理(生成した弾の親が共通の親だと並列で処理されないので注意)

        controlJobHandle.Complete(); // 全ての弾の移動完了を待つ
        for (int i = 0; i < sphereCastResults.Length; i++)
        {
            if (!bulletsIsActiveList[i])
                continue;

            sphereCastResults[i] = new();
        }

        var sphereCastCommandJobHandle = SpherecastCommand.ScheduleBatch(sphereCastCommands, sphereCastResults, 4); // Spherecastを行う(その間に着弾予測時間を超えた弾を処理する)

        DisableBulletIDArray = DisableBulletIDQueue.ToArray(Allocator.TempJob);
        foreach (int index in DisableBulletIDArray)
        {
            ReturnBullet(index);
        }
        DisableBulletIDArray.Dispose();
        DisableBulletIDQueue.Clear();

        sphereCastCommandJobHandle.Complete();
        var resultLoadJobHandle = new SphereCastResultsLoad()
        {
            bulletsIsActiveArray = bulletsIsActiveList.AsArray(),
            sphereCastResults = sphereCastResults,
            TargetDistance = TargetDistanceList.AsArray(),
            DisableBulletIDQueue = DisableBulletIDQueue.AsParallelWriter()
        }.Schedule(bulletsIsActiveList.Length, 4);

        resultLoadJobHandle.Complete();

        DisableBulletIDArray = DisableBulletIDQueue.ToArray(Allocator.TempJob);
        foreach (int index in DisableBulletIDArray)
        {
            ReturnBullet(index);
        }
        DisableBulletIDArray.Dispose();
        DisableBulletIDQueue.Clear();
    }
    public void SetBulletLocal(Vector3 shotPos, Vector3 forward,int id, int amount)
    {
        int seed = UnityEngine.Random.Range(0, 100);
        ShotServerRpc(shotPos, forward, seed, id, amount, ownID);
        for (int i = 0; i < amount; i++)
        {
            var index = GetBullet(shotPos);

            var config = configsSO.P_ScriptableObject[id];

            var spreadHz = config.ProjectileSpreadHz;

            System.Random randomX = new(seed + i); // クライアント間でランダムの結果を共通化したいのでシードを共通にする
            System.Random randomY = new(seed + i + 1);
            System.Random randomZ = new(seed + i + 2);

            int rX = randomX.Next(-50, 50);
            int rY = randomY.Next(-50, 50);
            int rZ = randomZ.Next(-50, 50);

            Vector3 dir = // 弾の進行方向を計算
                Quaternion.Euler(
                    spreadHz * rX / 50,
                    spreadHz * rY / 50,
                    spreadHz * rZ / 50) // 拡散率を適応
                * forward;

            var data = new BulletControl_Config()
            {
                Speed = config.ProjectileSpeed, // データベース内の弾速を参照
                DropForce = config.ProjectileDrop, // 弾の落下を参照
                Estimate = config.MaxRange / config.ProjectileSpeed, // 着弾予測時間を設定(この値を超えてもRayがヒットしなかった場合は非アクティブにする)
                Dir = dir.normalized // 進行方向を設定
            };

            bulletConfigsList[index] = data; // データを格納
        }
    }

    public void SetBulletClient(Vector3 shotPos, Vector3 forward, int seed,DACS_P_Configs config, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var index = GetBullet(shotPos);

            var spreadHz = config.ProjectileSpreadHz;

            System.Random randomX = new(seed + i); // クライアント間でランダムの結果を共通化したいのでシードを共通にする
            System.Random randomY = new(seed + i + 1);
            System.Random randomZ = new(seed + i + 2);

            int rX = randomX.Next(-50, 50);
            int rY = randomY.Next(-50, 50);
            int rZ = randomZ.Next(-50, 50);

            Vector3 dir = // 弾の進行方向を計算
                Quaternion.Euler(
                    spreadHz * rX / 50,
                    spreadHz * rY / 50,
                    spreadHz * rZ / 50) // 拡散率を適応
                * forward;

            var data = new BulletControl_Config()
            {
                Speed = config.ProjectileSpeed, // データベース内の弾速を参照
                DropForce = config.ProjectileDrop, // 弾の落下を参照
                Estimate = config.MaxRange / config.ProjectileSpeed, // 着弾予測時間を設定(この値を超えてもRayがヒットしなかった場合は非アクティブにする)
                Dir = dir.normalized // 進行方向を設定
            };

            bulletConfigsList[index] = data; // データを格納
        }
    }
    public void SetBulletServer(Vector3 shotPos, Vector3 forward, int seed,int id, int amount, ulong nwID)
    {
        for (int i = 0; i < amount; i++)
        {
            var index = GetBullet(shotPos, nwID);

            var config = configsSO.P_ScriptableObject[id];

            var spreadHz = config.ProjectileSpreadHz;

            System.Random randomX = new(seed + i); // クライアント間でランダムの結果を共通化したいのでシードを共通にする
            System.Random randomY = new(seed + i + 1);
            System.Random randomZ = new(seed + i + 2);

            int rX = randomX.Next(-50, 50);
            int rY = randomY.Next(-50, 50);
            int rZ = randomZ.Next(-50, 50);

            Vector3 dir = // 弾の進行方向を計算
                Quaternion.Euler(
                    spreadHz * rX / 50,
                    spreadHz * rY / 50,
                    spreadHz * rZ / 50) // 拡散率を適応
                * forward;

            var data = new BulletControl_Config()
            {
                Speed = config.ProjectileSpeed, // データベース内の弾速を参照
                DropForce = config.ProjectileDrop, // 弾の落下を参照
                Estimate = config.MaxRange / config.ProjectileSpeed, // 着弾予測時間を設定(この値を超えてもRayがヒットしなかった場合は非アクティブにする)
                Dir = dir.normalized // 進行方向を設定
            };

            bulletConfigsList[index] = data; // データを格納
            nwIDsList[index] = nwID;
        }
    }

    int GetBullet(Vector3 pos, ulong nwID = 0)
    {
        int index;
        Debug.Log(nwID);
        if (bulletIDQueue_FirstHalf.Count > 0) // 優先度の高い弾があればそちらから使う
        {
            index = bulletIDQueue_FirstHalf.Dequeue();
        }
        else if (bulletIDQueue_SecondHalf.Count > 0)
        {
            index = bulletIDQueue_SecondHalf.Dequeue();
        }
        else // プール内に待機中のオブジェクトが無い場合は生成する
        {
            index = bulletsIsActiveList.Length;
            var obj = Instantiate(bulletPrefab);
            transformsList.Add(obj.transform);
            bulletConfigsList.Add(new BulletControl_Config());
            bulletsElapsedList.Add(0);
            bulletsIsActiveList.Add(true);
            TargetDistanceList.Add(0);
            if (nwID != 0)nwIDsList.Add(nwID);
        }

        // transformsList[index].GetComponent<TrailRenderer>().Clear();
        var t = transformsList[index];

        t.gameObject.SetActive(true);
        // t.GetComponent<NetworkTransform>().Teleport(pos, new quaternion(), Vector3.one);
        t.position = pos;
        t.GetComponent<TrailRenderer>().Clear();
        bulletsIsActiveList[index] = true;
        bulletsElapsedList[index] = 0;

        return index;
    }

    void ReturnBullet(int index)
    {
        transformsList[index].gameObject.SetActive(false);

        if (index >= transformsList.Count / 2)
        {
            bulletIDQueue_SecondHalf.Enqueue(index);
        }
        else
        {
            bulletIDQueue_FirstHalf.Enqueue(index);
        }
    }

    void SetArrays()
    {
        if (transformsList.Count == currentSpawnedBulletsAmount &&
            AccessArrayUpdateTiming < 10 &&
            bulletIDQueue_FirstHalf.Count != 0) // 弾が新しく生成されていなければ処理をスキップ
            return;

        currentSpawnedBulletsAmount = transformsList.Count;
        AccessArrayUpdateTiming = 0;

        int indexCount = transformsList.Count;

        if (bulletIDQueue_SecondHalf.Count != indexCount / 2)
        {
            transformAccessArray.Dispose();
            transformAccessArray = new(transformsList.ToArray()); // 優先度の低い弾も使用中なら全体を操作する
            sphereCastResults.Dispose();
            sphereCastCommands.Dispose();
            sphereCastResults = new(indexCount, Allocator.Persistent);
            sphereCastCommands = new(indexCount, Allocator.Persistent);
        }
        else
        {
            transformAccessArray.Dispose();
            transformAccessArray = new(transformsList.GetRange(0, indexCount / 2 - 1).ToArray()); // 処理対象を絞る
            sphereCastResults.Dispose();
            sphereCastCommands.Dispose();
            sphereCastResults = new(indexCount / 2, Allocator.Persistent);
            sphereCastCommands = new(indexCount / 2, Allocator.Persistent);
        }
    }

    public override void OnDestroy()
    {
        nwIDsList.Dispose();
        bulletConfigsList.Dispose();
        bulletsElapsedList.Dispose();
        bulletIDQueue_FirstHalf.Dispose();
        bulletIDQueue_SecondHalf.Dispose();
        DisableBulletIDQueue.Dispose();
        if (DisableBulletIDArray.IsCreated) DisableBulletIDArray.Dispose();
        bulletsIsActiveList.Dispose();
        transformAccessArray.Dispose();
        sphereCastResults.Dispose();
        sphereCastCommands.Dispose();
        TargetDistanceList.Dispose();
    }

    [BurstCompile]
    struct ControlJob : IJobParallelForTransform
    {
        [ReadOnly] public float t;
        public NativeArray<bool> bulletsIsActiveArray;
        [ReadOnly] public NativeArray<BulletControl_Config> bulletConfigsArray;
        public NativeArray<float> elapsedArray;
        public NativeQueue<int>.ParallelWriter DisableBulletIDQueue;
        public NativeArray<float> TargetDistance;
        public NativeArray<SpherecastCommand> sphereCastCommands;
        [ReadOnly] public QueryParameters queryParameters;

        public void Execute(int index, TransformAccess xform)
        {
            if (!bulletsIsActiveArray[index]) // 弾が非アクティブなら処理をスキップ
            {
                sphereCastCommands[index] = new();
                return;
            }

            var config = bulletConfigsArray[index];

            var elapsed = elapsedArray[index] += t; // 経過時間の更新

            if (elapsed > config.Estimate) // 着弾予測時間を超過していたら弾を非アクティブにする(ここでは予約のみ)
            {
                DisableBulletIDQueue.Enqueue(index); // キューに追加
                bulletsIsActiveArray[index] = false;
                TargetDistance[index] = 0;
                return;
            }

            Vector3 v;
            if (TargetDistance[index] > 0) // Rayがヒットした場合はヒットした地点の直前まで移動させる
            {
                v = 0.9f * TargetDistance[index] * config.Dir;
            }
            else
            {
                v = config.Dir * config.Speed;
                v.y -= config.DropForce * elapsed * elapsed / 2;
                v *= t;
            }

            xform.position += v; // 位置の更新

            sphereCastCommands[index] = new SpherecastCommand(
                    xform.position,
                    0.05f,
                    config.Dir,
                    queryParameters,
                    config.Speed * t * 1.5f);

            TargetDistance[index] = 0;
        }
    }

    struct SphereCastResultsLoad : IJobParallelFor
    {
        public NativeArray<bool> bulletsIsActiveArray;
        public NativeArray<RaycastHit> sphereCastResults;
        public NativeArray<float> TargetDistance;
        public NativeQueue<int>.ParallelWriter DisableBulletIDQueue;
        public void Execute(int index)
        {
            if (!bulletsIsActiveArray[index])
                return;

            var hit = sphereCastResults[index];

            var distance = hit.distance;

            if (distance > 0.15)
            {
                TargetDistance[index] = distance;
            }
            else if (distance == 0)
            {
                TargetDistance[index] = 0;
            }
            else
            {
                TargetDistance[index] = 0;
                DisableBulletIDQueue.Enqueue(index);
                bulletsIsActiveArray[index] = false;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShotServerRpc(Vector3 position, Vector3 forward, int seed, int id, int amount, ulong nwID)
    {
        Debug.Log("Call SErverRPC");

        SetBulletServer(position, forward, seed, id, amount, nwID);
        SetBulletClientRpc(position, forward, seed, id, amount, nwID);
    }

    [ClientRpc]
    public void SetBulletClientRpc(Vector3 position, Vector3 forward, int seed,int id, int amount, ulong nwID)
    {
        Debug.Log("Call SErverRPC IsHost : " + isHost + " nwID : :" + (nwID == ownID));
        if (isHost || nwID == ownID)
            return;

        var config = configsSO.P_ScriptableObject[id];
        float maxRange = config.MaxRange;
        float maxDistance = maxRange * 0.35f;
        var playerPos = PlayerTransform.position;

        // プレイヤーと弾道の距離が離れすぎていたらシュミレーションを中止
        if (Vector3.Distance(playerPos, forward * maxRange) < maxDistance || // 終点
            Vector3.Distance(playerPos, 0.5f * maxRange * forward) < maxDistance || // 中間点
            Vector3.Distance(playerPos, position) < maxDistance) // 始点
                {Debug.Log("shoot");
                    SetBulletClient(position, forward, seed, config, amount);}
    }

    void LinearCal(Vector3 a, Vector3 b)
    {
        float num = a.x - b.x;
        float num2 = a.y - b.y;
        float num3 = a.z - b.z;
    }
}
