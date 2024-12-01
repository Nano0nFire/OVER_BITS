using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Cysharp.Threading.Tasks;

public class UIGeneral : MonoBehaviour
{
    ClientGeneralManager cgManager;
    public CustomLifeAvatar cla;
    public UI_PlayerSettings uI_PlayerSettings;
    public PlayerDataManager pdManager {get; private set;}
    [SerializeField] ItemDataBase itemDataBase;
    DACS_InventorySystem inventorySystem; // ClientGeneralManagerが設定
    [SerializeField] GameObject SlotPrefab;
    // [SerializeField] List<PanelSwitcher> psList;
    // [SerializeField] List<SlotGenerator> SGList;
    [Header("Infomation Panel Settings")]
    [SerializeField] TextMeshProUGUI txt_Info_ItemName;
    [SerializeField] TextMeshProUGUI txt_Info_ItemInfo;
    [SerializeField] Sprite txt_Info_ItemImage;
    UI_Component[] UIComponents;
    public UI_Component[] InventoryParents;
    List<Transform> SlotXforms = new();
    [SerializeField] int activatedSlotIndex = 0;
    public int ActivePanelIndex;

    public void Setup(ClientGeneralManager cgManager)
    {
        this.cgManager = cgManager;
        pdManager = cgManager.pdManager;
        inventorySystem = cgManager.invSystem;
        pdManager.OnItemAdded += LoadData;
        cla = cgManager.GetComponent<CustomLifeAvatar>();

        UIComponents = GetComponentsInChildren<UI_Component>(true);
        int i = 0;
        foreach (var panel in UIComponents)
        {
            panel.uiGeneral = this;
            panel.panelID = i;
            i ++;
        }
        InventoryParents = new UI_Component[itemDataBase.ItemListCount];
        foreach (var component in GetComponentsInChildren<UI_Component>(true))
        {
            component.uiGeneral = this;
            component.itemDataBase = itemDataBase;
            component.Setup();
        }
    }

    // void OnEnable()
    // {
    //     foreach (var item in psList)
    //         item.EnableUI();
    //     foreach (var item in SGList)
    //     {
    //         item.cla = cla;
    //         item.EnableUI();
    //     }
    // }
    // void OnDisable()
    // {
    //     foreach (var item in SGList) item.DisableUI();
    // }

    public void UIexit(GameObject Menu)
    {
        Menu.SetActive(!Menu.activeSelf);

        if (Menu == cgManager.MainMenu)
        {
            cgManager.UseInput = Menu.activeSelf;
        }
    }

    /// <summary>
    /// 引数分のSlotをリストで返す <br />
    /// 不足分は自動で生成して補う
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public List<Transform> GetSlots(int amount)
    {
        if (0 >= amount)
            return null;

        if (SlotXforms.Count < amount) // 生成済みのSlotが必要量に達しているか確認して、足りない場合は不足分を生成
        {
            int shortage = amount - SlotXforms.Count;
            for(int i = 0; i < shortage; i++)
                SlotXforms.Add(Instantiate(SlotPrefab).transform);
        }
        activatedSlotIndex = amount-1;
        return SlotXforms.GetRange(0, amount);
    }

    /// <summary>
    /// Slotを一つだけ返す<br />
    /// 不足分は自動で生成して補う<br />
    /// 複数受け取りたい場合は"GetSlots(int)"を使用する
    /// </summary>
    /// <returns></returns>
    public Transform GetSlot()
    {
        if (SlotXforms.Count < activatedSlotIndex+1) // 生成済みのSlotが必要量に達しているか確認して、足りない場合は不足分を生成
        {
            SlotXforms.Add(Instantiate(SlotPrefab).transform);
        }
        return SlotXforms[activatedSlotIndex+1];
    }

    public void LoadInfoPanel(ItemData itemID)
    {
        var data = itemDataBase.GetItem(itemID.FirstIndex, itemID.SecondIndex);

    }

    public void SaveData() => Save(); // UIからの呼び出し用
    async void Save()
    {
        switch (UIComponents[ActivePanelIndex].uiType)
        {
            case UIType.Null:
                break;

            case UIType.Settings:
                var data = uI_PlayerSettings.data;
                await pdManager.SaveData(data);
                cgManager.LoadSettings();
                break;
        }
    }
    public void LoadData(int index)
    {
        if (!InventoryParents[index].enabled) // 非アクティブ状態ならローカルデータの更新はしない
            return;

        Load(index);
    }

    /// <summary>
    /// ローカルデータの更新と対応するUIの更新を行う
    /// </summary>
    /// <param name="index"></param> <summary>
    ///
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public async void Load(int index)
    {
        string key = inventorySystem.GetListName(index); // 変換したデータからFirstIndexを取得しKeyを取得
        UniTask<List<ItemData>> task = pdManager.LoadData<List<ItemData>>(key); // 非同期ロード開始
        inventorySystem.GetInventoryData(index).Clear(); // ロード中に古いデータを整理
        var data = await task;
        Debug.Log($"data length : {data.Count} \nlocal length : {inventorySystem.GetInventoryData(index).Count}");
        inventorySystem.GetInventoryData(index).AddRange(data); // ロードが完了したデータを再設定
        InventoryParents[index].LoadSlot(inventorySystem.GetInventoryData(index)); // スロットを更新
        Debug.Log($"local length : {inventorySystem.GetInventoryData(index).Count}");
    }

    public void CLACombine()
    {
        cla.Combiner();
    }
}

[System.Serializable]
public enum UIType
{
    Null,
    PlayerProfile,
    Settings,
    Inventory
}