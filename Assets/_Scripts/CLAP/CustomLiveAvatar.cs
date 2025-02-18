using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using CLAPlus;
using CLAPlus.Extension;
using Cysharp.Threading.Tasks;
using System.Linq;

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
        [SerializeField] ItemDataBase itemDataBase; // モデルデータ
        public List<int> ModelListIndexs = new(); // 各装備品のリストの番号
        public List<int> ModelIDs = new(); // 各装備品のID
        // NetworkList<int> syncedModelIDs = new();
        [SerializeField] Transform PlayerObj; // プレイヤーキャラクター
        [SerializeField] GameObject RootBone;
        [SerializeField] SpringSystem SpringSystem;
        [SerializeField] Transform skins;
        static readonly List<string> PartsType = new() { "Base", "Face", "Tops", "Bottoms", "Shoes", "Hair"}; // 最終的にメッシュをavatarObjと同じ階層に置くときにつける名前の一覧

        public List<Color> colors = new(6);
        int SpringSystemIndex = -1;

        [Space(1),Header("Ex Options")]
        [SerializeField] SkinnedMeshRenderer headback;

        [Space(1),Header("Debug")]
        /// <summary>
        /// ベースモデルのボーンと装備するモデルのボーンを足したもの<br />
        /// 順番はrootボーンから階層順になっている
        /// </summary>
        /// <returns></returns>
        [SerializeField] List<Transform> bonesList = new();
        [SerializeField] List<GameObject> partsList = new(); // 装備するモデルデータ
        bool IsLoaded = false;
        bool IsCombining = false;

        void Start()
        {
            if (headback != null)
                headback.materials[0] = new Material(headback.materials[0]);
        }
        public override async void OnNetworkSpawn() // 他のクライアント側のシーンでスポーンした時を想定
        {
            if (IsOwner)
                return;

            await UniTask.WaitUntil(() => ClientGeneralManager.IsLoaded);
            CombineServerRpc((long)ClientGeneralManager.clientID);
        }

        public async void Combiner()
        {
            if (IsOwner)
            {
                await PlayerDataManager.SaveData(ModelIDs, "CustomLifeAvatarParts");
                SerializableColor.ToSerializableColors(colors.ToArray(), out var output);
                await PlayerDataManager.SaveData(output, "CustomLifeAvatarColors");
                UpdateDataServerRpc(ModelIDs.ToArray(), colors.ToArray());
                if (ClientGeneralManager.IsLoaded)
                    CombineServerRpc();
            }
        }

        void StartCombine(bool UseSpringSystem = false)
        {
            if (IsCombining) // 処理を混同させないために、アバター作成中には合成を開始させない
                return;
            IsCombining = true;
            Reset();
            SetModels();
            SetBones();
            BoneReset();
            ClreanUpObjects();
            if (UseSpringSystem)
                SpringSystemSetup();

            if (headback != null)
                headback.materials[0].color = colors[5];

            Clear(); // 使用したListをリセット

            IsCombining = false;
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
                    Transform[] newBones = new Transform[smRenderer.bones.Length];

                    for (int i = 0; i < partsBonesArray.Length; i++)
                    {
                        if (partsBonesArray[i] == null)
                            continue;
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
                string name = "empty";
                int index = 0;
                foreach (string key in  PartsType)
                {
                    if (part.name.Contains(key))
                    {
                        name = key;
                        index = PartsType.IndexOf(name);
                        Debug.Log(name + PartsType.IndexOf(name));
                        break;
                    }
                }

                foreach (SkinnedMeshRenderer smr in part.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    smr.gameObject.name = name;
                    smr.materials[0] = new(smr.materials[0]);
                    smr.materials[0].color = colors[index];
                    smr.transform.parent = skins;
                }
                Destroy(part);
            }
        }

        void SpawnModel(int ListIndex)
        {
            var obj = itemDataBase.GetItem(ModelListIndexs[ListIndex], ModelIDs[ListIndex]).ItemModel;
            if (obj == null)
                return;

            var instantiatedObj = Instantiate(obj);

            var transform = instantiatedObj.transform;

            transform.SetParent(PlayerObj);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;

            partsList.Add(instantiatedObj);

            Debug.LogWarning(instantiatedObj.name);
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
        public void CombineServerRpc(long clientID = -1)
        {
            if (!IsLoaded)
            {
                WaitLoading();
                return;
            }
            if (clientID == -1)
            {
                CombineClientRpc(ModelIDs.ToArray(), colors.ToArray());
                return;
            }

            ClientRpcParams rpcParams = new() // ClientRPCを送る対象を選択
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { (ulong)clientID } }
            };
            CombineClientRpc(ModelIDs.ToArray(), colors.ToArray(), rpcParams);
        }

        async void WaitLoading(long clientID = -1)
        {
            if (!IsServer)
                return;
            await UniTask.WaitUntil(()=>IsLoaded); // ロード待ち
            if (clientID == -1)
            {
                CombineClientRpc(ModelIDs.ToArray(), colors.ToArray());
                return;
            }

            ClientRpcParams rpcParams = new() // ClientRPCを送る対象を選択
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { (ulong)clientID } }
            };
            CombineClientRpc(ModelIDs.ToArray(), colors.ToArray(), rpcParams);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateDataServerRpc(int[] IDs, Color[] Colors)
        {
            if (!IsServer)
                return;

            ModelIDs = new(IDs);
            colors = new(Colors);
            IsLoaded = true;
        }

        [ClientRpc]
        public void CombineClientRpc(int[] IDs, Color[] Colors, ClientRpcParams rpcParams = default)
        {
            if (!IsOwner)
            {
                ModelIDs = new(IDs);
                colors = new(Colors);
            }
            IsLoaded = true;
            StartCombine(true);
        }
    }
}