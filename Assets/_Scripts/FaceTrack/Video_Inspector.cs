// using DlibDotNet;
// using OpenCvSharp;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Runtime.InteropServices;
// using System.Threading.Tasks;
// using UnityEngine;
// using CLAPlus_Face2Face.Extention;
// using System.IO;

// namespace CLAPlus_Face2Face
// {
//     public partial class FaceTracking : MonoBehaviour
//     {
//         #pragma warning disable 0649

//         private bool inputR = false;        /// <param name="inputR"> Rボタンが押された     pushed R button</param>
//         private bool running = true;        /// <param name="running">  非同期停止用        for stop ansync</param>
//         public bool StopBlink = false; /// <param name="StopBlink">  まばたきを止めるか  stop blink</param>
//         public bool checkC, checkO;
//         //ロックトークン       locktoken --------------------------------------------------
//         private object lock_fps = new object();
//         private object lock_pos_rot = new object();
//         private object lock_eye_blink = new object();
//         private object lock_capture = new object();
//         private object lock_landmarks = new object();
//         private object lock_isSuccess = new object();
//         private object[] lock_out_mat;
//         private object[] lock_imagebytes;
//         //----------------------------------------------------------------------------------
//         private int diff_time = 1000 / 60;  /// <param name="diff_time">非同期開始時間のずれ          time of diff async start</param>
//         private IntPtr[] ptr;               /// <param name="ptr">カメラに保存された画像のポインタ    pointer of image taken by camera</param>
//         //出力        output  --------------------------------------------------------------
//         public Vector3 Position { get; private set; } = new Vector3();
//         public Quaternion Rotation { get; private set; } = new Quaternion();
//         public float LeftEyeCloseness = default;
//         public float RightEyeCloseness = default;
//         public float mouthOpenRatio = default;
//         public Vector3 LeftEyeRotation { get; private set; } = new Vector3();
//         public bool IsSuccess { get; private set; } = false;
//         public Vector3 RightEyeRotation { get; private set; } = new Vector3();
//         private DlibDotNet.Point[] landmark_detection;

//         //キャッシュ用の変数         Variable for cache -----------------------------------
//         private Vector3 pos;                                                             /// <param name="pos">最終的に計算された位置      Final calculated position</param>
//         private Vector3 rot;                                                             /// <param name="rot">最終的に計算された回転      Final calculated rotation</param>
//         private LinkedList<Vector3> pos_chain = new LinkedList<Vector3>();               /// <param name="pos_chain">検出位置の配列   Array of detected positions</param>
//         private LinkedList<Vector3> rot_chain = new LinkedList<Vector3>();               /// <param name="rot_chain">検出回転の配列   Array of detected rotations</param>
//         private LinkedList<float> eye_L = new LinkedList<float>();                       /// <param name="eye_L">左目の開き具合の配列      Array of left eye opening</param>
//         private LinkedList<float> eye_R = new LinkedList<float>();                       /// <param name="eye_R">右目の開き具合の配列      Array of right eye opening</param>
//         private LinkedList<Vector3> eye_rot_L = new LinkedList<Vector3>();               /// <param name="eye_rot_L">左目の回転の配列      Array of left eye rotation</param>
//         private LinkedList<Vector3> eye_rot_R = new LinkedList<Vector3>();               /// <param name="eye_rot_R">右目の回転の配列      Array of right eye rotation</param>
//         private LinkedList<DateTime> fin_time = new LinkedList<DateTime>();              /// <param name="fin_time">検出が終わった時間の配列   Array of detect time finished</param>
//         private LinkedList<int> elapt_time = new LinkedList<int>();                      /// <param name="elapt_time">検出にかかった時間の配列 Array of detecting time</param>
//         private int[] frame_time = new int[7];                                           /// <param name="frame_time">フレームが切り替わる時間の配列 Array of frame time</param>
//         private FrontalFaceDetector[] detector;                                          /// <param name="detector"> 顔検出器                face detector</param>
//         private ShapePredictor shape = new ShapePredictor();                             /// <param name="shape">    顔の特徴点検出器        Facial landmark detector</param>
//         private DlibDotNet.Point[][] eye_point_L;                                        /// <param name="eye_point_L">左目の検出された点    Point of left eye</param>
//         private float[] eye_ratio_L;                                                     /// <param name="eye_ratio_L">左目の縦横比          Aspect ratio of left eye</param>
//         private DlibDotNet.Point[][] eye_point_R;                                        /// <param name="eye_point_R">右目の検出された点    Point of right eye</param>
//         private float[] eye_ratio_R;                                                     /// <param name="eye_ratio_L">右目の縦横比          Aspect ratio of right eye</param>
//         private float eye_L_c = default;                                                 /// <param name="eye_L_c">左目の開き具合            Left eye openness</param>
//         private float eye_R_c = default;                                                 /// <param name="eye_R_c">右目の開き具合            Right eye openness</param>
//         private float eye_L_ratio = default;                                             /// <param name="eye_L_o">左目の開き具合            Left eye openness</param>
//         private float eye_R_ratio = default;                                             /// <param name="eye_R_o">右目の開き具合            Right eye openness</param>


