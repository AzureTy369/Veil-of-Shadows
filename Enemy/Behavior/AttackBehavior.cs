// AttackBehavior.cs
using UnityEngine;

public class AttackBehavior : MonoBehaviour
{
    private EnemyBase controller;

    private void Awake()
    {
        controller = GetComponent<EnemyBase>();
    }

    public void UpdateAttack()
    {
        if (controller.IsAttacking) // Nếu đang tấn công thì không chuyển state
        {
            return;
        }

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
}