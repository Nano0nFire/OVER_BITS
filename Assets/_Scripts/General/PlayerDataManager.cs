using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.CloudSave;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Newtonsoft.Json;

public class PlayerDataManager : NetworkBehaviour
{
    public PlayerProfileData LoadedPlayerProfileData{get ; private set ;}
    [HideInInspector] public DACS_InventorySystem inventorySystem; // ClientGeneralManagerから設定(ClientOnly)
    [HideInInspector] public Action<int> OnItemAdded;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        PlayerAccountService.Instance.SignedIn += SignedIn;
        DontDestroyOnLoad(gameObject);
    }

    public async UniTask InitSignIn()
    {
        if (AuthenticationService.Instance.IsSignedIn) // 以前ログインしていたならロードだけする
            LoadedPlayerProfileData = await LoadData<PlayerProfileData>();
        else // 履歴がないならログインする
            await PlayerAccountService.Instance.StartSignInAsync();
    }

    async void SignedIn() // SignIn開始
    {
        try
        {
            var accessToken = PlayerAccountService.Instance.AccessToken;
            await SignInWithUnityAsync(accessToken);
            LoadedPlayerProfileData = await LoadData<PlayerProfileData>();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private async UniTask SignInWithUnityAsync(string accessToken)
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

    public async UniTask SetPlayerNameAsync(string newName)
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
    public async UniTask SaveData<T>(T SaveData, string CustomKey = null)
    {
        string jsonData = JsonConvert.SerializeObject(SaveData); // データをJsonに変換

        Debug.Log(jsonData);

        string Key;
        if (CustomKey != null)
        {
            Key = CustomKey;
        }
        else // CustomKeyを使用しないのならデータの型をKeyとして使う
        {
            Key = typeof(T).ToString();
            var lastDot = Key.LastIndexOf('.');
            if (lastDot != -1)
                Key = Key[lastDot..]; // 型名のネームスペース部分を消す
        }

        var data = new Dictionary<string, object>
        {
            {Key, jsonData}
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);

        Debug.Log($"{Key} : Save done");
    }

    /// <summary>
    /// 引数無しで保存したいデータの型名をKeyとして保存する <br />
    /// 引数を使用することで同じ型でも独自のKeyを使用して保存できる
    /// </summary>
    /// <param name="CustomKey">任意のKey名(省略で保存するデータの型名が使用される)</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async UniTask<T> LoadData<T>(string CustomKey = null) where T : new()
    {
        try
        {
            string Key;
            if (CustomKey != null)
            {
                Key = CustomKey;
            }
            else // CustomKeyを使用しないのならデータの型をKeyとして使う
            {
                Key = typeof(T).ToString();
                var lastDot = Key.LastIndexOf('.');
                if (lastDot != -1)
                    Key = Key[lastDot..]; // 型名のネームスペース部分を消す
            }

            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>{Key});

            var item = savedData[Key];
            var jsonData = item.Value.GetAsString();
            Debug.Log(jsonData + Key);

            // デシリアライズして元のデータ形式に変換
            return JsonConvert.DeserializeObject<T>(jsonData);
        }
        catch (KeyNotFoundException) // データがない場合
        {
            Debug.Log("Create New Data");
            await CreateAndSaveNewData<T>(CustomKey); // 新しいデータを作成し保存
            return await LoadData<T>(CustomKey); // 作成後のデータをロード
        }
    }

    public async UniTask CreateAndSaveNewData<T>(string CustomKey = null) where T : new() // 初期値の設定はここで行う
    {
        if (typeof(T) == typeof(SettingsData))
        {
            SettingsData data = new()
            {
                ToggleDash = true,
                ToggleCrouch = false,
                InvertAim = false,
                HzCameraSens = 50,
                VCameraSens = 50,
            };
            await SaveData(data); // 新規データの作成
        }
        else
        {
            var data = new T();
            await SaveData(data, CustomKey); // 新規データの作成
        }
    }
    #region AddItemData
    public void AddItem(ItemData itemData, ulong wnID)
    {
        if (!IsServer)
            return;

        ClientRpcParams rpcParams = new() // ClientRPCを送る対象を選択
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { wnID } }
        };
        AddItemOrderToClientClientRpc(JsonConvert.SerializeObject(itemData), rpcParams);
    }

    [ClientRpc]
    public void AddItemOrderToClientClientRpc(string data, ClientRpcParams rpcParams = default)
    {
        if (!IsClient)
            return;

        AddItemOrderToClient(data);
        Debug.Log(data);
    }

    async void AddItemOrderToClient(string data)
    {
        var itemData = JsonConvert.DeserializeObject<ItemData>(data); // Jsonから変換
        Debug.Log(itemData);
        string key = inventorySystem.GetListName(itemData.FirstIndex); // 変換したデータからFirstIndexを取得しKeyを取得
        var PlayerInventoryData = await LoadData<List<ItemData>>(key); // Keyを元にインベントリデータを取得
        PlayerInventoryData.Add(itemData); // アイテムの追加
        await SaveData(PlayerInventoryData, key); // 追加した後のインベントリデータを保存
        OnItemAdded.Invoke(itemData.FirstIndex);
    }
    #endregion
}

[Serializable]
public struct PlayerProfileData
{
    public string PlayerID;
    public string PlayerName;
}

[Serializable]
public struct SettingsData
{
    public bool ToggleDash; //ダッシュ切り替え or 長押しダッシュ
    public bool ToggleCrouch; //しゃがみ切り替え or 長押ししゃがみ
    public bool InvertAim; //垂直方向の視点操作の反転
    public float HzCameraSens; //水平方向の視点感度
    public float VCameraSens; //水s直方向の視点感度
}