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
	private float _fallSpeedYDampingChangeThreshold;

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
		_fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;
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

	// private void Run(float lerpAmount)
	// {
	// 	float targetSpeed = _moveInput.x * Data.runMaxSpeed;
	// 	targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);
	// 	float accelRate;
	// 	if (LastOnGroundTime > 0)
	// 		accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
	// 	else
	// 		accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
	// 	if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
	// 	{
	// 		accelRate *= Data.jumpHangAccelerationMult;
	// 		targetSpeed *= Data.jumpHangMaxSpeedMult;
	// 	}
	// 	if(Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
	// 	{
	// 		accelRate = 0; 
	// 	}
	// 	float speedDif = targetSpeed - RB.velocity.x;
	// 	float movement = speedDif * accelRate;
	// 	RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
	// }

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

	// private void Jump()
	// {
	// 	LastPressedJumpTime = 0;
	// 	LastOnGroundTime = 0;
	// 	float force = Data.jumpForce;
	// 	if (RB.velocity.y < 0)
	// 		force -= RB.velocity.y;
	// 	RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
	// 	CurrentAnimState = PlayerAnimState.Jump;
	// }

	// private void WallJump(int dir)
	// {
	// 	LastPressedJumpTime = 0;
	// 	LastOnGroundTime = 0;
	// 	LastOnWallRightTime = 0;
	// 	LastOnWallLeftTime = 0;
	// 	Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
	// 	force.x *= dir;
	// 	if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
	// 		force.x -= RB.velocity.x;
	// 	if (RB.velocity.y < 0)
	// 		force.y -= RB.velocity.y;
	// 	RB.AddForce(force, ForceMode2D.Impulse);
	// 	CurrentAnimState = PlayerAnimState.Jump;
	// }

	// private IEnumerator StartDash(Vector2 dir)
	// {
	// 	LastOnGroundTime = 0;
	// 	LastPressedDashTime = 0;
	// 	float startTime = Time.time;
	// 	_dashesLeft--;
	// 	_isDashAttacking = true;
	// 	SetGravityScale(0);
	// 	while (Time.time - startTime <= Data.dashAttackTime)
	// 	{
	// 		RB.velocity = dir.normalized * Data.dashSpeed;
	// 		yield return null;
	// 	}
	// 	startTime = Time.time;
	// 	_isDashAttacking = false;
	// 	SetGravityScale(Data.gravityScale);
	// 	RB.velocity = Data.dashEndSpeed * dir.normalized;
	// 	while (Time.time - startTime <= Data.dashEndTime)
	// 	{
	// 		yield return null;
	// 	}
	// 	IsDashing = false;
	// }

	// private IEnumerator RefillDash(int amount)
	// {
	// 	_dashRefilling = true;
	// 	yield return new WaitForSeconds(Data.dashRefillTime);
	// 	_dashRefilling = false;
	// 	_dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 1);
	// }

	// private void Slide()
	// {
	// 	if(RB.velocity.y > 0)
	// 	{
	// 	    RB.AddForce(-RB.velocity.y * Vector2.up, ForceMode2D.Impulse);
	// 	}
	// 	float speedDif = Data.slideSpeed - RB.velocity.y;
	// 	float movement = speedDif * Data.slideAccel;
	// 	movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif)  * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));
	// 	RB.AddForce(movement * Vector2.up);
	// 	CurrentAnimState = PlayerAnimState.WallSlide;
	// }

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

	public void InterpolationCamera()
	{
		if(RB.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping
		&& !CameraManager.instance.LerpedFromPlayerFalling)
		{
			CameraManager.instance.LerpYDamping(true);
		}

		if(RB.velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping
		&& CameraManager.instance.LerpedFromPlayerFalling)
		{
			CameraManager.instance.LerpedFromPlayerFalling = false;

			CameraManager.instance.LerpYDamping(false);
		}
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