//         //顔検出に関する変数     Variable for face detecting ------------------------------
//         private byte[][] bytes;                             /// <param name="bytes">Mat→array2Dへの変換時のbyte配列   Byte array when converting from Mat to array2D</param>
//         private Mat model_points_mat;                       /// <param name="model_points_mat">3Dモデルの点のMat       3D model points Mat</param>
//         private Mat camera_matrix_mat;                      /// <param name="camera_matrix_mat">カメラ行列のMat        Camera matrix Mat</param>
//         private Mat dist_coeffs_mat;                        /// <param name="dist_coeffes_mat">レンズの補正のためのMat Mat for lens correction</param>
//         private double[][] proj;                            /// <param name="proj">投影行列の配列                      Array of projected matrix</param>
//         private double[][] pos_double;                      /// <param name="pos_double">位置の配列(x,y,z)             Array of positions (x, y, z)</param>
//         //検出の成功・失敗      success or fail detect ------------------------------------
//         private int suc = 0;
//         private int fail = 0;

//         //Unityで設定するやつ  setting parameter on Unity ---------------------------------
//         [SerializeField]
//         private Video caputure;                             /// <param name="caputure"> ビデオキャプチャー      Video capture</param>
//         [SerializeField]
//         private SkinnedMeshRenderer smRenderer;

//         [SerializeField]
//         private string shape_file_68 = default;             /// <param name="shape_file_68">shape_file_68のあるパス            Path with shape_file_68</param>

//         [Range(30, 288)]
//         [SerializeField]
//         private int fps_limit;                              /// <param name="fps_limit">fps制限                    limit of fps</param>

//         [SerializeField]
//         private Vector3 rotation_verocity_ristrict = new Vector3(30, 30, 30);                        /// <param name="rotation_verocity_ristrict">回転速度の制限        Speed ​​limit of rotation</param>
//         [SerializeField]
//         private Vector3 rotation_range = new Vector3(45, 90, 45);                                    /// <param name="rotation_range">頭の回転できる範囲                Range of head rotation</param>
//         [SerializeField]
//         private Vector3 pos_offset = new Vector3(0, 0, 0);                 /// <param name="pos_offset">3Dモデルからの位置のオフセット        Position offset from 3D model</param>
//         [SerializeField]
//         private Vector3 rot_offset = new Vector3(0, 0, 0);                 /// <param name="rot_offset">3Dモデルからの回転のオフセット        rotation offset from 3D model</param>
//         public Transform model;                                            /// <param name="model">3Dモデル                                   3Dmodel</param>                                                               /// <param name="shape_file_68">shape_file_68のあるパス            Path with shape_file_68</param>
//         [SerializeField]
//         private SmoothingMethod smoothing;
//         [Range(1, 30)]
//         [SerializeField]
//         private int smooth = 16;                                            /// <param name="smooth">動きのスムーズさ。大きいとスムーズになるが遅くなる。 Smoothness of movement. It will be smooth if it is large, but it will be slow</param>
//         [Range(0, 1)]
//         [SerializeField]
//         private float alpha = 0.85f;
//         [Range(1, 10)]
//         [SerializeField]
//         private int resolution = 1;
//         [Range(2, 16)]
//         [SerializeField]
//         public int thread = 2;                                            /// <param name="thread">このプログラムをいくつのスレッドでやるか   How many threads use
//         [SerializeField]
//         private bool un_safe = false;

//         [Range(0, 1)]
//         [SerializeField]
//         private float eye_ratio_h_r = 0.3f;                   /// <param name="eye_ratio_h_r">右目を開けているときの縦横比   Aspect ratio when eye open</param>
//         [Range(0, 1)]
//         [SerializeField]
//         private float eye_ratio_l_r = 0.2f;                   /// <param name="eye_ratio_l_r">右目を閉じているときの縦横比   Aspect ratio when eye close</param>
//         [Range(0, 1)]
//         [SerializeField]
//         private float eye_ratio_h_l = 0.3f;                   /// <param name="eye_ratio_h_r">左目を開けているときの縦横比   Aspect ratio when eye open</param>
//         [Range(0, 1)]
//         [SerializeField]
//         private float eye_ratio_l_l = 0.2f;                   /// <param name="eye_ratio_l_r">左目を閉じているときの縦横比   Aspect ratio when eye close</param>
// #pragma warning restore 0649
//         //----------------------------------------------------------------------------------------------------------------------
//         /// <summary>
//         /// 検出をする非同期メゾットを実行     Execute asynchronous method to detect
//         /// </summary>
//         private async void DetectAsync()
//         {
//             await Task.Delay(100);
//             while (running)
//             {
//                 List<Task> tasks = new List<Task>();
//                 for (int i = 0; i < thread - 1; i++)
//                 {
//                     tasks.Add(Task.Run(() => { FaceTrack(i); }));
//                     await Task.Delay(diff_time);
//                 }
//                 await Task.WhenAny(tasks);
//             }
//         }

//         //-----------------------------------------------------------------------------------------------------------------------
//         /// <summary>
//         /// 顔検出＆計算      Face detection & calculation
//         /// </summary>
//         private void FaceTrack(int threadNo)
//         {
//             System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
//             stopwatch.Start();
//             bool result;
//             Vector3 est_pos;
//             Vector3 est_rot;

