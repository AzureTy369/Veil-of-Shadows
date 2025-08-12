using UnityEngine;

public class JumpAction
{
    private PlayerMovement _player;
    private PlayerData _playerData;
    private IPhysicsBody2D _body;

    public JumpAction(PlayerMovement player, PlayerData playerData, IPhysicsBody2D body)
    {
        _player = player;
        _playerData = playerData;
        _body = body;
    }

    public void UpdateJumpAction()
    {
        _player.LastPressedJumpTime = 0;
        _player.LastOnGroundTime = 0;
        float force = _playerData.jumpForce;
        if (_body.Velocity.y < 0)
            force -= _body.Velocity.y;
        _body.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        _player.CurrentAnimState = PlayerAnimState.Jump;
    }
}
