using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class UI_PlayerDataManager : MonoBehaviour
{
    [SerializeField] PlayerDataManager pdManager;
    [SerializeField] TMP_InputField PlayerNameField;
    [SerializeField] TMP_Text PlayerName;
    [SerializeField] TMP_Text PlayerID;
    [SerializeField] GameObject SignInPanel;
    [SerializeField] GameObject LoadingPanel;
    [SerializeField] GameObject PlayerSettingPanel;
    [SerializeField] GameObject StartGamePanel;
    NetworkManager nwManager;

    void Start()
    {
        nwManager = NetworkManager.Singleton;
    }

    public async void SignIn()
    {
        LoadingPanel.SetActive(true);
        await pdManager.InitSignIn();
        LoadingPanel.SetActive(false);
        Debug.Log(pdManager.LoadedPlayerProfileData.PlayerName);

        SignInPanel.SetActive(false);
        PlayerSettingPanel.SetActive(true);
        Debug.Log(pdManager.LoadedPlayerProfileData.PlayerName);
        PlayerName.text = pdManager.LoadedPlayerProfileData.PlayerName;
        PlayerID.text = pdManager.LoadedPlayerProfileData.PlayerID;
        var lastDot = PlayerName.text.LastIndexOf('#');
        if (lastDot != -1)
            PlayerNameField.text = PlayerName.text[..lastDot]; // 型名のネームスペース部分を消す
    }

    public void SignOut()
    {
        pdManager.InitSignOut();
        SignInPanel.SetActive(true);
        PlayerSettingPanel.SetActive(false);
    }

    public async void Enter()
    {
        if (PlayerNameField.text != "")
            await pdManager.SetPlayerNameAsync(PlayerNameField.text);
        PlayerProfileData playerData = new()
        {
            PlayerID = AuthenticationService.Instance.PlayerInfo.Id,
            PlayerName = await AuthenticationService.Instance.GetPlayerNameAsync()
        };
        await pdManager.SaveData(playerData);
        await pdManager.LoadData<PlayerProfileData>();
        PlayerName.text = "Player Name : " + pdManager.LoadedPlayerProfileData.PlayerName;
        PlayerID.text = "ID : " + pdManager.LoadedPlayerProfileData.PlayerID;
        PlayerSettingPanel.SetActive(false);
        StartGamePanel.SetActive(true);
    }

    public void StartHost()
    {
        nwManager.StartHost();
        SceneEventProgressStatus status = nwManager.SceneManager.LoadScene("TestWorld", LoadSceneMode.Single);
    }

    public void StartClient()
    {
        nwManager.StartClient();
    }
}
