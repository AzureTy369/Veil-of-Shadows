// EnemyDieState.cs
using UnityEngine;
using System.Collections;

public class EnemyDieState : EnemyState
{
    public override void OnEnter(EnemyBase controller)
    {
        controller.animator.Play("Death");
        controller.StopMovement();
        controller.IsAttacking = false; 
        
        // Disable hitbox child object when enemy dies
        if (controller.hitboxCollider != null)
        {
            controller.hitboxCollider.gameObject.SetActive(false);
        }
        // Disable BodyDamage object (CollisionDamage) if exists
        var bodyDamage = controller.GetComponentInChildren<CollisionDamage>(true);
        if (bodyDamage != null)
        {
            bodyDamage.gameObject.SetActive(false);
        }
        

        controller.StartCoroutine(DeathSequence(controller));
    }

    public override void OnUpdate(EnemyBase controller)
    {
        // Do nothing - death sequence handles everything
    }

    public override void OnExit(EnemyBase controller)
    {
        // This state shouldn't be exited
    }

    private IEnumerator DeathSequence(EnemyBase controller)
    {
        // Wait for death animation
        yield return new WaitForSeconds(2f);

        // Optional: Drop items, play effects, etc.

        // Destroy the enemy
        Object.Destroy(controller.gameObject);
    }
}