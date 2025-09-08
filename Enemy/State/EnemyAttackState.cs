// EnemyAttackState.cs
using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private AttackBehavior attackBehavior;

    public override void OnEnter(EnemyBase controller)
    {
        attackBehavior = controller.GetComponent<AttackBehavior>();
        if (attackBehavior == null)
        {
            Debug.LogError($"[EnemyAttackState] Missing AttackBehavior on {controller.name}");
            return;
        }

        controller.StopMovement();
        
        if (controller.CanAttack())
        {
            controller.PerformAttack();
            controller.IsAttacking = true; // Đặt trạng thái tấn công
        }
    }

    public override void OnUpdate(EnemyBase controller)
    {
        if (attackBehavior != null)
        {
            attackBehavior.UpdateAttack();
        }
    }

    public override void OnExit(EnemyBase controller)
    {
        controller.IsAttacking = false; // Reset trạng thái tấn công khi rời khỏi state Attack
    }
}