using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	private PlayerData Data;

	#region COMPONENTS
    public Rigidbody2D RB { get; private set; }
	public event System.Action<PlayerAnimState> OnAnimStateChanged;
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
	#endregion

	#region INPUT PARAMETERS
	public Vector2 _moveInput { get; private set; }
	public float LastPressedJumpTime { get; set; }
	public float LastPressedDashTime { get; set; }
	#endregion

	#region CHECK PARAMETERS
	[Header("Checks")]
	[SerializeField] private Transform _groundCheckPoint;
	[SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
	[Space(5)]
	[SerializeField] private Transform _frontWallCheckPoint;
	[SerializeField] private Transform _backWallCheckPoint;
	[SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    #region LAYERS & TAGS
    [Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;
	#endregion

	[Header("Camera")]
	[SerializeField] private GameObject _cameraFollowGO;
	private CameraFollowObject _cameraFollowObject;

	#region ACTIONS
	private RunAction _runAction;
	private JumpAction _jumpAction;
	private WallJumpAction _wallJumpAction;
	private WallSlideAction _wallSlideAction;
	private DashAction _dashAction;
	#endregion

    private void Awake()
	{
		RB = GetComponent<Rigidbody2D>();
	}

	void Start()
	{
		IsFacingRight = true;
		_cameraFollowObject = _cameraFollowGO.GetComponent<CameraFollowObject>();
		_runAction = new RunAction(this, Data, RB);
		_jumpAction = new JumpAction(this, Data, RB);
		_wallJumpAction = new WallJumpAction(this, Data, RB);
		_wallSlideAction = new WallSlideAction(this, Data, RB);
		_dashAction = new DashAction(this, Data, RB);
	}

	public void SetData(PlayerData data)
	{
		Data = data;
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
			if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
				LastOnGroundTime = Data.coyoteTime;
			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
					|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
				LastOnWallRightTime = Data.coyoteTime;
			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
				|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
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
			CameraManager.instance.ResetYDamping(); // Đảm bảo Damping luôn trả về mặc định khi chạm đất
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
			_dashAction.UpdateDashAction();
		}

		// Slide
		if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
			IsSliding = true;
		else
			IsSliding = false;

		// Gravity
		if (!_isDashAttacking)
		{
			if (IsSliding)
			{
				SetGravityScale(0);
			}
			else if (RB.velocity.y < 0 && _moveInput.y < 0)
			{
				SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
			}
			else if (_isJumpCut)
			{
				SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
			}
			else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
			{
				SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
			}
			else if (RB.velocity.y < 0)
			{
				SetGravityScale(Data.gravityScale * Data.fallGravityMult);
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
			}
			else
			{
				SetGravityScale(Data.gravityScale);
			}
		}
		else
		{
			SetGravityScale(0);
		}

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

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
		Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
	}
}