//             result = Dlib68(threadNo, out est_pos, out est_rot);

//             est_rot = RotReSharp(est_rot + rot_offset, est_pos);

//             //中心を初期化    initialize center
//             if (inputR)
//             {
//                 lock (lock_pos_rot)
//                 {
//                     for (int i = 0; i < smooth; i++)
//                     {
//                         rot_chain.AddLast(Vector3.zero);
//                         rot_chain.RemoveFirst();
//                     }
//                 }
//                 inputR = false;
//             }
//             est_rot = EstimateErrCheck(est_rot, rotation_verocity_ristrict, rot_chain.Skip(smooth - thread - 1).Average());
//             est_rot = RangeCheck(est_rot, rotation_range);

//             lock (lock_pos_rot)
//             {
//                 if (smoothing == SmoothingMethod.Average)
//                 {
//                     rot_chain.AddLast(est_rot);
//                     rot_chain.RemoveFirst();
//                     pos = pos_chain.Average();
//                     rot = rot_chain.Average();
//                 }
//                 else
//                 {
//                     rot = (1.0f - alpha) * est_rot + alpha * rot;
//                 }
//             }

//             lock (lock_isSuccess)
//             {
//                 IsSuccess = result;
//             }

//             stopwatch.Stop();
//             if (result)
//             {
//                 ++suc;
//                 lock (lock_fps)
//                 {

//                     if (fin_time.Count > 7)
//                     {
//                         fin_time.RemoveFirst();
//                         elapt_time.RemoveFirst();
//                     }
//                     fin_time.AddLast(DateTime.Now);
//                     elapt_time.AddLast((int)stopwatch.ElapsedMilliseconds);
//                 }
//             }
//             else
//             {
//                 ++fail;
//             }

//         }

//         //--------------------------------------------------------------------------------------------------------
//         /// <summary>
//         /// Dlib68を利用した顔検出                   Face detection using Dlib68
//         /// </summary>
//         /// <param name="threadNo">スレッド番号      Thread number</param>
//         /// <param name="est_pos">推定された位置     Estimated position</param>
//         /// <param name="est_rot">推定された回転     Estimated quotation</param>
//         /// <returns>推定できたか                    Whether it could be estimated</returns>
//         private bool Dlib68(int threadNo, out Vector3 est_pos, out Vector3 est_rot)
//         {
//             Mat image_r = new Mat();
//             Array2D<RgbPixel> array2D = new Array2D<RgbPixel>();
//             Mat image = new Mat();
//             try
//             {
//                 lock (lock_capture)
//                 {
//                     image_r = caputure.Read();
//                     if (image_r.Data == null)
//                     {
//                         throw new NullReferenceException("capture is null");
//                     }

//                     if (ptr.Contains(image_r.Data))
//                     {
//                         throw new InvalidOperationException("taken same data");
//                     }
//                     else
//                     {
//                         ptr[threadNo] = image_r.Data;
//                     }
//                 }

//                 if (resolution == 1)
//                 {
//                     image = image_r.Clone();
//                 }
//                 else
//                 {
//                     Cv2.Resize(image_r, image, new Size(image_r.Cols / resolution, image_r.Rows / resolution));
//                 }

//                 GC.KeepAlive(image_r);

//                 lock (lock_imagebytes[threadNo])
//                 {
//                     Marshal.Copy(image.Data, bytes[threadNo], 0, bytes[threadNo].Length);
//                     array2D = Dlib.LoadImageData<RgbPixel>(bytes[threadNo], (uint)image.Height, (uint)image.Width, (uint)(image.Width * image.ElemSize()));
//                 }

//                 Rectangle rectangles = default;
//                 if (un_safe)
//                 {
//                     rectangles = detector[0].Operator(array2D).FirstOrDefault();
//                 }
//                 else
//                 {
//                     rectangles = detector[threadNo].Operator(array2D).FirstOrDefault();
//                 }

//                 DlibDotNet.Point[] points = new DlibDotNet.Point[68];

//                 if (rectangles == default)
//                 {
//                     throw new InvalidOperationException("rectangles has no elements");
//                 }

//                 using (FullObjectDetection shapes = shape.Detect(array2D, rectangles))
//                 {
//                     for (uint i = 0; i < 68; i++)
//                     {
//                         points[i] = shapes.GetPart(i);
//                     }
//                     lock (lock_landmarks)
//                     {
//                         landmark_detection = points;
//                     }
//                 }

//                 Point2f[] image_points = new Point2f[8];
//                 image_points[0] = new Point2f(points[30].X, points[30].Y);
//                 image_points[1] = new Point2f(points[8].X, points[8].Y);
//                 image_points[2] = new Point2f(points[45].X, points[45].Y);
//                 image_points[3] = new Point2f(points[36].X, points[36].Y);
//                 image_points[4] = new Point2f(points[54].X, points[54].Y);
//                 image_points[5] = new Point2f(points[48].X, points[48].Y);
//                 image_points[6] = new Point2f(points[42].X, points[42].Y);
//                 image_points[7] = new Point2f(points[39].X, points[39].Y);
//                 var image_points_mat = new Mat(image_points.Length, 1, MatType.CV_32FC2, image_points);
//                 eye_point_R[threadNo][0] = points[42]; eye_point_L[threadNo][0] = points[39];
//                 eye_point_R[threadNo][1] = points[45]; eye_point_L[threadNo][1] = points[36];
//                 eye_point_R[threadNo][2] = points[43]; eye_point_L[threadNo][2] = points[38];
//                 eye_point_R[threadNo][3] = points[47]; eye_point_L[threadNo][3] = points[40];
//                 eye_point_R[threadNo][4] = points[44]; eye_point_L[threadNo][4] = points[37];
//                 eye_point_R[threadNo][5] = points[46]; eye_point_L[threadNo][5] = points[41];

