using UnityEngine;
using System.Collections;

public class DashAction
{
    private PlayerMovement _player;
    private PlayerData _playerData;
    private IPhysicsBody2D _body;

    public DashAction(PlayerMovement player, PlayerData playerData, IPhysicsBody2D body)
    {
        _player = player;
        _playerData = playerData;
        _body = body;
    }

    public void UpdateDashAction()
    {
        _player.StartCoroutine(StartDash(_player._lastDashDir));
    }

    private IEnumerator StartDash(Vector2 dir)
	{
		_player.LastOnGroundTime = 0;
		_player.LastPressedDashTime = 0;
		float startTime = Time.time;
		_player._dashesLeft--;
		_player._isDashAttacking = true;
		_player.SetGravityScale(0);
		while (Time.time - startTime <= _playerData.dashAttackTime)
		{
			_body.Velocity = dir.normalized * _playerData.dashSpeed;
			yield return null;
		}
		startTime = Time.time;
		_player._isDashAttacking = false;
		_player.SetGravityScale(_playerData.gravityScale);
		_body.Velocity = _playerData.dashEndSpeed * dir.normalized;
		while (Time.time - startTime <= _playerData.dashEndTime)
		{
			yield return null;
		}
		_player.IsDashing = false;
	}

    public IEnumerator RefillDash(int amount)
	{
		_player._dashRefilling = true;
		yield return new WaitForSeconds(_playerData.dashRefillTime);
		_player._dashRefilling = false;
		_player._dashesLeft = Mathf.Min(_playerData.dashAmount, _player._dashesLeft + amount);
	}
}
