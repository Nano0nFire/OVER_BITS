using Unity.Netcode;
using UnityEngine;

public class SingleCommunication : NetworkBehaviour
{
    private void Awake()
    {
        PlayerDataManager.Instance.singleCommunication = this;
    }

    public void AddItem(string data, int amount, ulong clientID)
    {
        ClientRpcParams rpcParams = new() // ClientRPCを送る対象を選択
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientID } }
        };
        AddItemOrderClientRpc(data, amount, rpcParams);
        Debug.Log("AddItemOrder");
    }

    [ClientRpc]
    public void AddItemOrderClientRpc(string data, int amount, ClientRpcParams rpcParams = default)
    {
        PlayerDataManager.AddItem(data, amount);
    }
}
