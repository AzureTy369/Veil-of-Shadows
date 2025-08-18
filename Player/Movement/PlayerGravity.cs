using UnityEngine;

public class PlayerGravity
{
    private PlayerMovement movement;
    private Rigidbody2D RB;
    private PlayerData Data;
    public PlayerGravity(PlayerMovement movement, Rigidbody2D rb, PlayerData data)
    {
        this.movement = movement;
        this.RB = rb;
        this.Data = data;
    }
    public void HandleGravity(UnityEngine.Vector2 moveInput)
    {
        if (!movement._isDashAttacking)
        {
            if (movement.IsSliding)
            {
                SetGravityScale(0);
            }
            else if (RB.velocity.y < 0 && moveInput.y < 0)
            {
                SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
                RB.velocity = new UnityEngine.Vector2(RB.velocity.x, UnityEngine.Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
            }
            else if (movement._isJumpCut)
            {
                SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
                RB.velocity = new UnityEngine.Vector2(RB.velocity.x, UnityEngine.Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
            }
            else if ((movement.IsJumping || movement.IsWallJumping || movement._isJumpFalling) && UnityEngine.Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
            {
                SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
            }
            else if (RB.velocity.y < 0)
            {
                SetGravityScale(Data.gravityScale * Data.fallGravityMult);
                RB.velocity = new UnityEngine.Vector2(RB.velocity.x, UnityEngine.Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
            }
            else
            {
                SetGravityScale(Data.gravityScale);
            }
        }
        else
        {
            SetGravityScale(0);
        }
    }
    public void SetGravityScale(float scale)
    {
        RB.gravityScale = scale;
    }
} 