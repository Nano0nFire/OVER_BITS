using UnityEngine;

public class UI_ItemEquip : MonoBehaviour
{
    [SerializeField] GameObject HotbarSelecter;
    [SerializeField] GameObject ColorSelecter;
    public void OnEquip()
    {
        switch (UIGeneral.SelectedItem.FirstIndex)
        {
            case 20:
            case 21:
            case 22:
            case 23:
            case 24:
                ColorSelecter.SetActive(true);
                break;

            default:
                HotbarSelecter.SetActive(true);
                break;
        }
    }
}
