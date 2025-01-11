using Cysharp.Threading.Tasks;
using DACS.Extensions;
using DACS.Projectile;
using UnityEngine;
using UnityEngine.AI;
namespace DACS.CustomActions
{
    public class CA_Aurora : CustomAction
    {
        public override EntityStatusData EntityData { get => _entityData; set => _entityData = value; }
        EntityStatusData _entityData;

        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] Rigidbody rb;

        public override async UniTask Action(int index)
        {
            switch (index)
            {
                case 0:
                case 1:
                    navMeshAgent.enabled = false;
                    rb.isKinematic = false;

                    rb.AddForce(transform.forward * 500 + transform.up * 100);

                    await UniTask.Delay(3000);

                    rb.AddForce(transform.forward * -500);

                    await UniTask.WaitUntil(() =>  NavMesh.SamplePosition(navMeshAgent.transform.localPosition, out var navHit, 0.1f, NavMesh.AllAreas) == false, cancellationToken : destroyCancellationToken);

                    navMeshAgent.enabled = true;
                    rb.isKinematic = true;

                    break;

            }
        }
    }
}
