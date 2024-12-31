using System.Collections.Generic;
using UnityEngine;

namespace DACS.Projectile
{
    [CreateAssetMenu(fileName = "DACS_P_Configs", menuName = "DACS/Projectile")]
    public class ProjectileSO : ScriptableObject
    {
        [NonReorderable] public List<DACS_P_Configs> P_ScriptableObject;
    }
}