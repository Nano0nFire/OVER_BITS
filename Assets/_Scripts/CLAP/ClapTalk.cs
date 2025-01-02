using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
public class ClapTalk : MonoBehaviour
{
    static string OpenVCChannelName = "OpenVC";
    [SerializeField] bool Join = false;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await VivoxService.Instance.InitializeAsync();
        await VivoxService.Instance.LoginAsync();
        await VivoxService.Instance.JoinEchoChannelAsync(OpenVCChannelName, ChatCapability.AudioOnly);
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
        await VivoxService.Instance.JoinEchoChannelAsync(OpenVCChannelName, ChatCapability.TextAndAudio);
    }
        public static async void InitVivoxAsync()
    {
        var options = new InitializationOptions();
        options.SetVivoxCredentials("https://unity.vivox.com/appconfig/15669-over_-27234-udash", "mtu1xp.vivox.com", "15669-over_-27234-udash", "qVZlx3Kd2MsTgooQyYVNnFsq1KLUC2xr");
        await UnityServices.InitializeAsync(options);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await VivoxService.Instance.InitializeAsync();
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
}
