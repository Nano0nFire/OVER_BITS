using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubPanelSwitcher : MonoBehaviour
{
    public GameObject ActivePanel{get; private set;}
    [SerializeField] GameObject DefPanel;
    [SerializeField] [NonReorderable] List<GameObject> PanelList;

    public void SwitchActive(int i)
    {
        ActivePanel.SetActive(false);
        ActivePanel = PanelList[i];
        ActivePanel.SetActive(true);
    }
    public void EnableUI()
    {
        DefPanel.SetActive(true);
    }
    public void DisableUI()
    {
        DefPanel.SetActive(false);
    }
}