//                 Mat rvec_mat = new Mat();
//                 Mat tvec_mat = new Mat();
//                 Mat projMatrix_mat = new Mat();
//                 Cv2.SolvePnP(model_points_mat, image_points_mat, camera_matrix_mat, dist_coeffs_mat, rvec_mat, tvec_mat);
//                 Marshal.Copy(tvec_mat.Data, pos_double[threadNo], 0, 3);
//                 Cv2.Rodrigues(rvec_mat, projMatrix_mat);
//                 Marshal.Copy(projMatrix_mat.Data, proj[threadNo], 0, 9);

//                 est_pos.x = -(float)pos_double[threadNo][0];
//                 est_pos.y = (float)pos_double[threadNo][1];
//                 est_pos.z = (float)pos_double[threadNo][2];

//                 est_rot = RotMatToQuatanion(proj[threadNo]).eulerAngles;
//                 est_rot.x *= -1;

//                 BlinkTracker(threadNo, eye_point_L[threadNo], eye_point_R[threadNo], est_rot);

//                 // 口の開け閉めの割合を計算
//                 CalculateMouthOpenRatio(points);

//                 image_points_mat.Dispose();
//                 rvec_mat.Dispose();
//                 tvec_mat.Dispose();
//                 projMatrix_mat.Dispose();
//                 GC.KeepAlive(image);
//             }
//             catch (Exception e)
//             {
//                 Debug.Log(e.ToString());
//                 est_pos = pos; est_rot = rot;
//                 if (array2D.IsEnableDispose)
//                 {
//                     array2D.Dispose();
//                 }
//                 if (image.IsEnabledDispose)
//                 {
//                     image.Dispose();
//                 }
//                 return false;
//             }
//             if (array2D.IsEnableDispose)
//             {
//                 array2D.Dispose();
//             }
//             lock (lock_imagebytes[threadNo])
//             {
//                 if (image.IsEnabledDispose)
//                 {
//                     image.Dispose();
//                 }
//             }

//             return true;
//         }

//         /// <summary>
//         /// まばたきを推定
//         /// </summary>
//         /// <param name="threadNo">走らせるスレッド番号</param>
//         /// <param name="left">左目のPoint</param>
//         /// <param name="right">右目のPoint</param>
//         /// <param name="est_rot">推定された回転</param>
//         private void BlinkTracker(int threadNo, DlibDotNet.Point[] left, DlibDotNet.Point[] right, Vector3 est_rot)
//         {
//             if (StopBlink)
//             {
//                 if (Mathf.Cos(est_rot.x) > Mathf.Sqrt(0.5f) && Mathf.Cos(est_rot.y) > Mathf.Sqrt(0.5f))
//                 {
//                     eye_L_ratio = (Distance(left[2], left[3]) + Distance(left[4], left[5])) / 2 / Mathf.Cos(est_rot.x * Mathf.Deg2Rad) / Distance(left[0], left[1]) * Mathf.Cos(est_rot.y * Mathf.Deg2Rad);
//                     eye_R_ratio = (Distance(right[2], right[3]) + Distance(right[4], right[5])) / 2 / Mathf.Cos(est_rot.x * Mathf.Deg2Rad) / Distance(right[0], right[1]) * Mathf.Cos(est_rot.y * Mathf.Deg2Rad);
//                 }
//                 else
//                 {
//                     eye_L_ratio = (Distance(left[2], left[3]) + Distance(left[4], left[5])) / 2 / Distance(left[0], left[1]);
//                     eye_R_ratio = (Distance(right[2], right[3]) + Distance(right[4], right[5])) / 2 / Distance(right[0], right[1]);
//                 }
//             }
//             else
//             {
//                 // 頭の回転が一定の範囲内であるかをチェック
//                 if (Mathf.Cos(est_rot.x) > Mathf.Sqrt(0.5f) && Mathf.Cos(est_rot.y) > Mathf.Sqrt(0.5f))
//                 {
//                     // 左目のまばたきの割合を計算
//                     eye_ratio_L[threadNo] = 1.0f - ((Distance(left[2], left[3]) + Distance(left[4], left[5])) / Mathf.Cos(est_rot.x * Mathf.Deg2Rad) / Distance(left[0], left[1]) * Mathf.Cos(est_rot.y * Mathf.Deg2Rad) / 2.0f - eye_ratio_l_l) / (eye_ratio_h_l - eye_ratio_l_l);
//                     // 右目のまばたきの割合を計算
//                     eye_ratio_R[threadNo] = 1.0f - ((Distance(right[2], right[3]) + Distance(right[4], right[5])) / Mathf.Cos(est_rot.x * Mathf.Deg2Rad) / Distance(right[0], right[1]) * Mathf.Cos(est_rot.y * Mathf.Deg2Rad) / 2.0f - eye_ratio_l_r) / (eye_ratio_h_r - eye_ratio_l_r);
//                 }
//                 else
//                 {
//                     // 左目のまばたきの割合を計算（回転を考慮しない場合）
//                     eye_ratio_L[threadNo] = 1.0f - ((Distance(left[2], left[3]) + Distance(left[4], left[5])) / Distance(left[0], left[1]) / 2.0f - eye_ratio_l_l) / (eye_ratio_h_l - eye_ratio_l_l);
//                     // 右目のまばたきの割合を計算（回転を考慮しない場合）
//                     eye_ratio_R[threadNo] = 1.0f - ((Distance(right[2], right[3]) + Distance(right[4], right[5])) / Distance(right[0], right[1]) / 2.0f - eye_ratio_l_r) / (eye_ratio_h_r - eye_ratio_l_r);
//                 }

