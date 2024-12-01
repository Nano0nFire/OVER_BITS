using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DACS_Entities", menuName = "DACS/Entities")]

public class DACS_Entities_ScriptableObject : ScriptableObject
{
    public int EntityTypeNum = 7; // 0~6
    [NonReorderable] public List<EntityConfigs> SystemEntity; // 0
    [NonReorderable] public List<EntityConfigs> Boss; // 1
    [NonReorderable] public List<EntityConfigs> MiddleBoss; // 2
    [NonReorderable] public List<EntityConfigs> TopEntities; // 3
    [NonReorderable] public List<EntityConfigs> MiddleEntities; // 4
    [NonReorderable] public List<EntityConfigs> BottomEntities; // 5
    [NonReorderable] public List<EntityConfigs> NeutralityEntities; // 6

    public List<EntityConfigs> GetList(int FirstIndex)
    {
        return FirstIndex switch
        {
            0 => SystemEntity,
            1 => Boss,
            2 => MiddleBoss,
            3 => TopEntities,
            4 => MiddleEntities,
            5 => BottomEntities,
            6 => NeutralityEntities,
            _ => null,
        };
    }

    public EntityConfigs GetEntity(int FirstIndex, int SecondIndex)
    {
        return GetList(FirstIndex)[SecondIndex];
    }
}
