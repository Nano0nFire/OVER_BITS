using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;
using Unity.Entities.UniversalDelegates;
using Cinemachine;
using Unity.Mathematics;
using UnityEditor;

public class ClientGeneralManager : NetworkBehaviour
{
    public GameObject MainMenu;
    [SerializeField] Rigidbody rb;
    [SerializeField] UIGeneral uiGeneral;
    [SerializeField] CLAPlus_MovementModule clap_m;
    [SerializeField] CLAPlus_AnimationControlModuel clap_a;
    [SerializeField] GameObject avatar;
    [SerializeField] PlayerStatus playerStatus;
    [SerializeField] Transform CameraPos;
    [SerializeField] NetworkObject nwObject;
    public DACS_InventorySystem invSystem;
    public PlayerDataManager pdManager{get; private set;}
    GameObject masterObj;
    DACS_P_BulletControl_Normal test;
    public ulong nwID{get; private set;}
    States KeepState;
    public bool UseInput
    {
        get
        {
            return MainMenu.activeSelf;
        }
        set
        {
            Cursor.visible = value;
            Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked ;
        }
    }
    public bool ActionLock;
    CancellationTokenSource clap_mCTSource = new();
    CancellationTokenSource CTSource = new();

    bool CrouchPress;
    bool watchCrouchPress;
    bool RunningWallAnimation = false;
    bool isOwner;

    [Header("Movement Settings")]
    public bool ToggleDash = false; //ダッシュ切り替え or 長押しダッシュ
    public bool ToggleCrouch = false; //しゃがみ切り替え or 長押ししゃがみ

    [Header("Aim Settings")]
    CinemachineVirtualCamera CVCamera;
    public bool InvertAim; //垂直方向の視点操作の反転
    public bool FirstPersonMode;
    public override async void OnNetworkSpawn()
    {
        Debug.Log("ClientGeneralManager : Loading");

        await UniTask.Delay(500);

        isOwner = nwObject.IsOwner;

        masterObj = GameObject.Find("Master");

        if (!isOwner)
            return;

        // データ系
        pdManager = FindFirstObjectByType<PlayerDataManager>();
        pdManager.inventorySystem = invSystem;
        invSystem.Setup(this);

        // Network
        nwID = nwObject.NetworkObjectId;

        var LocalGM = masterObj.GetComponent<LocalGeneralManager>();
        // UI
        MainMenu = LocalGM.MainMenu;
        uiGeneral = MainMenu.GetComponent<UIGeneral>();
        uiGeneral.Setup(this);
        LocalGM.UI_playerSettings.Setup(this);
        uiGeneral.uI_PlayerSettings = LocalGM.UI_playerSettings;

        // カメラ
        CVCamera = LocalGM.CVCamera;
        CVCamera.Follow = CameraPos;

        // EntityManager
        LocalGM.emSystem.Setup(this);

        GetComponent<Rigidbody>().useGravity = true;
        test = masterObj.GetComponent<DACS_P_BulletControl_Normal>();
        test.PlayerTransform = CameraPos;
        test.nwObject = nwObject;
        InputSetUp();
        clap_a.isOwner = isOwner;

        // 設定
        LoadSettings();

        Debug.Log("ClientGeneralManager : Complete");
    }

    void InputSetUp()
    {
        var inputActions = masterObj.GetComponent<PlayerInput>();
        SetInputAction(inputActions, "Move", MoveKeyInput);
        SetInputAction(inputActions, "Look", CameraInput);
        SetInputAction(inputActions, "Jump", JumpKeyInput);
        SetInputAction(inputActions, "Dash", DashKeyInput);
        SetInputAction(inputActions, "Crouch", CrouchKeyInput);
        SetInputAction(inputActions, "ActionKey", SubAction);
        SetInputAction(inputActions, "MenuEscape", UIexit);
        SetInputAction(inputActions, "RightClick", TestShot);
        SetInputAction(inputActions, "SwitchViewMode", SwitchViewMode);
    }

    public async void LoadSettings()
    {
        var loadedData = await pdManager.LoadData<SettingsData>();
        ToggleDash = loadedData.ToggleDash; //ダッシュ切り替え or 長押しダッシュ
        ToggleCrouch = loadedData.ToggleCrouch; //しゃがみ切り替え or 長押ししゃがみ
        InvertAim = loadedData.InvertAim; //垂直方向の視点操作の反転
        clap_m.HzCameraSens = loadedData.HzCameraSens / 10; //水平方向の視点感度
        clap_m.VCameraSens = loadedData.VCameraSens / 10; //水直方向の視点感度
    }
    void SetInputAction(PlayerInput inputActions, string key, Action<InputAction.CallbackContext> action)
    {
        inputActions.actions[key].performed += action;
        inputActions.actions[key].started += action;
        inputActions.actions[key].canceled += action;
    }

    // void Update()
    // {
    //     if (isOwner)
    //         UpdatePlayerPositionServerRpc(rb.position, rb.linearVelocity);
    // }
    public void UIexit(InputAction.CallbackContext context)
    {
        if (context.performed && isOwner)
        {
            uiGeneral.UIexit(MainMenu);
            clap_m.HzInput = 0;
            clap_m.VInput = 0;
        }
    }

    public void MoveKeyInput(InputAction.CallbackContext context)
    {
        if (UseInput || ActionLock || !isOwner)
            return;

        clap_m.HzInput = context.ReadValue<Vector2>().x;
        clap_m.VInput = context.ReadValue<Vector2>().y;
    }

