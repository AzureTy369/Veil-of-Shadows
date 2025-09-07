// FlyingBehavior.cs
using UnityEngine;

public class FlyingBehavior : MonoBehaviour
{
    private FlyingEnemy controller;

    private void Awake()
    {
        controller = GetComponent<FlyingEnemy>();
    }

    public void UpdateFly()
    {
        // Tương tự PatrolBehavior nhưng cho bay
        if (controller.CanSeePlayer())
        {
            controller.stateMachine.ChangeState(EnemyStateType.Chase);
            return;
        }

        if (controller.flyPoints == null || controller.flyPoints.Length == 0)
        {
            controller.stateMachine.ChangeState(EnemyStateType.Idle);
            return;
        }

        if (controller.IsAtFlyPoint())
        {
            if (!controller.isWaitingAtFlyPoint)
            {
                controller.isWaitingAtFlyPoint = true;
                controller.StopMovement();
                controller.animator.Play("Idle");
            }

            controller.flyWaitTimer += Time.deltaTime;

            if (controller.flyWaitTimer >= controller.data.patrolWaitTime) // Reuse patrolWaitTime
            {
                controller.animator.Play("Fly"); 
                controller.NextFlyPoint();
                controller.flyWaitTimer = 0f;
                controller.isWaitingAtFlyPoint = false;
            }
        }
        else
        {
            controller.isWaitingAtFlyPoint = false;
            Vector2 target = controller.GetFlyTarget();
            var flyingData = controller.Data as FlyingEnemyData_SO;
            float speedMultiplier = flyingData != null ? flyingData.flySpeedMultiplier : 1f;
            controller.MoveTowards(target, speedMultiplier); // Sử dụng multiplier từ data
        }
    }
}