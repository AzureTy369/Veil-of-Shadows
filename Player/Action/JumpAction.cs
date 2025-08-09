using UnityEngine;

public class JumpAction
{
    private PlayerMovement _player;
    private PlayerData _playerData;
    private Rigidbody2D _rb;

    public JumpAction(PlayerMovement player, PlayerData playerData, Rigidbody2D rb)
    {
        _player = player;
        _playerData = playerData;
        _rb = rb;
    }

    public void UpdateJumpAction()
    {
        _player.LastPressedJumpTime = 0;
        _player.LastOnGroundTime = 0;
        float force = _playerData.jumpForce;
        if (_rb.velocity.y < 0)
            force -= _rb.velocity.y;
        _rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        _player.CurrentAnimState = PlayerAnimState.Jump;
    }
}
