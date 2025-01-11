using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using DACS.Projectile;
using DACS.Extensions;

namespace DACS.Entities
{

    public class EnemyComponent : Entity // Entity直下
    {
        [HideInInspector] public EntitySpawner Spawner; // SpawnerがこのEntityを生成した時に設定
        [HideInInspector] public BulletControl_Basic bcNormal;
        [SerializeField] Rigidbody rb;
        public int entityID_FirstIndex;
        public int entityID_SecondIndex;
        public bool IsActive
        {
            get => _IsActive;
            set
            {
                _IsActive = value;
                if (!value)
                    CTSource.Cancel(); // このEntityが無効になった場合はDoTダメージなどの処理を停止させる
            }
        }
        bool _IsActive = false;
        public CancellationTokenSource CTSource{ get => _ctSource;}
        CancellationTokenSource _ctSource;
        CancellationToken ctToken;
        [HideInInspector] public int lvl; // spawnerが設定
        public override float MaxHP{get; set;}
        public override float HP
        {
            get
            {
                return _hp;
            }
            set
            {
                _hp = value;
                if (_hp > MaxHP)
                    _hp = MaxHP;
                if (_hp <= 0)
                    _hp = 0;
            }
        }
        float _hp;
        public override float Def{get; set;}
        public override float Abno{get; set;}
        public override float Atk{get; set;}
        public override float DodgeChance{get; set;}
        int ReviveNumber = 0;
        [SerializeField]int AttackCount = 0;

        [SerializeField] EntityState entityState;
        [SerializeField] EntityConfigs entityConfig;
        Transform Target;
        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] bool isAttacking = false;
        [SerializeField] int AttackPatternsCount;
        [SerializeField] bool UseRandomAttack = true;
        [Tooltip("デフォルト値(0)の場合、AttackPatterns内のすべてのパターンがランダム攻撃の抽選対象となる。<br />例えば、このパラメータの値を3にした場合はAttackPatterns内の0~3のパターンが対象となる。それ以降のパターンは特殊攻撃パターンとして扱われ、UseSpecialAttackCountの値の回数だけ攻撃したら、特殊パターンのうちからランダムで攻撃される")]
        [SerializeField] int RandomAttackPatternRange = 0;
        [SerializeField] int UseSpecialAttackCount = 0;
        [SerializeField] CustomAction CustomActionComponent;
        List<EntityDrop> dropTable;
        float TotalDropChanceAmount = 0;
        ulong AttackerClientID; // 最後に攻撃したプレイヤーのNetworkID

        public void OnActivate(EntityConfigs entityConfig)
        {
            _ctSource = new();
            ctToken = CTSource.Token;
            IsActive = true;

            this.entityConfig = entityConfig;
            BaseEntityStatus entityData = this.entityConfig.EntityStatus;
            MaxHP = entityData.HPperLevel * lvl;
            HP = MaxHP;
            Def = entityData.DefperLevel * lvl;
            Atk = entityData.AtkperLevel * lvl;
            DodgeChance = entityData.DodgeChance;
            ReviveNumber = entityData.ReviveNumber;
            dropTable = this.entityConfig.DropTable;
            TotalDropChanceAmount = 0;
            foreach (var dropdata in dropTable)
            {
                TotalDropChanceAmount += dropdata.DropChance; // ドロップ確率の総量を計算して、RandomのRangeを決める
            }

            navMeshAgent.speed = entityConfig.Speed;
            isAttacking = false;
            navMeshAgent.isStopped = false;
            rb.isKinematic = true;
        }

        void Update()
        {
            if (!IsActive)
                return;

            switch (entityState)
            {
                case EntityState.Search:
                    SearchPlayer();
                    break;

                case EntityState.Chase:
                    Chasing();
                    break;

                case EntityState.Attack:
                    if (!isAttacking)
                        StartAttack();
                    break;
            }
            // if (entityState == EntityState.Attack || entityState == EntityState.Chase)
            // {
            //     LockOn();
            // }
        }

        public override void OnDamage(DamageData damage, ulong clientID = 0)
        {
            if (DodgeChance - damage.HitChance > UnityEngine.Random.Range(0.0f, 100.0f)) // 回避処理
            {
                OnDodge();
                return;
            }

            if (clientID != 0)
                AttackerClientID = clientID;

            float DmgReceived = damage.Dmg - (Def - damage.Penetration > 0 ? Def - damage.Penetration : 0) * damage.DefMagnification;
            HP -= DmgReceived > 0 ? DmgReceived : 0;

            if (HP == 0)
            {
                if (ReviveNumber > 0) // 蘇生処理
                {
                    ReviveNumber --;
                    HP = MaxHP * 0.5f; // 最大HPの半分の体力で復活
                }
                else
                    OnDead();
            }
        }
        async void StartAttack()
        {
            isAttacking = true; // 攻撃開始
            navMeshAgent.enabled = false;
            rb.isKinematic = false;

            if (UseRandomAttack)
            {
                if (UseSpecialAttackCount <= AttackCount)
                {
                    await CustomActionComponent.Action(UnityEngine.Random.Range(RandomAttackPatternRange + 1, AttackPatternsCount - 1));
                    AttackCount = 0;
                }
                else
                {
                    await CustomActionComponent.Action(UnityEngine.Random.Range(0, RandomAttackPatternRange));
                    AttackCount ++;
                }
            }
            else
            {
                await CustomActionComponent.Action(AttackCount);
                AttackCount = AttackPatternsCount == AttackCount ? 0 : AttackCount ++;
            }

            isAttacking = false; // 攻撃終了
            navMeshAgent.enabled = true;
            rb.isKinematic = true;
            entityState = EntityState.Chase;
        }

