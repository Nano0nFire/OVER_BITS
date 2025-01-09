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
using System.Linq;
using DACS.Inventory;
using CLAPlus.ClapTalk;

/// <summary>
/// LocalOnly(DontDestory)
/// </summary>
public class PlayerDataManager : NetworkBehaviour
{
    public static PlayerProfileData LoadedPlayerProfileData{get ; private set ;}
    public static SettingsData PlayerSettingsData;
    [HideInInspector] public static Action<int> OnItemAdded;
    public SingleCommunication singleCommunication;

    // シングルトンインスタンス
    private static PlayerDataManager instance;

    // インスタンスへのプロパティ
    public static PlayerDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<PlayerDataManager>();

                if (instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(PlayerDataManager).Name);
                    instance = singletonObject.AddComponent<PlayerDataManager>();
                }
            }
            return instance;
        }
    }

    // シングルトンの初期化
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject); // 重複するインスタンスを破棄
        }
    }

    async void Start()
    {
        await UnityServicesManager.InitUnityServices();
        PlayerAccountService.Instance.SignedIn += SignedIn;
    }

    public static async UniTask InitSignIn()
    {
        if (AuthenticationService.Instance.IsSignedIn) // 以前ログインしていたならロードだけする
            LoadedPlayerProfileData = await LoadData<PlayerProfileData>();
        else if (AuthenticationService.Instance.SessionTokenExists) // セッションが有効なら再度サインイン
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await UniTask.WaitUntil(() => AuthenticationService.Instance.IsSignedIn); // ログインが終わるまで待機
            LoadedPlayerProfileData = await LoadData<PlayerProfileData>();
        }
        else
        {
            await PlayerAccountService.Instance.StartSignInAsync();
            await UniTask.WaitUntil(() => AuthenticationService.Instance.IsSignedIn); // ログインが終わるまで待機
        }

        ClapTalk.LoginToVivoxAsync();
        await UniTask.Delay(500); // 全体の読み込みに余裕を持たせる
    }

    async void SignedIn() // SignIn開始
    {
        try
        {
            var accessToken = PlayerAccountService.Instance.AccessToken;
            await SignInWithUnityAsync(accessToken);
            // LoadedPlayerProfileData = await LoadData<PlayerProfileData>();
            LoadedPlayerProfileData = new()
            {
                PlayerID = AuthenticationService.Instance.PlayerInfo.Id,
                PlayerName = await LoadData<PlayerProfileData>().ContinueWith(x => x.PlayerName) // ロードが終わるまで待機
            };
            await SaveData(LoadedPlayerProfileData);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    async UniTask SignInWithUnityAsync(string accessToken)
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

    // async UniTask LinkWithUnityAsync(string accessToken)
    // {
    //     try
    //     {
    //         await AuthenticationService.Instance.LinkWithUnityAsync(accessToken);
    //         Debug.Log("Link is successful.");
    //     }
    //     catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
    //     {
    //         // Prompt the player with an error message.
    //         Debug.LogError("This user is already linked with another account. Log in instead.");
    //     }

    //     catch (AuthenticationException ex)
    //     {
    //         // Compare error code to AuthenticationErrorCodes
    //         // Notify the player with the proper error message
    //         Debug.LogException(ex);
    //     }
    //     catch (RequestFailedException ex)
    //     {
    //         // Compare error code to CommonErrorCodes
    //         // Notify the player with the proper error message
    //         Debug.LogException(ex);
    //     }
    // }

    private void OnDestroy()
    {
        // UnityServicesManager.Logout();
        PlayerAccountService.Instance.SignedIn -= SignedIn;
    }

    public static async UniTask SetPlayerNameAsync(string newName)
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
    public static async UniTask SaveData<T>(T SaveData, string CustomKey = null)
    {
        string jsonData = JsonConvert.SerializeObject(SaveData); // データをJsonに変換

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
    public static async UniTask<T> LoadData<T>(string CustomKey = null) where T : new()
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

    public static async UniTask CreateAndSaveNewData<T>(string CustomKey) where T : new() // 初期値の設定はここで行う
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
        else if (typeof(T) == typeof(PlayerProfileData))
        {
            PlayerProfileData data = new()
            {
                PlayerName = "Player" + UnityEngine.Random.Range(1000, 9999),
            };
            await SaveData(data); // 新規データの作成
        }
        else if (CustomKey == "HotbarData")
        {
            List<ItemData> data = new ItemData[5].ToList();
            await SaveData(data, CustomKey); // 新規データの作成
        }
        else
        {
            var data = new T();
            await SaveData(data, CustomKey); // 新規データの作成
        }
    }
    #region AddItemData
    public void AddItem(ItemData itemData, ulong clientID, int amount = 0)
    {
        if (!IsServer)
            return;

        singleCommunication.AddItem(JsonConvert.SerializeObject(itemData), amount, clientID);
    }

    public static async void AddItem(string data, int amount = 0)
    {
        // if (!IsClient)
        //     return;

        var itemData = JsonConvert.DeserializeObject<ItemData>(data); // Jsonから変換
        string key = InventorySystem.GetListName(itemData.FirstIndex); // 変換したデータからFirstIndexを取得しKeyを取得
        var PlayerInventoryData = await LoadData<List<ItemData>>(key); // Keyを元にインベントリデータを取得
        if (amount == 0) {
            PlayerInventoryData.Add(itemData);
            InventorySystem.GetInventoryData(itemData.FirstIndex).Add(itemData);
        } else {
            int itemIndex;
            int secondIndex = itemData.SecondIndex;
            int length = PlayerInventoryData.Count;
            for (itemIndex = 0; itemIndex < length; itemIndex++)
                if (PlayerInventoryData[itemIndex].SecondIndex == secondIndex)
                    break;

            if (length == itemIndex){ // アイテムがリスト内に存在しなかった場合
                itemData.Amount += amount;
                PlayerInventoryData.Add(itemData);
                InventorySystem.GetInventoryData(itemData.FirstIndex).Add(itemData);
            } else {
                itemData = PlayerInventoryData[itemIndex];
                itemData.Amount += amount;
                PlayerInventoryData[itemIndex] = itemData;
                InventorySystem.GetInventoryData(itemData.FirstIndex)[itemIndex] = itemData;
            }
        }
        await SaveData(PlayerInventoryData, key); // 追加した後のインベントリデータを保存
        OnItemAdded.Invoke(itemData.FirstIndex);
    }
    #endregion
    #region RemoveItem
    /// <summary>
    /// ServerOnly
    /// </summary>
    /// <param name="itemData"></param>
    /// <param name="clientID"></param>
    public void RemoveItem(ItemData itemData, ulong clientID, int amount)
    {
        if (!IsServer)
            return;

        ClientRpcParams rpcParams = new() // ClientRPCを送る対象を選択
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientID } }
        };
        RemoveItemOrderClientRpc(JsonConvert.SerializeObject(itemData), amount, rpcParams);
    }

    [ClientRpc]
    public void RemoveItemOrderClientRpc(string data, int amount, ClientRpcParams rpcParams = default)
    {
        if (!IsClient)
            return;

        RemoveItem(data, amount);
    }

    /// <summary>
    /// ClientOnly
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    async void RemoveItem(string data, int amount = 0)
    {
        if (!IsClient)
            return;

        var itemData = JsonConvert.DeserializeObject<ItemData>(data); // Jsonから変換
        string key = InventorySystem.GetListName(itemData.FirstIndex); // 変換したデータからFirstIndexを取得しKeyを取得
        var PlayerInventoryData = await LoadData<List<ItemData>>(key); // Keyを元にインベントリデータを取得
        if (amount == 0) {
            PlayerInventoryData.Remove(itemData);
            InventorySystem.GetInventoryData(itemData.FirstIndex).Remove(itemData);
        } else {
            int itemIndex;
            int secondIndex = itemData.SecondIndex;
            int length = PlayerInventoryData.Count;
            for (itemIndex = 0; itemIndex < length; itemIndex++)
                if (PlayerInventoryData[itemIndex].SecondIndex == secondIndex)
                    break;

            if (length == itemIndex){ // アイテムがリスト内に存在しなかった場合
                itemData.Amount += amount;
                PlayerInventoryData.Add(itemData);
                InventorySystem.GetInventoryData(itemData.FirstIndex).Add(itemData);
            } else {
                itemData = PlayerInventoryData[itemIndex];
                itemData.Amount += amount;
                PlayerInventoryData[itemIndex] = itemData;
                InventorySystem.GetInventoryData(itemData.FirstIndex)[itemIndex] = itemData;
            }
        }
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
    public int InputDeviceIndex;
    public int OutputDeviceIndex;
    public bool UseToggleMute;
}