using System.Collections;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    private PlayerData Data;

    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
    public PlayerAnimState CurrentAnimState { get; private set; }
    #endregion

    #region STATE PARAMETERS
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; set; }
    public bool IsWallJumping { get; set; }
    public bool IsDashing { get; set; }
    public bool IsSliding { get; set; }

    public float LastOnGroundTime { get; set; }
    public float LastOnWallTime { get; set; }
    public float LastOnWallRightTime { get; set; }
    public float LastOnWallLeftTime { get; set; }

    public bool _isJumpCut { get; set; }
    public bool _isJumpFalling { get; set; }
    public float _wallJumpStartTime { get; set; }
    public int _lastWallJumpDir { get; set; }
    public int _dashesLeft { get; set; }
    public bool _dashRefilling { get; set; }
    public Vector2 _lastDashDir { get; set; }
    public bool _isDashAttacking { get; set; }
    #endregion

    #region INPUT PARAMETERS
    private Vector2 _moveInput;
    public float LastPressedJumpTime { get; set; }
    public float LastPressedDashTime { get; set; }
    #endregion

    #region CAMERA
    [Header("Camera")]
    [SerializeField] private GameObject _cameraFollowGO;
    private CameraFollowObject _cameraFollowObject;
    private float _fallSpeedYDampingChangeThreshold;
    #endregion
    
    #region MODULES
    private PlayerRun playerRun;
    private PlayerJump playerJump;
    private PlayerDash playerDash;
    private PlayerSlide playerSlide;
    private PlayerGravity playerGravity;
    private PlayerCollision playerCollision;
    private PlayerCamera playerCamera;
    public PlayerGravity Gravity => playerGravity;
    #endregion

    #region EVENTS
    public Action<PlayerAnimState> OnAnimStateChanged;
    #endregion

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        playerCollision = GetComponent<PlayerCollision>();
        playerCamera = GetComponent<PlayerCamera>();
    }

    private void Start()
    {
        IsFacingRight = true;
        _cameraFollowObject = _cameraFollowGO.GetComponent<CameraFollowObject>();
        _fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;
        playerCamera.Initialize(RB, _fallSpeedYDampingChangeThreshold);
    }

    public void SetData(PlayerData data)
    {
        Data = data;
        playerRun = new PlayerRun(this, RB, Data);
        playerJump = new PlayerJump(this, RB, Data);
        playerDash = new PlayerDash(this, RB, Data);
        playerSlide = new PlayerSlide(this, RB, Data);
        playerGravity = new PlayerGravity(this, RB, Data);
    }

    public void Move(Vector2 input)
    {
        _moveInput = input;
        if (_moveInput.x > 0 || _moveInput.x < 0)
            TurnCheck();
        // Animation event
        if (LastOnGroundTime > 0)
        {
            if (Mathf.Abs(_moveInput.x) > 0.01f)
                SetAnimState(PlayerAnimState.Run);
            else
                SetAnimState(PlayerAnimState.Idle);
        }
    }

    public void JumpInput()
    {
        LastPressedJumpTime = Data.jumpInputBufferTime;
    }

    public void JumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpCut())
            _isJumpCut = true;
    }

    public void DashInput()
    {
        LastPressedDashTime = Data.dashInputBufferTime;
    }

    public void OnFixedUpdate()
    {
        // Timers
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;

        // Collision
        if (!IsDashing && !IsJumping)
        {
            if (playerCollision.IsGrounded())
                LastOnGroundTime = Data.coyoteTime;
            if (playerCollision.IsOnWall(IsFacingRight, IsWallJumping))
                LastOnWallRightTime = Data.coyoteTime;
            if (playerCollision.IsOnWallLeft(IsFacingRight, IsWallJumping))
                LastOnWallLeftTime = Data.coyoteTime;
            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }

        // Jump
        if (IsJumping && RB.velocity.y < 0)
        {
            IsJumping = false;
            _isJumpFalling = true;
        }
        if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
        {
            IsWallJumping = false;
        }
        if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
            _isJumpCut = false;
            _isJumpFalling = false;
            CameraManager.instance.ResetYDamping();
        }
        if (!IsDashing)
        {
            if (CanJump() && LastPressedJumpTime > 0)
            {
                IsJumping = true;
                IsWallJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;
                playerJump.Jump();
            }
            else if (CanWallJump() && LastPressedJumpTime > 0)
            {
                IsWallJumping = true;
                IsJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;
                _wallJumpStartTime = Time.time;
                _lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;
                playerJump.WallJump(_lastWallJumpDir);
            }
        }

        // Dash
        if (CanDash() && LastPressedDashTime > 0)
        {
            Sleep(Data.dashSleepTime);
            if (_moveInput != Vector2.zero)
                _lastDashDir = _moveInput;
            else
                _lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;
            IsDashing = true;
            IsJumping = false;
            IsWallJumping = false;
            _isJumpCut = false;
            StartCoroutine(playerDash.StartDash(_lastDashDir));
        }

        // Slide
        if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
            IsSliding = true;
        else
            IsSliding = false;

        // Gravity
        playerGravity.HandleGravity(_moveInput);

        // Movement
        if (!IsDashing)
        {
            if (IsWallJumping)
                playerRun.Run(Data.wallJumpRunLerp, _moveInput);
            else
                playerRun.Run(1, _moveInput);
        }
        else if (_isDashAttacking)
        {
            playerRun.Run(Data.dashEndRunLerp, _moveInput);
        }
        if (IsSliding)
            playerSlide.Slide();

        // Animation: Fall
        if (LastOnGroundTime <= 0 && RB.velocity.y < -0.01f && !IsSliding && !IsJumping && !IsWallJumping)
        {
            SetAnimState(PlayerAnimState.Fall);
        }
    }

    private void Sleep(float duration)
    {
        StartCoroutine(PerformSleep(duration));
    }

    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }

    private void Turn()
    {
        if (IsFacingRight)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            IsFacingRight = !IsFacingRight;
            _cameraFollowObject.CallTurn();
        }
        else
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            IsFacingRight = !IsFacingRight;
            _cameraFollowObject.CallTurn();
        }
    }

    public void TurnCheck()
    {
        if (_moveInput.x > 0 && !IsFacingRight)
        {
            Turn();
        }
        else if (_moveInput.x < 0 && IsFacingRight)
        {
            Turn();
        }
    }

    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }

    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
             (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
    }

    private bool CanJumpCut()
    {
        return IsJumping && RB.velocity.y > 0;
    }

    private bool CanWallJumpCut()
    {
        return IsWallJumping && RB.velocity.y > 0;
    }

    private bool CanDash()
    {
        if (!IsDashing && _dashesLeft < Data.dashAmount && LastOnGroundTime > 0 && !_dashRefilling)
        {
            StartCoroutine(playerDash.RefillDash(1));
        }
        return _dashesLeft > 0;
    }

    private bool CanSlide()
    {
        if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <= 0)
            return true;
        else
            return false;
    }

    public void DampingYCamera()
    {
        playerCamera.DampingYCamera();
    }

    public void SetAnimState(PlayerAnimState newState)
    {
        if (CurrentAnimState != newState)
        {
            CurrentAnimState = newState;
            OnAnimStateChanged?.Invoke(newState);
        }
    }
}