using System;
using UnityEngine;
using UnityEngine.UI;
using DACS.Inventory;
using System.Collections.Generic;

public class UI_Hotbar : MonoBehaviour
{
    public ItemData SelectedItem
    {
        get => _SelectedItem;
        set
        {
            SwitchSelecterState();
            _SelectedItem = value;
        }
    }
    ItemData _SelectedItem;
    [HideInInspector] public HotbarSystem hotbarSystem;
    [SerializeField] ItemDataBase itemDataBase;
    [SerializeField] GameObject PriSelecter;
    [SerializeField] GameObject SecSelecter;
    [SerializeField] GameObject GraSelecter;
    [SerializeField] GameObject Sub0Selecter;
    [SerializeField] GameObject Sub1Selecter;
    [SerializeField] Image PriImage;
    [SerializeField] Image SecImage;
    [SerializeField] Image GraImage;
    [SerializeField] Image Sub0Image;
    [SerializeField] Image Sub1Image;
    [SerializeField] Image PriFlameImage;
    [SerializeField] Image SecFlameImage;
    [SerializeField] Image GraFlameImage;
    [SerializeField] Image Sub0FlameImage;
    [SerializeField] Image Sub1FlameImage;

    void SwitchSelecterState()
    {
        PriSelecter.SetActive(SelectedItem.FirstIndex switch
        {
            0 => true,
            1 => true,
            2 => true,
            3 => true,
            4 => true,
            5 => true,
            8 => true,
            9 => true,
            10 => true,
            11 => true,
            12 => true,
            13 => true,
            14 => true,
            15 => true,
            16 => true,
            17 => true,
            18 => true,
            _ => false,
        });
        SecSelecter.SetActive(SelectedItem.FirstIndex switch
        {
            0 => true,
            1 => true,
            2 => true,
            3 => true,
            4 => true,
            5 => true,
            8 => true,
            9 => true,
            10 => true,
            11 => true,
            12 => true,
            13 => true,
            14 => true,
            15 => true,
            16 => true,
            17 => true,
            18 => true,
            _ => false,
        });
        GraSelecter.SetActive(SelectedItem.FirstIndex switch
        {
            0 => true,
            7 => true,
            _ => false,
        });
        Sub0Selecter.SetActive(SelectedItem.FirstIndex switch
        {
            0 => true,
            7 => true,
            32 => true,
            _ => false,
        });
        Sub1Selecter.SetActive(SelectedItem.FirstIndex switch
        {
            0 => true,
            7 => true,
            32 => true,
            _ => false,
        });
    }

    public void LoadHotbar()
    {
        List<ItemData> itemDatas = InventorySystem.HotbarData;
        for (int i = 0; i < 5; i ++)
        {
            SelectedItem = itemDatas[i];
            EquipItem(i);
        }
    }

    public void EquipItem(int slotNumber)
    {
        InventorySystem.ChangeHotbar(slotNumber, SelectedItem);
        var image = SelectedItem.FirstIndex >= 0 ? itemDataBase.GetItem(SelectedItem.FirstIndex, SelectedItem.SecondIndex).ItemImage : null;
        hotbarSystem.SpawnItemObjectLocal(SelectedItem.FirstIndex, SelectedItem.SecondIndex, slotNumber);
        switch (slotNumber)
        {
            case 0:
                PriImage.sprite = image;
                break;

            case 1:
                SecImage.sprite = image;
                break;

            case 2:
                GraImage.sprite = image;
                break;

            case 3:
                Sub0Image.sprite = image;
                break;

            case 4:
                Sub1Image.sprite = image;
                break;
        }
    }

    public void SelectHotbarSlot(int index)
    {
        PriFlameImage.color = Color.black;
        SecFlameImage.color = Color.black;
        GraFlameImage.color = Color.black;
        Sub0FlameImage.color = Color.black;
        Sub1FlameImage.color = Color.black;

        switch (index)
        {
            case 0:
                PriFlameImage.color = Color.white;
                break;

            case 1:
                SecFlameImage.color = Color.white;
                break;

            case 2:
                GraFlameImage.color = Color.white;
                break;

            case 3:
                Sub0FlameImage.color = Color.white;
                break;

            case 4:
                Sub1FlameImage.color = Color.white;
                break;
        };
    }

    public void ConsumeItem()
    {

    }
}
