using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemDataConfigs
{
    public string ItemName;
    [Multiline(3)]
    public string ItemInfo;
    public GameObject ItemModel;
    public Sprite ItemImage;
    public ItemRarities ItemRarity;
    public bool ItemLock;
    public bool CanStack;
    public int StackAmount;
    [Tooltip("1,2桁目の数字でアイテムの動作モードを設定\n3,4桁目の数字で動作の詳細設定\n5,6桁目の数字で動作の詳細設定\n7,8桁目の数字で動作の詳細設定")]
    public string R_Function;
    [Tooltip("1,2桁目の数字でアイテムの動作モードを設定\n3,4桁目の数字で動作の詳細設定\n5,6桁目の数字で動作の詳細設定\n7,8桁目の数字で動作の詳細設定")]
    public string L_Function;
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Data", order = 1)]
public class ItemDataBase : ScriptableObject
{
    public int ItemListCount = 33; // リストの数
    [NonReorderable, Header("0"), InspectorName("0 : " +"testItemDataList")] public List<ItemDataConfigs> testItemDataList = new(); //0
    [NonReorderable, Header("1"), InspectorName("1 : " +"AxeDataList")] public List<ItemDataConfigs> AxeDataList = new(); //1
    [NonReorderable, Header("2"), InspectorName("2 : " +"PicaxeDataList")] public List<ItemDataConfigs> PicaxeDataList = new(); //2
    [NonReorderable, Header("3"), InspectorName("3 : " +"DrillDataList")] public List<ItemDataConfigs> DrillDataList = new(); //3
    [NonReorderable, Header("4"), InspectorName("4 : " +"HammerDataList")] public List<ItemDataConfigs> HammerDataList = new(); //4
    [NonReorderable, Header("5"), InspectorName("5 : " +"ShieldDataList")] public List<ItemDataConfigs> ShieldDataList = new(); //5
    [NonReorderable, Header("6"), InspectorName("6 : " +"ConsumablesDataList")] public List<ItemDataConfigs> ConsumablesDataList = new(); //6
    [NonReorderable, Header("7"), InspectorName("7 : " +"GranadeDataList")] public List<ItemDataConfigs> GranadeDataList = new(); //7
    [NonReorderable, Header("8"), InspectorName("8 : " +"MagicDataList")] public List<ItemDataConfigs> MagicDataList = new(); //8
    [NonReorderable, Header("9"), InspectorName("9 : " +"FishingRodDataList")] public List<ItemDataConfigs> FishingRodDataList = new(); //9
    [NonReorderable, Header("10"), InspectorName("10 : " +"LongSwordDataList")] public List<ItemDataConfigs> LongSwordDataList = new(); //10
    [NonReorderable, Header("11"), InspectorName("11 : " +"ShortSwordDataList")] public List<ItemDataConfigs> ShortSwordDataList = new(); //11
    [NonReorderable, Header("12"), InspectorName("12 : " +"SpearDataList")] public List<ItemDataConfigs> SpearDataList = new(); //12
    [NonReorderable, Header("13"), InspectorName("13 : " +"HandgunDataList")] public List<ItemDataConfigs> HandgunDataList = new(); //13
    [NonReorderable, Header("14"), InspectorName("14 : " +"RifleDataList")] public List<ItemDataConfigs> RifleDataList = new(); //14
    [NonReorderable, Header("15"), InspectorName("15 : " +"SMGDataList")] public List<ItemDataConfigs> SMGDataList = new(); //15
    [NonReorderable, Header("16"), InspectorName("16 : " +"ShotgunDataList")] public List<ItemDataConfigs> ShotgunDataList = new(); //16
    [NonReorderable, Header("17"), InspectorName("17 : " +"LMGDataList")] public List<ItemDataConfigs> LMGDataList = new(); //17
    [NonReorderable, Header("18"), InspectorName("18 : " +"SniperRifleDataList")] public List<ItemDataConfigs> SniperRifleDataList = new(); //18
    [NonReorderable, Header("19"), InspectorName("19 : " +"BaseBodyList")] public List<ItemDataConfigs> BaseBodyList = new(); //19
    [NonReorderable, Header("20"), InspectorName("20 : " +"FaceDataList")] public List<ItemDataConfigs> FaceDataList = new(); //20
    [NonReorderable, Header("21"), InspectorName("21 : " +"TopsDataList")] public List<ItemDataConfigs> TopsDataList = new(); //21
    [NonReorderable, Header("22"), InspectorName("22 : " +"BottomsDataList")] public List<ItemDataConfigs> BottomsDataList = new(); //22
    [NonReorderable, Header("23"), InspectorName("23 : " +"ShoesDataList")] public List<ItemDataConfigs> ShoesDataList = new(); //23
    [NonReorderable, Header("24"), InspectorName("24 : " +"HairDataList")] public List<ItemDataConfigs> HairDataList = new(); //24
    [NonReorderable, Header("25"), InspectorName("25 : " +"AccessoryDataList")] public List<ItemDataConfigs> AccessoryDataList = new(); //25
    [NonReorderable, Header("26"), InspectorName("26 : " +"HelmetDataList")] public List<ItemDataConfigs> HelmetDataList = new(); //26
    [NonReorderable, Header("27"), InspectorName("27 : " +"ChestArmorDataList")] public List<ItemDataConfigs> ChestArmorDataList = new(); //27
    [NonReorderable, Header("28"), InspectorName("28 : " +"LegArmorDataList")] public List<ItemDataConfigs> LegArmorDataList = new(); //28
    [NonReorderable, Header("29"), InspectorName("29 : " +"BootsArmorDataList")] public List<ItemDataConfigs> BootsArmorDataList = new(); //29
    [NonReorderable, Header("30"), InspectorName("30 : " +"PotionDataList")] public List<ItemDataConfigs> PotionDataList = new(); //30
    [NonReorderable, Header("31"), InspectorName("31 : " +"MaterialDataList")] public List<ItemDataConfigs> MaterialDataList = new(); //31
    [NonReorderable, Header("32"), InspectorName("32 : " +"FoodDataList")] public List<ItemDataConfigs> FoodDataList = new(); //32

    public List<ItemDataConfigs> GetList(int FirstNum)
    {
        return FirstNum switch
        {
            0 => testItemDataList,
            1 => AxeDataList,
            2 => PicaxeDataList,
            3 => DrillDataList,
            4 => HammerDataList,
            5 => ShieldDataList,
            6 => ConsumablesDataList,
            7 => GranadeDataList,
            8 => MagicDataList,
            9 => FishingRodDataList,
            10 => LongSwordDataList,
            11 => ShortSwordDataList,
            12 => SpearDataList,
            13 => HandgunDataList,
            14 => RifleDataList,
            15 => SMGDataList,
            16 => ShotgunDataList,
            17 => LMGDataList,
            18 => SniperRifleDataList,
            19 => BaseBodyList,
            20 => FaceDataList,
            21 => TopsDataList,
            22 => BottomsDataList,
            23 => ShoesDataList,
            24 => HairDataList,
            25 => AccessoryDataList,
            26 => HelmetDataList,
            27 => ChestArmorDataList,
            28 => LegArmorDataList,
            29 => BootsArmorDataList,
            30 => PotionDataList,
            31 => MaterialDataList,
            32 => FoodDataList,
            _ => null,
        };
    }
    public ItemDataConfigs GetItem(int FirstIndex, int SecondIndex)
    {
        List<ItemDataConfigs> list = GetList(FirstIndex);
        return list[SecondIndex];
    }
}

[System.Serializable]
public struct ItemData
{
    /// <summary>
    /// -1の場合は範囲外エラーを示す<br />
    /// -2の場合はアイテムが存在しないことを示す
    /// </summary>
    public int FirstIndex; // GetItemの最初のインデックスに該当するもの
    public int SecondIndex; // GetItemの二番目のインデックスに該当する
    public int Amount; // アイテムの数
    public List<int> Mods;
    public List<int> Enchants;
    public int PriAddon;
    public int SecAddon;
    public List<int> Attributes;
}
public enum ItemRarities
{
    normal,
    uncommon,
    rare,
    epic,
    legend,
    overrank,
    named,
}

[Serializable]
public class ItemFunction
{
    public int funcA;
    public int funcB;
}

public static class RarityToColor
{
    public static Color ToColor(ItemRarities rarity)
    {
        return rarity switch
        {
            ItemRarities.normal => new Color(0.65f, 0.65f, 0.65f, 1),
            ItemRarities.uncommon => new Color(0, 0.92f, 0, 1),
            ItemRarities.rare => new Color(0, 0.235f, 0.784f, 1),
            ItemRarities.epic => new Color(0.561f, 0, 0.635f, 1),
            ItemRarities.legend => new Color(1, 1, 0, 1),
            ItemRarities.overrank => new Color(1, 0, 0.03091812f, 1),
            ItemRarities.named => new Color(1, 0, 1, 1),
            _ => new Color(0, 0, 0, 0),
        };
    }
}