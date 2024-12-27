using System.Collections.Generic;
using UnityEngine;

public class UI_Component : MonoBehaviour
{
    [HideInInspector] public UIGeneral uiGeneral;
    [HideInInspector] public int panelID; // このパネルの登録ID(UIGeneralが設定する)
    public UIType uiType{get => _UIType;}
    [SerializeField] UIType _UIType = UIType.Null;

    public void Setup()
    {
        GetComponent<UI_SlotLoader>()?.Setup(uiGeneral);
    }
    void OnEnable()
    {
        if (uiGeneral != null)
            uiGeneral.ActivePanelIndex = panelID;
    }
}
