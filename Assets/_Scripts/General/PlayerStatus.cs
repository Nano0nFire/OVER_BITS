using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DACS.Inventory;
using Unity.Netcode;
using UnityEngine;

public class PlayerStatus : NetworkBehaviour // プレイヤーオブジェクト直下
{
    public string PlayerName;
    public static SimpleData[] HotbarData = new SimpleData[5];
    public bool IsLoaded { get => loadedDataAmount == 2; } // データの総量を設定しておく
    [SerializeField] HotbarSystem hotbarSystem;
    [HideInInspector] public ulong loadedDataAmount = 0;
    public void SetPlayerName(string name)
    {
        SetPlayerNameServerRpc(name);
        loadedDataAmount++;
    }
    public void SetHotbarData(SimpleData[] HotbarData)
    {
        SetHotbarDataServerRpc(HotbarData);
        loadedDataAmount++;
    }

    public override async void OnNetworkSpawn() // 他のクライアント側のシーンでスポーンした時を想定
    {
        if (IsOwner)
            return;

        await UniTask.WaitUntil(() => ClientGeneralManager.IsLoaded);
        PullDataServerRpc(ClientGeneralManager.clientID);
    }


    [ServerRpc(RequireOwnership = false)]
    public void PullDataServerRpc(ulong clientID)
    {
        WaitPushing(clientID);
    }

    async void WaitPushing(ulong clientID)
    {
        if (!IsServer)
            return;

        ClientRpcParams rpcParams = new() // ClientRPCを送る対象を選択
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientID } }
        };
        await UniTask.WaitUntil(() => IsLoaded);
        SetPlayerNameClientRpc(PlayerName, rpcParams);
        SetHotbarDataClientRpc(HotbarData, rpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PushDataServerRpc(string name)
    {
        PlayerName = name;
        loadedDataAmount ++;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerNameServerRpc(string name)
    {
        SetPlayerNameClientRpc(name);
        loadedDataAmount ++;
    }

    [ClientRpc]
    public void SetPlayerNameClientRpc(string name, ClientRpcParams rpcParams = default)
    {
        PlayerName = name;
        if (!IsServer)
            loadedDataAmount ++;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetHotbarDataServerRpc(SimpleData[] datas)
    {
        SetHotbarDataClientRpc(datas);
        loadedDataAmount ++;
    }

    [ClientRpc]
    public void SetHotbarDataClientRpc(SimpleData[] datas, ClientRpcParams rpcParams = default)
    {
        HotbarData = datas;
        int i = 0;
        foreach (var data in datas)
        {
            hotbarSystem.SpawnItemObjectLocal(data.f, data.s, i);
            i++;
        }

        if (!IsServer)
            loadedDataAmount ++;
    }

    [System.Serializable]
    public struct SimpleData : INetworkSerializable
    {
        public int f;
        public int s;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref f);
            serializer.SerializeValue(ref s);
        }
    }
}
