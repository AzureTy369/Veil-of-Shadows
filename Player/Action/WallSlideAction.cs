using UnityEngine;

public class WallSlideAction
{
    private PlayerMovement _player;
    private PlayerData _playerData;
    private Rigidbody2D _rb;

    public WallSlideAction(PlayerMovement player, PlayerData playerData, Rigidbody2D rb)
    {
        _player = player;
        _playerData = playerData;
		_rb = rb;
    }

    public void UpdateWallSlideAction()
    {
        if(_rb.velocity.y > 0)
		{
		    _rb.AddForce(-_rb.velocity.y * Vector2.up, ForceMode2D.Impulse);
		}
		float speedDif = _playerData.slideSpeed - _rb.velocity.y;
		float movement = speedDif * _playerData.slideAccel;
		movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif)  * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));
		_rb.AddForce(movement * Vector2.up);
		_player.CurrentAnimState = PlayerAnimState.WallSlide;
    }
}