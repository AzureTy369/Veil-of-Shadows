using UnityEngine;

public interface IGravityService
{
    void Apply(
        PlayerData data,
        IPhysicsBody2D body,
        Vector2 input,
        bool isSliding,
        bool isDashAttacking,
        bool isJumpCut,
        bool isJumping,
        bool isWallJumping,
        bool isJumpFalling
    );
} 