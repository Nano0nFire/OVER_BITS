using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Entities.UniversalDelegates;

/****************************************************************************

使用するモデルのルール
~~素体側~~
・ファイル名には"Base"を含める　例) KawaiiBase.fbx
・ヒューマノイドアバターを設定しておく

~~装備品側~~
・素体+αになるようにボーンを組む
・ファイル名にはパーツの名前を含める　例)　KawaiiChest.fbx(Chestパーツとして処理)

****************************************************************************/

public class CustomLifeAvatar : NetworkBehaviour
{
    public bool IsFailure{ get; private set; }
    [SerializeField] ItemDataBase modelDataList; // モデルデータ
    public List<int> ModelListIndexs = new(); // 各装備品のリストの番号
    public List<int> ModelIDs = new(); // 各装備品のID
    // NetworkList<int> syncedModelIDs = new();
    [SerializeField] GameObject PlayerObj; // プレイヤーキャラクター
    [SerializeField] GameObject RootBone;
    [SerializeField] Transform skins;
    [SerializeField] List<string> partsType = new(); // 最終的にメッシュをavatarObjと同じ階層に置くときにつける名前の一覧
    List<GameObject> partsList = new(); // 装備するモデルデータ
    List<Transform> bonesList = new(); // ベースモデルのボーンと装備するモデルのボーンを足したもの

    public void Combiner()
    {
        if (partsList != null)
            ResetSettings();
        else
        {
            partsList = new();
            // if (IsOwner)
            //     foreach (int n in ModelIDs)
            //         AddListServerRpc(n);
        }

        if (IsOwner)
        {
            // for (int i = 0; i < ModelIDs.Count; i++)
            //     ChangeListServerRpc(i, ModelIDs[i]);

            CombineServerRpc(ModelIDs.ToArray());
        }
    }

    void StartCombine()
    {
        if (partsList != null)
            ResetSettings();
        else
            partsList = new();

        SetModels();
        Debug.Log("Completed : SetModels");
        SetBones();
        Debug.Log("Completed : SetBones");
        BoneReset();
        Debug.Log("Completed : BoneReset");
        ClreanUpObjects();
        Debug.Log("Completed : ClreanUpObjects");
    }

    void ResetSettings()
    {
        partsList.Clear();
        bonesList.Clear();

        for (int i = 0; i < skins.childCount; i++)
            Destroy(skins.GetChild(i).gameObject);
    }

    void SetModels()
    {
        if (PlayerObj == null) PlayerObj = gameObject;

        for (int i = 0; i < ModelListIndexs.Count; i++)
            SpawnModel(i);

        // for (int i = 0; i < instantiatedObjects.transform.childCount; i++)
        //     partsList.Add(skins.GetChild(i).gameObject);
    }

    void SetBones()
    {
        bonesList.AddRange(RootBone.GetComponentsInChildren<Transform>());

        foreach (GameObject equipObj in partsList) // 全ての装備品とベースモデルのボーンを一つのリストにし、共通のルートボーンにする
            BoneBuild(equipObj.transform);

        bonesList.Clear();
        bonesList.AddRange(RootBone.GetComponentsInChildren<Transform>()); // ボーンの順番通りにするためにリストを再更新

        // bonesNameList.Clear();
        // foreach (Transform bone in bonesList)
        //     bonesNameList.Add(bone.name);
    }

    void BoneReset()
    {
        foreach (GameObject Parts in partsList)
        {
            foreach (SkinnedMeshRenderer smRenderer in Parts.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Transform[] partsBonesArray = smRenderer.bones;
                Transform[] newBones = new Transform[bonesList.Count];

                for (int i = 0; i < partsBonesArray.Length; i++)
                {
                    if (partsBonesArray[i] == null) continue;
                    newBones[i] = FindBone(bonesList, partsBonesArray[i]);
                }

                smRenderer.bones = newBones;
                smRenderer.rootBone = bonesList[0];
            }
        }
    }

    void BoneBuild(Transform equipObj)
    {
        Transform equipsRootBone = equipObj.GetComponentInChildren<SkinnedMeshRenderer>().rootBone;

        List<string> nameList = new();
        foreach (Transform bone in bonesList)
            nameList.Add(bone.name);

        foreach (Transform equipsBone in equipsRootBone.GetComponentsInChildren<Transform>())
        {
            var name = equipsBone.name;
            if(nameList.Contains(equipsBone.name))
            {
                for (int i = 0; i < nameList.Count; i++)
                {
                    if (name != nameList[i])
                        continue;

                    equipsBone.SetPositionAndRotation(bonesList[i].position, bonesList[i].rotation);
                }

                continue;
            }

            nameList.Add(equipsBone.name);
            bonesList.Add(equipsBone);
            equipsBone.parent = bonesList[nameList.IndexOf(equipsBone.parent.name)];
        }
    }

    Transform FindBone(List<Transform> boneList, Transform target)
    {
        foreach (Transform b in boneList)
        {
            if (b.name == target.name) return b;
        }

        return null;
    }
    void ClreanUpObjects()
    {
        foreach (GameObject part in partsList)
        {
            foreach (SkinnedMeshRenderer smr in part.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                foreach (string key in partsType)
                    if (smr.gameObject.transform.parent.name.Contains(key))
                        smr.gameObject.name = key;

                var obj = smr.gameObject;
                obj.transform.parent = skins;

                Destroy(part);
            }
        }
    }

    void SpawnModel(int ListIndex)
    {
        var obj = modelDataList.GetItem(ModelListIndexs[ListIndex], ModelIDs[ListIndex]).ItemModel;
        if (obj == null)
            return;

        var instantiatedObj = Instantiate(obj);

        var transform = instantiatedObj.transform;

        transform.SetParent(PlayerObj.transform);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        partsList.Add(instantiatedObj);
    }

    [ServerRpc]
    public void CombineServerRpc(int[] IDs)
    {
        CombineClientRpc(IDs);
    }
    [ClientRpc]
    public void CombineClientRpc(int[] IDs)
    {
        if (!IsOwner)
            ModelIDs = new(IDs);

        StartCombine();
    }

    // [ServerRpc]
    // public void ChangeListServerRpc(int index, int value)
    // {
    //     syncedModelIDs[index] = value;
    // }
    // [ServerRpc]
    // public void AddListServerRpc(int value)
    // {
    //     syncedModelIDs.Add(value);
    // }
}
