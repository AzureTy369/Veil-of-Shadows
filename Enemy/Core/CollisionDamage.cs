using UnityEngine;

public class CollisionDamage : MonoBehaviour
{
    private EnemyBase enemyBase;
    private bool hasDamageCollision;
    private float damageCollisionTimer = 0f;

    void Awake()
    {
        enemyBase = GetComponentInParent<EnemyBase>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the other object has a Collider and the Player tag
        if (enemyBase != null && !hasDamageCollision && other != null && other.CompareTag("Player"))
        {
            hasDamageCollision = true;
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            PlayerMovement playerM = other.GetComponent<PlayerMovement>();
            if (player != null && playerM != null)
            {
                DamageCollision(player, playerM.IsFacingRight, playerM.RB);
            }
        }
        if (hasDamageCollision)
        {
            damageCollisionTimer += Time.deltaTime;
            if (damageCollisionTimer >= 2f)
            {
                hasDamageCollision = false;
                damageCollisionTimer = 0f;
            }
        }
        
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other != null && other.CompareTag("Player"))
        {
            hasDamageCollision = false;
        }
    }

    private void DamageCollision(PlayerHealth playerHealth, bool isFacingRight, Rigidbody2D rb)
    {
        Vector2 direction = isFacingRight ? new Vector2(-1f, rb.velocity.y) : new Vector2(1f, rb.velocity.y);
        Vector2 knockbackForce = new Vector2(enemyBase.Data.knockbackForceX, enemyBase.Data.knockbackForceY);
        playerHealth.TakeDamage(enemyBase.Data.attackDamage, direction, knockbackForce, enemyBase.Data.knockbackDuration);
    }
}
