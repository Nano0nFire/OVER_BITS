using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;


public class CLAPlus_MovementModule : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] CapsuleCollider col;
    [SerializeField] Transform CameraPos;
    [SerializeField] Transform raycastPos;
    [SerializeField] LayerMask GroundLayer; // 地面のレイヤー

    [Header("Movement Speed")]
    [SerializeField] float Speed;
    static readonly float DefWalkSpeed = 2, DefDashSpeed = 8, DefCrouchSpeed = 1; // デフォルトの移動速度
    public float ExWalkSpeed, ExDashSpeed, ExCrouchSpeed; // 追加の移動速度
    [SerializeField] float MovePowerLimiter;
    static readonly float OnGroundMovePowerLimiter = 5, InAirMovePowerLimiter = 2;
    public RaycastHit slopeHit;
    static readonly float maxSlopeAngle = 18;
    float MaxGroundCheckDistance
    {
        get
        {
            if (Mathf.Abs(SlopeAngle) > 0.5)
            {
                OnSlope = true;
                PublicGroundCheckDistance =  Mathf.Abs(col.radius * (1 / Mathf.Cos(SlopeAngle * Mathf.Deg2Rad) - 1));
            }
            else
            {
                OnSlope = false;
                PublicGroundCheckDistance =  0.01f;
            }

            return PublicGroundCheckDistance;
        }
    }

    public float PublicGroundCheckDistance;
    float SlopeAngle
    {
        get
        {
            Physics.Raycast(col.bounds.center, Vector2.down, out slopeHit, GroundLayer);
            return Vector3.Angle(Vector3.up, slopeHit.normal);
        }
    }
    bool IsGrounded // 地面に接しているか
    {
        get
        {
            _IsGrounded = Physics.CheckSphere(new Vector3(col.bounds.center.x, col.bounds.min.y, col.bounds.center.z), MaxGroundCheckDistance, GroundLayer);
            return _IsGrounded;
        }
    }
    public bool _IsGrounded
    {
        get
        {
            return _isGrounded;
        }
        set
        {
            if (_isGrounded != value)
            {
                _isGrounded = value;
                StateController(20); // OnSlopeが切り替わったときにStateControllerを呼び出す
            }
        }
    }
    bool _isGrounded;
    bool GroundedCheck;
    [SerializeField] bool _onSlope;

    public bool OnSlope
    {
        get
        {
            return _onSlope;
        }
        set
        {
            if (_onSlope != value) // 値が変更された場合のみ
            {
                _onSlope = value;
                StateController(20); // OnSlopeが切り替わったときにStateControllerを呼び出す
            }
        }
    }

    [Header("Jump")]
    static readonly float DefJumpPower = 5; // デフォルトのジャンプ力
    public float ExJumpPower; // 追加のジャンプ力
    static readonly float jumpCT = 0.5f;
    float JumpPower
    {
        get
        {
            return DefJumpPower + ExJumpPower;
        }
    }

    [Header("ActionSettings")]
    public int MaxActioinPoint = 1;
    public float ChargeTimePerPoint;
    public float ChargingPoint {get; private set;}
    public int ActionPoint // 空中で可能な動作の回数
    {
        get
        {
            return _ActionPoint;
        }
        private set
        {
            if (value < 0)
                return;
            else if (value == MaxActioinPoint)
            {
                IsChargingActionPoint = false;
                _ActionPoint = value;
            }
            else if (value > MaxActioinPoint)
            {
                IsChargingActionPoint = false;
                _ActionPoint = MaxActioinPoint;
            }
            else ChargeActionPoint();
        }
    }
    int _ActionPoint = 1;
    bool IsChargingActionPoint;
    public bool canAction = true;

    [Header("WallRun & Climb")]
    public RaycastHit rightWallHit, leftWallHit, forwardWallHit;
    public bool IsCheckingWall;
    [SerializeField] float wallrunForce, FrontCheckDistance, SideCheckDistance, wallKickPower;
    Vector3 wallForward, wallNormal;
    public float WallAngle
    {
        get
        {
            return Mathf.Atan2(wallForward.z, wallForward.x) * Mathf.Rad2Deg;
        }
    }
    public bool IsWallRight
    {
        get
        {
            return Physics.Raycast(transform.position, raycastPos.right, out rightWallHit, SideCheckDistance, GroundLayer);
        }
    }
    public bool IsWallLeft
    {
        get
        {
            return Physics.Raycast(transform.position, -raycastPos.right, out leftWallHit, SideCheckDistance, GroundLayer);
        }
    }
    public bool IsWallForward
    {
        get
        {
            return Physics.Raycast(transform.position, raycastPos.forward, out forwardWallHit, FrontCheckDistance, GroundLayer);
        }
    }

    [Header("Dodge")]
    [SerializeField] float dodgeCT;
    [SerializeField] float dodgeSpeed = 1, dodgeActionSpeed = 1; // dodge中の移動速度とカーブの再生速度
    [SerializeField] AnimationCurve dodgeCurve;
    float dodgeAnimTime;
    Vector3 dodgeVec;

    [Header("Rush")]
    [SerializeField] float rushCT;
    [SerializeField] float rushSpeed; // rush時のスピード
    public int rushActiveTime; // rush後の速度制限緩和の効果時間
    bool isRush;

    [Header("Slide")]
    [HideInInspector] public bool CanSlide = true;
    [HideInInspector] public bool isSlide;
    float CanSlideSpeed
    {
        get
        {
            return DefDashSpeed + ExDashSpeed - 1.2f; // ダッシュ時の速度から1.2を引いた速さを目安としてスライディングができるかどうかを判断する
        }
    }
    static readonly float slideCT = 2;
    static readonly float slidePower = 500;

    [Header("Parameter")]
    [HideInInspector] public float HzInput, VInput; // デバイスからの入力(GeneralManagerが常に設定)
    public float InputDir
    {
        get
        {
            return - Vector2.SignedAngle(new Vector2(0,1), new Vector2(HzInput, VInput));
        }
    }
    [HideInInspector] public float HzCameraSens, VCameraSens;
    public float HzRotation
    {
        get
        {
            return _HzRotation;
        }
        set
        {
            _HzRotation += value / HzCameraSens;
            if (_HzRotation > 180) _HzRotation = -180;
            else if (_HzRotation < -180) _HzRotation = 180; // 180度までいったらループさせる
        }
    }
    public float VRotation
    {
        get
        {
            return _VRotation;
        }
        set
        {
            _VRotation += value / VCameraSens;
            if (_VRotation > 90) _VRotation = 90;
            else if (_VRotation < -90) _VRotation = -90; // 90度の角度制限
        }
    }
    float _HzRotation, _VRotation;
    float xForce, yForce, zForce; // x方向とz方向に加える力(小数点以下3桁)
    Vector3 moveDir; // 動く方向(inputとSpeedから求められる)
    Vector3 beforeForwardVec, beforeRightVec; // アクション時にtransformの情報を保存する
    float beforeHzInput, beforeVInput; // アクション時にインプットの情報を保存する
    public float NowSpeed
    {
        get
        {
            return rb.linearVelocity.magnitude;
        }
    }

    States State
    {
        get
        {
            if (IsGrounded)
                return Gstate;
            else
            {
                if (Astate == States.falling)
                {
                    CheckGround();
                }
                return Astate;
            }
        }
    }
    public States Astate // ActionState
    {
        get
        {
            return _Astate;
        }
        set
        {
            _Astate = value;
            StateController();
        }
    }
    public States Gstate // IsGroundedState
    {
        get
        {
            return _Gstate;
        }
        set
        {
            _Gstate = value;
            StateController();
        }
    }
    [SerializeField] States _Astate, _Gstate;
    States KeepGState;
    public States PublicState;
    CancellationToken OnDestroyToken;

    public States test;
    public float testHZ, testV;
    public Vector3 tetsveb;
    public bool testBooool;

    void Start()
    {
        Gstate = States.walk;
        Astate = States.falling;
        StateController();
        ActionPoint = MaxActioinPoint;

        OnDestroyToken = this.GetCancellationTokenOnDestroy();
    }

    void FixedUpdate()
    {
        CameraUpdate();

        test = State;
        testV = NowSpeed;

        if (xForce == 0 && yForce == 0 && zForce == 0 && HzInput == 0 && VInput == 0 && State != States.wallRun && State != States.climb || State == States.dodge)
            return;

        Movement();
    }

    void MoveDirCalculator(Vector3 moveDir, bool UseYLimiter = false)
    {
        xForce = Mathf.Floor(MovePowerLimiter * (moveDir.x - rb.linearVelocity.x) * 1000) / 1000; // 4桁目以降は切り捨て
        yForce = UseYLimiter ? Mathf.Floor(MovePowerLimiter * (moveDir.y - rb.linearVelocity.y) * 1000) / 1000 : Mathf.Floor(moveDir.y * 1000) / 1000; // 4桁目以降は切り捨て
        zForce = Mathf.Floor(MovePowerLimiter * (moveDir.z - rb.linearVelocity.z) * 1000) / 1000; // 4桁目以降は切り捨て
    }

    void Movement()
    {
        Transform transformCashed = transform;

        switch (State)
        {
            case States.walk:
            case States.dash:
            case States.crouch:
                if (Mathf.Abs(SlopeAngle) > maxSlopeAngle) // 急な坂の時は登らない
                {
                    moveDir = Vector3.zero;
                    break;
                }
                else if (Mathf.Abs(SlopeAngle) < 0.5) // 平面に近い面は傾斜による調整を行わない
                {
                    moveDir = Speed * (transformCashed.right * HzInput + transformCashed.forward * VInput);
                    break;
                }
                else
                {
                    moveDir = Speed * Vector3.ProjectOnPlane(transformCashed.right * HzInput + transformCashed.forward * VInput, slopeHit.normal);

                    if (MathF.Abs(rb.linearVelocity.y) < testHZ && (MathF.Abs(xForce) + MathF.Abs(zForce)) / 2 < testHZ)
                    {
                        rb.AddForce(-testV * Mathf.Sin(SlopeAngle * Mathf.Deg2Rad) * rb.mass * Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal), ForceMode.Force);
                    }

                    break;
                }

            case States.falling:
            case States.Jump:
            case States.AirJump:
            case States.slide:
                moveDir = Speed * (transformCashed.right * HzInput + transformCashed.forward * VInput);
                break;

            case States.wallRun:
            case States.climb:
                ClimbAndWallRun();
                break;

            case States.rush:
                moveDir = Speed * (transformCashed.right * HzInput + transformCashed.forward * VInput);
                break;
        };

        MoveDirCalculator(moveDir, State == States.climb);
        rb.AddForce(xForce, yForce, zForce);
    }

    public void Jump()
    {
        if (State == States.wallRun)
        {
            Astate = States.AirJump;
            rb.AddForce(wallNormal * wallKickPower + Vector3.up * wallKickPower / 2, ForceMode.Impulse);
        }
        else if (IsGrounded)
        {
            if (State != States.rush) Astate = States.Jump;
            rb.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);
        }
        else if (ActionPoint != 0 && canAction)
        {
            if (State != States.rush) Astate = States.AirJump;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, JumpPower, rb.linearVelocity.z);
            ActionPoint --;
            ActionCoolTime(jumpCT);
        }

        CheckGround();
    }

    void ClimbAndWallRun()
    {
        if (IsWallRight || IsWallLeft)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // 上下方向への力をカットする

            wallNormal = IsWallRight ? rightWallHit.normal : leftWallHit.normal;
            wallForward = Vector3.Cross(wallNormal, transform.up);

            if ((raycastPos.forward - wallForward).magnitude > (raycastPos.forward - -wallForward).magnitude)
                wallForward = -wallForward;

            moveDir = Speed * (transform.right * HzInput + wallForward * VInput);

            if (!(InputDir > 60 && InputDir < 120 && IsWallLeft) && !(InputDir > -120 && InputDir < -60 && IsWallRight))
                rb.AddForce(-wallNormal * 5); //壁への吸いつき
            else
            {
                Astate = States.falling;
                IsCheckingWall = false;
            }
        }
        else if (IsWallForward)
        {
            wallNormal = forwardWallHit.normal;
            wallForward = Vector3.Cross(wallNormal, transform.up);

            moveDir = Speed * (wallForward * HzInput + transform.up * VInput);

            rb.AddForce(-wallNormal * 5); //壁への吸いつき
        }
        else
        {
            Astate = States.falling;
            return;
        }
    }

    public async void Dodge()
    {
        if (!canAction) return;

        KeepGState = Gstate;

        Gstate = States.dodge;
        Astate = States.dodge;

        ActionCoolTime(dodgeCT);
        dodgeAnimTime = 0;
        beforeForwardVec = transform.forward;
        beforeRightVec = transform.right;
        beforeHzInput = HzInput;
        beforeVInput = VInput;

        while (dodgeAnimTime < 1)
        {
            dodgeVec = dodgeCurve.Evaluate(dodgeAnimTime) * Speed * (beforeRightVec * beforeHzInput + beforeForwardVec * beforeVInput);

            rb.linearVelocity = new Vector3(dodgeVec.x, rb.linearVelocity.y, dodgeVec.z);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            dodgeAnimTime += Time.deltaTime * dodgeActionSpeed;
        }

        Gstate = KeepGState;
        if (!IsGrounded) Astate = States.falling;
    }

    public async void Rush(bool interrupt = false)
    {
        if (!canAction) return;

        if (!interrupt)
            KeepGState = Gstate;

        isRush = true;

        Gstate = States.rush;
        Astate = States.rush;

        ActionCoolTime(rushCT);

        rb.AddForce((Speed * (transform.right * HzInput + transform.forward * VInput)) + Vector3.up * JumpPower / 2, ForceMode.Impulse);

        await UniTask.Delay(TimeSpan.FromSeconds(rushActiveTime), cancellationToken : OnDestroyToken);

        isRush = false;
        Gstate = isSlide ? States.slide : KeepGState;

        if (!IsGrounded)
            Astate = States.falling;
    }

    public async void Slide(CancellationToken token, bool interrupt = false, Action callback = null)
    {
        try
        {
            if (!CanSlide && NowSpeed > CanSlideSpeed)
                return;

            CanSlide = false;
            isSlide = true;

            if (!interrupt)
                KeepGState = Gstate;

            Gstate = States.slide;

            rb.AddForce(slidePower * Vector3.ProjectOnPlane(transform.right * HzInput + transform.forward * VInput, slopeHit.normal));

            await UniTask.WaitUntil(() => NowSpeed < CanSlideSpeed, cancellationToken : token);

            isSlide = false;
            callback?.Invoke(); // スライディングが終わったときに指定の関数を呼び出す

            Gstate = isRush ? States.rush : KeepGState;

            await UniTask.Delay(TimeSpan.FromSeconds(slideCT), cancellationToken : OnDestroyToken);
            CanSlide = true;
        }
        catch (OperationCanceledException)
        {
            callback?.Invoke(); // キャンセルしたときに指定の関数を呼び出す

            Gstate = isRush ? States.rush : KeepGState;

            await UniTask.Delay(TimeSpan.FromSeconds(slideCT), cancellationToken : OnDestroyToken);
            CanSlide = true;
        }
    }

    public void SlideCancell()
    {

    }

    async void StateController(int t = 0)
    {
        PublicState = State;

        if (t != 0) await UniTask.Delay(t);

        switch (State)
        {
            case States.falling:
                rb.useGravity = true;
                MovePowerLimiter = InAirMovePowerLimiter;
                break;

            case States.walk:
                rb.useGravity = true;
                MovePowerLimiter = OnGroundMovePowerLimiter;
                Speed = DefWalkSpeed + ExWalkSpeed;
                break;

            case States.dash:
                rb.useGravity = true;
                MovePowerLimiter = OnGroundMovePowerLimiter;
                Speed = DefDashSpeed + ExDashSpeed;
                break;

            case States.crouch:
                rb.useGravity = true;
                MovePowerLimiter = OnGroundMovePowerLimiter;
                Speed = DefCrouchSpeed + ExCrouchSpeed;
                break;

            case States.Jump:
                rb.useGravity = true;
                MovePowerLimiter = InAirMovePowerLimiter;
                break;

            case States.AirJump:
                rb.useGravity = true;
                MovePowerLimiter = InAirMovePowerLimiter;
                break;

            case States.climb:
                rb.useGravity = false;
                MovePowerLimiter = OnGroundMovePowerLimiter;
                Speed = DefWalkSpeed + ExWalkSpeed;
                break;

            case States.wallRun:
                rb.useGravity = false;
                MovePowerLimiter = OnGroundMovePowerLimiter;
                Speed = DefDashSpeed + ExDashSpeed;
                break;

            case States.dodge:
                rb.useGravity = true;
                Speed = dodgeSpeed;
                break;

            case States.rush:
                rb.useGravity = true;
                MovePowerLimiter = 10; // 移動制限緩和
                Speed = rushSpeed;
                break;

            case States.slide:
                rb.useGravity = true;
                MovePowerLimiter = InAirMovePowerLimiter;
                Speed = DefDashSpeed + ExDashSpeed;
                break;
        }

        if (OnSlope)
        {
            MovePowerLimiter = InAirMovePowerLimiter;
        }
    }

    public void CameraUpdate()
    {
        rb.MoveRotation(Quaternion.Euler(0, HzRotation, 0));
        CameraPos.localRotation = Quaternion.Euler(VRotation, 0, 0);
    }

    async void CheckGround()
    {
        try
        {
            if (GroundedCheck)
                return;

            GroundedCheck = true;

            await UniTask.WaitUntil(() => IsGrounded, cancellationToken : OnDestroyToken); // このスクリプト内で発行したTokenを使用

            GroundedCheck = false;

            if (State != States.rush) Astate = States.falling;
            StateController();
        }
        catch (OperationCanceledException)
        {
            GroundedCheck = false;

            if (State != States.rush)
                Astate = States.falling;

            StateController();
        }
    }

    public async void CheckWall(CancellationToken token, Action<States> callback = null) // 呼び出し先でTokenを発行
    {
        try
        {
            if (IsCheckingWall)
                return;

            IsCheckingWall = true;

            States currentState = States.falling; // 現在の状態を保存

            do
            {
                if (IsWallRight || IsWallLeft)
                    Astate = States.wallRun;
                else if (IsWallForward)
                    Astate = States.climb;

                if (currentState == Astate) // 状態に変化がない場合はそのあとの処理をスキップ
                {
                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate); // 繰り返し処理が高速で行われないようにFixedUpdate分待つ
                    continue;
                }

                currentState = Astate; // 保存しておいた現在の状態を更新

                callback?.Invoke(currentState);

                await UniTask.Delay(100, cancellationToken : token); // 壁走りのCT
            } while (!IsGrounded && IsCheckingWall);
            IsCheckingWall = false;

            callback?.Invoke(State);
        }
        catch (OperationCanceledException)
        {
            IsCheckingWall = false;

            callback?.Invoke(State);
        }
    }

    async void ChargeActionPoint()
    {
        if (IsChargingActionPoint) return;

        IsChargingActionPoint = true; // ActionPointが
        while (IsChargingActionPoint)
        {
            await UniTask.Delay(100);
            ChargingPoint += 0.1f;
            if (ChargingPoint >= ChargeTimePerPoint)
            {
                ChargingPoint = 0;
                ActionPoint++;
            }
        }
    }

    async void ActionCoolTime(float t)
    {
        if (!canAction) return;
        canAction = false;
        float actionCT = 0;

        while (actionCT <= t)
        {
            await UniTask.Delay(100);
            actionCT += 0.1f;
        }

        canAction = true;
    }
}
public enum States
{
    falling,
    walk,
    dash,
    crouch,
    Jump,
    AirJump,
    climb,
    wallRun,
    slide,
    dodge,
    rush,
}