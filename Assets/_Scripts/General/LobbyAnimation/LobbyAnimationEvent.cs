using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using CLAPlus.Extension;
using Unity.Mathematics;
using UnityEngine;
using Unity.VisualScripting;

public class LobbyAnimationEvent : MonoBehaviour
{
    [SerializeField] CinemachineDollyCart dollyCart;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] RMeshAPI rMeshAPI;
    [SerializeField] LobbyLogoPush logoPush;
    [SerializeField] ArmControl armControl;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] List<AnimationEvent> animationEvents;
    [SerializeField] Transform AngleLocker;
    [SerializeField] int eventCounter = 0;
    float speed = 0;
    [SerializeField] bool isLoop = false;
    bool delay = false;
    bool isFaded = false;


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

        if (dollyCart.m_Position >= animationEvents[eventCounter].pos || (eventCounter == animationEvents.Count-1 && dollyCart.m_Position <= 1))
        {
            delay = true;
            virtualCamera.LookAt = animationEvents[eventCounter].target;
            CustomAction(animationEvents[eventCounter].ActionIndex);
            await UniTask.Delay((int)(animationEvents[eventCounter].delay * 1000), cancellationToken: this.GetCancellationTokenOnDestroy());
            eventCounter++;

            delay = false;
        }
        else
        {
            speed = math.lerp(speed, animationEvents[eventCounter].speed, 0.1f);
            dollyCart.m_Position += speed * Time.deltaTime;
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
                AngleLock(new Vector3(0, 0, 180), 5f);
                break;
            case 6:
                armControl.Play();
                break;
            case 7:
                logoPush.Spread();
                break;
            case 8:
                logoPush.Reset();
                break;
            case 9:
                AngleSet(new Vector3(0, -90, 0));
                break;
            default:
                break;
        }
    }

    async void Fadein()
    {
        if (isFaded)
            return;
        else
            isFaded = true;

        for (float i = 0; i < 1; i += 0.01f)
        {
            canvasGroup.alpha = i;
            await UniTask.Delay(10, cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }

    async void AngleSet(Vector3 target, float t = 1)
    {
        AngleLock(new Vector3(0, 0, 0));
        Vector3 temp = virtualCamera.transform.eulerAngles;
        temp = Extensions.SimplifyRotation(temp);
        for (float i = 0; i < 1; i += 0.01f / t)
        {
            virtualCamera.transform.eulerAngles = Vector3.Lerp(temp, target, i);
            await UniTask.Delay(10, cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }

    async void AngleLock(Vector3 target, float t = 1)
    {
        Vector3 temp = AngleLocker.transform.localEulerAngles;
        temp = Extensions.SimplifyRotation(temp);
        for (float i = 0; i < 1; i += 0.01f / t)
        {
            AngleLocker.transform.localEulerAngles = Vector3.Lerp(temp, target, i);
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
