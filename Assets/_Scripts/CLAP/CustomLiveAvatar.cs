using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using CLAPlus;

/****************************************************************************

使用するモデルのルール
~~全体ルール~~
・Head->HairPoint->Spacer->髪の毛の順番で組む
・髪の毛がない場合でもHairPointとSpacerは作ってほしい(そのほうが安定するし確実)
・HairPointとSpacerはBlender上でそれぞれz方向に0.05の大きさで作る

~~素体側~~
・ファイル名には"Base"を含める　例) KawaiiBase.fbx
・ヒューマノイドアバターを設定しておく

~~装備品側~~
・素体+αになるようにボーンを組む
・ファイル名にはパーツの名前を含める　例)　KawaiiChest.fbx(Chestパーツとして処理)

****************************************************************************/
namespace CLAPlus
{
    public class CustomLifeAvatar : NetworkBehaviour
    {
        public bool IsFailure{ get; private set; }
        [SerializeField] ItemDataBase modelDataList; // モデルデータ
        public List<int> ModelListIndexs = new(); // 各装備品のリストの番号
        public List<int> ModelIDs = new(); // 各装備品のID
        // NetworkList<int> syncedModelIDs = new();
        [SerializeField] Transform PlayerObj; // プレイヤーキャラクター
        [SerializeField] GameObject RootBone;
        [SerializeField] SpringSystem SpringSystem;
        [SerializeField] Transform skins;
        [SerializeField] List<string> partsType = new(); // 最終的にメッシュをavatarObjと同じ階層に置くときにつける名前の一覧
        readonly List<GameObject> partsList = new(); // 装備するモデルデータ
        /// <summary>
        /// ベースモデルのボーンと装備するモデルのボーンを足したもの<br />
        /// 順番はrootボーンから階層順になっている
        /// </summary>
        /// <returns></returns>
        readonly List<Transform> bonesList = new();
        int SpringSystemIndex = -1;

        public void Combiner()
        {
            if (IsOwner)
            {
                CombineServerRpc(ModelIDs.ToArray());
            }
        }

        void StartCombine(bool UseSpringSystem = false)
        {
            Reset();
            SetModels();
            SetBones();
            BoneReset();
            ClreanUpObjects();
            if (UseSpringSystem)
                SpringSystemSetup();

            Clear(); // 使用したListをリセット
        }

        void Reset()
        {
            if (SpringSystemIndex != -1)
                    SpringSystem.ParentsList.RemoveAt(SpringSystemIndex);

            for (int i = 0; i < skins.childCount; i++)
                Destroy(skins.GetChild(i).gameObject);
        }
        void Clear()
        {
            partsList.Clear();
            bonesList.Clear();
        }


        void SetModels()
        {
            if (PlayerObj == null) PlayerObj = transform;

            for (int i = 0; i < ModelListIndexs.Count; i++)
                SpawnModel(i);
        }

        void SetBones()
        {
            bool OnHairProcessing;
            GameObject trash = new("trash");
            trash.transform.SetParent(PlayerObj);
            foreach (var b in RootBone.GetComponentsInChildren<Transform>()) // 髪の毛部分を全削除
            {
                if (b.name.Contains("Sec_Hair"))
                {
                    OnHairProcessing = true;
                    Debug.Log(b.name);
                }
                else
                    OnHairProcessing = false;

                if (OnHairProcessing)
                    b.SetParent(trash.transform);
                else
                    bonesList.Add(b);
            }
            Destroy(trash);


            foreach (GameObject equipObj in partsList) // 全ての装備品とベースモデルのボーンを一つのリストにし、共通のルートボーンにする
                BoneBuild(equipObj.transform);

            bonesList.Clear();
            int i = 0;
            foreach (var b in RootBone.GetComponentsInChildren<Transform>())
            {
                bonesList.Add(b);
                i++;
            }
        }

        void BoneReset() // SkinnedMeshRendererの再設定
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

            // ベースオブジェクトがTポーズからずれている(アニメーションをしている)ので
            // 装飾品オブジェクトのルートボーン以下のボーンの位置と向きをベースオブジェクトに合わせる
            foreach (Transform equipsBone in equipsRootBone.GetComponentsInChildren<Transform>())
            {
                var name = equipsBone.name;
                // if (name.Contains("FaceEye"))
                //     continue;
                if(nameList.Contains(name))
                {
                    for (int i = 0; i < nameList.Count; i++)
                    {
                        if (name != nameList[i]) // ベースオブジェクト内の共通ボーンを探す
                            continue;

                        equipsBone.SetPositionAndRotation(bonesList[i].position, bonesList[i].rotation);

                        break;
                    }

                    continue; // すでにボーンが設定済みなので以下の処理を飛ばす
                }

                nameList.Add(equipsBone.name);
                bonesList.Add(equipsBone);
                equipsBone.parent = bonesList[nameList.IndexOf(equipsBone.parent.name)];
            }
        }


        Transform FindBone(List<Transform> boneList, Transform target)
        {
            foreach (Transform b in boneList)
                if (b.name == target.name)
                    return b;

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

            transform.SetParent(PlayerObj);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;

            partsList.Add(instantiatedObj);
        }

        void SpringSystemSetup()
        {
            foreach (var bone in bonesList)
            {
                if (!bone.name.Contains("Spacer"))
                    continue;

                SpringSystemIndex = SpringSystem.ParentsList.Count;
                SpringSystem.ParentsList.Add(bone);
                SpringSystem.Setup();
                break;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void CombineServerRpc(int[] IDs)
        {
            CombineClientRpc(IDs);
        }
        [ClientRpc]
        public void CombineClientRpc(int[] IDs)
        {
            if (!IsOwner)
                ModelIDs = new(IDs);

            StartCombine(IsOwner);
        }
    }
}