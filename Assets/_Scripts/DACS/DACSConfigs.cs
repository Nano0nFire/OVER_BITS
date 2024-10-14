using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class DACS_P_Configs
{
    #region Damage
    [Header("Dmg Options")]
    [Space]
    public float BaseDmg;
    public bool UseBonusDmg;
    public float DistanceBonusDmg;
    public float MaxBonusDmg;
    public bool UseHeadShotBonus;
    public float HeadShotBonus;
    public float CritChance;
    public float CritDmg;

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

public struct BulletControl_Config
{
    public float Speed; // 弾速
    public float DropForce; // 弾の落下
    public float Estimate; // 着弾予測時間
    public Vector3 Dir; // 進行方向
}