    public void JumpKeyInput(InputAction.CallbackContext context)
    {
        if (UseInput || !isOwner) return;

        if (context.performed)
        {
            clap_m.Jump();
            clap_a.Jump();
        }
    }
    public void DashKeyInput(InputAction.CallbackContext context)
    {
        if (UseInput || clap_m.Gstate == States.rush || !isOwner)
            return;

        if (context.performed)
            if (ToggleDash)
            {
                clap_m.Gstate = clap_m.Gstate == States.dash? States.walk : States.dash;
            }
            else clap_m.Gstate = States.dash;

        if (context.canceled && !ToggleDash)
            clap_m.Gstate = States.walk;
    }

    public void CrouchKeyInput(InputAction.CallbackContext context)
    {
        if (UseInput || !isOwner)
            return;

        if (context.performed)
        {
            CrouchPress = true;
            CrouchAction();
        }
        if (context.canceled)
        {
            CrouchPress = false;

            if (clap_m.PublicState == States.slide)
                clap_mCTSource.Cancel();

            if (!ToggleCrouch && clap_m.PublicState == States.crouch)
                clap_m.Gstate = KeepState;
        }
    }

    async void CrouchAction()
    {
        if (watchCrouchPress) // 処理が重ならないようにする
            return;

        watchCrouchPress = true;

        try
        {
            States stateCashed = clap_m.PublicState;

            while (CrouchPress)
            {
                if (!clap_m._IsGrounded && stateCashed != States.wallRun && stateCashed != States.climb)
                {
                    clap_mCTSource.Cancel();
                    CTSource.Cancel();

                    clap_mCTSource = new();
                    clap_m.CheckWall(clap_mCTSource.Token, SetWallAnimation);

                    break;
                }
                else if (clap_m._IsGrounded)
                {
                    if (clap_m.CanSlide && stateCashed != States.slide && stateCashed != States.dodge)
                    {
                        clap_mCTSource = new();
                        clap_a.Slide();
                        clap_m.Slide(clap_mCTSource.Token, stateCashed == States.rush, clap_a.AnimationCancel); // 第二引数でRUSH時の挙動に上書きしてスライディングをするか判別する

                        break;
                    }
                    else if (ToggleCrouch)
                    {
                        if (clap_m.Gstate == States.crouch)
                            clap_m.Gstate = States.walk;
                        else
                            clap_m.Gstate = States.crouch;

                        break;
                    }
                    else
                    {
                        KeepState = clap_m.Gstate;
                        clap_m.Gstate = States.crouch;

                        break;
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            }

            watchCrouchPress = false;
        }
        catch (OperationCanceledException)
        {
            watchCrouchPress = false;
        }
    }

    public void SubAction(InputAction.CallbackContext context)
    {
        if (!isOwner)
            return;

        if (context.performed && clap_m.canAction)
        {
            clap_mCTSource.Cancel();

            switch (playerStatus.SelectedActionSkill)
            {
                case States.dodge:
                    clap_m.Dodge();
                    clap_a.Dodge();
                    break;

                case States.rush:
                    clap_m.Rush(clap_m.PublicState == States.slide);
                    clap_a.Rush();
                    break;
            }
        }
    }

    public void CameraInput(InputAction.CallbackContext context)
    {
        if (UseInput || !isOwner)
            return;

        clap_m.HzRotation = context.ReadValue<Vector2>().x;
        clap_m.VRotation = InvertAim ? context.ReadValue<Vector2>().y : -context.ReadValue<Vector2>().y;
    }

    void SetWallAnimation(States CurrentState)
    {
        switch (CurrentState)
        {
            case States.wallRun:
                clap_a.WallRun(clap_m.IsWallRight); // アニメーションの実行
                RunningWallAnimation = true;
                break;

            case States.climb:
                clap_a.Climb();
                RunningWallAnimation = true;
                break;

            default:
                if (!RunningWallAnimation) // 壁走り、壁上り以外のアニメーションを実行中ならなにもしない
                    return;

                clap_a.AnimationCancel();

                RunningWallAnimation = false;
                break;
        }
    }

    public void TestShot(InputAction.CallbackContext context)
    {
        if (!isOwner)
            return;

        if (context.performed)
        {
            var xform = CameraPos.transform;
            test.SetBulletLocal(xform.position, xform.forward,0, 3);
            // UnityEditor.EditorApplication.isPaused = true;
        }
    }

    public void SwitchViewMode(InputAction.CallbackContext context)
    {
        if (FirstPersonMode)
        {
            var f = CVCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            f.CameraDistance = 4; // ThirdPersonの時はカメラの距離を4にする
            f.ShoulderOffset.y = 0;
            foreach (var renderer in avatar.GetComponentsInChildren<SkinnedMeshRenderer>())
                renderer.enabled = true;
            FirstPersonMode = false;
        }
        else
        {
            var f = CVCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            f.CameraDistance = 0; // ThirdPersonの時はカメラの距離を0にする
            f.ShoulderOffset.y = 0.3f;
            foreach (var renderer in avatar.GetComponentsInChildren<SkinnedMeshRenderer>())
                renderer.enabled = false;
            FirstPersonMode = true;
        }
    }

    public override void OnDestroy()
    {
        clap_mCTSource.Cancel();
        CTSource.Cancel();
    }
}
