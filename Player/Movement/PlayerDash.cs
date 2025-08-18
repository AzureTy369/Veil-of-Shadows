using UnityEngine;
using System.Collections;

public class PlayerDash
{
    private PlayerMovement movement;
    private Rigidbody2D RB;
    private PlayerData Data;
    public PlayerDash(PlayerMovement movement, Rigidbody2D rb, PlayerData data)
    {
        this.movement = movement;
        this.RB = rb;
        this.Data = data;
    }
    public IEnumerator StartDash(Vector2 dir)
    {
        movement.LastOnGroundTime = 0;
        movement.LastPressedDashTime = 0;
        float startTime = Time.time;
        movement._dashesLeft--;
        movement._isDashAttacking = true;
        movement.Gravity.SetGravityScale(0);
        while (Time.time - startTime <= Data.dashAttackTime)
        {
            RB.velocity = dir.normalized * Data.dashSpeed;
            yield return null;
        }
        startTime = Time.time;
        movement._isDashAttacking = false;
        movement.Gravity.SetGravityScale(Data.gravityScale);
        RB.velocity = Data.dashEndSpeed * dir.normalized;
        while (Time.time - startTime <= Data.dashEndTime)
        {
            yield return null;
        }
        movement.IsDashing = false;
    }
    public IEnumerator RefillDash(int amount)
    {
        movement._dashRefilling = true;
        yield return new WaitForSeconds(Data.dashRefillTime);
        movement._dashRefilling = false;
        movement._dashesLeft = Mathf.Min(Data.dashAmount, movement._dashesLeft + 1);
    }
} 