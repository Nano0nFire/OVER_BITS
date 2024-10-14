using System;
using System.IO.Ports;
using Codice.CM.Common;
using UnityEditor;
using UnityEngine;

/// <summary>
/// イベントデータの各要素のinspector表示をカスタマイズするためのDrawerクラス
/// </summary>
[CustomPropertyDrawer(typeof(DACS_P_Configs))]
public class DACSDRawer : PropertyDrawer
{
    float additionalHeight = 0;

    Rect BaseDmgRect;
    Rect UseBonusDmgRect;
    Rect DistanceBonusDmgRect;
    Rect MaxBonusDmgRect;
    Rect UseHeadShotBonusRect;
    Rect HeadShotBonusRect;
    Rect CritChanceRect;
    Rect CritDmgRect;
    Rect UseTrailRect;
    Rect TrailMaterialRect;
    Rect TrailTimeRect;
    Rect TrailCurveRect;
    Rect ProjectileTypeRect;
    Rect ProjectileObjectRect;
    Rect ProjectileSpeedRect;
    Rect ProjectileSpreadVRect;
    Rect ProjectileSpreadHzRect;
    Rect ADSProjectileSpreadVRect;
    Rect ADSProjectileSpreadHzRect;
    Rect ProjectileDropRect;
    Rect MaxRangeRect;
    Rect FireTypeRect;
    Rect FireRateRect;
    Rect ProjectileAmountRect;
    Rect BurstAmountRect;
    Rect BurstRateRect;
    Rect MagAmountRect;
    Rect reloadSpeedRect;
    Rect ReloadAmountRect;
    Rect ColRadiusRect;
    Rect ColMaxDistanceRect;
    Rect vOffsetRect;
    Rect hzOffsetRect;
    Rect UseHomingRect;
    Rect HomingTypeRect;
    Rect HomingRangeRect;
    Rect RotationSpeedRect;
    Rect ChargePerProjectileRect;
    Rect MaxChargeRect;

