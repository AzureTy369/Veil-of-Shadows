using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private bool hasAttacked;

    public override void OnEnter(EnemyController controller)
    {
        hasAttacked = false;
        controller.StopMovement();
        if (controller.CanAttack())
        {
            controller.PerformAttack();
            hasAttacked = true;
        }
    }

    public override void OnUpdate(EnemyController controller)
    {
        if (!controller.Player)
        {
            controller.stateMachine.ChangeState(EnemyStateType.Patrol);
            return;
        }

        // Face the player
        Vector2 directionToPlayer = controller.Player.position - controller.transform.position;
        controller.FaceDirection(directionToPlayer.x > 0);

        // If player moved out of attack range
        if (!controller.IsPlayerInAttackRange())
        {
            if (controller.CanSeePlayer())
            {
                controller.stateMachine.ChangeState(EnemyStateType.Chase);
            }
            else
            {
                controller.stateMachine.ChangeState(EnemyStateType.Patrol);
            }
            return;
        }

        // Attack again if cooldown is ready
        if (controller.CanAttack())
        {
            controller.PerformAttack();
        }
    }

    public override void OnExit(EnemyController controller)
    {
        // Nothing needed
    }
}