using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using TMPro;

public class PlayerDataManager : MonoBehaviour
{
    public int Level;
    public PlayerProfileData LoadedPlayerProfileData
    {
        get
        {
            return _LoadedPlayerProfileData;
        }
    }
    PlayerProfileData _LoadedPlayerProfileData;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        PlayerAccountService.Instance.SignedIn += SignedIn;
    }

    public async Task InitSignIn() // SignIn呼び出し
    {
        if (AuthenticationService.Instance.IsSignedIn)
            await LoadData();
        else
            await PlayerAccountService.Instance.StartSignInAsync();
    }

    async void SignedIn() // SignIn開始
    {
        try
        {
            var accessToken = PlayerAccountService.Instance.AccessToken;
            await SignInWithUnityAsync(accessToken);

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private async Task SignInWithUnityAsync(string accessToken)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);

            PlayerProfile playerProfile = new()
            {
                playerInfo = AuthenticationService.Instance.PlayerInfo,
                PlayerName = await AuthenticationService.Instance.GetPlayerNameAsync()
            };

            await LoadData();
        }
        catch (AuthenticationException ex)
        {
            await SaveData();
            await LoadData();
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    private void OnDestroy()
    {
        PlayerAccountService.Instance.SignedIn -= SignedIn;
    }

    public async Task SetPlayerNameAsync(string newName)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
            Debug.Log("Player name has been updated to: " + newName);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError("Failed to set player name: " + ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError("Request failed: " + ex.Message);
        }
    }
    public async Task SaveData()
    {
        PlayerProfileData playerData = new()
        {
            PlayerID = AuthenticationService.Instance.PlayerInfo.Id,
            PlayerName = await AuthenticationService.Instance.GetPlayerNameAsync(),
            Level = Level
        };

        string jsonData = JsonUtility.ToJson(playerData);

        var data = new Dictionary<string, object>
        {
            { "PlayerProfileData", jsonData }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);

        Debug.Log("Save done");
    }
    public async Task LoadData()
    {
        try
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>{"PlayerProfileData"});

            var item = savedData["PlayerProfileData"];
            var jsonData = item.Value.GetAsString();

            // デシリアライズして元のデータ形式に変換
            var playerData = JsonUtility.FromJson<PlayerProfileData>(jsonData);

            _LoadedPlayerProfileData.PlayerID = playerData.PlayerID;
            _LoadedPlayerProfileData.PlayerName = playerData.PlayerName;
            _LoadedPlayerProfileData.Level = playerData.Level;
        }
        catch (KeyNotFoundException)
        {
            await SaveData();
        }
    }
}

[Serializable]
public struct PlayerProfile
{
    public PlayerInfo playerInfo;
    public string PlayerName;
    public float Level;
}
[Serializable]
public struct PlayerProfileData
{
    public string PlayerID;
    public string PlayerName;
    public float Level;
}
[Serializable]
public struct PlayerData
{
    public PlayerInfo playerInfo;
    public string PlayerName;
    public float Level;
    public States SelectedActionSkill;

    // settings
    public bool InvertAim; //垂直方向の視点操作の反転
    public float HzCameraSens; //水平方向の視点感度
    public float VCameraSens; //水s直方向の視点感度
}