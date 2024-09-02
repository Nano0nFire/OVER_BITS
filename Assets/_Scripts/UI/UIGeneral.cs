using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class UIGeneral : MonoBehaviour
{
    [SerializeField] GeneralManager generalManager;
    [SerializeField] List<PanelSwitcher> psList;
    [SerializeField] List<SlotGenerator> SGList;

    void OnEnable()
    {
        foreach (var item in psList) item.EnableUI();
        foreach (var item in SGList) item.EnableUI();
    }
    void OnDisable()
    {
        foreach (var item in SGList) item.DisableUI();
    }

    public void UIexit(GameObject Menu)
    {
        Menu.SetActive(!Menu.activeSelf);

        if (Menu == generalManager.MainMenu)
        {
            generalManager.IsOpeningUI = Menu.activeSelf;
        }
    }
}
