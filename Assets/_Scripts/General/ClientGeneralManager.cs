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
using CLAPlus.ClapChat;
using CLAPlus.Extension;
using System.Collections.Generic;
using Unity.Netcode.Components;

public class ClientGeneralManager : NetworkBehaviour
{
    [HideInInspector] public GameObject MainMenu;
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
    Projectile projectile;
    public static ulong clientID{get; private set;}
    public ulong nwID{get; private set;}
    public static CustomLifeAvatar customLifeAvatar;
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

    public static ClientGeneralManager Instance;
    public static bool IsLoaded = false;


    public override async void OnNetworkSpawn()
    {
        Debug.Log("ClientGeneralManager : Loading");

        if (!nwObject.IsOwner)
        {
            Destroy(this); // 重複するインスタンスを破棄
            return;
        }
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            var objectsWithComponent = FindObjectsByType<ClientGeneralManager>(FindObjectsSortMode.None);
            foreach (var obj in objectsWithComponent)
            {
                if (obj != this)
                    Destroy(obj);
            }
        }

        UseInput = true;

        await UniTask.Delay(1000); // 1秒待機

        isOwner = nwObject.IsOwner;

        Debug.Log("ClientGeneralManager : Setting Up");

        // データ系

        PlayerDataManager.PlayerSettingsData = await PlayerDataManager.LoadData<SettingsData>();
        Debug.Log("ClientGeneralManager : PlayerDataManager Loaded");
        await invSystem.Setup();
        Debug.Log("ClientGeneralManager : InventorySystem Loaded");

        // Network
        clientID = nwObject.OwnerClientId;
        nwID = nwObject.NetworkObjectId;

        var LocalGM = LocalGeneralManager.Instance;
        // UI
        MainMenu = LocalGM.MainMenu;
        uiGeneral = MainMenu.GetComponent<UIGeneral>();
        uiGeneral.invSystem = invSystem;
        uiGeneral.Setup();
        uiGeneral.uI_PlayerSettings = LocalGM.UI_playerSettings;
        UI_Hotbar.Instance.hotbarSystem = hotbarSystem;
        var PlayerName = PlayerDataManager.LoadedPlayerProfileData.PlayerName;
        var lastDot = PlayerName.LastIndexOf('#');

        // カメラ
        CVCamera = LocalGM.CVCamera;
        CVCamera.Follow = CameraPos;

        // EntityManager
        LocalGM.emSystem.Setup(this);

        projectile = LocalGM.GetComponent<Projectile>();
        projectile.clientID = clientID;
        projectile.nwoID = nwObject.NetworkObjectId;
        projectile.CameraPos = CameraPos;
        projectile.Setup();
        hotbarSystem.ChangeActionPoint += (xform) => projectile.ShotPos = xform;
        var paControl = LocalGM.GetComponent<PlayerActionControl>();
        paControl.invSystem = invSystem;
        paControl.handControl = GetComponentInChildren<HandControl>();
        InputSetUp(LocalGM.GetComponent<PlayerInput>());
        clap_a.isOwner = isOwner;
        faceSync.tracker = LocalGM.GetComponent<Face2Face>();
        ClapChat.Setup();
        customLifeAvatar = GetComponent<CustomLifeAvatar>();
        customLifeAvatar.ModelIDs = await PlayerDataManager.LoadData<List<int>>("CustomLifeAvatarParts");
        var tempcolors = await PlayerDataManager.LoadData<List<SerializableColor>>("CustomLifeAvatarColors");
        SerializableColor.ToColors(tempcolors.ToArray(), out customLifeAvatar.colors);
        customLifeAvatar.Combiner(true);

        // 設定
        LoadSettings();

        // ホットバーの同期
        UI_Hotbar.Instance.LoadHotbar();

        // プレイヤーの初期位置にテレポート
        GetComponent<NetworkTransform>().Teleport(LocalGM.transform.position, LocalGM.transform.rotation, transform.localScale);

        IsLoaded = true;

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
        var loadedData = await PlayerDataManager.LoadData<SettingsData>();
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
            UI_Hotbar.Instance.SelectHotbarSlot(0);
            invSystem.SelectedSlotIndex = 0;
        }
    }

    public void SelectSecSlot(InputAction.CallbackContext callback)
    {
        if (callback.performed)
        {
            hotbarSystem.SelectedHotbarSlot(1);
            UI_Hotbar.Instance.SelectHotbarSlot(1);
            invSystem.SelectedSlotIndex = 1;
        }
    }

    public void SelectGraSlot(InputAction.CallbackContext callback)
    {
        if (callback.performed)
        {
            hotbarSystem.SelectedHotbarSlot(2);
            UI_Hotbar.Instance.SelectHotbarSlot(2);
            invSystem.SelectedSlotIndex = 2;
        }
    }

    public void SelectSub0Slot(InputAction.CallbackContext callback)
    {
        if (callback.performed)
        {
            hotbarSystem.SelectedHotbarSlot(3);
            UI_Hotbar.Instance.SelectHotbarSlot(3);
            invSystem.SelectedSlotIndex = 3;
        }
    }

    public void SelectSub1Slot(InputAction.CallbackContext callback)
    {
        if (callback.performed)
        {
            hotbarSystem.SelectedHotbarSlot(4);
            UI_Hotbar.Instance.SelectHotbarSlot(4);
            invSystem.SelectedSlotIndex = 4;
        }
    }

    public void ClearInput()
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