//                 // まばたきの割合をスムージングするためのロック
//                 lock (lock_eye_blink)
//                 {
//                     if (smoothing == SmoothingMethod.Average)
//                     {
//                         // 平均スムージングを適用
//                         eye_L.RemoveFirst(); eye_L.AddLast(eye_ratio_L[threadNo]);
//                         eye_R.RemoveFirst(); eye_R.AddLast(eye_ratio_R[threadNo]);
//                         eye_L_c = eye_L.Average();
//                         eye_R_c = eye_R.Average();
//                     }
//                     else
//                     {
//                         // 指数移動平均スムージングを適用
//                         eye_L_c = (1.0f - alpha) * eye_ratio_L[threadNo] + alpha * eye_L_c;
//                         eye_R_c = (1.0f - alpha) * eye_ratio_R[threadNo] + alpha * eye_R_c;
//                     }
//                 }
//             }
//         }

//         private void CalculateMouthOpenRatio(DlibDotNet.Point[] points)
//         {
//             // 口のランドマークポイントを使用して、口の開け閉めの割合を計算
//             float mouthHeight = Distance(points[62], points[66]);
//             float mouthWidth = Distance(points[60], points[64]);
//             mouthOpenRatio = mouthHeight / mouthWidth;
//         }

//         ///<summary>初期設定後、非同期で顔検出を始める           Start face detection asynchronously after initialization</summary>
//         async void Start()
//         {
//             if (caputure == null)
//             {
//                 Debug.LogError("Video is null");

// #if UNITY_EDITOR
//                 UnityEditor.EditorApplication.isPlaying = false;
// #else
//                 Application.Quit();
// #endif
//                 return;
//             }

//             System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
//             process.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
//             Mat image_r = new Mat();
//             await caputure.WaitOpen();
//             image_r = caputure.Read();
//             Mat image = new Mat();
//             if (resolution == 1)
//             {
//                 image = image_r.Clone();
//             }
//             else
//             {
//                 Cv2.Resize(image_r, image, new Size(image_r.Cols / resolution, image_r.Rows / resolution));
//             }

//             if (!File.Exists(shape_file_68))
//             {
//                 Debug.LogError("Path for 68 face landmarks is invalid");
// #if UNITY_EDITOR
//                 UnityEditor.EditorApplication.isPlaying = false;
// #else
//                 Application.Quit();
// #endif
//                 return;
//             }
//             if (un_safe)
//             {
//                 detector = new FrontalFaceDetector[1];
//                 detector[0] = Dlib.GetFrontalFaceDetector();
//             }
//             else
//             {
//                 detector = new FrontalFaceDetector[thread - 1];
//                 for (int i = 0; i < thread - 1; i++)
//                 {
//                     detector[i] = Dlib.GetFrontalFaceDetector();
//                 }
//             }
//             landmark_detection = new DlibDotNet.Point[68];
//             ptr = new IntPtr[thread - 1];
//             proj = new double[thread - 1][];
//             pos_double = new double[thread - 1][];
//             eye_point_L = new DlibDotNet.Point[thread - 1][];
//             eye_ratio_L = new float[thread - 1];
//             eye_point_R = new DlibDotNet.Point[thread - 1][];
//             eye_ratio_R = new float[thread - 1];
//             try
//             {
//                 shape = ShapePredictor.Deserialize(shape_file_68);
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError(e.ToString());
//                 Quit();
//                 while (true) { }
//             }
//             dist_coeffs_mat = new Mat(4, 1, MatType.CV_64FC1, 0);
//             var focal_length = image.Cols;
//             var center = new Point2d(image.Cols / 2, image.Rows / 2);
//             var camera_matrix = new double[3, 3] { { focal_length, 0, center.X }, { 0, focal_length, center.Y }, { 0, 0, 1 } };
//             camera_matrix_mat = new Mat(3, 3, MatType.CV_64FC1, camera_matrix);
//             SetmodelPoints();
//             for (int i = 0; i < thread - 1; i++)
//             {
//                 proj[i] = new double[9];
//                 pos_double[i] = new double[3];
//                 eye_point_L[i] = new DlibDotNet.Point[6];
//                 eye_point_R[i] = new DlibDotNet.Point[6];
//             }

