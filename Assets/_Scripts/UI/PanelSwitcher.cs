using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    [SerializeField] List<GameObject> panelList;
    [SerializeField] int mode;
    int activePanelNum = 0;
    [SerializeField] Image Panel;
    [SerializeField] TextMeshProUGUI Text;
    [SerializeField] GameObject Object;
    [SerializeField] Color DefPanelColor;
    [SerializeField] Color DefTextColor;
    [SerializeField] Color ActPanelColor;
    [SerializeField] Color ActTextColor;

    public void OnPushButtonP(GameObject Obj)
    {
        if (mode == 0)
        {
            InvChange(Object, DefTextColor, DefPanelColor);
            Object = Obj;
            InvChange(Object, ActTextColor, ActPanelColor);
        }
        else NormalChange(Obj);

    }
    public void OnPushButtonS(int num)
    {
        if (num >= panelList.Count) return;

        panelList[activePanelNum].SetActive(false);

        activePanelNum = num;

        panelList[activePanelNum].SetActive(true);
    }

    private void NormalChange(GameObject obj)
    {
        if (Panel != null)
        {
            Image panel = obj.GetComponentInChildren<Image>();
            Panel.color = DefPanelColor;
            Panel = panel;
            Panel.color = ActPanelColor;
        }

        if (Text != null)
        {
            TextMeshProUGUI text = obj.GetComponentInChildren<TextMeshProUGUI>();
            Text.color = DefTextColor;
            Text = text;
            text.color = ActTextColor;
        }
    }
    private void InvChange(GameObject obj, Color TextColor, Color PanelColor)
    {
        Transform[] children = obj.GetComponentsInChildren<Transform>();

        foreach (Transform childObj in children)
        {
            switch (childObj.gameObject.name)
            {
                case "BackImage":
                    childObj.GetComponent<Image>().color = PanelColor;
                    break;

                case "Text":
                    childObj.GetComponent<TextMeshProUGUI>().color = TextColor;
                    break;

                case "Image":
                    childObj.GetComponent<Image>().color = TextColor;
                    break;
            }
        }
    }
}
