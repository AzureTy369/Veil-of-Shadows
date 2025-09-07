// Cập nhật EnemyIdleState.cs (để hỗ trợ flying)
using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private float idleTimer;
    private float idleDuration;

    public override void OnEnter(EnemyBase controller)
    {
        idleTimer = 0f;
        idleDuration = Random.Range(1f, 3f); // Random idle time

        controller.StopMovement();
        controller.animator.Play("Idle");
    }

    public override void OnUpdate(EnemyBase controller)
    {
        idleTimer += Time.deltaTime;

        // Check for player
        if (controller.CanSeePlayer())
        {
            controller.stateMachine.ChangeState(EnemyStateType.Chase);
            return;
        }

        if (idleTimer >= idleDuration)
        {
            bool hasPatrol = false;
            if (controller is GroundEnemy groundEnemy && groundEnemy.patrolPoints != null && groundEnemy.patrolPoints.Length > 0)
            {
                hasPatrol = true;
            }
            else if (controller is FlyingEnemy flyEnemy && flyEnemy.flyPoints != null && flyEnemy.flyPoints.Length > 0)
            {
                hasPatrol = true;
            }

            if (hasPatrol)
            {
                controller.stateMachine.ChangeState(EnemyStateType.Patrol);
            }
        }
    }

    public override void OnExit(EnemyBase controller)
    {
        // Nothing needed
    }
}