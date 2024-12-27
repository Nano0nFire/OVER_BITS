using Unity.Netcode;
using UnityEngine;

public class SingleTargetRPCTest : NetworkBehaviour
{
    [SerializeField] private ulong targetClientId;

    [ServerRpc]
    public void ServerRPC(ulong targetClientId)
    {
        ClientRPC(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { targetClientId } } });
    }

    [ClientRpc]
    public void ClientRPC(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("ClientRPC called");
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ServerRPC(targetClientId);
            }
        }
    }
}
