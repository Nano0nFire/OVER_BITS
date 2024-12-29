using System.Collections.Generic;
using UnityEngine;

namespace CLAPlus.CustomLiveAvatar
{
    [CreateAssetMenu(fileName = "CLA Parts Lists", menuName = "CLA", order = 1)]
    public class CLAPartsLists : ScriptableObject
    {
        [Header("Parts List")]
        [NonReorderable] public List<GameObject> baseBodyList = new();
        [NonReorderable] public List<GameObject> hairPartsList = new();
        [NonReorderable] public List<GameObject> facePartsList = new();
        [NonReorderable] public List<GameObject> topsPartsList = new();
        [NonReorderable] public List<GameObject> bottomsPartsList = new();
        [NonReorderable] public List<GameObject> shoesPartsList = new();

        public List<GameObject> GetList(int num)
        {
            return num switch
            {
                -1 => baseBodyList,
                0 => hairPartsList,
                1 => facePartsList,
                2 => topsPartsList,
                3 => bottomsPartsList,
                4 => shoesPartsList,
                _ => null,
            };
        }
    }
}