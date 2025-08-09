using UnityEngine;

public class RunAction
{
    private PlayerMovement _player;
    private PlayerData _playerData;
	private Rigidbody2D _rb;
    
    public RunAction(PlayerMovement player, PlayerData playerData, Rigidbody2D rb)
    {
        _player = player;
        _playerData = playerData;
		_rb = rb;
    }

    public void UpdateRunAction(float lerpAmount)
    {
        float targetSpeed = _player._moveInput.x * _playerData.runMaxSpeed;
		targetSpeed = Mathf.Lerp(_rb.velocity.x, targetSpeed, lerpAmount);
		float accelRate;
		if (_player.LastOnGroundTime > 0)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? 
            _playerData.runAccelAmount : _playerData.runDeccelAmount;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? 
            _playerData.runAccelAmount * _playerData.accelInAir : _playerData.runDeccelAmount * _playerData.deccelInAir;
		if ((_player.IsJumping || _player.IsWallJumping || _player._isJumpFalling) 
        && Mathf.Abs(_rb.velocity.y) < _playerData.jumpHangTimeThreshold)
		{
			accelRate *= _playerData.jumpHangAccelerationMult;
			targetSpeed *= _playerData.jumpHangMaxSpeedMult;
		}
		if(_playerData.doConserveMomentum && Mathf.Abs(_rb.velocity.x) > Mathf.Abs(targetSpeed) 
        && Mathf.Sign(_rb.velocity.x) == Mathf.Sign(targetSpeed) 
        && Mathf.Abs(targetSpeed) > 0.01f && _player.LastOnGroundTime < 0)
		{
			accelRate = 0; 
		}
		float speedDif = targetSpeed - _rb.velocity.x;
		float movement = speedDif * accelRate;
		_rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }
}
