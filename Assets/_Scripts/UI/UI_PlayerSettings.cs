using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UI_PlayerSettings : MonoBehaviour
{
    bool Loaded = false;
    [SerializeField] Slider Sli_HzCameraSens;
    [SerializeField] TMP_InputField Inp_HzCameraSens;
    [SerializeField] Slider Sli_VCameraSens;
    [SerializeField] TMP_InputField Inp_VCameraSens;

    void Awake()
    {
        UIGeneral.action += SyncDataToUI;
    }
    public void SyncDataToUI()
    {
        Inp_HzCameraSens.text = PlayerDataManager.PlayerSettingsData.HzCameraSens.ToString();
        Sli_HzCameraSens.value = PlayerDataManager.PlayerSettingsData.HzCameraSens;
        Inp_VCameraSens.text = PlayerDataManager.PlayerSettingsData.VCameraSens.ToString();
        Sli_VCameraSens.value = PlayerDataManager.PlayerSettingsData.VCameraSens;

        Loaded = true;
    }

    public void HzCameraSens(bool IsSlider)
    {
        if (!Loaded)
            return;

        if (IsSlider)
        {
            PlayerDataManager.PlayerSettingsData.HzCameraSens = Sli_HzCameraSens.value;
            Inp_HzCameraSens.text = PlayerDataManager.PlayerSettingsData.HzCameraSens.ToString();
        }
        else
        {
            PlayerDataManager.PlayerSettingsData.HzCameraSens = float.Parse(Inp_HzCameraSens.text);
            Sli_HzCameraSens.value = PlayerDataManager.PlayerSettingsData.HzCameraSens;
        }
    }
    public void VCameraSens(bool IsSlider)
    {
        if (!Loaded)
            return;

        if (IsSlider)
        {
            PlayerDataManager.PlayerSettingsData.VCameraSens = Sli_VCameraSens.value;
            Inp_VCameraSens.text = PlayerDataManager.PlayerSettingsData.VCameraSens.ToString();
        }
        else
        {
            PlayerDataManager.PlayerSettingsData.VCameraSens = float.Parse(Inp_VCameraSens.text);
            Sli_VCameraSens.value = PlayerDataManager.PlayerSettingsData.VCameraSens;
        }
    }
}
