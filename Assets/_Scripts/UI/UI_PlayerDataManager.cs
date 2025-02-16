using UnityEngine;
using Unity.Services.Authentication;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using Cysharp.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using System.Net;

public class UI_PlayerDataManager : MonoBehaviour
{
    [SerializeField] TMP_InputField PlayerNameField;
    [SerializeField] TMP_Text PlayerName;
    [SerializeField] TMP_Text PlayerID;
    [SerializeField] GameObject SignInPanel;
    [SerializeField] GameObject LoadingPanel;
    [SerializeField] GameObject PlayerSettingPanel;
    [SerializeField] GameObject StartGamePanel;
    [SerializeField] TMP_Dropdown WorldDropdown;
    [SerializeField] TMP_InputField AddressField;
    [SerializeField] TMP_InputField PortField;
    [SerializeField] bool AllowRemote, ListenAny;
    [SerializeField] List<string> WorldNames;
    NetworkManager nwManager;

    void Start()
    {
        nwManager = NetworkManager.Singleton;
    }

    public async void SignIn()
    {
        LoadingPanel.SetActive(true);
        await PlayerDataManager.InitSignIn();
        LoadingPanel.SetActive(false);
        Debug.Log(PlayerDataManager.LoadedPlayerProfileData.PlayerName);

        SignInPanel.SetActive(false);
        PlayerSettingPanel.SetActive(true);
        Debug.Log(PlayerDataManager.LoadedPlayerProfileData.PlayerName);
        PlayerName.text = PlayerDataManager.LoadedPlayerProfileData.PlayerName;
        PlayerID.text = PlayerDataManager.LoadedPlayerProfileData.PlayerID;
        var lastDot = PlayerName.text.LastIndexOf('#');
        if (lastDot != -1)
            PlayerNameField.text = PlayerName.text[..lastDot]; // #以降を消す
    }

    public void SignOut()
    {
        UnityServicesManager.Logout();
        SignInPanel.SetActive(true);
        PlayerSettingPanel.SetActive(false);
    }

    public async void Enter()
    {
        if (PlayerNameField.text != "")
            await PlayerDataManager.SetPlayerNameAsync(PlayerNameField.text);
        PlayerProfileData playerData = new()
        {
            PlayerID = AuthenticationService.Instance.PlayerInfo.Id,
            PlayerName = await AuthenticationService.Instance.GetPlayerNameAsync()
        };
        await PlayerDataManager.SaveData(playerData);
        await PlayerDataManager.LoadData<PlayerProfileData>();
        PlayerName.text = "Player Name : " + PlayerDataManager.LoadedPlayerProfileData.PlayerName;
        PlayerID.text = "ID : " + PlayerDataManager.LoadedPlayerProfileData.PlayerID;
        PlayerSettingPanel.SetActive(false);
        StartGamePanel.SetActive(true);
    }

    public void AllowRemoteConnections(bool value) => AllowRemote = value;
    public void ToggleListenAny(bool value) => ListenAny = value;

    public async void StartHost()
    {
        // NetworkManagerから通信実体のTransportを取得
        var transport = Unity.Netcode.NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        var unityTransport = transport as UnityTransport;

        // どのIPアドレスから設けて、7777ポートで待ちます
        unityTransport.SetConnectionData(IPAddress.Any.ToString(), // アドレス
                                         PortField.text == string.Empty ? (ushort)7777 : ushort.Parse(PortField.text)); // ポート

        // transport.ConnectionData.ServerListenAddress = AllowRemote ? "0.0.0.0" : "127.0.0.1";
        await UniTask.Delay(1000);
        Debug.Log($"Start Host\nAddress : {unityTransport.ConnectionData.Address}\nPort : {unityTransport.ConnectionData.Port}\nServerListenAddress : {unityTransport.ConnectionData.ServerListenAddress}");
        nwManager.StartHost();
        nwManager.SceneManager.LoadScene(WorldNames[WorldDropdown.value], LoadSceneMode.Single);
    }

    public async void StartClient()
    {
        // NetworkManagerから通信実体のTransportを取得
        var transport = Unity.Netcode.NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        var unityTransport = transport as UnityTransport;

        // どのIPアドレスから設けて、7777ポートで待ちます
        unityTransport.SetConnectionData(Dns.GetHostAddresses(AddressField.text == string.Empty ? "127.0.0.1" : AddressField.text)[0].ToString(), // アドレス
                                         PortField.text == string.Empty ? (ushort)7777 : ushort.Parse(PortField.text)); // ポート

        // transport.ConnectionData.ServerListenAddress = AllowRemote ? "0.0.0.0" : "127.0.0.1";
        await UniTask.Delay(1000);
        Debug.Log($"Start Client\nAddress : {unityTransport.ConnectionData.Address}\nPort : {unityTransport.ConnectionData.Port}\nServerListenAddress : {unityTransport.ConnectionData.ServerListenAddress}");
        nwManager.StartClient();
    }
}
