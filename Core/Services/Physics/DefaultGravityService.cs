using UnityEngine;

public class DefaultGravityService : IGravityService
{
    public void Apply(
        PlayerData data,
        IPhysicsBody2D body,
        Vector2 input,
        bool isSliding,
        bool isDashAttacking,
        bool isJumpCut,
        bool isJumping,
        bool isWallJumping,
        bool isJumpFalling)
    {
        if (isDashAttacking)
        {
            body.SetGravityScale(0);
            return;
        }

        if (isSliding)
        {
            body.SetGravityScale(0);
            return;
        }

        Vector2 velocity = body.Velocity;

        if (velocity.y < 0 && input.y < 0)
        {
            body.SetGravityScale(data.gravityScale * data.fastFallGravityMult);
            body.Velocity = new Vector2(velocity.x, Mathf.Max(velocity.y, -data.maxFastFallSpeed));
            return;
        }

        if (isJumpCut)
        {
            body.SetGravityScale(data.gravityScale * data.jumpCutGravityMult);
            body.Velocity = new Vector2(velocity.x, Mathf.Max(velocity.y, -data.maxFallSpeed));
            return;
        }

        if ((isJumping || isWallJumping || isJumpFalling) && Mathf.Abs(velocity.y) < data.jumpHangTimeThreshold)
        {
            body.SetGravityScale(data.gravityScale * data.jumpHangGravityMult);
            return;
        }

        if (velocity.y < 0)
        {
            body.SetGravityScale(data.gravityScale * data.fallGravityMult);
            body.Velocity = new Vector2(velocity.x, Mathf.Max(velocity.y, -data.maxFallSpeed));
            return;
        }

        body.SetGravityScale(data.gravityScale);
    }
} 