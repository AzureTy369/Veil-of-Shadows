// Cập nhật EnemyPatrolState.cs (để hỗ trợ cả ground và flying)
using UnityEngine;

public class EnemyPatrolState : EnemyState
{
    private PatrolBehavior patrolBehavior;
    private FlyingBehavior flyingBehavior;

    public override void OnEnter(EnemyBase controller)
    {
        patrolBehavior = controller.GetComponent<PatrolBehavior>();
        flyingBehavior = controller.GetComponent<FlyingBehavior>();

        if (patrolBehavior == null && flyingBehavior == null)
        {
            Debug.LogError($"[EnemyPatrolState] Missing Patrol or Flying Behavior on {controller.name}");
            return;
        }

        controller.animator.Play(controller is FlyingEnemy ? "Fly" : "Walk");
    }

    public override void OnUpdate(EnemyBase controller)
    {
        if (patrolBehavior != null)
        {
            patrolBehavior.UpdatePatrol();
        }
        else if (flyingBehavior != null)
        {
            flyingBehavior.UpdateFly();
        }
    }

    public override void OnExit(EnemyBase controller)
    {
        controller.StopMovement();
    }
}