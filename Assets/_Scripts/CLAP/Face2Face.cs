using DlibDotNet;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using Cysharp.Threading.Tasks;

namespace CLAPlus.Face2Face
{
    public partial class Face2Face : MonoBehaviour
    {
        #pragma warning disable 0649

        public bool isRunning = false;        /// <param name="isRunning">  非同期停止用        for stop ansync</param>
        bool StopTrack = false; /// <param name="StopTrack">  まばたきを止めるか  stop blink</param>

        //ロックトークン       locktoken --------------------------------------------------
        private object lock_fps = new object();
        private object lock_capture = new object();
        private object[] lock_imagebytes;
        //----------------------------------------------------------------------------------
        private int diff_time = 1000 / 60;  /// <param name="diff_time">非同期開始時間のずれ          time of diff async start</param>
        private IntPtr[] ptr;               /// <param name="ptr">カメラに保存された画像のポインタ    pointer of image taken by camera</param>
        /// //出力        output  --------------------------------------------------------------
        public bool LeftEyeCloseness = false;
        public bool RightEyeCloseness = false;
        public bool MouthCloseness = false;
        

        //キャッシュ用の変数         Variable for cache -----------------------------------
        private LinkedList<int> elapt_time = new LinkedList<int>();                      /// <param name="elapt_time">検出にかかった時間の配列 Array of detecting time</param>
        private FrontalFaceDetector[] detector;                                          /// <param name="detector"> 顔検出器                face detector</param>
        private ShapePredictor shape = new ShapePredictor();                             /// <param name="shape">    顔の特徴点検出器        Facial landmark detector</param>
        private float eye_L_ratio = default;                                             /// <param name="eye_L_o">左目の開き具合            Left eye openness</param>
        private float eye_R_ratio = default;                                             /// <param name="eye_R_o">右目の開き具合            Right eye openness</param>
        private float mouth_ratio = default;
        private float eye_L_Width = 70;                                             /// <param name="eye_L_Width">左目の横幅            Width of left eye</param>
        private float eye_R_Width = 70;                                             /// <param name="eye_R_Width">右目の横幅            Width of right eye</param>
        private bool measureWidth = false;
        //顔検出に関する変数     Variable for face detecting ------------------------------
        private byte[][] bytes;                             /// <param name="bytes">Mat→array2Dへの変換時のbyte配列   Byte array when converting from Mat to array2D</param>

        //Unityで設定するやつ  setting parameter on Unity ---------------------------------
        [SerializeField]
        private CaptureDeviceControl caputure;                             /// <param name="caputure"> ビデオキャプチャー      Video capture</param>

        [SerializeField]
        private string shape_file_68 = default;             /// <param name="shape_file_68">shape_file_68のファイル名            Name if shape_file_68</param>

        [Range(10, 288)]
        [SerializeField]
        public int fps_limit;                              /// <param name="fps_limit">fps制限                    limit of fps</param>
        [Range(2, 16)]
        [SerializeField]
        public int thread = 2;                                            /// <param name="thread">このプログラムをいくつのスレッドでやるか   How many threads use
        [Range(0, 1)]
        [SerializeField]
        private float eye_ratio_h_r = 0.3f;                   /// <param name="eye_ratio_h_r">右目を開けているときの縦横比   Aspect ratio when eye open</param>
        [Range(0, 1)]
        [SerializeField]
        private float eye_ratio_l_r = 0.2f;                   /// <param name="eye_ratio_l_r">右目を閉じているときの縦横比   Aspect ratio when eye close</param>
        [Range(0, 1)]
        [SerializeField]
        private float eye_ratio_h_l = 0.3f;                   /// <param name="eye_ratio_h_r">左目を開けているときの縦横比   Aspect ratio when eye open</param>
        [Range(0, 1)]
        [SerializeField]
        private float eye_ratio_l_l = 0.2f;                   /// <param name="eye_ratio_l_r">左目を閉じているときの縦横比   Aspect ratio when eye close</param>
        [Range(0, 1)]
        [SerializeField]
        private float mouth_ratio_h = 0.1f;                   /// <param name="eye_ratio_h_r">左目を開けているときの縦横比   Aspect ratio when eye open</param>
        [Range(0, 1)]
        [SerializeField]
        private float mouth_ratio_l = 0.5f;

#pragma warning restore 0649

