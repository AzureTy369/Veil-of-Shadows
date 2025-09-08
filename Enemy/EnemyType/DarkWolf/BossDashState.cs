// BossDashState.cs
using UnityEngine;
using System.Collections;

public class BossDashState : EnemyState
{
    public override void OnEnter(EnemyBase controller)
    {
        if (!(controller is DarkWolf darkWolf))
        {
            // If not DarkWolf, fallback to normal attack or idle
            controller.stateMachine.ChangeState(EnemyStateType.Idle);
            return;
        }

        controller.animator.Play("Dash"); // Assume "Dash" animation exists in animator
        controller.StopMovement();
        controller.IsAttacking = true; // Prevent state changes during dash

        // Start the dash sequence
        controller.StartCoroutine(DashSequence(darkWolf));
    }

    private IEnumerator DashSequence(DarkWolf darkWolf)
    {
        // Chờ hiệu ứng trắng nếu có
        yield return darkWolf.StartCoroutine(darkWolf.WaitForFlashWhite());
        var data = darkWolf.Data as DarkWolfData_SO;
        if (data == null)
        {
            darkWolf.stateMachine.ChangeState(EnemyStateType.Idle);
            yield break;
        }
        int numDashes = data.numDashes;
        for (int i = 0; i < numDashes; i++)
        {
            darkWolf.FindPlayer();
            if (!darkWolf.Player)
            {
                darkWolf.stateMachine.ChangeState(EnemyStateType.Idle);
                yield break;
            }
            Vector2 directionToPlayer = (darkWolf.Player.position - darkWolf.transform.position).normalized;
            darkWolf.FaceDirection(directionToPlayer.x > 0);
            float dashSpeed = data.dashSpeed;
            darkWolf.rb.velocity = directionToPlayer * dashSpeed;
            yield return new WaitForSeconds(data.dashDuration);
            darkWolf.StopMovement();
            if (i < numDashes - 1)
            {
                yield return new WaitForSeconds(data.dashCooldown);
            }
        }
        darkWolf.ResetNormalAttackCount();
        darkWolf.IsAttacking = false;
        darkWolf.stateMachine.ChangeState(EnemyStateType.Idle);
    }

    public override void OnUpdate(EnemyBase controller)
    {
        // No update needed - coroutine handles the sequence
        // Optionally, check if player is still in sight, but for simplicity, let sequence complete
    }

    public override void OnExit(EnemyBase controller)
    {
        controller.StopMovement();
        controller.IsAttacking = false;
        controller.StopAllCoroutines(); // Stop any ongoing dash if state changes unexpectedly
    }
}