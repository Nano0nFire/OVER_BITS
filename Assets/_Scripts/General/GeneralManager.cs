using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;

public class GeneralManager : MonoBehaviour
{
    public GameObject MainMenu;
    public int FixedUpdateTimeSpan = 20;
    [SerializeField] UIGeneral uiGeneral;
    [SerializeField] CLAPlus_MovementModule clap_m;
    [SerializeField] CLAPlus_AnimationControlModuel clap_a;
    [SerializeField] PlayerStatus playerStatus;
    [SerializeField] States KeepState;
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
    CancellationTokenSource clap_mCTSource = new();
    CancellationTokenSource CTSource = new();

    bool CrouchPress;
    bool watchCrouchPress;
    bool IsCheckingWall = false;
    [Header("Movement Settings")]
    public bool ToggleDash = false; //ダッシュ切り替え or 長押しダッシュ
    public bool ToggleCrouch = false; //しゃがみ切り替え or 長押ししゃがみ

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
                clap_m.HzInput = 0;
                clap_m.VInput = 0;
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
        {
            clap_m.Jump();
            clap_a.Jump();
        }
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
            CrouchPress = true;
            CrouchAction();
        }
        if (context.canceled)
        {
            CrouchPress = false;
            if (clap_m.State == States.slide)
                clap_mCTSource.Cancel();

            if (!ToggleCrouch && clap_m.State == States.crouch)
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
            while (CrouchPress)
            {
                if (!clap_m._IsGrounded && clap_m.Astate != States.wallRun && clap_m.Astate != States.climb)
                {
                    clap_mCTSource.Cancel();
                    CTSource.Cancel();

                    clap_mCTSource = new();
                    clap_m.CheckWall(clap_mCTSource.Token, SetWallAnimation);

                    break;
                }
                else
                {
                    if (clap_m.State != States.slide && clap_m.State != States.dodge && clap_m.CanSlide)
                    {
                        clap_mCTSource = new();
                        clap_m.Slide(clap_mCTSource.Token, clap_m.State == States.rush);

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
                    clap_m.Rush(clap_m.State == States.slide);
                    clap_a.Rush();
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

    void SetWallAnimation(States CurrentState)
    {
        switch (CurrentState)
        {
            case States.wallRun:
                clap_a.WallRun(clap_m.IsWallRight); // アニメーションの実行
                break;

            case States.climb:
                clap_a.Climb();
                break;

            default:
                clap_a.EndWallRunAndClimb();
                break;
        }
    }

    void OnDestroy()
    {
        clap_mCTSource.Cancel();
        CTSource.Cancel();
    }
}
