using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PlayerSettings : MonoBehaviour
{
    public PlayerDataManager pdManager;
    public SettingsData data;
    bool Loaded = false;
    [SerializeField] Slider Sli_HzCameraSens;
    [SerializeField] TMP_InputField Inp_HzCameraSens;
    [SerializeField] Slider Sli_VCameraSens;
    [SerializeField] TMP_InputField Inp_VCameraSens;

    public async void Setup(ClientGeneralManager cgManager)
    {
        pdManager = cgManager.pdManager;

        data = await pdManager.LoadData<SettingsData>();
        Inp_HzCameraSens.text = data.HzCameraSens.ToString();
        Sli_HzCameraSens.value = data.HzCameraSens;
        Inp_VCameraSens.text = data.VCameraSens.ToString();
        Sli_VCameraSens.value = data.VCameraSens;

        Loaded = true;
    }
    public void HzCameraSens(bool IsSlider)
    {
        if (!Loaded)
            return;

        if (IsSlider)
        {
            data.HzCameraSens = Sli_HzCameraSens.value;
            Inp_HzCameraSens.text = data.HzCameraSens.ToString();
        }
        else
        {
            data.HzCameraSens = float.Parse(Inp_HzCameraSens.text);
            Sli_HzCameraSens.value = data.HzCameraSens;
        }
    }
    public void VCameraSens(bool IsSlider)
    {
        if (!Loaded)
            return;

        if (IsSlider)
        {
            data.VCameraSens = Sli_VCameraSens.value;
            Inp_VCameraSens.text = data.VCameraSens.ToString();
        }
        else
        {
            data.VCameraSens = float.Parse(Inp_VCameraSens.text);
            Sli_VCameraSens.value = data.VCameraSens;
        }
    }
}
