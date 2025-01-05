using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

namespace CLAPlus.ClapTalk
{
    public class ClapTalk
    {
        static string OpenVCChannelName = "OpenVC";
        static string TestChannelName = "TestChannel";
        public static bool isMuted = false;
        public static bool UseToggleMute = false;
        static ChannelType joinnedChannel = ChannelType.empty;
        Channel3DProperties channel3DProperties = new(10, 1, 1, AudioFadeModel.InverseByDistance);

        public static async void JoinVoiceChatChannel(ChannelType channelType)
        {
            await LeaveChannnelAsync();
            switch ((int)channelType)
            {
                case 0:
                    await VivoxService.Instance.JoinGroupChannelAsync(OpenVCChannelName, ChatCapability.AudioOnly);
                    break;

                case 1:
                    await VivoxService.Instance.JoinEchoChannelAsync(TestChannelName, ChatCapability.AudioOnly);
                    break;

                default:
                    break;
            }
            joinnedChannel = channelType;
        }

        public static async void LoginToVivoxAsync()
        {
            string name = AuthenticationService.Instance.PlayerName;
            var lastDot = name.LastIndexOf('#');
            if (lastDot != -1)
                name = name[..lastDot]; // #以降を消す
            LoginOptions options = new()
            {
                DisplayName = name,
                EnableTTS = true
            };
            await VivoxService.Instance.LoginAsync(options);
        }

        public static async UniTask LeaveChannnelAsync()
        {
            switch ((int)joinnedChannel)
            {
                case 0:
                    await VivoxService.Instance.LeaveChannelAsync(OpenVCChannelName);
                    break;

                case 1:
                    await VivoxService.Instance.LeaveChannelAsync(TestChannelName);
                    break;

                default:
                    break;
            }
        }

        public static void OnInputDeviceChanged(int index)
        {
            var selectedDevice = VivoxService.Instance.AvailableInputDevices[index];
            VivoxService.Instance.SetActiveInputDeviceAsync(selectedDevice);
            Debug.Log("Input device changed to: " + selectedDevice.DeviceName);
        }

        public static void OnOutputDeviceChanged(int index)
        {
            var selectedDevice = VivoxService.Instance.AvailableOutputDevices[index];
            VivoxService.Instance.SetActiveOutputDeviceAsync(selectedDevice);
            Debug.Log("Output device changed to: " + selectedDevice.DeviceName);
        }

        public static void ChangeMute(InputAction.CallbackContext context)
        {
            if (UIGeneral.ActiveUIType == UIType.Chat)
                return;

            if (UseToggleMute)
            {
                if (!context.performed)
                    return;

                if (isMuted)
                {
                    VivoxService.Instance.UnmuteInputDevice();
                    isMuted = false;
                }
                else
                {
                    VivoxService.Instance.MuteInputDevice();
                    isMuted = true;
                }
            }
            else
            {
                if (context.started)
                {
                    VivoxService.Instance.MuteInputDevice();
                    isMuted = true;
                }
                else if (context.canceled)
                {
                    VivoxService.Instance.UnmuteInputDevice();
                    isMuted = false;
                }
            }
        }
    }
    public enum ChannelType
    {
        empty = -1,
        OpenVC = 0,
        TestChannel = 1
    }
}