using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;
using Cinemachine;
using CLAPlus;
using CLAPlus.AnimationControl;
using DACS;
using DACS.Projectile;
using DACS.Inventory;
using CLAPlus.Face2Face;

public class ClientGeneralManager : NetworkBehaviour
{
    [HideInInspector] public GameObject MainMenu;
    GameObject ChatSpace;
    [SerializeField] UIGeneral uiGeneral;
    [SerializeField] CharactorMovement clap_m;
    [SerializeField] AnimationControl clap_a;
    [SerializeField] FaceSync faceSync;
    [SerializeField] GameObject avatar;
    [SerializeField] PlayerStatus playerStatus;
    [SerializeField] Transform CameraPos;
    [SerializeField] NetworkObject nwObject;
    public InventorySystem invSystem;
    public HotbarSystem hotbarSystem;
    public PlayerDataManager pdManager{get; private set;}
    Projectile projectile;
    public ulong nwID{get; private set;}
    States KeepState;
    public bool UseInput
    {
        get
        {
            return _UseInput;
        }
        set
        {
            Cursor.visible = !value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None ;
            _UseInput = value;
        }
    }
    bool _UseInput;
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

        UseInput = true;

        await UniTask.Delay(500);

        isOwner = nwObject.IsOwner;

        var masterObj = GameObject.Find("Master");

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
        uiGeneral.invSystem = invSystem;
        uiGeneral.Setup(this);
        LocalGM.UI_playerSettings.Setup(this);
        uiGeneral.uI_PlayerSettings = LocalGM.UI_playerSettings;
        var PlayerName = PlayerDataManager.LoadedPlayerProfileData.PlayerName;
        var lastDot = PlayerName.LastIndexOf('#');
        var chatComponent = masterObj.GetComponent<ClapChat>();
        if (lastDot != -1)
            chatComponent.PlayerName = PlayerName[..lastDot];
        chatComponent.cgManager = this;
        ChatSpace = chatComponent.canvas;

        // カメラ
        CVCamera = LocalGM.CVCamera;
        CVCamera.Follow = CameraPos;

        // EntityManager
        LocalGM.emSystem.Setup(this);

        GetComponent<Rigidbody>().useGravity = true;
        projectile = masterObj.GetComponent<Projectile>();
        projectile.nwID = nwID;
        projectile.CameraPos = CameraPos;
        projectile.Setup();
        hotbarSystem.ChangeActionPoint += (xform) => projectile.ShotPos = xform;
        var paControl = masterObj.GetComponent<PlayerActionControl>();
        paControl.invSystem = invSystem;
        paControl.handControl = GetComponentInChildren<HandControl>();
        InputSetUp(masterObj.GetComponent<PlayerInput>());
        clap_a.isOwner = isOwner;
        faceSync.tracker = masterObj.GetComponent<Face2Face>();

        // 設定
        LoadSettings();

        Debug.Log("ClientGeneralManager : Complete");
    }

    void InputSetUp(PlayerInput inputActions)
    {
        SetInputAction(inputActions, "Move", MoveKeyInput);
        SetInputAction(inputActions, "Look", CameraInput);
        SetInputAction(inputActions, "Jump", JumpKeyInput);
        SetInputAction(inputActions, "Dash", DashKeyInput);
        SetInputAction(inputActions, "Crouch", CrouchKeyInput);
        SetInputAction(inputActions, "ActionKey", SubAction);
        SetInputAction(inputActions, "SwitchViewMode", SwitchViewMode);
        SetInputAction(inputActions, "PrimarySlot", SelectPriSlot);
        SetInputAction(inputActions, "SecondarySlot", SelectSecSlot);
        SetInputAction(inputActions, "GranadeSlot", SelectGraSlot);
        SetInputAction(inputActions, "SubSlot1", SelectSub0Slot);
        SetInputAction(inputActions, "SubSlot2", SelectSub1Slot);
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

    public void SelectPriSlot(InputAction.CallbackContext callback)
    {
        if (callback.performed)
        {
            hotbarSystem.SelectedHotbarSlot(0);
            uiGeneral.uiHotbar.SelectHotbarSlot(0);
            invSystem.SelectedSlotIndex = 0;
        }
    }

    public void SelectSecSlot(InputAction.CallbackContext callback)
    {
        if (callback.performed)
        {
            hotbarSystem.SelectedHotbarSlot(1);
            uiGeneral.uiHotbar.SelectHotbarSlot(1);
            invSystem.SelectedSlotIndex = 1;
        }
    }

    public void SelectGraSlot(InputAction.CallbackContext callback)
    {
        if (callback.performed)
        {
            hotbarSystem.SelectedHotbarSlot(2);
            uiGeneral.uiHotbar.SelectHotbarSlot(2);
            invSystem.SelectedSlotIndex = 2;
        }
    }

    public void SelectSub0Slot(InputAction.CallbackContext callback)
    {
        if (callback.performed)
        {
            hotbarSystem.SelectedHotbarSlot(3);
            uiGeneral.uiHotbar.SelectHotbarSlot(3);
            invSystem.SelectedSlotIndex = 3;
        }
    }

    public void SelectSub1Slot(InputAction.CallbackContext callback)
    {
        if (callback.performed)
        {
            hotbarSystem.SelectedHotbarSlot(4);
            uiGeneral.uiHotbar.SelectHotbarSlot(4);
            invSystem.SelectedSlotIndex = 4;
        }
    }

    public void CleatInput()
    {
        clap_m.HzInput = 0;
        clap_m.VInput = 0;
    }

    public void MoveKeyInput(InputAction.CallbackContext context)
    {
        if (!UseInput || ActionLock)
            return;

        clap_m.HzInput = context.ReadValue<Vector2>().x;
        clap_m.VInput = context.ReadValue<Vector2>().y;
    }

    public void JumpKeyInput(InputAction.CallbackContext context)
    {
        if (!UseInput)
            return;

        if (context.performed)
        {
            clap_m.Jump();
            clap_a.Jump();
        }
    }
    public void DashKeyInput(InputAction.CallbackContext context)
    {
        if (!UseInput || clap_m.Gstate == States.rush)
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
        if (!UseInput)
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
        if (!UseInput)
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
