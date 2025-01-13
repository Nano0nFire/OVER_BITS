using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Jobs;
using DACS.Entities;
using DACS.Projectile;
public class HomingTEst : MonoBehaviour
{
        [SerializeField] ProjectileSO configsSO; // データベース
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] Transform targetOBJECT;
        [HideInInspector] public Transform PlayerTransform; // ClientGeneralManagerから設定
        [HideInInspector] public ulong ownID = 0; // ClientOnly
        [HideInInspector] public ulong nwoID = 0; // ClientOnly
        List<Transform> transformsList = new(); // 生成された弾を全て登録
        NativeList<Bullet_Homing_Config> bulletConfigsList; // 弾のデータを格納(データの内容は変更可能)
        NativeList<float> bulletsElapsedList; // 各弾の着弾予想時間を格納
        NativeList<bool> bulletsIsActiveList; // 弾の状態を登録
        NativeQueue<int> bulletIndexQueue_FirstHalf; // 利用可能な弾のIndexを格納(番号が全体の半数以下ならこっちに格納される)
        NativeQueue<int> bulletIndexQueue_SecondHalf; // 利用可能な弾のIndexを格納(番号が全体の半数より大きいならこっちに格納される)
        NativeQueue<int> DeactivateBulletIndexQueue; // 非アクティブにする予定の弾(競合防止のため、弾の状態の管理は最後にまとめて行う)
        NativeArray<int> DeactivateBulletIndexArray; // DisableQueueを処理する用(メモリアクセスがNativeArrayのほうが高速なため)
        QueryParameters queryParameters; // Raycast用(レイヤーマスクなどの情報が格納される)
        NativeArray<RaycastHit> sphereCastResults; // ヒット情報を格納
        NativeArray<SpherecastCommand> sphereCastCommands;
        NativeList<float> TargetDistanceList; // Rayのヒットしたポイントとの距離(ヒットしなかった場合は0)
        [SerializeField]List<Transform> Targets = new();
        NativeList<Vector3> TargetPositions;
        TransformAccessArray transformAccessArray;
        int currentSpawnedBulletsAmount = 0; // 処理対象の増減を感知する
        float AccessArrayUpdateTiming = 0; // 定期的にAccessArrayの更新をする

        public void Start()
        {
            bulletConfigsList = new(Allocator.Persistent);
            bulletsElapsedList = new(Allocator.Persistent);
            bulletIndexQueue_FirstHalf = new(Allocator.Persistent); // 利用可能な弾(非アクティブな弾)の番号を確保しておく
            bulletIndexQueue_SecondHalf = new(Allocator.Persistent);
            bulletsIsActiveList = new(Allocator.Persistent);
            DeactivateBulletIndexQueue = new(Allocator.Persistent);
            DeactivateBulletIndexArray = new(50, Allocator.Persistent);
            transformAccessArray = new TransformAccessArray(50);
            sphereCastResults = new(1, Allocator.Persistent);
            sphereCastCommands = new(1, Allocator.Persistent);
            TargetDistanceList = new(Allocator.Persistent);
            TargetPositions = new(Allocator.Persistent);

            for(int i = 0; i < 50; i++)
            {
                var index = bulletsIsActiveList.Length;
                var obj = Instantiate(bulletPrefab);
                transformsList.Add(obj.transform);
                bulletsIsActiveList.Add(false);
                bulletConfigsList.Add(new());
                bulletsElapsedList.Add(0);
                bulletIndexQueue_SecondHalf.Enqueue(index);
                TargetDistanceList.Add(-1);
                Targets.Add(transform);
                TargetPositions.Add(Vector3.zero);
            }

            queryParameters = new QueryParameters
            {
                layerMask = -1,
                hitMultipleFaces = false,
                hitTriggers = QueryTriggerInteraction.UseGlobal,
                hitBackfaces = false
            };
        }

        void FixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                SetBulletServer(transform.position, transform.forward, 2, 1, targetOBJECT);
            }
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
                DeactivateBulletIndexQueue = DeactivateBulletIndexQueue.AsParallelWriter(),
                sphereCastCommands = sphereCastCommands,
                queryParameters = queryParameters,
                TargetDistance = TargetDistanceList.AsArray(),
                targetPositions = TargetPositions.AsArray(),
            }.Schedule(transformAccessArray); // 並列で処理(生成した弾の親が共通の親だと並列で処理されないので注意)

            controlJobHandle.Complete(); // 全ての弾の移動完了を待つ
            for (int i = 0; i < sphereCastResults.Length; i++)
            {
                if (!bulletsIsActiveList[i])
                    continue;

                sphereCastResults[i] = new();
            }

            var sphereCastCommandJobHandle = SpherecastCommand.ScheduleBatch(sphereCastCommands, sphereCastResults, 4); // Spherecastを行う(その間に着弾予測時間を超えた弾を処理する)

            DeactivateBulletIndexArray = DeactivateBulletIndexQueue.ToArray(Allocator.TempJob);
            foreach (int index in DeactivateBulletIndexArray)
            {
                ReturnBullet(index);
            }
            DeactivateBulletIndexArray.Dispose();
            DeactivateBulletIndexQueue.Clear();

            for (int i = 0; i < transformAccessArray.length; i++)
            {
                if (!bulletsIsActiveList[i])
                    continue;

                TargetPositions[i] = Targets[i].position;
            }

            sphereCastCommandJobHandle.Complete();
            var resultLoadJobHandle = new SphereCastResultsLoad()
            {
                bulletsIsActiveArray = bulletsIsActiveList.AsArray(),
                sphereCastResults = sphereCastResults,
                TargetDistance = TargetDistanceList.AsArray(),
                DeactivateBulletIndexQueue = DeactivateBulletIndexQueue.AsParallelWriter()
            }.Schedule(bulletsIsActiveList.Length, 4);

            resultLoadJobHandle.Complete();

            DeactivateBulletIndexArray = DeactivateBulletIndexQueue.ToArray(Allocator.TempJob);
            foreach (int index in DeactivateBulletIndexArray)
            {
                ReturnBullet(index); // 弾をプールに戻す
            }
            DeactivateBulletIndexArray.Dispose();
            DeactivateBulletIndexQueue.Clear();
        }

        public void SetBulletServer(Vector3 shotPos, Vector3 forward, int id, int amount, Transform target)
        {
            int seed = Random.Range(-10000, 10000);

            var config = configsSO.P_ScriptableObject[id];
            var spreadHz = config.ProjectileSpreadHz;
            var spreadV = config.ProjectileSpreadV;
            for (int i = 0; i < amount; i++)
            {
                var index = GetBullet();

                System.Random random = new(seed + i); // クライアント間でランダムの結果を共通化したいのでシードを共通にする
                float rX = random.Next(-10000, 10000);
                float rY = random.Next(-10000, 10000);

                // spreadHzとspreadVの値を適切に反映
                float horizontalSpread = spreadHz * rX / 10000f;
                float verticalSpread = spreadV * rY / 10000f;

                Vector3 dir = // 弾の進行方向を計算
                    Quaternion.Euler(
                        horizontalSpread,
                        verticalSpread, // 拡散率を適応
                        0)
                    * forward;

                Debug.Log(dir);

                InitBullet(index, shotPos, dir, target, config);
            }
        }

        int GetBullet()
        {
            int index;
            if (bulletIndexQueue_FirstHalf.Count > 0) // 優先度の高い弾があればそちらから使う
            {
                index = bulletIndexQueue_FirstHalf.Dequeue();
            }
            else if (bulletIndexQueue_SecondHalf.Count > 0)
            {
                index = bulletIndexQueue_SecondHalf.Dequeue();
            }
            else // プール内に待機中のオブジェクトが無い場合は生成する
            {
                index = bulletsIsActiveList.Length;
                var obj = Instantiate(bulletPrefab);
                transformsList.Add(obj.transform);
                bulletConfigsList.Add(new());
                bulletsElapsedList.Add(0);
                bulletsIsActiveList.Add(true);
                TargetDistanceList.Add(-1); // 値が-1の時は移動処理が省略される
                Targets.Add(transform);
                TargetPositions.Add(Vector3.zero);
            }

            return index;
        }

        void ReturnBullet(int index)
        {
            transformsList[index].gameObject.SetActive(false);

            if (index >= transformsList.Count / 2)
            {
                bulletIndexQueue_SecondHalf.Enqueue(index);
            }
            else
            {
                bulletIndexQueue_FirstHalf.Enqueue(index);
            }
        }

        void InitBullet(int index, Vector3 pos, Vector3 dir, Transform target, DACS_P_Configs config)
        {
            var t = transformsList[index];

            t.gameObject.SetActive(true);
            t.SetPositionAndRotation(pos, Quaternion.Euler(dir));
            var trail = t.GetComponent<TrailRenderer>();
            trail.Clear();
            trail.colorGradient = config.TrailColor;
            trail.widthCurve = config.TrailCurve;
            trail.time = config.TrailTime;
            bulletsIsActiveList[index] = true;
            bulletsElapsedList[index] = 0;
            Targets[index] = target;
            TargetPositions[index] = Vector3.zero;

            bulletConfigsList[index] = new Bullet_Homing_Config()
            {
                Speed = config.ProjectileSpeed, // データベース内の弾速を参照
                springConstant = config.springConstant, // 弾の落下を参照
                damping = config.damping,
                Estimate = Vector3.Distance(pos, target.position) / config.ProjectileSpeed * 1.5f, // 着弾予測時間を設定(この値を超えてもRayがヒットしなかった場合は非アクティブにする) Changed
            };
        }

        void SetArrays()
        {
            if (transformsList.Count == currentSpawnedBulletsAmount &&
                AccessArrayUpdateTiming < 10 &&
                bulletIndexQueue_FirstHalf.Count != 0) // 弾が新しく生成されていなければ処理をスキップ
                return;

            currentSpawnedBulletsAmount = transformsList.Count;
            AccessArrayUpdateTiming = 0;

            int indexCount = transformsList.Count;

            if (bulletIndexQueue_SecondHalf.Count != indexCount / 2)
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

        void OnDestroy()
        {
            bulletConfigsList.Dispose();
            bulletsElapsedList.Dispose();
            bulletIndexQueue_FirstHalf.Dispose();
            bulletIndexQueue_SecondHalf.Dispose();
            DeactivateBulletIndexQueue.Dispose();
            if (DeactivateBulletIndexArray.IsCreated) DeactivateBulletIndexArray.Dispose();
            bulletsIsActiveList.Dispose();
            transformAccessArray.Dispose();
            sphereCastResults.Dispose();
            sphereCastCommands.Dispose();
            TargetDistanceList.Dispose();
            TargetPositions.Dispose();
        }

        [BurstCompile]
        struct ControlJob : IJobParallelForTransform
        {
            [ReadOnly] public float t;
            public NativeArray<bool> bulletsIsActiveArray;
            [ReadOnly] public NativeArray<Bullet_Homing_Config> bulletConfigsArray;
            public NativeArray<float> elapsedArray;
            public NativeQueue<int>.ParallelWriter DeactivateBulletIndexQueue;
            public NativeArray<float> TargetDistance;
            public NativeArray<SpherecastCommand> sphereCastCommands;
            [ReadOnly] public NativeArray<Vector3> targetPositions; // 標的の座標
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
                    DeactivateBulletIndexQueue.Enqueue(index); // キューに追加
                    bulletsIsActiveArray[index] = false;
                    TargetDistance[index] = 0;
                    return;
                }

                Vector3 v = Vector3.zero;
                float dis = TargetDistance[index];
                var dir = xform.rotation * Vector3.forward;
                if (dis > 0) // Rayがヒットした場合はヒットした地点の直前まで移動させる
                {
                    v = 0.95f * dis * dir;
                }
                else if (dis == 0)
                {
                    if (Vector3.Distance(targetPositions[index], xform.position) > 3)
                    {

                        // 目標の方向を計算
                        Vector3 targetDirection = (targetPositions[index] - xform.position).normalized;

                        // 回転を更新
                        xform.rotation = Quaternion.Slerp(xform.rotation, Quaternion.LookRotation(targetDirection), t * config.springConstant);

                        // バネトルクの影響を受けた方向に移動
                        dir = xform.rotation * Vector3.forward;
                        v = t * config.Speed * dir;
                    }
                    else
                    {
                        v = t * config.Speed * dir;
                    }
                }

                xform.position += v; // 位置の更新

                sphereCastCommands[index] = new SpherecastCommand(
                        xform.position,
                        0.05f,
                        dir,
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
            public NativeQueue<int>.ParallelWriter DeactivateBulletIndexQueue;
            public void Execute(int index)
            {
                if (!bulletsIsActiveArray[index])
                    return;

                var hit = sphereCastResults[index];

                var distance = hit.distance;

                if (distance > 0.5) // 次のフレームで着弾する可能性があるものをピックアップして、対象との距離の分だけ次のフレームで動かす
                {
                    TargetDistance[index] = distance;
                }
                else if (distance == 0) // ヒット無しの場合は無視
                {
                    TargetDistance[index] = 0;
                }
                else // 対象との距離が十分に縮んだらヒット判定を出す
                {
                    // Debug.Log("HIT");
                    TargetDistance[index] = -1; // 最初のフレームではraycastを優先させたいので移動処理を省略するために-1をセットしておく
                    DeactivateBulletIndexQueue.Enqueue(index);
                    bulletsIsActiveArray[index] = false;
                }
            }
        }
    }