        //----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 検出をする非同期メゾットを実行     Execute asynchronous method to detect
        /// </summary>
        private async void DetectAsync()
        {
            await UniTask.Delay(100);
            while (isRunning)
            {
                List<UniTask> tasks = new List<UniTask>();
                for (int i = 0; i < thread - 1; i++)
                {
                    tasks.Add(UniTask.Run(() => { FaceTrackLite(i); }));
                    await UniTask.Delay(diff_time);
                }
                await UniTask.WhenAny(tasks);
            }
        }

        private void FaceTrackLite(int threadNo)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            bool result;

            result = CalculateFaceTrack(threadNo);

            stopwatch.Stop();


            if (result)
            {
                lock (lock_fps)
                {

                    if (elapt_time.Count > 7)
                        elapt_time.RemoveFirst();
                    elapt_time.AddLast((int)stopwatch.ElapsedMilliseconds);
                }
            }
        }

        bool CalculateFaceTrack(int threadNo)
        {
            Array2D<RgbPixel> array2D = new Array2D<RgbPixel>();
            Mat image;

            lock (lock_capture)
            {
                image = caputure.Read();
                if (image.Data == null || ptr.Contains(image.Data))
                {
                    image.Dispose();
                    array2D.Dispose();
                    return false;
                }
                else
                    ptr[threadNo] = image.Data;
            }

            lock (lock_imagebytes[threadNo])
            {
                Marshal.Copy(image.Data, bytes[threadNo], 0, bytes[threadNo].Length);
                array2D = Dlib.LoadImageData<RgbPixel>(bytes[threadNo], (uint)image.Height, (uint)image.Width, (uint)(image.Width * image.ElemSize()));
            }

            Rectangle rectangles;
            rectangles = detector[threadNo].Operator(array2D).FirstOrDefault();

            if (rectangles == default)
            {
                image.Dispose();
                array2D.Dispose();
                return false;
            }

            DlibDotNet.Point[] points = new DlibDotNet.Point[68];

            using (FullObjectDetection shapes = shape.Detect(array2D, rectangles))
            {
                for (uint i = 0; i < 68; i++)
                {
                    points[i] = shapes.GetPart(i);
                }
                shapes.Dispose();
            }

            if (measureWidth)
            {
                eye_L_Width = Distance(points[36], points[39]);
                eye_R_Width = Distance(points[42], points[45]);
            }

            CalculateEyeOpenRatio(points);

            // 口の開け閉めの割合を計算
            Calculatemouth_ratio(points);

            image.Dispose();
            array2D.Dispose();
            return true;
        }

        private void Calculatemouth_ratio(Span<DlibDotNet.Point> points)
        {
            // 口のランドマークポイントを使用して、口の開け閉めの割合を計算
            float mouthHeight = Distance(points[62], points[66]);
            float mouthWidth = Distance(points[60], points[64]);
            mouth_ratio = mouthHeight / mouthWidth;
        }

        private void CalculateEyeOpenRatio(Span<DlibDotNet.Point> points)
        {
            // 口のランドマークポイントを使用して、口の開け閉めの割合を計算
            float lh = (Distance(points[38], points[40]) + Distance(points[37], points[41])) / 2;
            float rh = (Distance(points[43], points[47]) + Distance(points[44], points[46])) / 2;
            eye_L_ratio = lh / eye_L_Width;
            eye_R_ratio = rh / eye_R_Width;
        }

        ///<summary>初期設定後、非同期で顔検出を始める           Start face detection asynchronously after initialization</summary>
        void Start()
        {
            InitTracker();
        }

        public async void StartTracking()
        {
            if (isRunning)
                StopTracking();

            await UniTask.Delay(100);

            InitCamera();

            isRunning = true;
            //フェイストラッキング開始      start face tracking
            _ = UniTask.Run(DetectAsync);
        }

        public async void StopTracking()
        {
            RightEyeCloseness = false;
            LeftEyeCloseness = false;
            MouthCloseness = false;
            await UniTask.Delay(100);
            isRunning = false;
            FaceSync.Stop = true;
        }

        void InitTracker()
        {
            System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;

            var path = Path.Combine(Application.streamingAssetsPath, shape_file_68);

            if (!File.Exists(path))
            {
                Debug.LogError("Path for 68 face landmarks is invalid");
                return;
            }

            detector = new FrontalFaceDetector[thread - 1];
            for (int i = 0; i < thread - 1; i++)
            {
                detector[i] = Dlib.GetFrontalFaceDetector();
            }

            ptr = new IntPtr[thread - 1];

            try
            {
                shape = ShapePredictor.Deserialize(path);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return;
            }

            bytes = new byte[thread - 1][];
            lock_imagebytes = new object[thread - 1];
        }

