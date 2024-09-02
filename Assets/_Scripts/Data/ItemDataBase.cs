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

    public enum ItemRarities
    {
        normal,
        uncommon,
        rare,
        epic,
        legend,
        named,
    }
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Data", order = 1)]
public class ItemDataBase : ScriptableObject
{
    [NonReorderable] public List<ItemDataConfigs> testItemDataList = new(); //0
    [NonReorderable] public List<ItemDataConfigs> AxeDataList = new(); //1
    [NonReorderable] public List<ItemDataConfigs> PicaxeDataList = new(); //2
    [NonReorderable] public List<ItemDataConfigs> DrillDataList = new(); //3
    [NonReorderable] public List<ItemDataConfigs> HammerDataList = new(); //4
    [NonReorderable] public List<ItemDataConfigs> ShieldDataList = new(); //5
    [NonReorderable] public List<ItemDataConfigs> BowDataList = new(); //6
    [NonReorderable] public List<ItemDataConfigs> ArrowDataList = new(); //7
    [NonReorderable] public List<ItemDataConfigs> MagicDataList = new(); //8
    [NonReorderable] public List<ItemDataConfigs> FishingRodDataList = new(); //9
    [NonReorderable] public List<ItemDataConfigs> LongSwordDataList = new(); //10
    [NonReorderable] public List<ItemDataConfigs> ShortSwordDataList = new(); //11
    [NonReorderable] public List<ItemDataConfigs> SpearDataList = new(); //12
    [NonReorderable] public List<ItemDataConfigs> HandgunDataList = new(); //13
    [NonReorderable] public List<ItemDataConfigs> RifleDataList = new(); //14
    [NonReorderable] public List<ItemDataConfigs> SMGDataList = new(); //15
    [NonReorderable] public List<ItemDataConfigs> ShotgunDataList = new(); //16
    [NonReorderable] public List<ItemDataConfigs> LMGDataList = new(); //17
    [NonReorderable] public List<ItemDataConfigs> SniperRifleDataList = new(); //18
    [NonReorderable] public List<ItemDataConfigs> BaseBodyList = new(); //19
    [NonReorderable] public List<ItemDataConfigs> HairDataList = new(); //20
    [NonReorderable] public List<ItemDataConfigs> FaceDataList = new(); //21
    [NonReorderable] public List<ItemDataConfigs> TopsDataList = new(); //22
    [NonReorderable] public List<ItemDataConfigs> BottomsDataList = new(); //23
    [NonReorderable] public List<ItemDataConfigs> ShoesDataList = new(); //24
    [NonReorderable] public List<ItemDataConfigs> AccessoryDataList = new(); //25
    [NonReorderable] public List<ItemDataConfigs> HelmetDataList = new(); //26
    [NonReorderable] public List<ItemDataConfigs> ChestArmorDataList = new(); //27
    [NonReorderable] public List<ItemDataConfigs> LegArmorDataList = new(); //28
    [NonReorderable] public List<ItemDataConfigs> BootsArmorDataList = new(); //29
    [NonReorderable] public List<ItemDataConfigs> PotionDataList = new(); //30
    [NonReorderable] public List<ItemDataConfigs> MaterialDataList = new(); //31
    [NonReorderable] public List<ItemDataConfigs> FoodDataList = new(); //32

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
            6 => BowDataList,
            7 => ArrowDataList,
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
            20 => HairDataList,
            21 => FaceDataList,
            22 => TopsDataList,
            23 => BottomsDataList,
            24 => ShoesDataList,
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
    public ItemDataConfigs GetItem(int FirstNum, int SecondNum)
    {
        List<ItemDataConfigs> list = GetList(FirstNum);
        return list[SecondNum];
    }
}