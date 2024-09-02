using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class GeneralManager : MonoBehaviour
{
    public GameObject MainMenu;
    public int FixedUpdateTimeSpan = 20;
    [SerializeField] UIGeneral uiGeneral;
    [SerializeField] CLAPlus_MovementModule clap_m;
    [SerializeField] States KeepState;
    public States SelectedSubAction = States.dodge;
    public bool IsOpeningUI
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
    CancellationTokenSource CTSource = new();
    bool CrouchPress;
    bool watchCrouchPress;
    [Header("Movement Settings")]
    public bool ToggleDash = false; //ダッシュ切り替え or 長押しダッシュ
    public bool ToggleCrouch = false; //しゃがみ切り替え or 長押ししゃがみ
    public bool ToggleClimb = true;

    [Header("Aim Settings")]
    public bool InvertAim; //垂直方向の視点操作の反転
    public float HzCameraSens //水平方向の視点感度
    {
        get
        {
            return clap_m.HzCameraSens;
        }
        set
        {
            clap_m.HzCameraSens = value;
        }
    }
    public float VCameraSens //水s直方向の視点感度
    {
        get
        {
            return clap_m.VCameraSens;
        }
        set
        {
            clap_m.VCameraSens = value;
        }
    }

    public void UIexit(InputAction.CallbackContext context)
    {
        if (context.performed)
            {
                uiGeneral.UIexit(MainMenu);
            }
    }
    public void MoveKeyInput(InputAction.CallbackContext context)
    {
        if (IsOpeningUI || ActionLock) return;

        clap_m.HzInput = context.ReadValue<Vector2>().x;
        clap_m.VInput = context.ReadValue<Vector2>().y;
    }
    public void JumpKeyInput(InputAction.CallbackContext context)
    {
        if (IsOpeningUI) return;

        if (context.performed)
            clap_m.Jump();
    }
    public void DashKeyInput(InputAction.CallbackContext context)
    {
        if (IsOpeningUI || clap_m.Gstate == States.rush) return;

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
        if (IsOpeningUI) return;

        if (context.performed)
        {
            Debug.Log("Call CrouchKeyInput");
            CrouchPress = true;
            CrouchAction().Forget();
        }
        if (context.canceled)
        {
            CrouchPress = false;
            if (clap_m.State == States.slide)
                CTSource.Cancel();

            if (!ToggleCrouch && clap_m.State == States.crouch)
                clap_m.Gstate = KeepState;
        }
    }

    async UniTask CrouchAction()
    {
        if (watchCrouchPress) return;
        watchCrouchPress = true;

        while (CrouchPress)
        {
            Debug.Log("running");
            if (!clap_m._IsGrounded)
            {
                if (clap_m.InputDir > -30 && clap_m.InputDir < 30 && clap_m.IsWallForward)
                {
                    clap_m.Astate = States.climb;
                    break;
                }
                else if (clap_m.IsWallRight || clap_m.IsWallLeft)
                {
                    clap_m.Astate = States.wallRun;
                    if (ToggleClimb)
                        clap_m.CheckWall();

                    break;
                }
            }
            else
            {
                if (clap_m.State != States.slide && clap_m.State != States.dodge && clap_m.CanSlide)
                {
                    CTSource = new();

                    clap_m.Slide(CTSource.Token, clap_m.State == States.rush);

                    Debug.Log("Call Slide");
                    break;
                }
                else if (ToggleCrouch)
                {
                    if (clap_m.Gstate == States.crouch) clap_m.Gstate = States.walk;
                    else clap_m.Gstate = States.crouch;

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

    public void SubAction(InputAction.CallbackContext context)
    {
        if (context.performed && clap_m.canAction)
        {
            CTSource.Cancel();

            switch (SelectedSubAction)
            {
                case States.dodge:
                    clap_m.Dodge();
                    break;

                case States.rush:
                    clap_m.Rush(clap_m.State == States.slide);
                    break;
            }
        }
    }

    public void CameraInput(InputAction.CallbackContext context)
    {
        if (IsOpeningUI) return;

        clap_m.HzRotation = context.ReadValue<Vector2>().x;
        clap_m.VRotation = InvertAim ? context.ReadValue<Vector2>().y : -context.ReadValue<Vector2>().y;
    }
}
