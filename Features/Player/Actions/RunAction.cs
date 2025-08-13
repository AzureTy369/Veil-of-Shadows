using UnityEngine;

public class RunAction
{
    private PlayerMovement _player;
    private PlayerData _playerData;
    private IPhysicsBody2D _body;
    
    public RunAction(PlayerMovement player, PlayerData playerData, IPhysicsBody2D body)
    {
        _player = player;
        _playerData = playerData;
        _body = body;
    }

    public void UpdateRunAction(float lerpAmount)
    {
        float targetSpeed = _player._moveInput.x * _playerData.runMaxSpeed;
        targetSpeed = Mathf.Lerp(_body.Velocity.x, targetSpeed, lerpAmount);
        float accelRate;
        if (_player.LastOnGroundTime > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? 
            _playerData.runAccelAmount : _playerData.runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? 
            _playerData.runAccelAmount * _playerData.accelInAir : _playerData.runDeccelAmount * _playerData.deccelInAir;
        if ((_player.IsJumping || _player.IsWallJumping || _player._isJumpFalling) 
        && Mathf.Abs(_body.Velocity.y) < _playerData.jumpHangTimeThreshold)
        {
            accelRate *= _playerData.jumpHangAccelerationMult;
            targetSpeed *= _playerData.jumpHangMaxSpeedMult;
        }
        if(_playerData.doConserveMomentum && Mathf.Abs(_body.Velocity.x) > Mathf.Abs(targetSpeed) 
        && Mathf.Sign(_body.Velocity.x) == Mathf.Sign(targetSpeed) 
        && Mathf.Abs(targetSpeed) > 0.01f && _player.LastOnGroundTime < 0)
        {
            accelRate = 0; 
        }
        float speedDif = targetSpeed - _body.Velocity.x;
        float movement = speedDif * accelRate;
        _body.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }
}
