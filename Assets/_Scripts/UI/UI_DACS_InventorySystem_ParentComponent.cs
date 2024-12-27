// using System.Collections.Generic;
// using UnityEngine;

// public class UI_DACS_InventorySystem_ParentComponent : MonoBehaviour
// {
//     [HideInInspector] public UIGeneral uiGeneral; // SlotGeneratorが設定
//     public int FirstIndex;

//     void OnEnable()
//     {
//         uiGeneral.Load(FirstIndex);
//     }

//     public void UpdateSlot(List<ItemData> InvData)
//     {
//         Transform[] slots = uiGeneral.GetSlots(InvData.Count).ToArray();
//     }
// }
