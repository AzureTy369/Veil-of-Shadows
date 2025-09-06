using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Health & Defense")]
    public float maxHealth = 100f;
    
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseSpeedMultiplier = 1.5f;
    
    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public float attackRange = 1.5f;
    public float kockbackForceX = 6;
    public float kockbackForceY = 6;
    public float knockbackDuration = 0.25f;
    
    [Header("Detection")]
    public float detectionRange = 5f;
    public float loseTargetTime = 2f;
    
    [Header("Patrol")]
    public float patrolWaitTime = 2f;

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        moveSpeed = Mathf.Max(0.1f, moveSpeed);
        attackDamage = Mathf.Max(0f, attackDamage);
        detectionRange = Mathf.Max(attackRange, detectionRange);
        attackCooldown = Mathf.Max(0.1f, attackCooldown);
    }
}