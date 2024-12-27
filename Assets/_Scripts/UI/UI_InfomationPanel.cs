using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_InfomationPanel : MonoBehaviour
{
    [SerializeField] UI_Hotbar uiHotbar;
    [SerializeField] ItemDataBase itemDataBase;

    [Header("Infomation Panel Settings")]
    [SerializeField] TextMeshProUGUI txt_ItemName;
    [SerializeField] TextMeshProUGUI txt_ItemInfo;
    [SerializeField] Image spr_ItemImage;
    public void LoadInfoPanel(ItemData itemID)
    {
        uiHotbar.SelectedItem = itemID;
        var data = itemDataBase.GetItem(itemID.FirstIndex, itemID.SecondIndex);

        txt_ItemName.text = data.ItemName;
        txt_ItemInfo.text = data.ItemInfo;
        spr_ItemImage.sprite = data.ItemImage;
    }
}
