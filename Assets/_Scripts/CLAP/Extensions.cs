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

        /// <summary>
        /// </summary>
        /// <param name="A">投射先</param>
        /// <param name="B">投射させたいベクトル</param>
        /// <returns></returns>
        public static Vector3 ProjectVector(Vector3 A, Vector3 B)
        {
            return Vector3.Dot(B, A) / Vector3.Dot(A, A) * A;
        }
    }
}
