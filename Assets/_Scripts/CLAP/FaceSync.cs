using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace CLAPlus.Face2Face
{
    public class FaceSync : NetworkBehaviour
    {
        [HideInInspector] public Face2Face tracker;
        public SkinnedMeshRenderer smr;
        public static bool UseFaceSync = true;

        static readonly int MouthBlendshapeIndex = 39;
        static readonly int RightEyeBlendshapeIndex = 14;
        static readonly int LeftEyeBlendshapeIndex = 15;
        static readonly float eye_smooth = 10;                          /// <param name="eye_smooth">まばたきの滑らかさ   Smoothness of blink</param>
        static readonly float mouth_smooth = 5;                        /// <param name="mouth_smooth">口の開閉の滑らかさ   Smoothness of mouth open/close

        float eye_l_c;
        float eye_r_c;
        float mouth_c;

        bool SavingData;
        public static bool Stop;

        void Update()
        {
            if (Stop)
            {
                StopTrack();
                Stop = false;
            }

            if (tracker == null)
                return;
            if (IsOwner && !tracker.isRunning)
                return;

            if (IsOwner)
            {
                if (tracker.isRunning)
                {
                    CalculateLerp(tracker.MouthCloseness, tracker.RightEyeCloseness, tracker.LeftEyeCloseness);
                    smr.SetBlendShapeWeight(MouthBlendshapeIndex, mouth_c);
                    smr.SetBlendShapeWeight(RightEyeBlendshapeIndex, eye_r_c);
                    smr.SetBlendShapeWeight(LeftEyeBlendshapeIndex, eye_l_c);

                    if (UseFaceSync && SavingData)
                        FaceSyncServerRpc(tracker.MouthCloseness, tracker.RightEyeCloseness, tracker.LeftEyeCloseness);
                    SavingData = !SavingData;
                }
            }
            else
            {
                if (UseFaceSync)
                {
                    smr.SetBlendShapeWeight(MouthBlendshapeIndex, mouth_c);
                    smr.SetBlendShapeWeight(RightEyeBlendshapeIndex, eye_r_c);
                    smr.SetBlendShapeWeight(LeftEyeBlendshapeIndex, eye_l_c);
                }
            }
        }

        void CalculateLerp(bool mouth, bool eye_r, bool eye_l)
        {
            float t = Time.deltaTime;

            eye_l_c = math.lerp(eye_l_c,
                                eye_l ? 100 : 0,
                                t * eye_smooth);
            eye_r_c = math.lerp(eye_r_c,
                                eye_r ? 100 : 0,
                                t * eye_smooth);
            mouth_c = math.lerp(mouth_c,
                                mouth ? 100 : 0,
                                t * mouth_smooth);
        }

        void StopTrack()
        {
            smr.SetBlendShapeWeight(MouthBlendshapeIndex, 0);
            smr.SetBlendShapeWeight(RightEyeBlendshapeIndex, 0);
            smr.SetBlendShapeWeight(LeftEyeBlendshapeIndex, 0);
        }

        [ServerRpc]
        public void FaceSyncServerRpc(bool mouth, bool eye_r, bool eye_l)
        {
            if (IsServer)
                FaceSyncClientRpc(mouth, eye_r, eye_l);
        }

        [ClientRpc]
        public void FaceSyncClientRpc(bool mouth, bool eye_r, bool eye_l)
        {
            if (UseFaceSync)
                CalculateLerp(mouth, eye_r, eye_l);
        }
    }
}