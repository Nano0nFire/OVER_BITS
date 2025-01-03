using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using Cysharp.Threading.Tasks;
public class ClapTalk : MonoBehaviour
{
    static string OpenVCChannelName = "OpenVC";
    static string TestChannelName = "TestChannel";
    [SerializeField] bool Join = false;
    [SerializeField] bool isReady = false;
    static ChannelType joinnedChannel = ChannelType.empty;
    Channel3DProperties channel3DProperties = new(10, 1, 1, AudioFadeModel.InverseByDistance);


    async void Start()
    {
        await UnityServicesManager.InitUnityServices();
        isReady = true;
    }
    void Update()
    {
        if (Join)
        {
            JoinEchoChannelAsync();
            Join = false;
        }
    }
    public async void JoinEchoChannelAsync()
    {
        await LeaveChannnelAsync();
        await VivoxService.Instance.JoinEchoChannelAsync(TestChannelName, ChatCapability.TextAndAudio);
        joinnedChannel = ChannelType.TestChannel;
    }

    public async void JoinOpenChannelAsync()
    {
        await LeaveChannnelAsync();
        await VivoxService.Instance.JoinGroupChannelAsync(OpenVCChannelName, ChatCapability.TextAndAudio);
        joinnedChannel = ChannelType.OpenVC;
    }

    public static async void LoginToVivoxAsync()
    {
        LoginOptions options = new()
        {
            // DisplayName = PlayerDataManager.LoadedPlayerProfileData.PlayerName,
            DisplayName = "nano_on_fire",
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

    public void LogIn() => UnityServicesManager.InitUnityServices();
    public void Logout() => UnityServicesManager.Logout();

    enum ChannelType
    {
        empty = -1,
        OpenVC = 0,
        TestChannel = 1
    }
}
