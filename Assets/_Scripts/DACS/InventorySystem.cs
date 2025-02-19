using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace DACS.Inventory
{
    public class InventorySystem : MonoBehaviour // PlayerObject直下
    {
        public ItemData SelectedItem
        {
            get
            {
                if (SelectedSlotIndex >= HotbarData.Count)
                    return new ItemData{FirstIndex = -1}; // 範囲外エラー
                else
                    return HotbarData[SelectedSlotIndex];
            }
        }
        public int SelectedSlotIndex;
        readonly int InventoryCount = 33;

        public async UniTask Setup()
        {
            if (await PlayerDataManager.HasData("LoginLog"))
            {
                HotbarData = await PlayerDataManager.LoadData<List<ItemData>>("HotbarData");
                List<UniTask> tasks = new();
                for (int i = 0; i < InventoryCount; i ++) // インベントリのロード
                {
                    tasks.Add(LoadInventory(i));
                }
                await UniTask.WhenAll(tasks);
            }
            else
            {
                await CreateProfile();
            }

            string log = "LastLogin : " + DateTime.Now.ToString();

            PlayerDataManager.SaveData(log, "LoginLog").Forget();
        }

        async UniTask CreateProfile()
        {
            HotbarData = await PlayerDataManager.LoadData<List<ItemData>>("HotbarData");
            for (int i = 0; i < InventoryCount; i ++) // インベントリのロード
            {
                await LoadInventory(i);
            }
        }

        async UniTask LoadInventory(int index)
        {
            Debug.Log("LoadInventory : " + GetListName(index));
            GetInventoryData(index).AddRange(await PlayerDataManager.LoadData<List<ItemData>>(GetListName(index)));
        }

        public static void ChangeHotbar(int index, ItemData itemData) // ホットバーの使用状況を保存
        {
            HotbarData[index] = itemData;
            PlayerDataManager.SaveData(HotbarData, "HotbarData").Forget();
        }

        public void ConsumeItem(ItemData itemData)
        {

        }

        public static string GetListName(int FirstNum)
        {
            return FirstNum switch
            {
                0 => "testItemInventoryData",
                1 => "AxeInventoryData",
                2 => "PicaxeInventoryData",
                3 => "DrillInventoryData",
                4 => "HammerInventoryData",
                5 => "ShieldInventoryData",
                6 => "ConsumablesInventoryData",
                7 => "GranadeInventoryData",
                8 => "MagicInventoryData",
                9 => "FishingRodInventoryData",
                10 => "LongSwordInventoryData",
                11 => "ShortSwordInventoryData",
                12 => "SpearInventoryData",
                13 => "HandgunInventoryData",
                14 => "RifleInventoryData",
                15 => "SMGInventoryData",
                16 => "ShotgunInventoryData",
                17 => "LMGInventoryData",
                18 => "SniperRifleInventoryData",
                19 => "BaseBodyList",
                20 => "FaceInventoryData",
                21 => "TopsInventoryData",
                22 => "BottomsInventoryData",
                23 => "ShoesInventoryData",
                24 => "HairInventoryData",
                25 => "AccessoryInventoryData",
                26 => "HelmetInventoryData",
                27 => "ChestArmorInventoryData",
                28 => "LegArmorInventoryData",
                29 => "BootsArmorInventoryData",
                30 => "PotionInventoryData",
                31 => "MaterialInventoryData",
                32 => "FoodInventoryData",
                _ => null,
            };
        }

        #region InventoryDatas
        [NonReorderable] public static List<ItemData> testItemInventoryData = new(); //0
        [NonReorderable] public static List<ItemData> AxeInventoryData = new(); //1
        [NonReorderable] public static List<ItemData> PicaxeInventoryData = new(); //2
        [NonReorderable] public static List<ItemData> DrillInventoryData = new(); //3
        [NonReorderable] public static List<ItemData> HammerInventoryData = new(); //4
        [NonReorderable] public static List<ItemData> ShieldInventoryData = new(); //5
        [NonReorderable] public static List<ItemData> ConsumablesInventoryData = new(); //6
        [NonReorderable] public static List<ItemData> GranadeInventoryData = new(); //7
        [NonReorderable] public static List<ItemData> MagicInventoryData = new(); //8
        [NonReorderable] public static List<ItemData> FishingRodInventoryData = new(); //9
        [NonReorderable] public static List<ItemData> LongSwordInventoryData = new(); //10
        [NonReorderable] public static List<ItemData> ShortSwordInventoryData = new(); //11
        [NonReorderable] public static List<ItemData> SpearInventoryData = new(); //12
        [NonReorderable] public static List<ItemData> HandgunInventoryData = new(); //13
        [NonReorderable] public static List<ItemData> RifleInventoryData = new(); //14
        [NonReorderable] public static List<ItemData> SMGInventoryData = new(); //15
        [NonReorderable] public static List<ItemData> ShotgunInventoryData = new(); //16
        [NonReorderable] public static List<ItemData> LMGInventoryData = new(); //17
        [NonReorderable] public static List<ItemData> SniperRifleInventoryData = new(); //18
        [NonReorderable] public static List<ItemData> BaseBodyList = new(); //19
        [NonReorderable] public static List<ItemData> FaceInventoryData = new(); //20
        [NonReorderable] public static List<ItemData> TopsInventoryData = new(); //21
        [NonReorderable] public static List<ItemData> BottomsInventoryData = new(); //22
        [NonReorderable] public static List<ItemData> ShoesInventoryData = new(); //23
        [NonReorderable] public static List<ItemData> HairInventoryData = new(); //24
        [NonReorderable] public static List<ItemData> AccessoryInventoryData = new(); //25
        [NonReorderable] public static List<ItemData> HelmetInventoryData = new(); //26
        [NonReorderable] public static List<ItemData> ChestArmorInventoryData = new(); //27
        [NonReorderable] public static List<ItemData> LegArmorInventoryData = new(); //28
        [NonReorderable] public static List<ItemData> BootsArmorInventoryData = new(); //29
        [NonReorderable] public static List<ItemData> PotionInventoryData = new(); //30
        [NonReorderable] public static List<ItemData> MaterialInventoryData = new(); //31
        [NonReorderable] public static List<ItemData> FoodInventoryData = new(); //32

        [NonReorderable] public static List<ItemData> HotbarData = new();

        public static List<ItemData> GetInventoryData(int FirstNum)
        {
            return FirstNum switch
            {
                0 => testItemInventoryData,
                1 => AxeInventoryData,
                2 => PicaxeInventoryData,
                3 => DrillInventoryData,
                4 => HammerInventoryData,
                5 => ShieldInventoryData,
                6 => ConsumablesInventoryData,
                7 => GranadeInventoryData,
                8 => MagicInventoryData,
                9 => FishingRodInventoryData,
                10 => LongSwordInventoryData,
                11 => ShortSwordInventoryData,
                12 => SpearInventoryData,
                13 => HandgunInventoryData,
                14 => RifleInventoryData,
                15 => SMGInventoryData,
                16 => ShotgunInventoryData,
                17 => LMGInventoryData,
                18 => SniperRifleInventoryData,
                19 => BaseBodyList,
                20 => FaceInventoryData,
                21 => TopsInventoryData,
                22 => BottomsInventoryData,
                23 => ShoesInventoryData,
                24 => HairInventoryData,
                25 => AccessoryInventoryData,
                26 => HelmetInventoryData,
                27 => ChestArmorInventoryData,
                28 => LegArmorInventoryData,
                29 => BootsArmorInventoryData,
                30 => PotionInventoryData,
                31 => MaterialInventoryData,
                32 => FoodInventoryData,
                _ => null,
            };
        }
        #endregion
    }

    // public struct PlayerInventory_testItemInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_AxeInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_PicaxeInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_DrillInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_HammerInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_ShieldInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_ConsumablesInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_ArrowInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_MagicInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_FishingRodInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_LongSwordInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_ShortSwordInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_SpearInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_HandgunInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_RifleInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_SMGInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_ShotgunInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_LMGInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_SniperRifleInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_BaseBodyList
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_HairInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_FaceInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_TopsInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_BottomsInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_ShoesInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_AccessoryInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_HelmetInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_ChestArmorInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_LegArmorInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_BootsArmorInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_PotionInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_MaterialInventoryData
    // {
    //     public List<string> items;
    // }
    // public struct PlayerInventory_FoodInventoryData
    // {
    //     public List<string> items;
    // }
}