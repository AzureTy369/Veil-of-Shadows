using UnityEngine;
using System.Collections;

public class EnemyDieState : EnemyState
{
    public override void OnEnter(EnemyController controller)
    {
        controller.animator.Play("Die");
        controller.StopMovement();
        
        // // Disable collider
        // if (controller.GetComponent<Collider2D>())
        // {
        //     controller.GetComponent<Collider2D>().enabled = false;
        // }

        // Start death sequence
        controller.StartCoroutine(DeathSequence(controller));
    }

    public override void OnUpdate(EnemyController controller)
    {
        // Do nothing - death sequence handles everything
    }

    public override void OnExit(EnemyController controller)
    {
        // This state shouldn't be exited
    }

    private System.Collections.IEnumerator DeathSequence(EnemyController controller)
    {
        // Wait for death animation
        yield return new WaitForSeconds(2f);
        
        // Optional: Drop items, play effects, etc.
        
        // Destroy the enemy
        Object.Destroy(controller.gameObject);
    }
}