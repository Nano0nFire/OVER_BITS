using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode;
using TMPro;

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
        await pdManager.SaveData();
        PlayerName.text = pdManager.LoadedPlayerProfileData.PlayerName;
        PlayerID.text = pdManager.LoadedPlayerProfileData.PlayerID;
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
