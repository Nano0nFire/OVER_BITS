using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SlotComponent : MonoBehaviour
{
    [HideInInspector] public UI_InfomationPanel uiInfo; // UIGeneralが設定
    [HideInInspector] public ItemData itemData;
    [SerializeField] RectTransform rectTransform;
    [SerializeField] Image itemImage;
    [SerializeField] Image rarityImage;
    [SerializeField] TextMeshProUGUI itemCount;
    public void OnClick()
    {
        uiInfo.LoadInfoPanel(itemData);
        UIGeneral.SelectedItem = itemData;
    }

    public void Setup(ItemDataConfigs data)
    {
        rectTransform.localScale = Vector3.one;
        itemImage.sprite = data.ItemImage;
        rarityImage.color = RarityToColor.ToColor(data.ItemRarity);
        itemCount.text = itemData.Amount > 1 ? $"{itemData.Amount}" : " ";
    }
}
