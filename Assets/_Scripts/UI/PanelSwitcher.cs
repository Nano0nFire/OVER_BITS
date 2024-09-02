using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    [SerializeField] List<RectTransform> panelList;
    int activePanelNum = 0;
    [SerializeField] Button DefObject;
    [SerializeField] Color DefPanelColor;
    [SerializeField] Color DefTextColor;
    [SerializeField] Color ActPanelColor;
    [SerializeField] Color ActTextColor;
    public void EnableUI() // UIGeneralがCanvas読み込み時に呼び出し
    {
        PanelChange(DefObject, ActTextColor, ActPanelColor);
    }

    public void OnPushButtonP(Button Obj) // ボタンの視覚的変化
    {
        PanelChange(DefObject, DefTextColor, DefPanelColor);
        DefObject = Obj;
        PanelChange(DefObject, ActTextColor, ActPanelColor);
    }
    public void OnPushButtonS(int num) // 表示させるパネルを変更
    {
        if (num >= panelList.Count) return;
        if(panelList[activePanelNum] == null) Debug.LogWarning(panelList[activePanelNum].name);
        panelList[activePanelNum].gameObject.SetActive(false);

        activePanelNum = num;

        panelList[activePanelNum].gameObject.SetActive(true);
    }
    private void PanelChange(Button obj, Color TextColor, Color PanelColor)
    {
        if (obj == null)
        {
            Debug.Log(gameObject.name + "missing DefObj");
            return;
        }
        RectTransform[] children = obj.GetComponentsInChildren<RectTransform>();

        foreach (RectTransform childObj in children)
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
