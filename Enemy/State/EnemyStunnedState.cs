using UnityEngine;

public class EnemyStunnedState : EnemyState
{
    private float stunTimer;
    private float stunDuration;

    public override void OnEnter(EnemyController controller)
    {
        stunTimer = 0f;
        stunDuration = 0.3f; // Shorter stun for better game feel
        
        controller.StopMovement();
        
        // Hit animation is already triggered in TakeDamage method
        // No need to trigger it again here
    }

    public override void OnUpdate(EnemyController controller)
    {
        stunTimer += Time.deltaTime;
        
        if (stunTimer >= stunDuration)
        {
            // Return to appropriate state based on player proximity
            if (controller.CanSeePlayer())
            {
                if (controller.IsPlayerInAttackRange())
                {
                    controller.stateMachine.ChangeState(EnemyStateType.Attack);
                }
                else
                {
                    controller.stateMachine.ChangeState(EnemyStateType.Chase);
                }
            }
            else
            {
                controller.stateMachine.ChangeState(EnemyStateType.Patrol);
            }
        }
    }
    public override void OnExit(EnemyController controller)
    {
    }
}