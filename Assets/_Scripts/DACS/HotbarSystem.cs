using System;
using Unity.Netcode;
using UnityEngine;
using CLAPlus.AnimationControl;

namespace DACS.Inventory
{
    public class HotbarSystem : NetworkBehaviour
    {
        [SerializeField] PlayerStatus playerStatus;
        [SerializeField] ItemDataBase itemDataBase;
        [SerializeField] HandControl handControl;
        int SelectedSlotIndex = 0;
        [SerializeField] Transform ItemPos;
        [SerializeField] Transform RightHandPoint;
        [SerializeField] Transform LeftHandPoint;
        GameObject[] hotbarItemObjects = new GameObject[5];

        [SerializeField] ItemComponent[] objInfos = new ItemComponent[5];
        [HideInInspector] public Action<Transform> ChangeActionPoint;

        public void SelectedHotbarSlot(int index)
        {
            SetItemObjectLocal(index);
        }
        public void SpawnItemObjectLocal(int FirstIndex, int SecondIndex, int index, bool UseAutoSync = true)
        {
            if (FirstIndex < 0)
                return;
            var data = itemDataBase.GetItem(FirstIndex, SecondIndex);
            if (data.ItemModel == null)
                return;

            if (IsOwner && UseAutoSync)
                SpawnItemObjectServerRpc(FirstIndex, SecondIndex, index);

            if (hotbarItemObjects[index] != null)
                Destroy(hotbarItemObjects[index]);

            hotbarItemObjects[index] = Instantiate(data.ItemModel);

            objInfos[index] = hotbarItemObjects[index].GetComponent<ItemComponent>();
            var itemXform = hotbarItemObjects[index].transform;

            itemXform.SetParent(ItemPos);

            itemXform.localPosition = Vector3.zero;
            itemXform.transform.localRotation = Quaternion.identity;

            if (SelectedSlotIndex == index)
                SetItemObjectLocal(index);
        }
        void SetItemObjectLocal(int index)
        {
            if (IsOwner)
                SetItemObjectServerRpc(index);

            if (hotbarItemObjects[SelectedSlotIndex] != null)
                hotbarItemObjects[SelectedSlotIndex].SetActive(false); // すでに手に持っていたオブジェクトを非表示にする
            SelectedSlotIndex = index; // インデックスの更新

            if (hotbarItemObjects[index] == null)
            {
                handControl.ChangeHandState(-1);
                return;
            }

            if (hotbarItemObjects[index] != null)
                hotbarItemObjects[index].SetActive(true);
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

        [ServerRpc(RequireOwnership = false)]
        void SpawnItemObjectServerRpc(int FirstIndex, int SecondIndex, int SlotIndex)
        {
            if (!IsServer)
                return;

            SpawnItemObjectClientRpc(FirstIndex, SecondIndex, SlotIndex);
        }

        [ClientRpc]
        void SpawnItemObjectClientRpc(int FirstIndex, int SecondIndex, int SlotIndex)
        {
            if (IsOwner)
                return;

            SpawnItemObjectLocal(FirstIndex, SecondIndex, SlotIndex);
        }

        [ServerRpc(RequireOwnership = false)]
        void SetItemObjectServerRpc(int SlotIndex)
        {
            if (!IsServer)
                return;

            SetItemObjectClientRpc(SlotIndex);
        }

        [ClientRpc]
        void SetItemObjectClientRpc(int SlotIndex)
        {
            if (IsOwner)
                return;

            SetItemObjectLocal(SlotIndex);
        }
    }
}