using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class DACS_P_Configs
{
    #region Damage
    [Header("Dmg Options")]
    [Space]
    public DamageConfigs DamageConfig;

    #endregion
    #region Trail Options
    [Header("Trail Options")]
    [Space]
    public bool UseTrail;
    public Material TrailMaterial;
    public float TrailTime;
    public AnimationCurve TrailCurve;

    #endregion
    #region Projectile

    #region Projectile Options
    [Header("Projectile Options")]
    [Space]
    [Header("Shooting Options")]
    public ProjectileTypes ProjectileType;
    public GameObject ProjectileObject;
    public float ProjectileSpeed;
    public float ProjectileSpreadV;
    public float ProjectileSpreadHz;
    public float ADSProjectileSpreadV;
    public float ADSProjectileSpreadHz;
    public float ProjectileDrop;
    public float MaxRange;

    [Space]
    [Header("FireType Options")]
    public FireTypes FireType;
    public float FireRate;
    public int ProjectileAmount;
    public int BurstAmount;
    public float BurstRate;

    [Space]
    [Header("Ammo Options")]
    public int MagAmount;
    public int reloadSpeed;
    public int ReloadAmount;

    #endregion
    #region Collision Options
    [Header("Collision Options")]
    [Space]

    public float ColRadius;
    public float ColMaxDistance;

    #endregion
    #region Start Position Options
    [Header("Start Position Options")]
    [Space]

    public float vOffset;
    public float hzOffset;

    #endregion
    #region Homing Options
    [Header("Homing Options")]
    [Space]

    public bool UseHoming;
    public HomingTypes HomingType;
    public float HomingRange;
    public float RotationSpeed;

    #endregion
    #region Charge Options
    [Header("Charge Options")]
    [Space]

    public float ChargePerProjectile;
    public int MaxCharge;

    #endregion

    #endregion
}

public enum ProjectileTypes
{
    LightBullet,
    MediumBullet,
    HeavyBullet,
    Shell,
    Rockets
}

public enum AttackTypes
{
    Adjacent,
    Projectile
}
public enum FireTypes
{
    Fullauto,
    Burst,
    Semiauto,
    ChargeAndSingle,
    ChargeAndMulti,
    SetAndShot
}
public enum HomingTypes
{
    PointHoming,
    GuidedHoming,
    AutoHoming
}

[System.Serializable]
public struct DamageConfigs // 設定用 & AttackメソッドでEntityのステータスを加算して使用
{
    /// <summary>
    /// データベース内では弾のベースダメージを保持 <br />
    /// 受け渡し時はステータスによるダメージ増加を終えた状態で格納
    /// </summary>
    public float Dmg;
    public float BonusDmgPerDistance;
    public float MaxBonusDmg;
    public float HeadShotBonus;
    public float CritChance;
    public float CritDmg;
    public float DefMagnification; // Defの適応倍率
    public float Penetration; // Defに対する貫通力
    public float HitChance; // 回避率 - 命中率 が成立する
    public List<EffectConfig> Effects;
}
public struct DamageData // 受け渡し用
{
    public float Dmg;
    public float DefMagnification; // Defの適応倍率
    public float Penetration; // Defに対する貫通力
    public float HitChance; // 回避率 - 命中率 が成立する
}

[System.Serializable]
public struct EffectConfig
{
    public Effects Effect;
    public float Duration;
}

public enum Effects
{
    Ignite, // 継続型高ダメージ / 防御可
    Unstable, // HPとMPの自然回復が停止 / 回避可能(状態異常耐性)
    Poison, // 継続型低ダメージ / 防御不可
    Slowness, // 移動速度低下 / 回避可能(状態異常耐性)
    Weakness, // 被ダメージ増加 / 回避可能(状態異常耐性)

}

public struct BulletControl_Config
{
    public float Speed; // 弾速
    public float DropForce; // 弾の落下
    public float Estimate; // 着弾予測時間
    public Vector3 Dir; // 進行方向
}

[System.Serializable]
public class EntityConfigs
{
    public BaseEntityStatus EntityStatus;
    public GameObject EntityPrefab; // エンティティのプレハブ
    public string EntityName;
    public string EntityInfo;
    public float AttackRange;
    public float SearceRange;
    public float Speed;
    public List<EntityDrop> DropTable;
}

[System.Serializable]
public class BaseEntityStatus
{
    public int BaseLevel; // デフォルトのレベル
    public float HPperLevel; // レベル毎のHP増加度
    public float AtkperLevel;
    public float DefperLevel;
    public int ReviveNumber;
    public float DodgeChance;
}

[System.Serializable]
public struct EntityStatusData
{
    public float MaxHP;
    public float HP;
    public float Def;
    public float Abno;
    public float DodgeChance;
    public float Atk;
    public float MaxMP;
    public float MP;
    public float CritChance;
    public float CritDamage;
    public float Penetration;
    public float HitChance;
}
public struct EntityData
{
    public int FirstIndex;
    public int SecondIndex;
}

[System.Serializable]
public struct AttackPattern
{
    public AttackTypes attackType;
    public int attackSystemID;
    public float AttackDuration;
    public Vector3 AddForce_Before;
    public Vector3 AddForce_After;
}
[System.Serializable]
public struct EntityDrop
{
    public float DropChance;
    public ItemData itemData;
}
public enum EntityState
{
    Search,
    Stanby,
    Chase,
    Attack,
    Escape
}