//             //上記以外の設定       other setting
//             ptr[0] = image_r.Data;

//             pos = transform.position - pos_offset;
//             rot = transform.eulerAngles;

//             for (int i = 0; i < smooth; i++)
//             {
//                 pos_chain.AddLast(pos);
//                 rot_chain.AddLast(rot);
//             }

//             for (int i = 0; i < 8; i++)
//             {
//                 eye_L.AddLast(0.0f);
//                 eye_R.AddLast(0.0f);
//                 eye_rot_L.AddLast(Vector3.zero);
//                 eye_rot_R.AddLast(Vector3.zero);
//             }

//             bytes = new byte[thread - 1][];
//             lock_imagebytes = new object[thread - 1];
//             lock_out_mat = new object[thread - 1];
//             for (int i = 0; i < thread - 1; i++)
//             {
//                 bytes[i] = new byte[image.Width * image.Height * image.ElemSize()];
//                 lock_imagebytes[i] = new object();
//                 lock_out_mat[i] = new object();
//             }
//             if (image.IsEnabledDispose)
//             {
//                 image.Dispose();
//             }
//             if (model == null)
//             {
//                 model = transform;
//             }

//             //フェイストラッキング開始      start face tracking
//             _ = Task.Run(DetectAsync);
//         }

//         //--------------------------------------------------------------------------------------------------------------------------------
//         /// <summary>
//         /// 強制的に停止させる関数。ファイルを読み込めなかった時用。    Function that this process stop. For occer error in reading file.
//         /// </summary>
//         private void Quit()
//         {
// #if UNITY_EDITOR
//             UnityEditor.EditorApplication.isPlaying = false;
// #elif UNITY_STANDALONE
//     UnityEngine.Application.Quit();
// #endif
//         }

//         //--------------------------------------------------------------------------------------------------------
//         /// <summary>
//         /// 3Dモデルの点を設定
//         /// </summary>
//         private void SetmodelPoints()
//         {
//             var model_points = new Point3f[8];
//             model_points[0] = new Point3f(0.0f, 0.03f, 0.11f);
//             model_points[1] = new Point3f(0.0f, -0.06f, 0.08f);
//             model_points[2] = new Point3f(-0.048f, 0.07f, 0.066f);
//             model_points[3] = new Point3f(0.048f, 0.07f, 0.066f);
//             model_points[4] = new Point3f(-0.03f, -0.007f, 0.088f);
//             model_points[5] = new Point3f(0.03f, -0.007f, 0.088f);
//             model_points[6] = new Point3f(-0.015f, 0.07f, 0.08f);
//             model_points[7] = new Point3f(0.015f, 0.07f, 0.08f);

//             model_points_mat = new Mat(model_points.Length, 1, MatType.CV_32FC3, model_points);
//         }

//         //--------------------------------------------------------------------------------------------------------------------------------
//         private void Update()
//         {
//             try
//             {
//                 lock (lock_eye_blink)
//                 {
//                     if (eye_L_c < 0)
//                     {
//                         eye_L_c = 0;
//                     }
//                     else if (eye_L_c > 1)
//                     {
//                         eye_L_c = 1;
//                     }

//                     if (eye_R_c < 0)
//                     {
//                         eye_R_c = 0;
//                     }
//                     else if (eye_R_c > 1)
//                     {
//                         eye_R_c = 1;
//                     }

//                     LeftEyeCloseness = eye_L_c;
//                     RightEyeCloseness = eye_R_c;
//                 }

//                 lock (lock_fps)
//                 {
//                     if (fin_time.Count == 8)
//                     {
//                         LinkedListNode<DateTime> node = fin_time.First;
//                         for (int i = 0; i < 7; i++)
//                         {
//                             frame_time[i] = (int)(node.Next.Value - node.Value).TotalMilliseconds;
//                             node = node.Next;
//                         }
//                         //非同期で走らせてるやつのずらす時間を生成  Clac time which make diff of async roop.
//                         diff_time = (int)elapt_time.Average() / (thread - 1);
//                         if (diff_time < 1000 / fps_limit)
//                         {
//                             diff_time = 1000 / fps_limit;
//                         }
//                     }
//                 }

//                 lock (lock_landmarks)
//                 {
//                     Vector2[] parts = new Vector2[68];
//                     for (uint i = 0; i < 68; i++)
//                     {
//                         parts[i].x = landmark_detection[i].X;
//                         parts[i].y = landmark_detection[i].Y;
//                     }
//                 }
//                 if (checkC)
//                 {
//                     AdjustBlink();
//                     checkC = false;
//                 }

//                 if (StopBlink)
//                     return;

//                 smRenderer.SetBlendShapeWeight(15, LeftEyeCloseness * 100);
//                 smRenderer.SetBlendShapeWeight(14, RightEyeCloseness * 100);
//             }
//             catch (Exception) { }
//         }

//         //--------------------------------------------------------------------------------------------------------
//         /// <summary>
//         /// まばたきの割合を調整する
//         /// </summary>
//         public async void AdjustBlink()
//         {
//             StopBlink = true; // まばたきを止める

//             int i;
//             float tempL = 0, tempR = 0, resL, resR;

//             eye_ratio_h_l = 0.3f;
//             eye_ratio_h_r = 0.3f;
//             eye_ratio_l_l = 0.2f;
//             eye_ratio_l_r = 0.2f;

