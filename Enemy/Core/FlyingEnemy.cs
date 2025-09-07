// FlyingEnemy.cs
using UnityEngine;

public class FlyingEnemy : EnemyBase
{
    [Header("Flying Specific")]
    public Transform[] flyPoints; // Điểm bay tuần tra (tương tự patrol points nhưng 3D hơn nếu cần, nhưng 2D thì Vector2)
    [HideInInspector] public int flyIndex = 0;
    [HideInInspector] public float flyWaitTimer = 0f;
    [HideInInspector] public bool isWaitingAtFlyPoint = false;

    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        if (rb)
        {
            rb.gravityScale = 0f; // Không rơi, bay tự do
        }
    }

    protected override void ValidateSetup()
    {
        base.ValidateSetup();
        if (flyPoints.Length == 0)
        {
            Debug.LogWarning($"Flying Enemy {name} has no fly points!");
        }
        // Không cần groundCheck cho flying
        if (groundCheck) Destroy(groundCheck.gameObject);
    }

    public override bool IsGrounded()
    {
        return false; // Flying không cần ground
    }

    public override void MoveTowards(Vector2 target, float speedMultiplier = 1f)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        float moveSpeed = data.moveSpeed * speedMultiplier;
        rb.velocity = direction * moveSpeed; // Bay full 2D, không giữ y velocity như ground
        if (direction.magnitude > 0.1f)
        {
            FaceDirection(direction.x > 0);
        }
    }

    #region Fly System (Tương tự Patrol nhưng cho bay)
    public Vector2 GetFlyTarget()
    {
        if (flyPoints == null || flyPoints.Length == 0)
            return transform.position;

        return flyPoints[flyIndex].position;
    }

    public void NextFlyPoint()
    {
        if (flyPoints.Length > 0)
        {
            flyIndex = (flyIndex + 1) % flyPoints.Length;
        }
    }

    public bool IsAtFlyPoint()
    {
        if (flyPoints == null || flyPoints.Length == 0) return true;

        Vector2 target = flyPoints[flyIndex].position;
        return Vector2.Distance(transform.position, target) < 0.2f;
    }
    #endregion

    #region Gizmos Additions
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        // Fly Points
        if (flyPoints != null && flyPoints.Length > 0)
        {
            Gizmos.color = Color.magenta; // Màu khác để phân biệt
            for (int i = 0; i < flyPoints.Length; i++)
            {
                if (flyPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(flyPoints[i].position, 0.3f);
                    if (i < flyPoints.Length - 1 && flyPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(flyPoints[i].position, flyPoints[i + 1].position);
                    }
                    else if (i == flyPoints.Length - 1 && flyPoints[0] != null)
                    {
                        Gizmos.DrawLine(flyPoints[i].position, flyPoints[0].position);
                    }
                }
            }
        }
    }
    #endregion
}