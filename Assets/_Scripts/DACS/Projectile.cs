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

        static Projectile instance;

        // インスタンスへのプロパティ
        public static Projectile Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<Projectile>();

                    if (instance == null)
                    {
                        GameObject singletonObject = new(typeof(Projectile).Name);
                        instance = singletonObject.AddComponent<Projectile>();
                    }
                }
                return instance;
            }
        }

        public void Setup()
        {
            if (instance == null)
                instance = this;

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

        public void StartShooting(Vector3 shotPos, Vector3 forward, int ProjectileType, int id, int amount, EntityStatusData EntityData)
        {
            switch (ProjectileType)
            {
                case 0:
                    bcNormal.SetBulletServer(shotPos, forward, id, amount, EntityData);
                    break;
            }
        }

        public void StopShooting()
        {

        }
    }
}