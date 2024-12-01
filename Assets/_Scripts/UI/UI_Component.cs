using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_Component : MonoBehaviour
{
    [HideInInspector] public UIGeneral uiGeneral;
    [HideInInspector] public int panelID; // このパネルの登録ID(UIGeneralが設定する)
    [HideInInspector] public ItemDataBase itemDataBase;
    public UIType uiType{get => _UIType;}
    [SerializeField] UIType _UIType = UIType.Null;
    [SerializeField] bool hasSlot = false;
    [SerializeField] int IndexToReference;

    public void Setup()
    {
        switch (uiType)
        {
            case UIType.Inventory:
                if (hasSlot)
                    uiGeneral.InventoryParents[IndexToReference] = this;
                break;
        }
    }
    void OnEnable()
    {
        if (uiGeneral != null)
            uiGeneral.ActivePanelIndex = panelID;

        if (hasSlot)
            uiGeneral.Load(IndexToReference);
    }

    public void LoadSlot(List<ItemData> InvData) // UIGeneralがデータの更新またはこのUIが有効化されたときに呼び出す
    {
        Transform[] slots = uiGeneral.GetSlots(InvData.Count).ToArray();
        int i = 0;
        foreach (var slot in slots)
        {
            slot.SetParent(transform); // スロットの親を再設定
            var data = InvData[i];
            var component = slot.GetComponent<UI_SlotComponent>();
            component.itemData = data;
            component.Setup(itemDataBase.GetItem(data.FirstIndex, data.SecondIndex));
            i ++;
        }
    }
}
