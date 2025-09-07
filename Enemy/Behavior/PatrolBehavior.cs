// PatrolBehavior.cs
using UnityEngine;

public class PatrolBehavior : MonoBehaviour
{
    private GroundEnemy controller; // Chỉ dùng cho GroundEnemy hiện tại

    private void Awake()
    {
        controller = GetComponent<GroundEnemy>();
    }

    public void UpdatePatrol()
    {
        // Check for player first
        if (controller.CanSeePlayer())
        {
            controller.stateMachine.ChangeState(EnemyStateType.Chase);
            return;
        }

        // No patrol points - go idle
        if (controller.patrolPoints == null || controller.patrolPoints.Length == 0)
        {
            controller.stateMachine.ChangeState(EnemyStateType.Idle);
            return;
        }

        // At patrol point
        if (controller.IsAtPatrolPoint())
        {
            if (!controller.isWaitingAtPoint)
            {
                controller.isWaitingAtPoint = true;
                controller.StopMovement();
                controller.animator.Play("Idle");
            }

            controller.patrolWaitTimer += Time.deltaTime;

            if (controller.patrolWaitTimer >= controller.data.patrolWaitTime)
            {
                controller.animator.Play("Walk");
                controller.NextPatrolPoint();
                controller.patrolWaitTimer = 0f;
                controller.isWaitingAtPoint = false;
            }
        }
        else
        {
            // Luôn set isWaitingAtPoint = false khi chưa tới điểm
            controller.isWaitingAtPoint = false;
            // Move toward patrol point
            Vector2 target = controller.GetPatrolTarget();
            controller.MoveTowards(target);
        }
    }
}