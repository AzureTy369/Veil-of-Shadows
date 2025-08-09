using UnityEngine;

public class WallJumpAction
{
    private PlayerMovement _player;
    private PlayerData _playerData;
    private Rigidbody2D _rb;

    public WallJumpAction(PlayerMovement player, PlayerData playerData, Rigidbody2D rb)
    {
        _player = player;
        _playerData = playerData;
		_rb = rb;
    }

    public void UpdateWallJumpAction(int dir)
    {
        _player.LastPressedJumpTime = 0;
		_player.LastOnGroundTime = 0;
		_player.LastOnWallRightTime = 0;
		_player.LastOnWallLeftTime = 0;
		Vector2 force = new Vector2(_playerData.wallJumpForce.x, _playerData.wallJumpForce.y);
		force.x *= dir;
		if (Mathf.Sign(_rb.velocity.x) != Mathf.Sign(force.x))
			force.x -= _rb.velocity.x;
		if (_rb.velocity.y < 0)
			force.y -= _rb.velocity.y;
		_rb.AddForce(force, ForceMode2D.Impulse);
		_player.CurrentAnimState = PlayerAnimState.Jump;
    }
}