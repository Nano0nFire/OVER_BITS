using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DACS.Extensions
{
    public class CustomAction : MonoBehaviour
    {
        public virtual EntityStatusData EntityData { get; set; }
        public virtual async UniTask Action(int index){}
    }
}
