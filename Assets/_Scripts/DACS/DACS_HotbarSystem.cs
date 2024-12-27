using System;
using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using UnityEngine;

public class DACS_HotbarSystem : NetworkBehaviour
{
    [SerializeField] ItemDataBase itemDataBase;
    [SerializeField] CLAPlus_HandControl handControl;
    int SelectedSlotIndex = 0;
    [SerializeField] Transform ItemPos;
    [SerializeField] Transform RightHandPoint;
    [SerializeField] Transform LeftHandPoint;
    GameObject[] hotbarItemObjects = new GameObject[5];

    DACS_ObjectInfo[] objInfos = new DACS_ObjectInfo[5];
    [HideInInspector] public Action<Transform> ChangeActionPoint;

    public void SelectedHotbarSlot(int index)
    {
        SetItemObjectLocal(index);
    }
    public void SpawnItemObjectLocal(int FirstIndex, int SecondIndex, int index)
    {
        var data = itemDataBase.GetItem(FirstIndex, SecondIndex);
        if (data.ItemModel == null)
            return;

        if (IsOwner)
            SpawnItemObjectServerRpc(FirstIndex, SecondIndex, index);

        Debug.Log("Start");

        if (hotbarItemObjects[index] != null)
            Destroy(hotbarItemObjects[index]);

        Debug.Log("Destroy");
        hotbarItemObjects[index] = Instantiate(data.ItemModel);
        Debug.Log("Instantiate");

        objInfos[index] = hotbarItemObjects[index].GetComponent<DACS_ObjectInfo>();
        var itemXform = hotbarItemObjects[index].transform;

        itemXform.SetParent(ItemPos);

        itemXform.localPosition = Vector3.zero;
        itemXform.transform.localRotation = Quaternion.identity;

        if (SelectedSlotIndex == index)
            SetItemObjectLocal(index);
    }
    void SetItemObjectLocal(int index)
    {
        hotbarItemObjects[SelectedSlotIndex]?.SetActive(false);
        SelectedSlotIndex = index;
        if (hotbarItemObjects[index] == null)
        {
            handControl.ChangeHandState(-1);
            return;
        }

        if (IsOwner)
            SetItemObjectServerRpc(index);

        hotbarItemObjects[index]?.SetActive(true);
        if (objInfos[index] != null)
        {
            handControl.rHandIKTarget = objInfos[index].rIKPos;
            handControl.lHandIKTarget = objInfos[index].lIKPos;
            handControl.StatePreset = objInfos[index].preset;
            handControl.ItemXform = hotbarItemObjects[index].transform;
            handControl.ChangeHandState(IsOwner ? 0 : objInfos[index].SyncPresetNumber);
            if (IsOwner)
                ChangeActionPoint.Invoke(objInfos[index].ActionPoint); // 銃口の位置やアクションの起点となる部分を更新する
        }
        else
        {
            handControl.ChangeHandState(-1);
        }
    }

    [ServerRpc]
    void SpawnItemObjectServerRpc(int FirstIndex, int SecondIndex, int SlotIndex)
    {
        if (!IsServer)
            return;

        Debug.Log("ItemSpawn - ServerRPC");
        SpawnItemObjectClientRpc(FirstIndex, SecondIndex, SlotIndex);
    }

    [ClientRpc]
    void SpawnItemObjectClientRpc(int FirstIndex, int SecondIndex, int SlotIndex)
    {
        if (IsOwner)
            return;

        Debug.Log("ItemSpawn - ClienRPC");
        SpawnItemObjectLocal(FirstIndex, SecondIndex, SlotIndex);
    }

    [ServerRpc]
    void SetItemObjectServerRpc(int SlotIndex)
    {
        if (!IsServer)
            return;

        Debug.Log("ItemSet - ServerRPC");
        SetItemObjectClientRpc(SlotIndex);
    }

    [ClientRpc]
    void SetItemObjectClientRpc(int SlotIndex)
    {
        if (IsOwner)
            return;

        Debug.Log("ItemSet - ClientRPC");
        SetItemObjectLocal(SlotIndex);
    }
}
