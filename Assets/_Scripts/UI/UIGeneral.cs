using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using CLAPlus;

public class UIGeneral : MonoBehaviour
{
    ClientGeneralManager cgManager;
    CustomLifeAvatar cla;
    [HideInInspector] public UI_PlayerSettings uI_PlayerSettings;
    PlayerDataManager pdManager;
    [SerializeField] ItemDataBase itemDataBase;
    [SerializeField] GameObject ChatSpace;
    public UI_Hotbar uiHotbar;
    [SerializeField] UI_InfomationPanel infomationPanelController;
    [HideInInspector] public DACS_InventorySystem invSystem; // ClientGeneralManagerが設定
    [SerializeField] GameObject SlotPrefab;
    UI_Component[] UIComponents;
    public UI_SlotLoader[] InventoryParents;
    bool[] InventoryIsUpdated;
    List<Transform> SlotXforms = new();
    int activatedSlotIndex = 0;
    public int ActivePanelIndex;

    public void Setup(ClientGeneralManager cgManager)
    {
        this.cgManager = cgManager;
        pdManager = cgManager.pdManager;
        invSystem = cgManager.GetComponent<DACS_InventorySystem>();
        uiHotbar.inventorySystem = invSystem;
        uiHotbar.hotbarSystem = cgManager.GetComponent<DACS_HotbarSystem>();
        pdManager.OnItemAdded += Load;
        cla = cgManager.GetComponent<CustomLifeAvatar>();

        UIComponents = GetComponentsInChildren<UI_Component>(true);
        int max = itemDataBase.ItemListCount;
        InventoryParents = new UI_SlotLoader[max];
        int i = 0;
        foreach (var component in UIComponents)
        {
            component.uiGeneral = this;
            component.panelID = i;
            i ++;
            component.Setup();
        }
        InventoryIsUpdated = new bool[max];
        for (i = 0; i < max; i ++)
        {
            InventoryIsUpdated[i] = true;
        }
    }

    public void UIexit(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        gameObject.SetActive(false);
        cgManager.UseInput = true;
        ChatSpace.SetActive(false);
    }

    public void ShowHideMenu(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        gameObject.SetActive(!gameObject.activeSelf);
        cgManager.UseInput = !gameObject.activeSelf;
        cgManager.CleatInput();
        ChatSpace.SetActive(false);
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
            {
                var newSlot = Instantiate(SlotPrefab).transform;
                newSlot.GetComponent<UI_SlotComponent>().uiInfo = infomationPanelController;
                SlotXforms.Add(newSlot);
            }
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
            var newSlot = Instantiate(SlotPrefab).transform;
            newSlot.GetComponent<UI_SlotComponent>().uiInfo = infomationPanelController;
            SlotXforms.Add(newSlot);
        }
        return SlotXforms[activatedSlotIndex+1];
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

    /// <summary>
    /// ローカルデータの更新と対応するUIの更新を行う
    /// </summary>
    /// <param name="forceLoad">true : 強制的に読み込む<br />false(デフォルト) : データに更新があった場合のみ読み込む</param>
    /// <param name="index">読み込み先のインデックス</param>
    /// <returns></returns>
    public void Load(int index)
    {
        if (!InventoryParents[index].enabled) // 非アクティブ状態なら更新はしない
            return;
        InventoryParents[index].LoadSlot(invSystem.GetInventoryData(index)); // スロットを更新
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