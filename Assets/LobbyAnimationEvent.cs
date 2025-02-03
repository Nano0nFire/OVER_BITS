using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class LobbyAnimationEvent : MonoBehaviour
{
    [SerializeField] CinemachineDollyCart dollyCart;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] RMeshAPI rMeshAPI;
    [SerializeField] LobbyLogoPush logoPush;
    [SerializeField] List<AnimationEvent> animationEvents;
    [SerializeField] int eventCounter = 0;
    float speed = 0;
    bool delay = false;


    async void Update()
    {
        if (eventCounter > animationEvents.Count || delay)
            return;

        speed = math.lerp(speed, animationEvents[eventCounter].speed, 0.1f);
        dollyCart.m_Position += speed * Time.deltaTime;
        if (dollyCart.m_Position >= animationEvents[eventCounter].pos)
        {
            delay = true;
            await UniTask.Delay((int)(animationEvents[eventCounter].delay * 1000), cancellationToken: this.GetCancellationTokenOnDestroy());
            eventCounter++;
            virtualCamera.LookAt = animationEvents[eventCounter].target;
            CustomAction(animationEvents[eventCounter].ActionIndex);

            delay = false;
        }
    }

    void CustomAction(int index)
    {
        switch (index)
        {
            case 0:
                rMeshAPI.Play(math.PI / 2);
                break;
            case 1:
                rMeshAPI.Play(math.PI);
                break;
            case 2:
                logoPush.Play(0.5f);
                break;
            default:
                break;
        }
    }
    [System.Serializable]
    struct AnimationEvent
    {
        public float speed;
        public float pos;
        public float delay;
        public Transform target;
        public int ActionIndex;
    }
}
