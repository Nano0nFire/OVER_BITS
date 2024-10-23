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
    }

    public async void Enter()
    {
        await pdManager.SetPlayerNameAsync(PlayerNameField.text);
        PlayerProfile playerData = new()
        {
            PlayerID = AuthenticationService.Instance.PlayerInfo.Id,
            PlayerName = await AuthenticationService.Instance.GetPlayerNameAsync()
        };
        await pdManager.SaveData<PlayerProfile>(playerData);
        await pdManager.LoadData<PlayerProfile>();
        PlayerName.text = "Player Name : " + pdManager.LoadedPlayerProfileData.PlayerName;
        PlayerID.text = "ID : " + pdManager.LoadedPlayerProfileData.PlayerID;
        PlayerSettingPanel.SetActive(false);
        StartGamePanel.SetActive(true);
    }

    public void StartHost()
    {
        nwManager.StartHost();
        nwManager.SceneManager.LoadScene("TestWorld", LoadSceneMode.Single);
    }

    public void StartClient()
    {
        nwManager.StartClient();
    }
}
