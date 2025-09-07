// GroundEnemy.cs
using UnityEngine;

public class GroundEnemy : EnemyBase
{
    [Header("Patrol")]
    public Transform[] patrolPoints;
    [HideInInspector] public int patrolIndex = 0;
    [HideInInspector] public float patrolWaitTimer = 0f;
    [HideInInspector] public bool isWaitingAtPoint = false;

    protected override void ValidateSetup()
    {
        base.ValidateSetup();
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning($"Enemy {name} has no patrol points!");
        }
    }

    #region Patrol System

    public Vector2 GetPatrolTarget()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return transform.position;

        return patrolPoints[patrolIndex].position;
    }

    public void NextPatrolPoint()
    {
        if (patrolPoints.Length > 0)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }
    }

    public bool IsAtPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return true;

        Vector2 target = patrolPoints[patrolIndex].position;
        return Vector2.Distance(transform.position, target) < 0.2f;
    }

    #endregion

    #region Private Methods

    private bool ShouldAvoidEdge(Vector2 direction)
    {
        if (!IsGrounded()) return false;
        Vector2 edgeCheckPos = (Vector2)transform.position + direction * 2.5f + Vector2.down * 1.5f;
        // Sử dụng OverlapCircle thay cho CheckCircle
        return !Physics2D.OverlapCircle(edgeCheckPos, 0.3f, groundMask);
    }

    #endregion

    #region Gizmos Additions
    private void OnDrawGizmosSelectedGround()
    {
        // Patrol Points
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);
                    // Draw lines between patrol points
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                    else if (i == patrolPoints.Length - 1 && patrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        OnDrawGizmosSelectedGround();
    }
    #endregion
}