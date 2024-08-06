using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CLA Parts Lists", menuName = "CLA", order = 1)]
public class CLAPartsLists : ScriptableObject
{
    [NonReorderable] public List<GameObject> baseBodyList = new();
    [NonReorderable] public List<GameObject> hairPartsList = new();
    [NonReorderable] public List<GameObject> facePartsList = new();
    [NonReorderable] public List<GameObject> chestPartsList = new();
    [NonReorderable] public List<GameObject> legPartsList = new();
    [NonReorderable] public List<GameObject> shoesPartsList = new();
}
