using UnityEngine;
using TMPro;
using Unity.Services.Vivox;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CLAPlus.ClapTalk
{
    public class UI_ClapTalk : MonoBehaviour
    {
        [SerializeField] TMP_Dropdown inputDropdown;
        [SerializeField] TMP_Dropdown outputDropdown;
        [SerializeField] Image MuteImage;

        void OnEnable()
        {
            LoadDropdowns();
        }

        void Awake()
        {
            UIGeneral.action += SyncDataToUI;
        }
        public void SyncDataToUI()
        {
            inputDropdown.value = PlayerDataManager.PlayerSettingsData.InputDeviceIndex;
            ClapTalk.OnInputDeviceChanged(inputDropdown.value);
            outputDropdown.value = PlayerDataManager.PlayerSettingsData.OutputDeviceIndex;
            ClapTalk.OnOutputDeviceChanged(outputDropdown.value);
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

        public void OnInputDeviceChanged(int index)
        {
            ClapTalk.OnInputDeviceChanged(index);
            PlayerDataManager.PlayerSettingsData.InputDeviceIndex = index;
        }
        public void OnOutputDeviceChanged(int index)
        {
            ClapTalk.OnOutputDeviceChanged(index);
            PlayerDataManager.PlayerSettingsData.OutputDeviceIndex = index;
        }
        public void UseToggleMute(bool value) => ClapTalk.UseToggleMute = !value;
        public void JoinVoiceChatChannel(int index) => ClapTalk.JoinVoiceChatChannel((ChannelType)index);
        public void Mute(InputAction.CallbackContext context)
        {
            ClapTalk.ChangeMute(context);
            MuteImage.gameObject.SetActive(ClapTalk.isMuted);
        }
    }
}
