using Unity.Netcode;
using UnityEngine;

public class SingleCommunication : NetworkBehaviour
{
    private void Awake()
    {
        Debug.Log("AAAAA");
        PlayerDataManager.Instance.singleCommunication = this;
    }
    [ClientRpc]
    public void AddItemOrderClientRpc(string data, int amount, ClientRpcParams rpcParams = default)
    {
        Debug.Log("single communication");

        PlayerDataManager.AddItem(data, amount);
    }
}
