using UnityEngine;

public class CollisionDamage : MonoBehaviour
{
    private EnemyBase enemyBase;
    private bool hasDamageCollision;
    private float damageCollisionTimer = 0f;

    void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (enemyBase != null && !hasDamageCollision && other.collider.CompareTag("Player"))
        {
            hasDamageCollision = true;
            PlayerHealth player = other.collider.GetComponent<PlayerHealth>();
            PlayerMovement playerM = other.collider.GetComponent<PlayerMovement>();
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

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
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
