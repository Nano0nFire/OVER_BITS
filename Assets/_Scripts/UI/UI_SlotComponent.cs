using UnityEngine;
using UnityEngine.UI;

public class UI_SlotComponent : MonoBehaviour
{
    public UIGeneral uiGeneral; // SlotGeneratorが設定
    public ItemData itemData;
    [SerializeField] Image itemImage;
    [SerializeField] Image rarityImage;
    public void OnClick()
    {
        uiGeneral.LoadInfoPanel(itemData);
    }

    public void Setup(ItemDataConfigs data)
    {
        itemImage.sprite = data.ItemImage;
        rarityImage.color = RarityToColor.ToColor(data.ItemRarity);
    }
}