//             // 開いた状態を測定
//             for (i = 0; i < 20; i++) // 20フレーム分のまばたきの平均を取る
//             {
//                 await Task.Delay(50); // 50ms待つ
//                 tempL += eye_L_ratio;
//                 tempR += eye_R_ratio;
//             }

//             resL = tempL / 20;
//             resR = tempR / 20;

//             await Task.Delay(2000); // 1秒待つ

//             // 閉じた状態を測定
//             for (i = 0, tempL = 0, tempR = 0; i < 20; i++) // 20フレーム分のまばたきの平均を取る
//             {
//                 await Task.Delay(50); // 50ms待つ
//                 tempL += eye_L_ratio;
//                 tempR += eye_R_ratio;
//             }

//             eye_ratio_h_l = resL;
//             eye_ratio_h_r = resR;
//             eye_ratio_l_l = tempL / 20;
//             eye_ratio_l_r = tempR / 20;

//             StopBlink = false; // まばたきを再開
//         }

//         //--------------------------------------------------------------------------------------------------------
//         /// <summary>
//         /// まばたきの割合を調整する
//         /// </summary>
//         public async void AdjustBlink_Close()
//         {
//             StopBlink = true; // まばたきを止める

//             int i;
//             float tempL = 0, tempR = 0;
//             eye_ratio_l_l = 0.2f;
//             eye_ratio_l_r = 0.2f;
//             for (i = 0; i < 20; i++) // 20フレーム分のまばたきの平均を取る
//             {
//                 await Task.Delay(50); // 50ms待つ
//                 tempL += LeftEyeCloseness;
//                 tempR += RightEyeCloseness;
//             }
//             eye_ratio_l_l = tempL / 20;
//             eye_ratio_l_r = tempR / 20;

//             StopBlink = false; // まばたきを再開
//         }
//         //--------------------------------------------------------------------------------------------------------
//         /// <summary>
//         /// 成功率をログに流す       write success rate in log
//         /// </summary>
//         private void OnDestroy()
//         {
//             Debug.Log("suc = " + suc + ",fail= " + fail + ": " + (float)suc / (suc + fail));
//             running = false;
//         }

//         //--------------------------------------------------------------------------------------------------------
//         private float Distance(DlibDotNet.Point point1, DlibDotNet.Point point2)
//         {
//             return Mathf.Sqrt((point1.X - point2.X) * (point1.X - point2.X) + (point1.Y - point2.Y) * (point1.Y - point2.Y));
//         }


//         //--------------------------------------------------------------------------------------------------------
//         /// <summary>
//         /// 回転行列からクォータニオンに変換    Convert rotation matrix to quaternion
//         /// </summary>
//         /// <param name="projmat">回転行列      rotation matrix</param>
//         /// <returns>クォータニオン             quaternion</returns>
//         private Quaternion RotMatToQuatanion(double[] projmat)
//         {
//             Quaternion quaternion = new Quaternion();
//             double[] elem = new double[4]; // 0:x, 1:y, 2:z, 3:w
//             elem[0] = projmat[0] - projmat[4] - projmat[8] + 1.0f;
//             elem[1] = -projmat[0] + projmat[4] - projmat[8] + 1.0f;
//             elem[2] = -projmat[0] - projmat[4] + projmat[8] + 1.0f;
//             elem[3] = projmat[0] + projmat[4] + projmat[8] + 1.0f;

//             uint biggestIndex = 0;
//             for (uint i = 1; i < 4; i++)
//             {
//                 if (elem[i] > elem[biggestIndex])
//                 {
//                     biggestIndex = i;
//                 }
//             }

//             if (elem[biggestIndex] < 0.0f)
//             {
//                 return quaternion;
//             }

//             float v = (float)Math.Sqrt(elem[biggestIndex]) * 0.5f;
//             float mult = 0.25f / v;

//             switch (biggestIndex)
//             {
//                 case 0:
//                     quaternion.x = v;
//                     quaternion.y = (float)(projmat[1] + projmat[3]) * mult;
//                     quaternion.z = (float)(projmat[6] + projmat[2]) * mult;
//                     quaternion.w = (float)(projmat[5] - projmat[7]) * mult;
//                     break;
//                 case 1:
//                     quaternion.x = (float)(projmat[1] + projmat[3]) * mult;
//                     quaternion.y = v;
//                     quaternion.z = (float)(projmat[5] + projmat[7]) * mult;
//                     quaternion.w = (float)(projmat[6] - projmat[2]) * mult;
//                     break;
//                 case 2:
//                     quaternion.x = (float)(projmat[6] + projmat[2]) * mult;
//                     quaternion.y = (float)(projmat[5] + projmat[7]) * mult;
//                     quaternion.z = v;
//                     quaternion.w = (float)(projmat[1] - projmat[3]) * mult;
//                     break;
//                 case 3:
//                     quaternion.x = (float)(projmat[5] - projmat[7]) * mult;
//                     quaternion.y = (float)(projmat[6] - projmat[2]) * mult;
//                     quaternion.z = (float)(projmat[1] - projmat[3]) * mult;
//                     quaternion.w = v;
//                     break;
//             }

