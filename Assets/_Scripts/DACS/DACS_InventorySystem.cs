using System.Collections.Generic;
using UnityEngine;

public class DACS_InventorySystem : MonoBehaviour // PlayerObject直下
{
    public PlayerDataManager pdManager; // ClientGeneralManagerが設定
    [SerializeField, NonReorderable] List<SlotGenerator> SlotGeneratorsList;
    int count = 0; // 追加されたアイテムをカウントし、一定数以上追加されたら自動でセーブする
    [SerializeField] int AutoSaveCount = 10; // 自動セーブを始める数(デフォルトで10回更新があると自動でセーブするようになっている)
    public PlayerInventory_Sword Pi_Sword {get {return pi_Sword;}}
    PlayerInventory_Sword pi_Sword;
    public PlayerHotbar PlayerHotbar {get {return playerHotbar;}}
    PlayerHotbar playerHotbar;


    public async void SetUp()
    {
        pi_Sword = await pdManager.LoadData<PlayerInventory_Sword>();
        playerHotbar = await pdManager.LoadData<PlayerHotbar>();
    }
    async void Update()
    {
        if (count >= AutoSaveCount)
        {
            await pdManager.SaveData(playerHotbar);
            await pdManager.SaveData(Pi_Sword);

            count = 0; // カウントリセット
        }
    }
    public void AddItem(ItemID itemID)
    {
        string jsonData = JsonUtility.ToJson(itemID);
        switch (itemID.FirstIndex) // ItemDataBaseのGetListの番号に元図いて割り当てる
        {
            case 10:
                pi_Sword.LongSword.Add(jsonData);
                break;
        }

        count ++;
    }
    public void ChangeHotbar(int index, ItemID itemID)
    {
        string jsonData = JsonUtility.ToJson(itemID);
        switch (index)
        {
            case 0:
                playerHotbar.PrimarySlot = jsonData;
                break;
            case 1:
                playerHotbar.SecondarySlot = jsonData;
                break;
            case 2:
                playerHotbar.GranadeSlot = jsonData;
                break;
            case 3:
                playerHotbar.SubSlot0 = jsonData;
                break;
            case 4:
                playerHotbar.SubSlot1 = jsonData;
                break;
        }

        AddItem(itemID);
        count ++;
    }

    public void LoadInventory(int index, IReadOnlyList<ItemID> items)
    {
        SlotGeneratorsList[index].itemIDsList = items;
    }
}

public struct PlayerHotbar
{
    public string PrimarySlot;
    public string SecondarySlot;
    public string GranadeSlot;
    public string SubSlot0;
    public string SubSlot1;
}

public struct PlayerInventory_Sword // ItemIDをJsonにして保存
{
    public List<string> LongSword;
    public List<string> ShortSword;
    public List<string> Spear;
    public List<string> Axe;
    public List<string> Hammer;
    public List<string> Shield;
}
public struct PlayerInventory_testItemDataList
{
    public List<string> items;
}
public struct PlayerInventory_AxeDataList
{
    public List<string> items;
}
public struct PlayerInventory_PicaxeDataList
{
    public List<string> items;
}
public struct PlayerInventory_DrillDataList
{
    public List<string> items;
}
public struct PlayerInventory_HammerDataList
{
    public List<string> items;
}
public struct PlayerInventory_ShieldDataList
{
    public List<string> items;
}
public struct PlayerInventory_ConsumablesDataList
{
    public List<string> items;
}
public struct PlayerInventory_ArrowDataList
{
    public List<string> items;
}
public struct PlayerInventory_MagicDataList
{
    public List<string> items;
}
public struct PlayerInventory_FishingRodDataList
{
    public List<string> items;
}
public struct PlayerInventory_LongSwordDataList
{
    public List<string> items;
}
public struct PlayerInventory_ShortSwordDataList
{
    public List<string> items;
}
public struct PlayerInventory_SpearDataList
{
    public List<string> items;
}
public struct PlayerInventory_HandgunDataList
{
    public List<string> items;
}
public struct PlayerInventory_RifleDataList
{
    public List<string> items;
}
public struct PlayerInventory_SMGDataList
{
    public List<string> items;
}
public struct PlayerInventory_ShotgunDataList
{
    public List<string> items;
}
public struct PlayerInventory_LMGDataList
{
    public List<string> items;
}
public struct PlayerInventory_SniperRifleDataList
{
    public List<string> items;
}
public struct PlayerInventory_BaseBodyList
{
    public List<string> items;
}
public struct PlayerInventory_HairDataList
{
    public List<string> items;
}
public struct PlayerInventory_FaceDataList
{
    public List<string> items;
}
public struct PlayerInventory_TopsDataList
{
    public List<string> items;
}
public struct PlayerInventory_BottomsDataList
{
    public List<string> items;
}
public struct PlayerInventory_ShoesDataList
{
    public List<string> items;
}
public struct PlayerInventory_AccessoryDataList
{
    public List<string> items;
}
public struct PlayerInventory_HelmetDataList
{
    public List<string> items;
}
public struct PlayerInventory_ChestArmorDataList
{
    public List<string> items;
}
public struct PlayerInventory_LegArmorDataList
{
    public List<string> items;
}
public struct PlayerInventory_BootsArmorDataList
{
    public List<string> items;
}
public struct PlayerInventory_PotionDataList
{
    public List<string> items;
}
public struct PlayerInventory_MaterialDataList
{
    public List<string> items;
}
public struct PlayerInventory_FoodDataList
{
    public List<string> items;
}
