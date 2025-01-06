using System.Collections.Generic;
using UnityEngine;

public class UI_Component : MonoBehaviour
{
    [HideInInspector] public UIGeneral uiGeneral;
    [HideInInspector] public int panelID; // このパネルの登録ID(UIGeneralが設定する)
    public UIType uiType{get => _UIType;}
    [SerializeField] UIType _UIType = UIType.Null;

    void OnEnable()
    {
        if (uiGeneral != null)
            UIGeneral.ActivePanelIndex = panelID;
    }
}
