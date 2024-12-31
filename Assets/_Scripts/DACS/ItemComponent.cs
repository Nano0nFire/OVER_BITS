using System;
using UnityEngine;

namespace DACS.Inventory
{
    public class ItemComponent : MonoBehaviour
    {
        public Transform ActionPoint { get => _ActionPoint; }
        [SerializeField] Transform _ActionPoint;
        public Transform rIKPos { get => _rIKPos; }
        [SerializeField] Transform _rIKPos;

        public Transform lIKPos { get => _lIKPos; }

        [SerializeField] Transform _lIKPos;
        public int SyncPresetNumber { get => _SyncPresetNumber; }

        [SerializeField, Tooltip("同期先で固定される状態のプリセットの番号")] int _SyncPresetNumber;
        [Tooltip("0 : デフォルト\n1 : 構え")]public Preset[] preset;

        [Serializable]
        public class Preset
        {
            public Vector3 Pos;
            public Vector3 Rot;
            [Tooltip("このアイテムの掴み方\n0:両手\n1:右手\n2:左手")]
            public int IsRightHandItem;
            public int HandPresetNumber;
        }
    }
}