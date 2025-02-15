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

        public static string ColorToHexString(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);

            return $"#{r:X2}{g:X2}{b:X2}";
        }

        public static Color HexToColorConverter(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Substring(1);

            byte r = 255, g = 255, b = 255, a = 255;

            r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }
    }
}
