using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SlotGenerator : MonoBehaviour
{

    [SerializeField] GameObject BaseSlotObj;
    [SerializeField] int GridSideMax = 9;
    [SerializeField] int SellSize = 143;
    [SerializeField] ItemDataBase list;
    [SerializeField] int ListIndex; // ItemDataBaseのインデックス(一番上を0としたときの数)
    [SerializeField] string DefText;
    [SerializeField] Sprite DefImage;
    [SerializeField, Tooltip("0 = デフォルト, 1 = CLAモード")]
    int Mode = 0;
    public CustomLifeAvatar cla;
    [SerializeField] int claIndex;
    List<GameObject> Slots = new();
    int i = 0;

    public void EnableUI() // UIGeneralがCanvas読み込み時に呼び出し
    {
        i = 0;

        foreach (ItemDataConfigs itemData in list.GetList(ListIndex))
        {
            GameObject item = itemData.ItemModel;

            GameObject slot = Instantiate(BaseSlotObj); // スロット生成
            slot.SetActive(true); // 有効化
            Slots.Add(slot); // スロットリストに追加
            slot.GetComponent<RectTransform>().SetParent(transform, false); // 生成したスロットの親を自分に設定

            RectTransform[] children = slot.GetComponentsInChildren<RectTransform>();
            foreach (RectTransform child in children)
            {
                switch (child.name)
                {
                    case "Text":
                        if (itemData.ItemName == null) child.GetComponent<TextMeshProUGUI>().text = DefText;
                        else child.GetComponent<TextMeshProUGUI>().text = itemData.ItemName;
                        break;
                    case "Image":
                        if (itemData.ItemImage == null) child.GetComponent<Image>().sprite = DefImage;
                        else child.GetComponent<Image>().sprite = itemData.ItemImage;
                        break;
                }
            }

            if (Slots.Count % GridSideMax == 0) // 親パネルの大きさを調整
                GetComponent<RectTransform>().sizeDelta = new Vector2(0, SellSize * (Slots.Count / GridSideMax));

            switch (Mode)
            {
                case 1:
                    CLAPOption(slot);
                    break;
            }

            i++;
        }
    }

    public void DisableUI() // UIGeneralがCanvasが非表示にされたときに呼び出し
    {
        foreach (GameObject item in Slots)
            Destroy(item);

        Slots.Clear();
    }

    void CLAPOption(GameObject slot)
    {
        CLAPUIConector conector = slot.AddComponent<CLAPUIConector>();
        conector.cla = cla;
        conector.listIndex = claIndex;
        conector.DataListNum = i;

        slot.GetComponent<Button>().onClick.AddListener(conector.SetCLAP);
    }
}

