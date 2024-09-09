using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using System.Data.Common;
using System;

public class CharactorMovement_v2 : MonoBehaviour
{
    #region Movement

    [Header("Movement")]
    [SerializeField] private float speed = 4;
    public float dashSpeed = 8, walkSpeed = 4, crouchSpeed = 3;
    [SerializeField] private float InAirMoveLimiter = 0.7f, GroundedMoveLimiter = 5.0f, hzSpeed, ySpeed, vSpeed, hzInput, vInput, moveForceLimiter = 5.0f, xForce, zForce;
    private Vector3 moveDir;

    [Header("Jump")]
    public float jumpPower = 5;
    private bool CanDoubleJump = true, CanJump = true;

    [Header("Slope")]
    [SerializeField] private float slopeAngle, maxSlopeAngle;
    private RaycastHit slopeHit;
    private Vector3 SlideMoveDir;

    [Header("WallRun & Climb")]
    [SerializeField] private LayerMask wall;
    [SerializeField] private float testNum, wallrunForce, SideWallCheckDistance, FrontWallCheckDistance, wallKickPower = 10, wallrunSpeed, wallAngle, climbSpeed, climbForceLimiter;
    [SerializeField] private bool isWallRight, isWallLeft, isWallForward;
    private RaycastHit leftWallhit, rightWallhit, forwardWallhit;

    [Header("Sliding")]
    private int slidingNum = 1;

    [Header("Dodge")]
    public float dodgeCT = 1;
    private bool CanDodge = true;
    private float dodgeTimer, dodgeAnimTime, dodgevInput, dodgehzInput;
    [SerializeField] private  AnimationCurve dodgeCurve;

    #endregion
    #region Aim

    [Space(10)]
    [Header("Aim")]

    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform CameraPos;
    [SerializeField] private AxisState hzAxisState, vAxisState;
    [SerializeField] private float defCameraDistance, maxCameraDistance, minCameraDistance, DistanceSensitive;

    #endregion
    #region GroundCheck

    [Space(10)]
    [Header("GroundCheck")]

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.1f, playerHeight;

    #endregion
    #region Animation

    private Animator anim;

    #endregion
    #region Others

    private Rigidbody rb;
    private CapsuleCollider col;
    public float TimeScale = 1;
    [SerializeField] private Transform orientation, PlayerAvatar;
    [SerializeField] bool Crouch, Walk, Dash, Dodge, Slide, WallRun, Climb, Jump, AirJump;

    #endregion
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        anim = PlayerAvatar.GetComponent<Animator>();

