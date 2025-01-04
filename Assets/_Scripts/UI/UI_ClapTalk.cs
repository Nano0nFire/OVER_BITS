using UnityEngine;
using TMPro;
using Unity.Services.Vivox;

namespace CLAPlus.ClapTalk
{
    public class UI_ClapTalk : MonoBehaviour
    {
        [SerializeField] TMP_Dropdown inputDropdown;
        [SerializeField] TMP_Dropdown outputDropdown;

        void OnEnable()
        {
            LoadDropdowns();
        }

        void LoadDropdowns()
        {
            // 入力デバイスのドロップダウンを設定
            inputDropdown.ClearOptions();
            var inputDevices = VivoxService.Instance.AvailableInputDevices;
            foreach (var device in inputDevices)
            {
                inputDropdown.options.Add(new TMP_Dropdown.OptionData(device.DeviceName));
            }

            // 出力デバイスのドロップダウンを設定
            outputDropdown.ClearOptions();
            var outputDevices = VivoxService.Instance.AvailableOutputDevices;
            foreach (var device in outputDevices)
            {
                outputDropdown.options.Add(new TMP_Dropdown.OptionData(device.DeviceName));
            }
        }

        public void OnInputDeviceChanged(int index) => ClapTalk.OnInputDeviceChanged(index);

        public void OnOutputDeviceChanged(int index) => ClapTalk.OnOutputDeviceChanged(index);

        public void UseToggleMute(bool value) => ClapTalk.UseToggleMute = !value;
    }
}
