using UnityEngine;

public class WallSlideAction
{
    private PlayerMovement _player;
    private PlayerData _playerData;
    private IPhysicsBody2D _body;

    public WallSlideAction(PlayerMovement player, PlayerData playerData, IPhysicsBody2D body)
    {
        _player = player;
        _playerData = playerData;
        _body = body;
    }

    public void UpdateWallSlideAction()
    {
        if(_body.Velocity.y > 0)
        {
            _body.AddForce(-_body.Velocity.y * Vector2.up, ForceMode2D.Impulse);
        }
        float speedDif = _playerData.slideSpeed - _body.Velocity.y;
        float movement = speedDif * _playerData.slideAccel;
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif)  * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));
        _body.AddForce(movement * Vector2.up);
        _player.CurrentAnimState = PlayerAnimState.WallSlide;
    }
}