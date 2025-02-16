using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using CLAPlus;
using DACS.Inventory;
using CLAPlus.ClapChat;
using System;

public class UIGeneral : MonoBehaviour
{
    ClientGeneralManager cgManager;
    CustomLifeAvatar cla;
    public static Action action;
    [HideInInspector] public UI_PlayerSettings uI_PlayerSettings;
    [SerializeField] ItemDataBase itemDataBase;
    [SerializeField] UI_InfomationPanel infomationPanelController;
    [HideInInspector] public InventorySystem invSystem; // ClientGeneralManagerが設定
    [SerializeField] GameObject SlotPrefab;
    static UI_Component[] UIComponents;
    public UI_SlotLoader[] InventoryParents;
    bool[] InventoryIsUpdated;
    List<Transform> SlotXforms = new();
    int activatedSlotIndex = 0;
    public static ItemData SelectedItem;
    public static int ActivePanelIndex;
    public static UIType ActiveUIType
    {
        get => UIComponents[ActivePanelIndex].uiType;
    }
    // シングルトンインスタンス
    private static UIGeneral instance;

    // インスタンスへのプロパティ
    public static UIGeneral Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<UIGeneral>();

                if (instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(PlayerDataManager).Name);
                    instance = singletonObject.AddComponent<UIGeneral>();
                }
            }
            return instance;
        }
    }

    public void Setup()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject); // 重複するインスタンスを破棄
        }

        this.cgManager = ClientGeneralManager.Instance;
        PlayerDataManager.OnItemAdded += Load;
        cla = ClientGeneralManager.customLifeAvatar;

        UIComponents = FindObjectsByType<UI_Component>(FindObjectsSortMode.None);
        int max = itemDataBase.ItemListCount;
        InventoryParents = new UI_SlotLoader[max];
        int i = 0;
        foreach (var component in UIComponents)
        {
            component.uiGeneral = this;
            component.panelID = i;
            i ++;
        }
        foreach (var component in GetComponentsInChildren<UI_SlotLoader>(true))
            component.Setup();
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
        UI_ClapChat.CloseChatSpace();
    }

    public void ShowHideMenu(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        gameObject.SetActive(!gameObject.activeSelf);
        cgManager.UseInput = !gameObject.activeSelf;
        cgManager.ClearInput();
        UI_ClapChat.CloseChatSpace();
    }

    public static void SyncDataToUI() => action.Invoke();

    /// <summary>
    /// 引数分のSlotをリストで返す <br />
    /// 不足分は自動で生成して補う
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public ReadOnlySpan<Transform> GetSlots(int amount)
    {
        if (0 >= amount)
            return new();

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
        return SlotXforms.GetRange(0, amount).ToArray();
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
                await PlayerDataManager.SaveData(PlayerDataManager.PlayerSettingsData);
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
        InventoryParents[index].LoadSlot(InventorySystem.GetInventoryData(index).ToArray()); // スロットを更新
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
    Inventory,
    Chat,
    Loading
}