//             return quaternion;
//         }

//         /// <summary>
//         /// 推定した回転に異常がないか検査           check error in estemated rotation
//         /// </summary>
//         /// <param name="check">チェックされる対象   checking Vecter3</param>
//         /// <param name="range">動ける範囲           moveable range</param>
//         /// <param name="root">前回の回転            before rotation</param>
//         /// <returns>チェック後の回転。エラーの場合前のものが出される。      rotation after checked. If error exist, return before Vector3.</returns>
//         private Vector3 EstimateErrCheck(Vector3 check, Vector3 range, Vector3 root)
//         {

//             var delta = check - root;

//             if (root == Vector3.zero)
//             {
//                 return check;
//             }


//             if (Mathf.Abs(delta.x) > range.x * 3)
//             {
//                 check.x = root.x;
//             }
//             else if (delta.x > range.x)
//             {
//                 check.x = root.x + range.x;
//             }
//             else if (-delta.x > range.x)
//             {
//                 check.x = root.x - range.x;
//             }
//             else if (delta.x < range.x * 0.05)
//             {
//                 check.x = root.x;
//             }

//             if (Mathf.Abs(delta.y) > range.y * 3)
//             {
//                 check.y = root.y;
//             }
//             else if (delta.y > range.y)
//             {
//                 check.y = root.y + range.y;
//             }
//             else if (-delta.y > range.y)
//             {
//                 check.y = root.y - range.y;
//             }
//             else if (delta.y < range.y * 0.05)
//             {
//                 check.y = root.y;
//             }

//             if (Mathf.Abs(delta.z) > range.z * 3)
//             {
//                 check.z = root.z;
//             }
//             else if (delta.z > range.z)
//             {
//                 check.z = root.z + range.z;
//             }
//             else if (-delta.z > range.z)
//             {
//                 check.z = root.z - range.z;
//             }
//             else if (delta.z < range.z * 0.05)
//             {
//                 check.z = root.z;
//             }
//             return check;

//         }

//         //--------------------------------------------------------------------------------------------------------
//         /// <summary>
//         /// 回転を正しく直す        resharpen rotation
//         /// </summary>
//         /// <param name="rotSample">回転      rotation</param>
//         /// <param name="posSample">位置      position</param>
//         /// <returns>直された後の回転         reshaped rotation</returns>
//         private Vector3 RotReSharp(Vector3 rotSample, Vector3 posSample)
//         {
//             if (rotSample.x > 270)
//             {
//                 rotSample.x -= 360;
//             }
//             else if (rotSample.x < -270)
//             {
//                 rotSample.x += 360;
//             }

//             if (rotSample.y > 270)
//             {
//                 rotSample.y -= 360;
//             }
//             else if (rotSample.y < -270)
//             {
//                 rotSample.y += 360;
//             }

//             if (rotSample.z > 270)
//             {
//                 rotSample.z -= 360;
//             }
//             else if (rotSample.z < -270)
//             {
//                 rotSample.z += 360;
//             }
//             else if (rotSample.z > 90)
//             {
//                 rotSample.z -= 180;
//                 posSample.x *= -1;
//                 posSample.y *= -1;
//                 posSample.z *= -1;

//             }
//             else if (rotSample.z < -90)
//             {
//                 rotSample.z += 180;
//                 posSample.x *= -1;
//                 posSample.y *= -1;
//                 posSample.z *= -1;
//             }

//             return rotSample;
//         }

//         //--------------------------------------------------------------------------------------------------------
//         /// <summary>
//         /// 範囲を飛び出してないかチェック     range check
//         /// </summary>
//         /// <param name="check">チェックされる位置       checking position</param>
//         /// <param name="rad">動かせる範囲               range can move</param>
//         /// <returns>チェック後の位置                    checked position</returns>
//         private Vector3 RangeCheck(Vector3 check, Vector3 range)
//         {
//             if (Mathf.Abs(check.x) > range.x)
//             {
//                 if (check.x > 0) { check.x = range.x; } else { check.x = -range.x; }
//             }

//             if (Mathf.Abs(check.y) > range.y)
//             {
//                 if (check.y > 0) { check.y = range.y; } else { check.y = -range.y; }
//             }

//             if (Mathf.Abs(check.z) > range.z)
//             {
//                 if (check.z > 0) { check.z = range.z; } else { check.z = -range.z; }
//             }

//             return check;
//         }

//     }

//     public enum SmoothingMethod
//     {
//         Average,
//         LPF
//     }
// }
// namespace CLAPlus_Face2Face.Extention
// {
//     /// <summary>
//     /// AverageのVecter3拡張       Average Vecter 3
//     /// </summary>
//     static class CalcAverage
//     {
//         static public Vector3 Average(this LinkedList<Vector3> vector3s)
//         {
//             var vector = new Vector3();
//             foreach (var vec in vector3s)
//             {
//                 vector += vec;
//             }
//             vector /= vector3s.Count;

//             return vector;
//         }

//         static public Vector3 Average(this IEnumerable<Vector3> vector3s)
//         {
//             var vector = new Vector3();
//             foreach (var vec in vector3s)
//             {
//                 vector += vec;
//             }
//             vector /= vector3s.Count();

//             return vector;
//         }
//     }
// }