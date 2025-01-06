using UnityEngine;

namespace DACS.Projectile
{
    public class Projectile : MonoBehaviour // LocalOnly
    {
        public ulong clientID;
        public ulong nwoID;
        [HideInInspector] public Transform ShotPos;
        [HideInInspector] public Transform CameraPos;
        [SerializeField] BulletControl_Basic bcNormal;

        public void Setup()
        {
            bcNormal.ownID = clientID;
            bcNormal.nwoID = nwoID;
            bcNormal.PlayerTransform = CameraPos;
        }
        public void StartShooting(int ProjectileType, int id, int amount)
        {
            switch (ProjectileType)
            {
                case 0:
                    bcNormal.SetBulletLocal(ShotPos.position, CameraPos.forward, id, amount);
                    break;
            }
        }

        public void StopShooting()
        {

        }
    }
}