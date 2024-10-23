using System.Collections.Generic;
using UnityEngine;

public class UI_DACS_InventorySystem_SlotComponent : MonoBehaviour // Slotにアタッチ
{
    public UIGeneral uiGeneral; // SlotGeneratorが設定
    public ItemID itemID;
    public void OnClick()
    {
        uiGeneral.LoadInfoPanel(itemID);
    }
}
