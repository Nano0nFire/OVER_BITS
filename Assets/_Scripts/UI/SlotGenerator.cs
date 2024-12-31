// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
// public class SlotGenerator : MonoBehaviour // UI-SlotParentにアタッチ
// {

//     [SerializeField] UIGeneral uiGeneral;
//     [SerializeField] GameObject BaseSlotPrefab;
//     public List<ItemData> itemIDsList;
//     [SerializeField] int GridSideMax = 9;
//     [SerializeField] int SellSize = 143;
//     [SerializeField] ItemDataBase itemDataBase;
//     [SerializeField] int ListIndex; // ItemDataBaseのインデックス(一番上を0としたときの数)
//     [SerializeField] string DefText;
//     [SerializeField] Sprite DefImage;
//     [SerializeField, Tooltip("0 = デフォルト, 1 = CLAモード")]
//     int Mode = 0;
//     public CustomLifeAvatar cla;
//     [SerializeField] int claIndex;
//     readonly List<GameObject> Slots = new();
//     int i = 0;

//     public void EnableUI() // UIGeneralがCanvas読み込み時に呼び出し
//     {
//         i = 0;

//         foreach (ItemData itemID in itemIDsList)
//         {
//             GameObject slot = Instantiate(BaseSlotPrefab); // スロット生成
//             slot.SetActive(true); // 有効化
//             Slots.Add(slot); // スロットリストに追加
//             slot.GetComponent<RectTransform>().SetParent(transform, false); // 生成したスロットの親を自分に設定
//             var itemData = itemDataBase.GetItem(itemID.FirstIndex, itemID.SecondIndex); // アイテムデータの取得
//             RectTransform[] children = slot.GetComponentsInChildren<RectTransform>();
//             foreach (RectTransform child in children)
//             {
//                 switch (child.name)
//                 {
//                     case "Text":
//                         if (itemData.ItemName == null)
//                             child.GetComponent<TextMeshProUGUI>().text = DefText;
//                         else
//                             child.GetComponent<TextMeshProUGUI>().text = itemData.ItemName;
//                         break;

//                     case "Image":
//                         if (itemData.ItemImage == null)
//                             child.GetComponent<Image>().sprite = DefImage;
//                         else
//                             child.GetComponent<Image>().sprite = itemData.ItemImage;
//                         break;

//                     default:
//                         break;
//                 }
//             }

//             if (Slots.Count % GridSideMax == 0) // 親パネルの大きさを調整
//                 GetComponent<RectTransform>().sizeDelta = new Vector2(0, SellSize * (Slots.Count / GridSideMax));

//             switch (Mode)
//             {
//                 case 1:
//                     CLAPOption(slot);
//                     break;
//             }

//             InvSystemSetup(slot, itemID);

//             i++;
//         }
//     }

//     public void DisableUI() // UIGeneralがCanvasが非表示にされたときに呼び出し
//     {
//         foreach (GameObject item in Slots)
//             Destroy(item);

//         Slots.Clear();
//     }

//     void InvSystemSetup(GameObject slot, ItemData itemID)
//     {
//         var invSystem = slot.AddComponent<UI_InventorySystem_SlotComponent>();
//         invSystem.uiGeneral = uiGeneral;
//         invSystem.itemID = itemID;
//     }

//     void CLAPOption(GameObject slot)
//     {
//         UI_CLA_SlotComponent ui_Component = slot.AddComponent<UI_CLA_SlotComponent>();
//         ui_Component.cla = cla;
//         ui_Component.listIndex = claIndex;
//         ui_Component.DataListNum = i;

//         slot.GetComponent<Button>().onClick.AddListener(ui_Component.OnClick);
//     }
// }

