using Cysharp.Threading.Tasks;
using DACS.Entities;
using DACS.Extensions;
using DACS.Projectile;
using UnityEngine;
using UnityEngine.AI;
namespace DACS.CustomActions
{
    public class CA_Adminis : CustomAction
    {
        public override EntityStatusData EntityData { get => _entityData; set => _entityData = value; }
        EntityStatusData _entityData;

        [SerializeField] EnemyComponent enemyComponent;
        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] Rigidbody rb;
        [SerializeField] Transform shotPos;
        Transform target{ get => enemyComponent.target; }
        ulong targetNetworkObjectID{ get => enemyComponent.targetNetworkObjectID; }
        public override async UniTask Action(int index)
        {
            switch (index)
            {
                case 0:
                    Projectile.Projectile.Instance.StartShooting(shotPos.position, transform.forward, 0, 1, 2, EntityData);
                    await UniTask.Delay(10);
                    break;

                case 1:
                    navMeshAgent.enabled = false;
                    rb.isKinematic = false;

                    rb.AddForce(transform.right * 500 + transform.up * 100);

                    await UniTask.Delay(500);

                    rb.AddForce(transform.right * -500 + transform.up * 100);

                    await UniTask.WaitUntil(() =>  NavMesh.SamplePosition(navMeshAgent.transform.localPosition, out var navHit, 0.1f, NavMesh.AllAreas) == false, cancellationToken : destroyCancellationToken);

                    navMeshAgent.enabled = true;
                    rb.isKinematic = true;

                    break;

                case 2:
                    if (target == null)
                        break;
                    Projectile.Projectile.Instance.StartShooting(shotPos.position, transform.forward, 1, 2, 2, EntityData, targetNetworkObjectID, target);
                    await UniTask.Delay(2000);
                    break;

            }
        }
    }
}