        public async UniTask Attack(AttackPattern attackPattern)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(attackPattern.AttackDuration), cancellationToken : destroyCancellationToken);
        }
        public override void OnDead()
        {
            float random = UnityEngine.Random.Range(0, TotalDropChanceAmount);
            float cumulative = 0;
            ItemData dropItem;
            for (int i = 0; i < dropTable.Count; i ++)
            {
                cumulative += dropTable[i].DropChance;
                if (random < cumulative)
                {
                    dropItem = dropTable[i].itemData;
                    if (dropItem.FirstIndex != -1) // -1の場合はドロップ無し
                        Spawner.OnDrop(dropItem, AttackerClientID);
                    break;
                }
            }
            Spawner.OnDead(gameObject, AttackerClientID);
            IsActive = false;
        }

        public override async void DoTDamage(EffectConfig effectConfig) // 継続ダメージ(呼び出しは外部から)
        {
            float estimate = 0;
            float rate = 0;
            float damage = 0;
            float defmag = 0;
            float chance = 0;

            switch (effectConfig.Effect)
            {
                case Effects.Ignite:
                    rate = 1;
                    damage = MaxHP / 100 * 2;
                    defmag = 1;
                    break;

                case Effects.Unstable:
                    chance = Abno;
                    break;

                case Effects.Poison:
                    rate = 1;
                    damage = MaxHP / 100 * 1;
                    defmag = 0;
                    break;

                case Effects.Slowness:
                    rate = 1;
                    chance = Abno;
                    break;

                case Effects.Weakness:
                    rate = 1;
                    chance = Abno;
                    break;
            }
            if (chance > UnityEngine.Random.Range(0, 100))
                return;

            while (estimate <= effectConfig.Duration)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(rate), cancellationToken : ctToken);
                estimate += rate;
                OnDamage(new DamageData()
                {
                    Dmg = damage,
                    DefMagnification = defmag,
                    Penetration = 0,
                    HitChance = 0
                });
            }
        }

        void SearchPlayer()
        {
            if (Spawner.NearbyPalyersTransformList.Count == 0)
                return;

            Target = null;

            float tempDistance = entityConfig.SearceRange;
            foreach (var player in Spawner.NearbyPalyersTransformList)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance < tempDistance && distance < entityConfig.SearceRange) // 一番近いプレイヤーをターゲットとする
                {
                    tempDistance = distance;
                    Target = player;
                }
            }

            if (Target != null)
                entityState = EntityState.Chase;
        }

        void Chasing()
        {
            var distance = Vector3.Distance(transform.position, Target.position);
            if (distance < entityConfig.AttackRange)
                entityState = EntityState.Attack;
            else
                navMeshAgent.SetDestination(Target.transform.position);
        }

        // private void OnTriggerEnter(Collider other) // 索敵範囲内のプレイヤーを攻撃対象として認識する
        // {
        //     if (other.gameObject.CompareTag("Player"))
        //     {
        //         PlayersInSearchRange.Add(other.transform);
        //     }
        // }

        // private void OnTriggerExit(Collider other) // 索敵範囲内から出たプレイヤーを攻撃対象から外す
        // {
        //     if (other.gameObject.CompareTag("Player"))
        //         PlayersInSearchRange.Remove(other.transform);
        // }

        private void StartChace()
        {
            navMeshAgent.SetDestination(Target.transform.position);
            navMeshAgent.isStopped = false;
        }
        private void StopChace()
        {
            navMeshAgent.isStopped = true;
        }
        private void LockOn()
        {
            // ターゲットの方向を向くための方向ベクトルを計算
            Vector3 targetDirection = Target.transform.position - transform.position;

            // ターゲットの方向に向かって徐々に回転
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, entityConfig.Speed * Time.deltaTime);
        }

    }
}

namespace DACS.Entities
{
    public class Entity : NetworkBehaviour
    {
        public virtual float MaxHP{get; set;}
        public virtual float HP{get; set;}
        public virtual float Def{get; set;}
        public virtual float Abno{get; set;}
        public virtual float DodgeChance{get; set;}
        public virtual float Atk{get; set;}
        public virtual float MaxMP{get; set;}
        public virtual float MP{get; set;}
        public virtual float CritChance{get; set;}
        public virtual float CritDamage{get; set;}
        public virtual float Penetration{get; set;}
        public virtual float HitChance{get; set;}
        public virtual void OnDamage(DamageData damage, ulong clientID = 0){}
        public virtual void OnDead(){}
        public virtual void OnDodge(){}
        public virtual async void DoTDamage(EffectConfig effectConfig){}
    }
}