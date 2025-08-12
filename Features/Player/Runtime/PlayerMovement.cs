using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	private PlayerData Data;

	#region COMPONENTS
    public Rigidbody2D RB { get; private set; }
	public event System.Action<PlayerAnimState> OnAnimStateChanged;
	public event System.Action OnLanded;
	public event System.Action OnJumped;
	public event System.Action OnDashedStart;
	public event System.Action OnDashedEnd;
	public event System.Action<bool> OnFacingChanged; // bool: IsFacingRight
	private PlayerAnimState _currentAnimState;
	public PlayerAnimState CurrentAnimState
	{
		get => _currentAnimState;
		set
		{
			if (_currentAnimState == value) return;
			_currentAnimState = value;
			OnAnimStateChanged?.Invoke(_currentAnimState);
		}
	}
	#endregion

	#region STATE PARAMETERS
	public bool IsFacingRight { get; set; }
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
	private bool _wasGrounded;
	#endregion

	#region INPUT PARAMETERS
	public Vector2 _moveInput { get; private set; }
	public float LastPressedJumpTime { get; set; }
	public float LastPressedDashTime { get; set; }
	#endregion

	#region CHECK PARAMETERS
	[Header("Sensors")]
	[SerializeField] private PlayerSensors _sensors;
    #endregion

	#region ACTIONS
	private RunAction _runAction;
	private JumpAction _jumpAction;
	private WallJumpAction _wallJumpAction;
	private WallSlideAction _wallSlideAction;
	private DashAction _dashAction;
	#endregion

	private IPhysicsBody2D _body;
    private IGravityService _gravityService;

    private void Awake()
	{
		RB = GetComponent<Rigidbody2D>();
		_body = new Rigidbody2DAdapter(RB);
		if (_sensors == null) _sensors = GetComponent<PlayerSensors>();
	}

	void Start()
	{
		IsFacingRight = true;
		_runAction = new RunAction(this, Data, _body);
		_jumpAction = new JumpAction(this, Data, _body);
		_wallJumpAction = new WallJumpAction(this, Data, _body);
		_wallSlideAction = new WallSlideAction(this, Data, _body);
		_dashAction = new DashAction(this, Data, _body);
        if (_gravityService == null)
        {
            _gravityService = new DefaultGravityService();
        }
        _dashesLeft = Data != null ? Data.dashAmount : 0;
	}

	public void SetData(PlayerData data)
	{
		Data = data;
	}

    public void SetGravityService(IGravityService gravityService)
    {
        _gravityService = gravityService;
    }

    public void SetPhysicsBody(IPhysicsBody2D physicsBody)
    {
        _body = physicsBody;
    }

	public void Move(Vector2 input)
	{
		_moveInput = input;
		if (_moveInput.x > 0 || _moveInput.x < 0)
			TurnCheck();
		// Set anim state
		if (LastOnGroundTime > 0)
		{
			if (Mathf.Abs(_moveInput.x) > 0.01f)
				CurrentAnimState = PlayerAnimState.Run;
			else{
				CurrentAnimState = PlayerAnimState.Idle;
			}
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
		// Timers (use fixed delta for physics loop)
		float dt = Time.fixedDeltaTime;
		LastOnGroundTime -= dt;
		LastOnWallTime -= dt;
		LastOnWallRightTime -= dt;
		LastOnWallLeftTime -= dt;
		LastPressedJumpTime -= dt;
		LastPressedDashTime -= dt;

		// Sensors refresh
		if (_sensors != null)
			_sensors.Refresh();

		// Collision state from sensors
		if (!IsDashing && !IsJumping)
		{
			if (_sensors != null && _sensors.IsGrounded)
				LastOnGroundTime = Data.coyoteTime;
			bool frontWall = _sensors != null && _sensors.IsFrontWall;
			bool backWall = _sensors != null && _sensors.IsBackWall;
			if (((frontWall && IsFacingRight) || (backWall && !IsFacingRight)) && !IsWallJumping)
				LastOnWallRightTime = Data.coyoteTime;
			if (((frontWall && !IsFacingRight) || (backWall && IsFacingRight)) && !IsWallJumping)
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
		}
		if (!IsDashing)
		{
			if (CanJump() && LastPressedJumpTime > 0)
			{
				IsJumping = true;
				IsWallJumping = false;
				_isJumpCut = false;
				_isJumpFalling = false;
				_jumpAction.UpdateJumpAction();
				OnJumped?.Invoke();
			}
			else if (CanWallJump() && LastPressedJumpTime > 0)
			{
				IsWallJumping = true;
				IsJumping = false;
				_isJumpCut = false;
				_isJumpFalling = false;
				_wallJumpStartTime = Time.time;
				_lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;
				_wallJumpAction.UpdateWallJumpAction(_lastWallJumpDir);
				OnJumped?.Invoke();
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
			OnDashedStart?.Invoke();
			_dashAction.UpdateDashAction();
		}

		// Slide
		if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
			IsSliding = true;
		else
			IsSliding = false;

		// Gravity
		_gravityService.Apply(Data, _body, _moveInput, IsSliding, _isDashAttacking, _isJumpCut, IsJumping, IsWallJumping, _isJumpFalling);

		// Movement
		if (!IsDashing)
		{
			if (IsWallJumping)
				_runAction.UpdateRunAction(Data.wallJumpRunLerp);
			else
				_runAction.UpdateRunAction(1);
		}
		else if (_isDashAttacking)
		{
			_runAction.UpdateRunAction(Data.dashEndRunLerp);
		}
		if (IsSliding)
			_wallSlideAction.UpdateWallSlideAction();

		// Set trạng thái Fall nếu đang rơi, không phải WallSlide, không phải Jump
		if (LastOnGroundTime <= 0 && RB.velocity.y < -0.01f && !IsSliding && !IsJumping && !IsWallJumping)
		{
			CurrentAnimState = PlayerAnimState.Fall;
		}

		bool isGroundedNow = LastOnGroundTime > 0 && !IsJumping && !IsWallJumping;
		if (isGroundedNow && !_wasGrounded)
		{
			OnLanded?.Invoke();
			OnDashedEnd?.Invoke(); // kết thúc các effect dash còn tồn tại nếu có
		}
		_wasGrounded = isGroundedNow;
	}

	public void SetGravityScale(float scale)
	{
		RB.gravityScale = scale;
	}

	private void Sleep(float duration)
	{
		StartCoroutine(nameof(PerformSleep), duration);
	}

	private IEnumerator PerformSleep(float duration)
	{
		Time.timeScale = 0;
		yield return new WaitForSecondsRealtime(duration);
		Time.timeScale = 1;
	}

	private void Turn()
	{
		if(IsFacingRight)
		{
			Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
			transform.rotation = Quaternion.Euler(rotator);
			IsFacingRight = !IsFacingRight;
			OnFacingChanged?.Invoke(IsFacingRight);
		}
		else
		{
			Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
			transform.rotation = Quaternion.Euler(rotator);
			IsFacingRight = !IsFacingRight;
			OnFacingChanged?.Invoke(IsFacingRight);
		}
	}

	public void TurnCheck()
	{
		if(_moveInput.x > 0 && !IsFacingRight)
		{
			Turn();
		}
		else if(_moveInput.x < 0 && IsFacingRight)
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
			StartCoroutine(_dashAction.RefillDash(1));
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
}