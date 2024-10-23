using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using Unity.Entities.UniversalDelegates;

public class PlayerDataManager : MonoBehaviour
{
    public int Level;
    public PlayerProfile LoadedPlayerProfileData
    {
        get
        {
            return _LoadedPlayerProfileData;
        }
    }
    PlayerProfile _LoadedPlayerProfileData;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        PlayerAccountService.Instance.SignedIn += SignedIn;
    }

    public async Task InitSignIn()
    {
        if (AuthenticationService.Instance.IsSignedIn) // 以前ログインしていたならロードだけする
            _LoadedPlayerProfileData = await LoadData<PlayerProfile>();
        else // 履歴がないならログインする
            await PlayerAccountService.Instance.StartSignInAsync();
    }

    async void SignedIn() // SignIn開始
    {
        try
        {
            var accessToken = PlayerAccountService.Instance.AccessToken;
            await SignInWithUnityAsync(accessToken);
            _LoadedPlayerProfileData = await LoadData<PlayerProfile>();
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
        }
        catch (AuthenticationException ex)
        {
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
    public async Task SaveData<T>(T PlayerData)
    {
        string jsonData = JsonUtility.ToJson(PlayerData);

        var tempKey = typeof(T).ToString();
        var lastDot = tempKey.LastIndexOf('.');
        if (lastDot != -1)
            tempKey = tempKey[lastDot..]; // 型名のネームスペース部分を消す

        var data = new Dictionary<string, object>
        {
            {tempKey, jsonData}
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);

        Debug.Log("Save done");
    }
    public async Task<T> LoadData<T>() where T : struct
    {
        try
        {
            var tempKey = typeof(T).ToString();
            var lastDot = tempKey.LastIndexOf('.');
            if (lastDot != -1)
                tempKey = tempKey[lastDot..]; // 型名のネームスペース部分を消す

            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>{tempKey});

            var item = savedData[tempKey];
            var jsonData = item.Value.GetAsString();

            // デシリアライズして元のデータ形式に変換
            return JsonUtility.FromJson<T>(jsonData);
        }
        catch (KeyNotFoundException) // データがない場合
        {
            var data = new T();
            await SaveData<T>(data); // 新規データの作成
            var loadedData = await LoadData<T>(); // 作成後のデータをロード
            return loadedData;
        }
    }
}

[Serializable]
public struct PlayerProfile
{
    public string PlayerID;
    public string PlayerName;
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