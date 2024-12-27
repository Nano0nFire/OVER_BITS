using UnityEngine;

public class DACS_Projectile : MonoBehaviour // LocalOnly
{
    [HideInInspector] public ulong nwID;
    [HideInInspector] public Transform ShotPos;
    [HideInInspector] public Transform CameraPos;
    [SerializeField] DACS_P_BulletControl_Normal bcNormal;

    public void Setup()
    {
        bcNormal.ownID = nwID;
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
