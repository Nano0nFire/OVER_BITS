using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/****************************************************************************

使用するモデルのルール
~~素体側~~
・ファイル名には"Base"を含める　例) KawaiiBase.fbx
・ヒューマノイドアバターを設定しておく

~~装備品側~~
・素体+αになるようにボーンを組む
・ファイル名にはパーツの名前を含める　例)　KawaiiChest.fbx(Chestパーツとして処理)

****************************************************************************/

public class CustomLifeAvatar : MonoBehaviour
{
    public bool IsFailure{ get; private set; }
    [SerializeField] CLAPartsLists modelDataList; // モデルデータ
    public int BaseModelID = 0; // ベースモデルのID
    public List<int> SelectedEquipmentIDs = new(); // 各装備品のID

    GameObject PlayerObj; // プレイヤーキャラクター
    // GameObject avatarObj; // アバター用のオブジェクト
    GameObject baseModel; // ベースとなる素体
    GameObject armatureObj; // ルートボーンの親
    [SerializeField] List<GameObject> partsList = new(); // 装備するモデルデータ
    [SerializeField] List<Transform> bonesList = new(); // ベースモデルのボーンと装備するモデルのボーンを足したもの
    [SerializeField] List<string> bonesNameList = new(); // bonesListのstring版
    [SerializeField] List<string> partsType = new(); // 最終的にメッシュをavatarObjと同じ階層に置くときにつける名前の一覧
    [SerializeField] RuntimeAnimatorController animController;

    void Start()
    {
        Combiner();
    }

    async void Combiner()
    {
        await SetModels();
        Debug.Log("Completed : SetModels");
        await SetBones();
        Debug.Log("Completed : SetBones");
        await BoneReset();
        Debug.Log("Completed : BoneReset");
        await AnimatorSetting();
        Debug.Log("Completed : AnimatorSetting");
        await ClreanUpObjects();
        Debug.Log("Completed : ClreanUpObjects");

        Debug.Log("ALL Tasks : Successfully executed");
    }

    async Task SetModels()
    {
        PlayerObj = new("PlayerCharacter"); // プレイヤーオブジェクトの生成

        baseModel = PartsInstantiate(PlayerObj, modelDataList.baseBodyList[BaseModelID]);
        baseModel.name = "Avatar";

        foreach (Transform t in baseModel.GetComponentsInChildren<Transform>())
            if (t.name == "Armature") armatureObj = t.gameObject;

        PartsInstantiate(baseModel, modelDataList.hairPartsList[SelectedEquipmentIDs[0]], partsList);
        PartsInstantiate(baseModel, modelDataList.facePartsList[SelectedEquipmentIDs[1]], partsList);
        PartsInstantiate(baseModel, modelDataList.chestPartsList[SelectedEquipmentIDs[2]], partsList);
        PartsInstantiate(baseModel, modelDataList.legPartsList[SelectedEquipmentIDs[3]], partsList);
        PartsInstantiate(baseModel, modelDataList.shoesPartsList[SelectedEquipmentIDs[4]], partsList);
    }

    async Task SetBones()
    {
        bonesList.AddRange(baseModel.GetComponentInChildren<SkinnedMeshRenderer>().rootBone.GetComponentsInChildren<Transform>());
        bonesList[0].parent = armatureObj.transform;

        foreach (GameObject equipObj in partsList) // 全ての装備品とベースモデルのボーンを一つのリストにし、共通のルートボーンにする
            BoneBuild(bonesList, equipObj.transform, bonesNameList);

        bonesList.Clear();
        bonesList.AddRange(baseModel.GetComponentInChildren<SkinnedMeshRenderer>().rootBone.GetComponentsInChildren<Transform>()); // ボーンの順番通りにするためにリストを再更新

        bonesNameList.Clear();
        foreach (Transform bone in bonesList)
            bonesNameList.Add(bone.name);
    }

    async Task BoneReset()
    {
        partsList.Add(baseModel);

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

    GameObject PartsInstantiate(GameObject ParentObj, GameObject Obj, List<GameObject> list = null)
    {
        if (Obj == null) return null;
        GameObject InstantiatedObj = Instantiate(Obj);

        InstantiatedObj.transform.parent = ParentObj.transform;
        InstantiatedObj.transform.localPosition = Vector3.zero;
        InstantiatedObj.transform.localScale = Vector3.one;
        InstantiatedObj.transform.localRotation = Quaternion.identity;

        if (list != null)
        {
            list.Add(InstantiatedObj);
            return null;
        }
        else return InstantiatedObj;
    }

    void BoneBuild(List<Transform> boneList, Transform equipObj, List<string> nameList)
    {
        Transform equipsRootBone = equipObj.GetComponentInChildren<SkinnedMeshRenderer>().rootBone;

        nameList.Clear();
        foreach (Transform bone in boneList)
            nameList.Add(bone.name);

        foreach (Transform equipsBone in equipsRootBone.GetComponentsInChildren<Transform>())
        {
            if(nameList.Contains(equipsBone.name))
                continue;

            nameList.Add(equipsBone.name);
            bonesList.Add(equipsBone);
            equipsBone.parent = boneList[nameList.IndexOf(equipsBone.parent.name)];
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

    async Task AnimatorSetting()
    {
        Animator anim = baseModel.GetComponent<Animator>();
        anim.runtimeAnimatorController = animController;
    }

    async Task ClreanUpObjects()
    {
        foreach (GameObject part in partsList)
        {
            foreach (SkinnedMeshRenderer smr in part.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                foreach (string key in partsType)
                    if (smr.gameObject.transform.parent.name.Contains(key)) smr.gameObject.name = key;

                smr.gameObject.transform.parent = baseModel.transform;
            }

            if(part.name.Contains("Avatar")) continue;
            Destroy(part);
        }
    }
}