        SetCoolTime();
    }
    void FixedUpdate()
    {
        StateController();
        moveDir = speed * (transform.right * hzInput + transform.forward * vInput);
        xForce = moveForceLimiter * (moveDir.x - rb.linearVelocity.x);
        zForce = moveForceLimiter * (moveDir.z - rb.linearVelocity.z);
    }
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            rb.AddForce(transform.forward * 50, ForceMode.Impulse);
        }

        //入力系
        InputUpdate();

        //カメラ距離変更
        CameraUpdate();

        //WallRun
        CheckForWall();

        Animator();
        AnimationAdjust();

        Time.timeScale = TimeScale;

        DodgeCT();

        SpeedController();

        if (Dodge && !Slide) DodgeUpdate(dodgehzInput, dodgevInput);
        if (Slide && !Dodge) Sliding();

        if (Input.GetKeyDown(KeyCode.UpArrow)) speed += 0.5f;
        if (Input.GetKeyDown(KeyCode.DownArrow)) speed -= 0.5f;
    }

    #region Movement
    private void Movement()
    {
        if (OnSlope() && hzInput == 0 && vInput == 0 && ySpeed < 0.3f && rb.linearVelocity.magnitude < 0.4f) 
        {
            rb.linearVelocity = Vector3.zero;
        }
        else rb.useGravity = true;

        if (OnSlope())
        {
            rb.AddForce(GetGroundDir(new Vector3(xForce, 0, zForce)) * speed * 4);
        }
        else rb.AddForce(xForce, 0, zForce);
    }
    #endregion

    private Vector3 GetGroundDir(Vector3 inputDir)
    {
        return Vector3.ProjectOnPlane(inputDir, slopeHit.normal).normalized;
    }

    #region Wallrun
    private void WallrunMovement()
    {
        rb.useGravity = false;
        WallRun = true;
        Climb = false;

        Vector3 wallNormal = isWallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        wallAngle = Mathf.Atan2(wallForward.z, wallForward.x) * Mathf.Rad2Deg;

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude) wallForward = -wallForward;

        moveDir = wallrunSpeed * wallForward * wallrunForce;
        xForce = moveForceLimiter * (moveDir.x - rb.linearVelocity.x);
        zForce = moveForceLimiter * (moveDir.z - rb.linearVelocity.z);
        rb.AddForce(xForce, 0, zForce);

        if (!(isWallLeft && hzInput > 0) && !(isWallRight && hzInput < 0))
        {
           rb.AddForce(-wallNormal * 20);
           anim.SetBool("isWallRunning", true);
        }

        if (!isWallRight || !isWallLeft) anim.SetBool("isWallRunning", false);

        anim.SetInteger("JumpNum", 0);
    }
    #endregion

    #region Climb
    private void Climbing()
    {
        rb.useGravity = false;
        Climb = true;
        WallRun = false;

        Vector3 wallForward = Vector3.Cross(forwardWallhit.normal, transform.up);
        wallAngle = Mathf.Atan2(wallForward.z, wallForward.x) * Mathf.Rad2Deg;

        moveDir = climbSpeed * (wallForward * hzInput + transform.up * vInput) ;
        xForce = climbForceLimiter * (moveDir.x - rb.linearVelocity.x);
        zForce = climbForceLimiter * (moveDir.z - rb.linearVelocity.z);
        float yForce = climbForceLimiter * (moveDir.y - rb.linearVelocity.y);
        rb.AddForce(xForce, yForce, zForce);
    }
    #endregion

    #region Jump
    public void Jumping(InputAction.CallbackContext context)
    {

        if (context.performed && CanJump)
        {
            if (WallRun || Climb)
            {
                if (isWallRight) rb.AddForce(-transform.right * wallKickPower, ForceMode.Impulse);
                else if (isWallLeft) rb.AddForce(transform.right * wallKickPower, ForceMode.Impulse);
                else if (isWallForward) rb.AddForce(-transform.forward * wallKickPower, ForceMode.Impulse);
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpPower, rb.linearVelocity.z);
                anim.SetInteger("JumpNum", 0);
                anim.SetBool("isWallRunning", false);
            }

            if (IsGrounded() && !Dodge)
            {
                anim.SetInteger("JumpNum", 2);
                rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                CanDoubleJump = true;
            }
            else if (CanDoubleJump && !Dodge)
            {
                anim.SetInteger("JumpNum", 1);
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpPower, rb.linearVelocity.z);
                CanDoubleJump = false;
            }
        }

    }
    #endregion

    #region Dodge
    public void Dodging(InputAction.CallbackContext context)
    {
        if (context.performed && CanDodge &&!Climb)
        {
            anim.SetInteger("Dodge", 1);
            if (vInput == 0 && hzInput == 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, -jumpPower * 3, rb.linearVelocity.z);
                CanDodge = false;
            }
            else
            {
                StartDodge();
                CanDodge = false;
            }
        }
    }

    private void StartDodge()
    {
        Dodge = true;
        dodgeAnimTime = 0.0f;
        dodgevInput = vInput;
        dodgehzInput = hzInput;
        anim.SetFloat("DodgeVInput", dodgevInput);
        anim.SetFloat("DodgeHzInput", dodgehzInput);
    }

    private void DodgeUpdate(float hz, float v)
    {
        dodgeAnimTime += Time.deltaTime;
        if (IsGrounded())
        {
            if (dodgeAnimTime < 0.8f)
            {
                float t = dodgeAnimTime / 0.8f;
                float dodgeSpeed = dodgeCurve.Evaluate(t);
                Vector3 dodgeVelocity = dodgeSpeed * (transform.forward * v + transform.right * hz) * 20;
                rb.linearVelocity = dodgeVelocity;
            }
            else Dodge = false;
        }
        else
        {
            if (dodgeAnimTime < 0.5f)
            {
                float t = dodgeAnimTime / 0.5f;
                float dodgeSpeed = dodgeCurve.Evaluate(t);
                Vector3 dodgeVelocity = dodgeSpeed * (transform.forward * v + transform.right * hz) * 30;
                rb.linearVelocity = dodgeVelocity;
            }
            else Dodge = false;
        }
    }
    private void DodgeCT()
    {
        if (!CanDodge) dodgeTimer += Time.deltaTime;
        if (dodgeTimer >= dodgeCT)
        {
            CanDodge = true;
            dodgeTimer = 0;
        }
    }
    #endregion

    #region Slide
    private void StartSliding()
    {
        Slide = true;
        anim.SetBool("isSliding", true);
        slidingNum = 1;
        SlideMoveDir = transform.right * hzInput + transform.forward * vInput;
    }

    private void Sliding()
    {
        if((vSpeed < 6 && vSpeed > -6 && hzSpeed < 6 && hzSpeed > -6) || !IsGrounded())
        {
            Slide = false;
            anim.SetBool("isSliding", false);
        }
        else
        {
            if(OnSlope())
            {
                rb.AddForce(GetGroundDir(moveDir) * 4);
            }
            else if (slidingNum == 1)
            {
                rb.AddForce(SlideMoveDir * 8, ForceMode.Impulse);
                slidingNum = 0;
            }
        }
    }
    #endregion
    #region Others
    private void SetCoolTime()
    {
        dodgeTimer = dodgeCT;
    }


    private bool IsGrounded()
    {
        Vector3 groundCheckPosition = new Vector3(col.bounds.center.x, col.bounds.min.y, col.bounds.center.z);
        return Physics.CheckSphere(groundCheckPosition, groundCheckRadius, groundLayer);
    }
    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector2.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return slopeAngle < maxSlopeAngle && slopeAngle != 0;
        }
        return false;
    }
    private void CheckForWall()
    {
        isWallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, SideWallCheckDistance, wall); //Raycast(始点, 向き, 出力, 発射距離, hitレイヤー)
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, SideWallCheckDistance, wall);
        isWallForward = Physics.Raycast(transform.position, orientation.forward, out forwardWallhit, FrontWallCheckDistance, wall);
    }
    #endregion

    #region Input&Camera
    public void MoveInput(InputAction.CallbackContext context)
    {
        hzInput = context.ReadValue<Vector2>().x;
        vInput = context.ReadValue<Vector2>().y;
    }
    private void InputUpdate()
    {
        vSpeed = MathF.Round(Vector3.Dot(rb.linearVelocity, transform.forward) * 100) / 100;
        hzSpeed = MathF.Round(Vector3.Dot(rb.linearVelocity, transform.right) * 100) / 100;
        ySpeed = MathF.Round(Vector3.Dot(rb.linearVelocity, transform.up) * 100) / 100;

        //視点hz&v更新
        hzAxisState.Update(Time.deltaTime);
        vAxisState.Update(Time.deltaTime);
    }
    private void CameraUpdate()
    {
        var hzRotation = Quaternion.AngleAxis(hzAxisState.Value, Vector3.up);
        var vRotation = Quaternion.AngleAxis(vAxisState.Value, Vector3.right);

        rb.MoveRotation(hzRotation);
        CameraPos.localRotation = vRotation;

        var distance = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            distance.CameraDistance += defCameraDistance * Time.deltaTime * DistanceSensitive;
            distance.CameraDistance = Mathf.Clamp(distance.CameraDistance, minCameraDistance, maxCameraDistance);
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            distance.CameraDistance -= defCameraDistance * Time.deltaTime * DistanceSensitive;
            distance.CameraDistance = Mathf.Clamp(distance.CameraDistance, minCameraDistance, maxCameraDistance);
        }
        if ( distance.CameraDistance < minCameraDistance )
        {
            distance.CameraDistance = minCameraDistance;
        }
        if ( distance.CameraDistance > maxCameraDistance )
        {
            distance.CameraDistance = maxCameraDistance;
        }
    }
    #endregion

    #region Animator
    private void Animator()
    {
        if (anim.GetFloat("vSpeed") <= 0.01 && anim.GetFloat("vSpeed") >= -0.01 && vInput == 0) anim.SetFloat("vSpeed", 0);
        else anim.SetFloat("vSpeed", vSpeed, 0.1f, Time.deltaTime);
        if (anim.GetFloat("hzSpeed") <= 0.01 && anim.GetFloat("hzSpeed") >= -0.01 && hzInput == 0) anim.SetFloat("hzSpeed", 0);
        else anim.SetFloat("hzSpeed", hzSpeed, 0.1f, Time.deltaTime);
        anim.SetFloat("ySpeed", ySpeed);

        anim.SetFloat("vInput", vInput);
        anim.SetFloat("hzInput", hzInput);

        anim.SetBool("isWallRight", isWallRight);
        anim.SetBool("isWallLeft", isWallLeft);
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("AirJump")) anim.SetInteger("JumpNum", 0);
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("GroundedDodge") || anim.GetCurrentAnimatorStateInfo(0).IsName("InAirDodge")) anim.SetInteger("Dodge", 0);

        anim.SetBool("CanDoubleJump", CanDoubleJump);

        anim.SetBool("isCrouching", Crouch);

        anim.SetBool("isClimbing", Climb);

        anim.SetBool("isWallRunning", WallRun);

        if (IsGrounded() || OnSlope())//接地判定
        {
            anim.SetBool("InAir", false);
            anim.SetBool("AirJump", false);
            anim.SetBool("isWallRunning", false);
            moveForceLimiter = GroundedMoveLimiter;
        }
        else
        {
            anim.SetBool("InAir", true);
            anim.SetBool("isSliding", false);
            moveForceLimiter = InAirMoveLimiter;
        }

    }

    private void AnimationAdjust()
    {
        if (isWallLeft)
        {
            PlayerAvatar.transform.position = Vector3.Lerp(PlayerAvatar.transform.position, transform.position + transform.right * 0.45f, 0.1f);
            PlayerAvatar.transform.rotation = Quaternion.Euler(0f, -(wallAngle - 90), 0f);
        }
        if (isWallRight)
        {
            PlayerAvatar.transform.position = Vector3.Lerp(PlayerAvatar.transform.position, transform.position - transform.right * 0.45f, 0.1f);
            PlayerAvatar.transform.rotation = Quaternion.Euler(0f, -(wallAngle + 90), 0f);
        }
        if (Climb)
        {
            PlayerAvatar.transform.position = Vector3.Lerp(PlayerAvatar.transform.position, transform.position - transform.forward * -0.05f, 0.1f);
            PlayerAvatar.transform.rotation = Quaternion.Euler(0f, -wallAngle, 0f);
        }

        if (!isWallRight && !isWallLeft && !Climb)
        {
            PlayerAvatar.transform.position = Vector3.Lerp(PlayerAvatar.transform.position, transform.position, 0.1f);
        }

        if (Slide)
        {
            Vector3 slopeNormal = slopeHit.normal;
            Quaternion rotationY = Quaternion.Euler(0f, transform.rotation.y, 0f);
            Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, slopeNormal).normalized;
            Quaternion rotationXZ = Quaternion.LookRotation(projectedForward, slopeNormal);

            PlayerAvatar.transform.rotation = rotationXZ * rotationY;
        }
        if (!Slide && !isWallRight && !isWallLeft && !Climb)PlayerAvatar.transform.rotation = transform.rotation;
    }
    #endregion

    #region Controller
    private void StateController()
    {
        if(!Crouch) Walk = !Dash;
        else Walk = Dash = false;

        if (!Slide && !Dodge && !Climb && !WallRun) Movement();

        if (!Slide && !Dodge) CanJump = true | false;

        if ((isWallRight || isWallLeft) && vInput > 0.1f && !IsGrounded() && !Dodge && !Slide)
        {
            WallrunMovement();
        }
        else if (isWallForward && !Dodge && !Slide && !IsGrounded())
        {
            Climbing();
        }
        else
        {
            // rb.useGravity = true;
            WallRun = false;
            Climb = false;
        }
        CanDodge = !Slide;
    }

    private void SpeedController()
    {
        if (Walk) speed = walkSpeed;
        if (Dash) speed = dashSpeed;
        if (Crouch) speed = crouchSpeed;
    }

    public void SwitchWalkDash(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Dash = true;
        }
        if (context.canceled)
        {
            Walk = true;
            Dash = false;
        }
    }

    public void SwitchCrouch(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if((vSpeed < -6.8 || vSpeed > 6.8 || hzSpeed < -6.8 || hzSpeed > 6.8) && !Dodge) StartSliding();
            else Crouch = true;
        }
        if (context.canceled)
        {
            Walk = true;
            Crouch = false;
        }
    }
    #endregion
}
