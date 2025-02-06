using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using CLAPlus.Extension;
using Unity.Mathematics;
using UnityEngine;

public class LobbyAnimationEvent : MonoBehaviour
{
    [SerializeField] CinemachineDollyCart dollyCart;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] RMeshAPI rMeshAPI;
    [SerializeField] LobbyLogoPush logoPush;
    [SerializeField] ArmControl armControl;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] List<AnimationEvent> animationEvents;
    [SerializeField] quaternion CameraAngle;
    [SerializeField] int eventCounter = 0;
    float speed = 0;
    [SerializeField] bool isLoop = false;
    bool delay = false;


    async void Update()
    {
        if (delay)
            return;

        if (eventCounter == animationEvents.Count)
        {
            if (isLoop)
            {
                eventCounter = 0;
                dollyCart.m_Position = 0;
            }
            else
                return;
        }

        speed = math.lerp(speed, animationEvents[eventCounter].speed, 0.1f);
        dollyCart.m_Position += speed * Time.deltaTime;
        if (dollyCart.m_Position >= animationEvents[eventCounter].pos)
        {
            delay = true;
            virtualCamera.LookAt = animationEvents[eventCounter].target;
            CustomAction(animationEvents[eventCounter].ActionIndex);
            await UniTask.Delay((int)(animationEvents[eventCounter].delay * 1000), cancellationToken: this.GetCancellationTokenOnDestroy());
            eventCounter++;

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
            case 3:
                Fadein();
                break;
            case 4:
                AngleSet(new Vector3(90, 0, 90));
                break;
            case 5:
                AngleSet(new Vector3(0, 0, 0), true);
                break;
            case 6:
                armControl.Play();
                break;
            default:
                break;
        }
    }

    async void Fadein()
    {
        for (float i = 0; i < 1; i += 0.01f)
        {
            canvasGroup.alpha = i;
            await UniTask.Delay(10, cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }

    async void AngleSet(Vector3 target, bool local = false)
    {
        Vector3 temp = local ? virtualCamera.transform.localEulerAngles : virtualCamera.transform.eulerAngles;
        temp = Extensions.SimplifyRotation(temp);
        for (float i = 0; i < 1; i += 0.01f)
        {
            if (local)
                virtualCamera.transform.localEulerAngles = Vector3.Lerp(temp, target, i);
            else
                virtualCamera.transform.eulerAngles = Vector3.Lerp(temp, target, i);
            await UniTask.Delay(10, cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }
    [System.Serializable]
    struct AnimationEvent
    {
        public float speed;
        public float pos;
        public Transform target;
        public int ActionIndex;
        public float delay;
    }
}
