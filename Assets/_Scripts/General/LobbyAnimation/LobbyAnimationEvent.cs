using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using CLAPlus.Extension;
using Unity.Mathematics;
using UnityEngine;
using Unity.VisualScripting;
using System.Threading.Tasks;

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
    bool delay = true;
    bool isFaded = false;
    UniTask lastTask;
    bool hasTask = false;


    async void Awake()
    {
        await UniTask.Delay(1000);
        delay = false;
    }
    async void Update()
    {
        if (delay)
            return;

        if (eventCounter == animationEvents.Count)
        {
            if (isLoop)
            {
                eventCounter = 0;
                var temp = dollyCart.m_Position;
                for (float l = 0; l < 1; l += 0.1f)
                {
                    dollyCart.m_Position = math.lerp(temp, 0, l);
                    await UniTask.Delay(100);
                }
            }
            else
                return;
        }

        if (dollyCart.m_Position >= animationEvents[eventCounter].pos || (eventCounter == animationEvents.Count-1 && dollyCart.m_Position <= 1))
        {
            delay = true;
            virtualCamera.LookAt = animationEvents[eventCounter].target;
            if (animationEvents[eventCounter].delay >= 0)
            {
                if (hasTask)
                {
                    hasTask = false;
                    await CustomAction(animationEvents[eventCounter].ActionIndex);
                    Debug.Log("wait");
                    await lastTask;
                    Debug.Log("release");
                    await UniTask.Delay((int)(animationEvents[eventCounter].delay * 1000), cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
                    Debug.Log("end");
                }
                else
                {
                    await CustomAction(animationEvents[eventCounter].ActionIndex);
                    await UniTask.Delay((int)(animationEvents[eventCounter].delay * 1000), cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
                }
            }
            else
            {
                lastTask = CustomAction(animationEvents[eventCounter].ActionIndex);
                hasTask = true;
            }
            eventCounter++;

            delay = false;
        }
        else
        {
            speed = math.lerp(speed, animationEvents[eventCounter].speed, 0.1f);
            dollyCart.m_Position += speed * Time.deltaTime;
        }
    }

    async UniTask CustomAction(int index)
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
                await logoPush.Play(0.5f);
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
                await logoPush.Spread();
                break;
            case 8:
                await logoPush.LogoReset();
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
            await UniTask.Delay(10, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
        }
    }

    async void AngleSet(Vector3 target, float t = 1)
    {
        AngleLock(new Vector3(0, 0, 0));
        for (float i = 0; i < 1; i += 0.01f / t)
        {
            virtualCamera.transform.eulerAngles = Vector3.Lerp(Extensions.SimplifyRotation(virtualCamera.transform.eulerAngles), target, i);
            await UniTask.Delay(10, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
        }
    }

    async void AngleLock(Vector3 target, float t = 1)
    {
        for (float i = 0; i < 1; i += 0.01f / t)
        {
            AngleLocker.transform.localEulerAngles = Vector3.Lerp(Extensions.SimplifyRotation(AngleLocker.transform.localEulerAngles), target, i);
            await UniTask.Delay(10, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
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
