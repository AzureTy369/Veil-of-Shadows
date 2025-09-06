using UnityEngine;

public class EnemyChaseState : EnemyState
{
    private float loseTargetTimer;
    private const float LOSE_TARGET_TIME = 1f;

    public override void OnEnter(EnemyController controller)
    {
        controller.animator.Play("Run");
        loseTargetTimer = 0f;
    }

    public override void OnUpdate(EnemyController controller)
    {
        if (!controller.Player)
        {
            controller.stateMachine.ChangeState(EnemyStateType.Patrol);
            return;
        }

        // Check if can still see player
        if (controller.CanSeePlayer())
        {
            controller.animator.Play("Run");
            loseTargetTimer = 0f;
            
            // Check attack range
            if (controller.IsPlayerInAttackRange())
            {
                controller.stateMachine.ChangeState(EnemyStateType.Attack);
                return;
            }

            // Move towards player with increased speed
            controller.MoveTowards(controller.Player.position, 1.5f);
        }
        else
        {
            controller.StopMovement();
            controller.animator.Play("Idle");
            // Lost sight of player
            loseTargetTimer += Time.deltaTime;
            
            
            if (loseTargetTimer >= LOSE_TARGET_TIME)
            {
                controller.stateMachine.ChangeState(EnemyStateType.Patrol);
                return;
            }

        }
    }
    

    public override void OnExit(EnemyController controller)
    {
        controller.StopMovement();
    }
}