using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class UI_SlotLoader : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] int GridSideMax, SlotSize;
    public UIGeneral uiGeneral;
    [SerializeField] ItemDataBase itemDataBase;
    //[SerializeField] UIType uiType = UIType.Null;
    [SerializeField] int IndexToReference;

    public void Setup()
    {
        uiGeneral = UIGeneral.Instance;
        uiGeneral.InventoryParents[IndexToReference] = this;
    }
    void OnEnable()
    {
        uiGeneral.Load(IndexToReference);
    }

    public void LoadSlot(ReadOnlySpan<ItemData> InvData) // UIGeneralがデータの更新またはこのUIが有効化されたときに呼び出す
    {
        ReadOnlySpan<Transform> slots = uiGeneral.GetSlots(InvData.Length);
        rectTransform.sizeDelta = new Vector2(0, SlotSize * (slots.Length / GridSideMax));

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