    SerializedProperty UseBonusDmgProp;
    SerializedProperty UseHeadShotBonusProp;
    SerializedProperty UseTrailProp;
    SerializedProperty TrailMaterialProp;
    SerializedProperty TrailCurveProp;
    SerializedProperty ProjectileTypeProp;
    SerializedProperty ProjectileObjectProp;
    SerializedProperty AttackTypeProp;
    SerializedProperty FireTypeProp;
    SerializedProperty UseHomingProp;
    SerializedProperty HomingTypeProp;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (new EditorGUI.PropertyScope(position, label, property))
        {
            additionalHeight = 0;
            position.height = EditorGUIUtility.singleLineHeight;

            BaseDmgRect = GetRect(position, 2f);
            SetFloatField(BaseDmgRect, "BaseDmg", property);

            UseBonusDmgRect = GetRect(BaseDmgRect, 2f);
            UseBonusDmgProp = property.FindPropertyRelative("UseBonusDmg");
            UseBonusDmgProp.boolValue = EditorGUI.Toggle(UseBonusDmgRect, "UseBonusDmg", UseBonusDmgProp.boolValue);

            if (UseBonusDmgProp.boolValue)
            {
                DistanceBonusDmgRect = GetRect(UseBonusDmgRect, 2f);
                SetFloatField(DistanceBonusDmgRect, "DistanceBonusDmg", property);

                MaxBonusDmgRect = GetRect(DistanceBonusDmgRect, 2f);
                SetFloatField(MaxBonusDmgRect, "DistanceBonusDmg", property);

                UseHeadShotBonusRect = GetRect(MaxBonusDmgRect, 22f);
            }
            else
            {
                UseHeadShotBonusRect = GetRect(UseBonusDmgRect, 2f);
            }

            UseHeadShotBonusProp = property.FindPropertyRelative("UseHeadShotBonus");
            UseHeadShotBonusProp.boolValue = EditorGUI.Toggle(UseHeadShotBonusRect, "UseHeadShotBonus", UseHeadShotBonusProp.boolValue);

            if (UseHeadShotBonusProp.boolValue)
            {
                HeadShotBonusRect = GetRect(UseHeadShotBonusRect, 2f);
                SetFloatField(HeadShotBonusRect, "HeadShotBonus", property);

                CritChanceRect = GetRect(HeadShotBonusRect, 22f);
            }
            else
            {
                CritChanceRect = GetRect(UseHeadShotBonusRect, 2f);
            }

            SetFloatField(CritChanceRect, "CritChance", property);

            CritDmgRect = GetRect(CritChanceRect, 2f);
            SetFloatField(CritDmgRect, "CritDmg", property);

            UseTrailRect = GetRect(CritDmgRect, 22f);
            UseTrailProp = property.FindPropertyRelative("UseTrail");
            UseTrailProp.boolValue = EditorGUI.Toggle(UseTrailRect, "UseTrail", UseTrailProp.boolValue);

            if (UseTrailProp.boolValue)
            {
                TrailMaterialRect = GetRect(UseTrailRect, 2f);
                TrailMaterialProp = property.FindPropertyRelative("TrailMaterial");
                Material TrailMaterial = (Material)TrailMaterialProp.objectReferenceValue;
                TrailMaterial = (Material)EditorGUI.ObjectField(TrailMaterialRect, "TrailMaterial", TrailMaterial, typeof(Material), true);
                TrailMaterialProp.objectReferenceValue = TrailMaterial;

                TrailTimeRect = GetRect(TrailMaterialRect, 2f);
                SetFloatField(TrailTimeRect, "TrailTime", property);

                TrailCurveRect = GetRect(TrailTimeRect, 2f);
                TrailCurveProp = property.FindPropertyRelative("TrailCurve");
                AnimationCurve trailCurve = TrailCurveProp.animationCurveValue;
                trailCurve = EditorGUI.CurveField(TrailCurveRect, "TrailCurve", trailCurve);
                TrailCurveProp.animationCurveValue = trailCurve;

                ProjectileTypeRect = GetRect(TrailCurveRect, 22f);
            }
            else
            {
                ProjectileTypeRect = GetRect(UseTrailRect, 2f);
            }

            ProjectileTypeProp = property.FindPropertyRelative("ProjectileType");
            ProjectileTypeProp.enumValueIndex = EditorGUI.Popup(ProjectileTypeRect, "ProjectileType", ProjectileTypeProp.enumValueIndex, Enum.GetNames(typeof(ProjectileTypes)));

            ProjectileObjectRect = GetRect(ProjectileTypeRect, 2f);
            ProjectileObjectProp = property.FindPropertyRelative("ProjectileObject");
            GameObject projectileObject = (GameObject)ProjectileObjectProp.objectReferenceValue;
            projectileObject = (GameObject)EditorGUI.ObjectField(ProjectileObjectRect, "ProjectileObject", projectileObject, typeof(GameObject), true);
            ProjectileObjectProp.objectReferenceValue = projectileObject;

            ProjectileSpeedRect = GetRect(ProjectileObjectRect, 2f);
            SetFloatField(ProjectileSpeedRect, "ProjectileSpeed", property);

            ProjectileSpreadVRect = GetRect(ProjectileSpeedRect, 2f);
            SetFloatField(ProjectileSpreadVRect, "ProjectileSpreadV", property);

            ProjectileSpreadHzRect = GetRect(ProjectileSpreadVRect, 2f);
            SetFloatField(ProjectileSpreadHzRect, "ProjectileSpreadHz", property);

            ADSProjectileSpreadVRect = GetRect(ProjectileSpreadHzRect, 2f);
            SetFloatField(ADSProjectileSpreadVRect, "ADSProjectileSpreadV", property);

            ADSProjectileSpreadHzRect = GetRect(ADSProjectileSpreadVRect, 2f);
            SetFloatField(ADSProjectileSpreadHzRect, "ADSProjectileSpreadHz", property);

            ProjectileDropRect = GetRect(ADSProjectileSpreadHzRect, 2f);
            SetFloatField(ProjectileDropRect, "ProjectileDrop", property);

            MaxRangeRect = GetRect(ProjectileDropRect, 2f);
            SetFloatField(MaxRangeRect, "MaxRange", property);

            ProjectileAmountRect = GetRect(MaxRangeRect, 2f);
            SetIntField(ProjectileAmountRect, "ProjectileAmount", property);

            FireTypeRect = GetRect(ProjectileAmountRect, 2f);
            FireTypeProp = property.FindPropertyRelative("FireType");
            FireTypeProp.enumValueIndex = EditorGUI.Popup(FireTypeRect, "FireType", FireTypeProp.enumValueIndex, Enum.GetNames(typeof(FireTypes)));

            switch ((FireTypes)FireTypeProp.enumValueIndex)
            {
                case FireTypes.Fullauto:
                        FireRateRect = GetRect(FireTypeRect, 2f);
                    SetFloatField(FireRateRect, "FireRate", property);

                    vOffsetRect = GetRect(FireRateRect,2f);

                    break;

                case FireTypes.Burst:
                    FireRateRect = GetRect(FireTypeRect, 2f);
                    SetFloatField(FireRateRect, "FireRate", property);

                    BurstAmountRect = GetRect(FireRateRect, 2f);
                    SetIntField(BurstAmountRect, "BurstAmount", property);

                    BurstRateRect = GetRect(BurstAmountRect, 2f);
                    SetFloatField(BurstRateRect, "BurstRate", property);

                    vOffsetRect = GetRect(BurstRateRect, 2f);

                    break;

                case FireTypes.Semiauto:
                    FireRateRect = GetRect(FireTypeRect, 2f);
                    SetFloatField(FireRateRect, "FireRate", property);

                    vOffsetRect = GetRect(FireRateRect, 2f);

                    break;

                case FireTypes.ChargeAndSingle:
                    FireRateRect = GetRect(FireTypeRect, 2f);
                    SetFloatField(FireRateRect, "FireRate", property);

                    vOffsetRect = GetRect(FireRateRect, 2f);

                    break;

                case FireTypes.ChargeAndMulti:
                    FireRateRect = GetRect(FireTypeRect, 2f);
                    SetFloatField(FireRateRect, "FireRate", property);

                    ChargePerProjectileRect = GetRect(FireRateRect, 2f);
                    SetFloatField(ChargePerProjectileRect, "ChargePerProjectile", property);

                    MaxChargeRect = GetRect(ChargePerProjectileRect, 2f);
                    SetIntField(MaxChargeRect, "MaxCharge", property);

                    vOffsetRect = GetRect(MaxChargeRect, 2f);

                    break;

                case FireTypes.SetAndShot:
                    FireRateRect = GetRect(FireTypeRect, 2f);
                    SetFloatField(FireRateRect, "FireRate", property);

                    vOffsetRect = GetRect(FireRateRect, 2f);

                    break;
            }

            SetFloatField(vOffsetRect, "vOffset", property);

            hzOffsetRect = GetRect(vOffsetRect, 2f);
            SetFloatField(hzOffsetRect, "hzOffset", property);

            ColRadiusRect = GetRect(hzOffsetRect, 2f);
            SetFloatField(ColRadiusRect, "ColRadius", property);

            ColMaxDistanceRect = GetRect(ColRadiusRect, 2f);
            SetFloatField(ColMaxDistanceRect, "ColMaxDistance", property);

            MagAmountRect = GetRect(ColMaxDistanceRect, 2f);
            SetIntField(MagAmountRect, "MagAmount", property);

            reloadSpeedRect = GetRect(MagAmountRect, 2f);
            SetIntField(reloadSpeedRect, "reloadSpeed", property);

            ReloadAmountRect = GetRect(reloadSpeedRect, 2f);
            SetIntField(ReloadAmountRect, "ReloadAmount", property);

            UseHomingRect = GetRect(ReloadAmountRect, 2f);
            UseHomingProp = property.FindPropertyRelative("UseHoming");
            UseHomingProp.boolValue = EditorGUI.Toggle(UseHomingRect, "UseHoming", UseHomingProp.boolValue);

            if (UseHomingProp.boolValue)
            {
                HomingTypeRect = GetRect(UseHomingRect, 2f);
                HomingTypeProp = property.FindPropertyRelative("HomingType");
                HomingTypeProp.enumValueIndex = EditorGUI.Popup(HomingTypeRect, "HomingType", HomingTypeProp.enumValueIndex, Enum.GetNames(typeof(HomingTypes)));

                HomingRangeRect = GetRect(HomingTypeRect, 2f);
                SetFloatField(HomingRangeRect, "HomingRange", property);

                RotationSpeedRect = GetRect(HomingRangeRect, 2f);
                SetFloatField(RotationSpeedRect,"RotationSpeed", property);
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return additionalHeight;
    }
    private void SetFloatField(Rect rect, string Name, SerializedProperty property)
    {
        SerializedProperty Prop = property.FindPropertyRelative(Name);
        Prop.floatValue = EditorGUI.FloatField(rect, Name, Prop.floatValue);
    }
    private void SetIntField(Rect rect, string Name, SerializedProperty property)
    {
        SerializedProperty Prop = property.FindPropertyRelative(Name);
        Prop.intValue = EditorGUI.IntField(rect, Name, Prop.intValue);
    }
    private Rect GetRect(Rect rect, float Ex)
    {
        additionalHeight += Ex * 8;

        Rect Rect = new(rect) {y = rect.y + EditorGUIUtility.singleLineHeight + Ex};

        return Rect;
    }
}
