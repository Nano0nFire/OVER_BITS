using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Netcode;

public class PlayerStatus : NetworkBehaviour // プレイヤーオブジェクト直下
{
    public string PlayerName;
    public bool IsLoaded = false;
    public void SetPlayerName(string name) => SetPlayerNameServerRpc(name);

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
    }

    [ServerRpc(RequireOwnership = false)]
    public void PushDataServerRpc(string name)
    {
        PlayerName = name;
        IsLoaded = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerNameServerRpc(string name)
    {
        SetPlayerNameClientRpc(name);
        IsLoaded = true;
    }

    [ClientRpc]
    public void SetPlayerNameClientRpc(string name, ClientRpcParams rpcParams = default)
    {
        PlayerName = name;
        IsLoaded = true;
    }
}
