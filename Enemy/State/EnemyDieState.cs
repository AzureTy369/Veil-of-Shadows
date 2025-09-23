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

        // Return enemy to pool instead of destroying
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnToPool(controller.gameObject);
        }
        else
        {
            Object.Destroy(controller.gameObject);
        }
    }
}