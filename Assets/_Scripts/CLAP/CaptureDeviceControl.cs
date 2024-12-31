using System;
using UnityEngine;
using OpenCvSharp;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace CLAPlus.Face2Face
{
    [DefaultExecutionOrder(-1)]
    public class CaptureDeviceControl : MonoBehaviour, IDisposable
    {
        private WebCamTexture webcam;

        [NonSerialized] public int width = default;
        [NonSerialized] public int height = default;
        private SynchronizationContext context = default;
        Color32[] tex1;
        byte[] tmp1;

        public int sourse = -1; // デバイスの番号

        public void InitCamera()
        {
            if (webcam != null)
                if (webcam.isPlaying)
                    webcam.Stop();

            webcam = new WebCamTexture(WebCamTexture.devices[sourse].name);
            context = SynchronizationContext.Current;
            if (!webcam.isPlaying)
                webcam.Play();
        }

        public async UniTask WaitOpen()
        {
            int t = 0;
            while (Math.Abs(webcam.GetPixel(0, 0).r) < 0.0001f)
            {
                await UniTask.Delay(100);
                t++;
                if (t > 600)
                {
                    throw new Exception("カメラを開けませんでした。\nCan't camera open!");
                }
            }

            width = webcam.width;
            height = webcam.height;

            tex1 = new Color32[width * height];
            tmp1 = new byte[width * height * 3];
        }

        public Mat Read()
        {
            Mat mat = new Mat(height, width, MatType.CV_8UC3);

            // tex1 = new Color32[width * height];

            context.Send((_) => { webcam?.GetPixels32(tex1); }, null);

            // byte[] tmp1 = new byte[width * height * 3];
            int c1, c2;
            Color32 c;
            for (int h = 0; h < height; h++)
            {
                c1 = h * width;
                for (int w = 0; w < width; w++)
                {
                    c2 = 3 * (c1 + w);
                    c = tex1[tex1.Length - (c1 + width - w)];
                    tmp1[c2] = c.b;
                    tmp1[c2 + 1] = c.g;
                    tmp1[c2 + 2] = c.r;
                }
            }

            Marshal.Copy(tmp1, 0, mat.Data, width * height * 3);

            return mat;
        }

        public void Dispose()
        {
            webcam.Stop();
        }

        void OnDestroy()
        {
            Dispose();
        }
    }
}
