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
    [SerializeField] bool AllowRemote, UseRelay;
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
    public void ToggleUseRelay(bool value) => UseRelay = value;

    public async void StartHost()
    {
        var transport = nwManager.GetComponent<UnityTransport>();

        transport.ConnectionData.Port = PortField.text == string.Empty ? (ushort)7777 : ushort.Parse(PortField.text);
        transport.ConnectionData.ServerListenAddress = AllowRemote ? "0.0.0.0" : "127.0.0.1";
        transport.ConnectionData.Address = AddressField.text == string.Empty ? "127.0.0.1" : AddressField.text;

        await UniTask.Delay(1000);

        Debug.Log($"Start Host\nAddress : {transport.ConnectionData.Address}\nPort : {transport.ConnectionData.Port}\nPort : {transport.ConnectionData.ServerListenAddress}");
        nwManager.StartHost();
        nwManager.SceneManager.LoadScene(WorldNames[WorldDropdown.value], LoadSceneMode.Single);
    }

    public async void StartClient()
    {
        var transport = nwManager.GetComponent<UnityTransport>();
        transport.ConnectionData.Port = PortField.text == string.Empty ? (ushort)7777 : ushort.Parse(PortField.text);
        transport.ConnectionData.ServerListenAddress = AllowRemote ? "0.0.0.0" : "127.0.0.1";
        transport.ConnectionData.Address = AddressField.text == string.Empty ? "127.0.0.1" : AddressField.text;

        await UniTask.Delay(1000);

        Debug.Log($"Start Client\nAddress : {transport.ConnectionData.Address}\nPort : {transport.ConnectionData.Port}\nPort : {transport.ConnectionData.ServerListenAddress}");
        nwManager.StartClient();
    }
}
