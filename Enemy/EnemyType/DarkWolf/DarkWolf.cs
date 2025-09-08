// DarkWolf.cs
using UnityEngine;

public class DarkWolf : GroundEnemy
{
    [Header("Boss Cycle")]
    [HideInInspector] public int normalAttackCount = 0;

    protected override void ValidateSetup()
    {
        base.ValidateSetup();
        // Disable patrol for boss - no patrol points needed
        patrolPoints = new Transform[0];
        patrolIndex = 0;
        patrolWaitTimer = 0f;
        isWaitingAtPoint = false;
    }

    // Method to increment attack count (called from AttackState or Animation Event)
    public void IncrementNormalAttackCount()
    {
        normalAttackCount++;
        Debug.Log($"[DarkWolf] Normal attack count: {normalAttackCount}");
    }

    // Method to reset attack count after dash cycle
    public void ResetNormalAttackCount()
    {
        normalAttackCount = 0;
        Debug.Log("[DarkWolf] Reset normal attack count after dash cycle");
    }

    // Override to ensure no patrol behavior
    public override void MoveTowards(Vector2 target, float speedMultiplier = 1f)
    {
        base.MoveTowards(target, speedMultiplier);
    }
}