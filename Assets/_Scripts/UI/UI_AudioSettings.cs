using TMPro;
using Unity.Mathematics;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class UI_AudioSettings : MonoBehaviour
{
    [SerializeField] AudioSource BGM;
    [SerializeField] Slider BGMVolumeSlider;
    [SerializeField] TMP_InputField BGMVolumeInp;
    [SerializeField] Slider CLAPInputVolumeSlider;
    [SerializeField] TMP_InputField CLAPInputVolumeInp;
    [SerializeField] Slider CLAPOutputVolumeSlider;
    [SerializeField] TMP_InputField CLAPOutputVolumeInp;

    void Awake()
    {
        BGMVolumeSlider.onValueChanged.AddListener( delegate {OnBGMVolumeChange((int)BGMVolumeSlider.value);} );
        CLAPInputVolumeSlider.onValueChanged.AddListener( delegate {OnOutputVolumeChange((int)CLAPInputVolumeSlider.value);} );
        CLAPOutputVolumeSlider.onValueChanged.AddListener( delegate {OnInputVolumeChange((int)CLAPOutputVolumeSlider.value);} );
        BGMVolumeInp.onEndEdit.AddListener( delegate {OnBGMVolumeChange(math.clamp(int.Parse(BGMVolumeInp.text), 0, 100), false);} );
        CLAPInputVolumeInp.onEndEdit.AddListener( delegate {OnOutputVolumeChange(math.clamp(int.Parse(CLAPInputVolumeInp.text), 0, 100), false);} );
        CLAPOutputVolumeInp.onEndEdit.AddListener( delegate {OnInputVolumeChange(math.clamp(int.Parse(CLAPOutputVolumeInp.text), 0, 100), false);} );

        BGM.Play();
    }

    void OnBGMVolumeChange(int value, bool isSlider = true)
    {
        BGM.volume = (float)value / 100;
        if (isSlider)
        {
            BGMVolumeInp.text = value.ToString();
        }
        else
        {
            BGMVolumeSlider.value = value;
            BGMVolumeInp.text = value.ToString();
        }
    }

    void OnOutputVolumeChange(int value, bool isSlider = true)
    {
        VivoxService.Instance.SetOutputDeviceVolume(value - 50);
        if (isSlider)
        {
            CLAPInputVolumeInp.text = value.ToString();
        }
        else
        {
            CLAPInputVolumeSlider.value = value;
            CLAPInputVolumeInp.text = value.ToString();
        }
    }

    void OnInputVolumeChange(int value, bool isSlider = true)
    {
        VivoxService.Instance.SetInputDeviceVolume(value - 50);
        if (isSlider)
        {
            CLAPOutputVolumeInp.text = value.ToString();
        }
        else
        {
            CLAPOutputVolumeSlider.value = value;
            CLAPOutputVolumeInp.text = value.ToString();
        }
    }
}
