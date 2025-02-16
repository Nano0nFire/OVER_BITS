using System;
using System.Collections.Generic;
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

    [System.Serializable]
    public class SerializableColor
    {
        public float r, g, b, a;

        public SerializableColor() { }

        public SerializableColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        public SerializableColor(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public SerializableColor(bool UseRandom = false)
        {
            if (UseRandom)
            {
                System.Random random = new();
                r = (float)random.NextDouble();
                g = (float)random.NextDouble();
                b = (float)random.NextDouble();
                a = 1f;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
                a = 1f;
            }
        }

        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }

        public static void ToColors(ReadOnlySpan<SerializableColor> input, out List<Color> output)
        {
            int i = 0;
            output = new(input.Length);
            foreach (var sColor in input)
            {
                output.Add(sColor.ToColor()); // Add() を使って要素を追加
            }
        }
        public static void ToSerializableColors(ReadOnlySpan<Color> input, out List<SerializableColor> output)
        {
            int i = 0;
            output = new(input.Length);
            foreach (var sColor in input)
            {
                output.Add(new(sColor)); // Add() を使って要素を追加
            }
        }
    }
}
