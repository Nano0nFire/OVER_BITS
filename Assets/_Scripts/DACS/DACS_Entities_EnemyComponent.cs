using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.AI;

public class DACS_Entities_Enemy_Component : DACS_Entities // Entity直下
{
    [HideInInspector] public DACS_Entities_EnemySpawner Spawner; // SpawnerがこのEntityを生成した時に設定
    [HideInInspector] public DACS_P_BulletControl_Normal bcNormal;
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
    float MaxHP = 100;
    public float HP
    {
        get
        {
            return _hp;
        }
        private set
        {
            _hp = value;
            if (_hp > MaxHP)
                _hp = MaxHP;
            if (_hp <= 0)
                _hp = 0;
        }
    }
    public float _hp;
    float Def;
    float Abno;
    float Atk;
    float DodgeChance;
    int ReviveNumber = 0;
    int AttackCount = 0;
    [SerializeField] CapsuleCollider col;
    EntityState entityState;
    [SerializeField] Transform ProjectilePoint;
    [SerializeField] EntityConfigs entityConfig;
    Transform Target;
    [SerializeField] NavMeshAgent navMeshAgent;
    [SerializeField] bool isAttacking = false;
    [SerializeField] float MaxDistance, MinDistance;
    List<AttackPattern> AttackPatterns = new();
    List<EntityDrop> dropTable;
    float TotalDropChanceAmount = 0;
    ulong AttackerNWID; // 最後に攻撃したプレイヤーのNetworkID

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
        AttackPatterns = entityConfig.AttackPatterns;
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

    public override void OnDamage(DamageData damage, ulong nwID = 0)
    {
        if (DodgeChance - damage.HitChance > UnityEngine.Random.Range(0.0f, 100.0f)) // 回避処理
        {
            OnDodge();
            return;
        }

        if (nwID != 0)
            AttackerNWID = nwID;

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

        AttackPattern pattern;
        if (entityConfig.UseRandomAttack)
        {
            if (entityConfig.UseSpecialAttackCount <= AttackCount)
            {
                pattern = AttackPatterns[UnityEngine.Random.Range(entityConfig.RandomAttackPatternRange + 1, AttackPatterns.Count - 1)];
                rb.AddForce(transform.forward * pattern.AddForce_Before.x + transform.up * pattern.AddForce_Before.y + transform.right * pattern.AddForce_Before.z);
                await Attack(pattern);
                AttackCount = 0;
            }
            else
            {
                pattern = AttackPatterns[UnityEngine.Random.Range(0, entityConfig.RandomAttackPatternRange)];
                rb.AddForce(transform.forward * pattern.AddForce_Before.x + transform.up * pattern.AddForce_Before.y + transform.right * pattern.AddForce_Before.z);
                await Attack(pattern);
                AttackCount ++;
            }
        }
        else
        {
            pattern = AttackPatterns[UnityEngine.Random.Range(0, AttackPatterns.Count)];
            rb.AddForce(transform.forward * pattern.AddForce_Before.x + transform.up * pattern.AddForce_Before.y + transform.right * pattern.AddForce_Before.z);
            await Attack(pattern);
        }

        rb.AddForce(transform.forward * pattern.AddForce_After.x + transform.up * pattern.AddForce_After.y + transform.right * pattern.AddForce_After.z);

        await UniTask.WaitUntil(() =>  NavMesh.SamplePosition(navMeshAgent.transform.localPosition, out var navHit, 0.1f, NavMesh.AllAreas) == false, cancellationToken : destroyCancellationToken);

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
                    Spawner.OnDrop(dropItem, AttackerNWID);
                break;
            }
        }
        Spawner.OnDead(gameObject, AttackerNWID);
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

    // async void OnTriggerStay(Collider other)
    // {
    //     if (other.gameObject.CompareTag("Player"))
    //     {
    //         Target = other.transform;

    //         if (Vector3.Distance(transform.position, Target.transform.position) > entityConfig.AttackRange)
    //         {
    //             StartChace();
    //             entityState = EntityState.Chase;
    //             isAttacking = false;
    //         }
    //         else
    //         {
    //             if (isAttacking)
    //                 return;
    //             rb.AddForce(-10 * transform.forward);
    //             Debug.Log("AAAAAAAAAAAA");
    //             isAttacking = true;
    //             // StopChace();
    //             // if (!isAttacking)
    //             // {
    //             //     isAttacking = true;
    //             //     entityState = EntityState.Attack;

    //             //     await Attack(AttackPatterns[UnityEngine.Random.Range(0, AttackPatterns.Count - 1)]);

    //             //     isAttacking = false;
    //             //     entityState = EntityState.Chase;
    //             // }
    //         }
    //     }
    // }

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
public class DACS_Entities : MonoBehaviour
{
    public EntityStatusData entityStatusData;
    public virtual void OnDamage(DamageData damage, ulong nwID = 0){}
    public virtual void OnDead(){}
    public virtual void OnDodge(){}
    public virtual async void DoTDamage(EffectConfig effectConfig){}
}
