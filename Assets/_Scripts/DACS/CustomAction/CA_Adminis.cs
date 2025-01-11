using Cysharp.Threading.Tasks;
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

        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] Rigidbody rb;
        [SerializeField] Transform shotPos;
        public override async UniTask Action(int index)
        {
            switch (index)
            {
                case 0:
                    Projectile.Projectile.Instance.StartShooting(shotPos.position, transform.forward, 0, 1, 2, EntityData);
                    await UniTask.Delay(500);
                    break;

                case 1:
                    navMeshAgent.enabled = false;
                    rb.isKinematic = false;

                    rb.AddForce(transform.right * 100);

                    await UniTask.Delay(500);

                    rb.AddForce(transform.right * -100);

                    await UniTask.WaitUntil(() =>  NavMesh.SamplePosition(navMeshAgent.transform.localPosition, out var navHit, 0.1f, NavMesh.AllAreas) == false, cancellationToken : destroyCancellationToken);

                    navMeshAgent.enabled = true;
                    rb.isKinematic = true;

                    break;

            }
        }
    }
}
