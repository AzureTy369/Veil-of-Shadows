using UnityEngine;

public class PlayerJump
{
    private PlayerMovement movement;
    private Rigidbody2D RB;
    private PlayerData Data;
    public PlayerJump(PlayerMovement movement, Rigidbody2D rb, PlayerData data)
    {
        this.movement = movement;
        this.RB = rb;
        this.Data = data;
    }
    public void Jump()
    {
        movement.LastPressedJumpTime = 0;
        movement.LastOnGroundTime = 0;
        float force = Data.jumpForce;
        if (RB.velocity.y < 0)
            force -= RB.velocity.y;
        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        movement.SetAnimState(PlayerAnimState.Jump);
    }
    public void WallJump(int dir)
    {
        movement.LastPressedJumpTime = 0;
        movement.LastOnGroundTime = 0;
        movement.LastOnWallRightTime = 0;
        movement.LastOnWallLeftTime = 0;
        Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
        force.x *= dir;
        if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
            force.x -= RB.velocity.x;
        if (RB.velocity.y < 0)
            force.y -= RB.velocity.y;
        RB.AddForce(force, ForceMode2D.Impulse);
        movement.SetAnimState(PlayerAnimState.Jump);
    }
} 