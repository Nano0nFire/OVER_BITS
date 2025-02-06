using Unity.VisualScripting;
using UnityEngine;

namespace CLAPlus.Extension
{
    public static class Extensions
    {
        public static Vector3 SimplifyRotation(Vector3 input)
        {
            return new Vector3
                    (
                        (input.x>180) ? input.x-360 : input.x,
                        (input.y>180) ? input.y-360 : input.y,
                        (input.z>180) ? input.z-360 : input.z
                    );
        }
    }
}
