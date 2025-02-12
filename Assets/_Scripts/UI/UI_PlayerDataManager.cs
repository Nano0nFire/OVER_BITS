using UnityEngine;
using Unity.Services.Authentication;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.UI;

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

    public void StartHost()
    {
        nwManager.StartHost();
        nwManager.SceneManager.LoadScene(WorldNames[WorldDropdown.value], LoadSceneMode.Single);
    }

    public void StartClient()
    {
        nwManager.StartClient();
    }
}
