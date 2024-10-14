using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DACS_P_Configs", menuName = "DACS/Projectile")]
public class DACS_P_ScriptableObject : ScriptableObject
{
    [NonReorderable] public List<DACS_P_Configs> P_ScriptableObject;
}