        async void InitCamera()
        {
            caputure.InitCamera();

            Mat image;
            await caputure.WaitOpen();
            image = caputure.Read();

            ptr[0] = image.Data;
            var c = image.Width * image.Height * image.ElemSize();

            for (int i = 0; i < thread - 1; i++)
            {
                bytes[i] = new byte[c];
                lock_imagebytes[i] = new object();
            }
            if (image.IsEnabledDispose)
                image.Dispose();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void Update()
        {
            if (!isRunning)
                return;

            lock (lock_fps)
            {
                if (elapt_time.Count == 8)
                {
                    //非同期で走らせてるやつのずらす時間を生成  Clac time which make diff of async roop.
                    diff_time = (int)elapt_time.Average() / (thread - 1) * 2;
                    if (diff_time < 1000 / fps_limit)
                    {
                        diff_time = 1000 / fps_limit;
                    }
                }
            }

            if (StopTrack)
                return;

            float t = Time.deltaTime;

            LeftEyeCloseness = 1 - (eye_L_ratio - eye_ratio_l_l) / (eye_ratio_h_l - eye_ratio_l_l) > 0.6f;
            RightEyeCloseness = 1 - (eye_R_ratio - eye_ratio_l_r) / (eye_ratio_h_r - eye_ratio_l_r) > 0.6f;
            MouthCloseness = 1 - (mouth_ratio - mouth_ratio_l) / (mouth_ratio_h - mouth_ratio_l) > 0.6f;
        }

        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// まばたきの割合を調整する
        /// </summary>
        public async void BlinkAdjust()
        {
            StopTrack = true; // まばたきを止める

            int i;
            float tempL = 0, tempR = 0;

            eye_ratio_h_l = 0.3f;
            eye_ratio_h_r = 0.3f;
            eye_ratio_l_l = 0.2f;
            eye_ratio_l_r = 0.2f;

            // 目の横の長さを測定
            measureWidth = true;
            for (i = 0; i < 20; i++) // 20フレーム分の平均を取る
            {
                await UniTask.Delay(50); // 50ms待つ
                tempL += eye_L_Width;
                tempR += eye_R_Width;
            }
            measureWidth = false;

            eye_L_Width = tempL / 20;
            eye_R_Width = tempR / 20;

            // 開いた状態を測定
            for (i = 0, tempL = 0, tempR = 0; i < 20; i++) // 20フレーム分の平均を取る
            {
                await UniTask.Delay(50); // 50ms待つ
                tempL += eye_L_ratio;
                tempR += eye_R_ratio;
            }

            eye_ratio_h_l = tempL / 20;
            eye_ratio_h_r = tempR / 20;


            await UniTask.Delay(2000); // 1秒待つ

            // 閉じた状態を測定
            for (i = 0, tempL = 0, tempR = 0; i < 20; i++) // 20フレーム分の平均を取る
            {
                await UniTask.Delay(50); // 50ms待つ
                tempL += eye_L_ratio;
                tempR += eye_R_ratio;
            }

            eye_ratio_l_l = tempL / 20;
            eye_ratio_l_r = tempR / 20;

            StopTrack = false; // まばたきを再開
        }

        /// <summary>
        /// 口の調整する
        /// </summary>
        public async void MouthAdjust()
        {
            StopTrack = true; // まばたきを止める

            int i;
            float tempO = 0, tempC = 0;

            // 口があいた状態を計測
            for (i = 0; i < 20; i++) // 20フレーム分の平均を取る
            {
                await UniTask.Delay(50); // 50ms待つ
                tempO += mouth_ratio;
            }

            // 口が閉じた状態を計測
            for (i = 0; i < 20; i++) // 20フレーム分の平均を取る
            {
                await UniTask.Delay(50); // 50ms待つ
                tempC += mouth_ratio;
            }

            mouth_ratio_h = tempO / 20;
            mouth_ratio_l = tempC / 20;

            StopTrack = false; // まばたきを再開
        }



        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 成功率をログに流す       write success rate in log
        /// </summary>
        private void OnDestroy()
        {
            isRunning = false;
        }

        //--------------------------------------------------------------------------------------------------------
        private float Distance(DlibDotNet.Point point1, DlibDotNet.Point point2)
        {
            return Mathf.Sqrt((point1.X - point2.X) * (point1.X - point2.X) + (point1.Y - point2.Y) * (point1.Y - point2.Y));
        }
    }
}