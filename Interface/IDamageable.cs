using UnityEngine;
public interface IDamageable
{
    void TakeDamage(float damage);
    void TakeDamage(float damage, Vector2 knockbackDirection, Vector2 knockbackForce, float knockbackDuration);
}