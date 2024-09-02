using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;


public class CLAPSlotGenerator : MonoBehaviour
{
    // [SerializeField] GameObject BaseSlotObj;
    // [SerializeField] CLAPartsLists BaseList;
    // [SerializeField] int SelectedListNum; // CLAPartsListのBasePartsを-1として数えた値
    // [SerializeField] string DefText;
    // [SerializeField] Sprite DefImage;
    // [SerializeField] List<GameObject> Slots;
    // int i = 0;

    // public void EnableUI() // UIGeneralがCanvas読み込み時に呼び出し
    // {
    //     List<GameObject> list = BaseList.GetList(SelectedListNum);
    //     i = 0;

    //     foreach (GameObject item in list)
    //     {
    //         GameObject slot = Instantiate(BaseSlotObj);
    //         slot.SetActive(true);
    //         Slots.Add(slot);
    //         slot.GetComponent<RectTransform>().SetParent(transform, false);
    //         CLAPUIConector conector = slot.GetComponent<CLAPUIConector>();
    //         conector.ListType = SelectedListNum;
    //         conector.DataListNum = i;

    //         if (Slots.Count % 9 == 0) // 9はGridLayoutGroupを使用した時に一列に配置される最大の数のこと
    //             GetComponent<RectTransform>().sizeDelta = new Vector2(0, 143 * (Slots.Count / 9)); // 143はGridLayoutGroupコンポーネントのセルサイズと上の余白の和のこと(セルサイズ140 + 上の余白3)

    //         if (item == null) return;
    //         if (DefText != null)
    //         {
    //             TextMeshProUGUI textComp = slot.GetComponentInChildren<TextMeshProUGUI>();

    //             int index = item.name.IndexOf("_");
    //             if (item.name == null || index == -1) textComp.text = DefText;
    //             else textComp.text = item.name.Substring(index + 1);
    //         }

    //         if (DefImage != null)
    //         {
    //             RectTransform[] children = slot.GetComponentsInChildren<RectTransform>();
    //             foreach (RectTransform child in children)
    //             {
    //                 if (child.name == "Image")
    //                     child.GetComponent<Image>().sprite = DefImage;
    //             }
    //         }

    //         i++;
    //     }
    // }

    // public void DisableUI() // UIGeneralがCanvasが非表示にされたときに呼び出し
    // {
    //     foreach (GameObject item in Slots)
    //         Destroy(item);

    //     Slots.Clear();
    // }
}
