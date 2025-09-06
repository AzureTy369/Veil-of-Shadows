using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private float idleTimer;
    private float idleDuration;

    public override void OnEnter(EnemyController controller)
    {
        Debug.Log($"[EnemyIdleState] OnEnter: {controller.name}");
        idleTimer = 0f;
        idleDuration = Random.Range(1f, 3f); // Random idle time
        
        controller.StopMovement();
        controller.animator.Play("Idle");
        Debug.Log("Idle Start");
    }

    public override void OnUpdate(EnemyController controller)
    {
        Debug.Log($"[EnemyIdleState] OnUpdate: {controller.name} | idleTimer: {idleTimer:F2}/{idleDuration:F2}");
        idleTimer += Time.deltaTime;

        // Check for player
        if (controller.CanSeePlayer())
        {
            controller.stateMachine.ChangeState(EnemyStateType.Chase);
            return;
        }

        // Transition to patrol after idle time
        if (idleTimer >= idleDuration)
        {
            if (controller.patrolPoints != null && controller.patrolPoints.Length > 0)
            {
                controller.stateMachine.ChangeState(EnemyStateType.Patrol);
            }
        }
    }
   
    public override void OnExit(EnemyController controller)
    {
        Debug.Log($"[EnemyIdleState] OnExit: {controller.name}");
        // Nothing needed
    }
}