using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Added for List

[RequireComponent(typeof(EnemyStateMachine))]
[RequireComponent(typeof(TeamComponent))]
public class EnemyController : MonoBehaviour, IDamageable
{   
    [Header("Data")]
    public EnemyData data;
    
    [Header("Shaders")]
    private MaterialPropertyBlock _mpb;
    private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");
    
    [Header("Components")]
    public Animator animator;
    public Rigidbody2D rb;
    public EnemyStateMachine stateMachine;
    public Transform groundCheck;
    public LayerMask groundMask = 1;

    [Header("Hitbox")]
    [SerializeField] public Collider2D hitboxCollider;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    [HideInInspector] public int patrolIndex = 0;
    [HideInInspector] public float patrolWaitTimer = 0f;
    [HideInInspector] public bool isWaitingAtPoint = false;

    [Header("Detection")]
    public LayerMask playerMask = 1;
    public LayerMask obstacleMask = 1;
    
    [Header("Runtime")] 
    public float currentHealth;
    public bool isDead = false;
    public bool isFacingRight = true;

    // Events
    public System.Action<float> OnHealthChanged;
    public System.Action OnDeath;

    // Cached references
    private Transform playerTransform;
    private Collider2D enemyCollider;
    private SpriteRenderer spriteRenderer;

    // Timers
    [HideInInspector] public float lastAttackTime = 0f;

    private void Awake()
    {
        Debug.Log($"[EnemyController] Awake: {name}");
        InitializeComponents();
        InitializeHealth();
        FindPlayer();
        // Đảm bảo teamIndex đúng
        var team = GetComponent<TeamComponent>();
        if (team != null) team.teamIndex = TeamIndex.Enemy;
        _mpb = new MaterialPropertyBlock();
    }

    private void Start()
    {
        Debug.Log($"[EnemyController] Start: {name}");
        ValidateSetup();
        Debug.Log($"[EnemyController] ChangeState to Idle: {name}");
        stateMachine.ChangeState(EnemyStateType.Idle);
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    private void InitializeComponents()
    {
        stateMachine = GetComponent<EnemyStateMachine>();
        if (!animator) animator = GetComponent<Animator>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Setup rigidbody constraints
        if (rb)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 3f;
        }
    }

    private void InitializeHealth()
    {
        currentHealth = data != null ? data.maxHealth : 100f;
    }

    private void FindPlayer()
    {
        if (!playerTransform)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) playerTransform = player.transform;
        }
    }

    private void ValidateSetup()
    {
        if (!data)
        {
            Debug.LogError($"Enemy {name} missing EnemyData!");
        }

        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning($"Enemy {name} has no patrol points!");
        }

        if (!groundCheck)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }

    #region Damage System

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        
        OnHealthChanged?.Invoke(currentHealth);

        // Flash trắng khi bị trúng đòn
        if (spriteRenderer)
        {
            StartCoroutine(FlashWhite());
        }
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Enter stunned state for better hit reaction
            stateMachine.ChangeState(EnemyStateType.Stunned);
        }
    }

    public void TakeDamage(float amount, Vector2 knockbackDirection, Vector2 knockbackForce, float knockbackDuration)
    {
        // For now, just call the basic TakeDamage (ignoring knockback)
        TakeDamage(amount);
    }

    // Hiệu ứng flash trắng
    private IEnumerator FlashWhite()
    {
        spriteRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(FlashAmount, 1f);
        spriteRenderer.SetPropertyBlock(_mpb);
        yield return new WaitForSeconds(0.08f);
        _mpb.SetFloat(FlashAmount, 0f);
        spriteRenderer.SetPropertyBlock(_mpb);
    }

    public void Die()
    {
        if (isDead) return;
        Debug.Log($"[EnemyController] Die: {name}");
        isDead = true;
        OnDeath?.Invoke();
        stateMachine.ChangeState(EnemyStateType.Die);
    }

    #endregion
    
    #region Movement & Detection

    public bool IsGrounded()
    {
        if (!groundCheck) return true;
        // Sử dụng OverlapCircle thay cho CheckCircle
        return Physics2D.OverlapBox(groundCheck.position, new Vector2(0.6f, 0.3f), 0, groundMask);
    }

    public bool CanSeePlayer()
    {
        if (!playerTransform || !IsPlayerInDetectionRange()) return false;

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleMask);
        
        return hit.collider == null; // No obstacles between enemy and player
    }

    public bool IsPlayerInDetectionRange()
    {
        if (!playerTransform || !data) return false;
        return Vector2.Distance(transform.position, playerTransform.position) <= data.detectionRange;
    }

    public bool IsPlayerInAttackRange()
    {
        if (!playerTransform || !data) return false;
        return Vector2.Distance(transform.position, playerTransform.position) <= data.attackRange;
    }

    public bool CanAttack()
    {
        return !isDead && Time.time >= lastAttackTime + data.attackCooldown;
    }

    public void PerformAttack()
    {
        if (!CanAttack()) return;

        if (animator)
        {
            animator.SetTrigger("Attack");
        }
    }


    public void MoveTowards(Vector2 targetPosition, float speedMultiplier = 1f)
    {
        if (isDead || !rb) return;

        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        
        // Check for edges/cliffs before moving
        if (ShouldAvoidEdge(direction))
        {
            return;
        }

        float moveSpeed = data.moveSpeed * speedMultiplier;
        Debug.Log($"[MoveTowards] {name} | direction: {direction} | moveSpeed: {moveSpeed} | before velocity: {rb.velocity}");
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
        Debug.Log($"[MoveTowards] {name} | after velocity: {rb.velocity}");

        // Face the movement direction
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            FaceDirection(direction.x > 0);
        }
    }

    public void StopMovement()
    {
        if (rb)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    public void FaceDirection(bool facingRight)
    {
        if (isFacingRight != facingRight)
        {
            isFacingRight = facingRight;
            
            // Fallback to scale flip
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (isFacingRight ? 1 : -1);
            transform.localScale = scale;
            
        }
    }

    #endregion

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
    #region Animation Events
    // Gọi từ Animation Event đúng frame tấn công
    public void OnAttackHit()
    {
        if (hitboxCollider == null) return;
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        int count = hitboxCollider.Overlap(filter, results);
        for (int i = 0; i < count; i++)
        {
            Collider2D col = results[i];
            if (col.CompareTag("Player"))
            {
                PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    Vector2 direction = (playerHealth.transform.position - transform.position).normalized;
                    Vector2 knockbackForce = new Vector2(data.kockbackForceX, data.kockbackForceY);
                    playerHealth.TakeDamage(data.attackDamage, direction, knockbackForce, data.knockbackDuration);
                }
            }
        }
    }
    #endregion
    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (!data) return;
        // Detection Range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRange);
        // Attack Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
        // Ground Check
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, new Vector2(0.6f, 0.3f));
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
        // Line of sight to player
        if (playerTransform && CanSeePlayer())
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
        // Visualize edge check position for ShouldAvoidEdge
        if (Application.isPlaying && rb)
        {
            // Lấy hướng di chuyển hiện tại (nếu có)
            Vector2 direction = rb.velocity.normalized;
            if (direction != Vector2.zero)
            {
                Vector2 edgeCheckPos = (Vector2)transform.position + direction * 2.5f + Vector2.down * 1.5f;
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(edgeCheckPos, 0.3f);
            }
        }
    }

    #endregion

    // Properties for easy access
    public Transform Player => playerTransform;
    public EnemyData Data => data;
}




