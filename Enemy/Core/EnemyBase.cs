// EnemyBase.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyStateMachine))]
[RequireComponent(typeof(TeamComponent))]
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Data")]
    public EnemyData_SO data;

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
    protected Transform playerTransform;
    private Collider2D enemyCollider;
    private SpriteRenderer spriteRenderer;

    // Timers
    [HideInInspector] public float lastAttackTime = 0f;
    protected virtual void Awake()
    {
        InitializeComponents();
        InitializeHealth();
        FindPlayer();
        // Đảm bảo teamIndex đúng
        var team = GetComponent<TeamComponent>();
        if (team != null) team.teamIndex = TeamIndex.Enemy;
        _mpb = new MaterialPropertyBlock();
    }

    protected virtual void Start()
    {
        ValidateSetup();
        stateMachine.ChangeState(EnemyStateType.Idle);
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    protected virtual void InitializeComponents()
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

    protected virtual void InitializeHealth()
    {
        currentHealth = data != null ? data.maxHealth : 100f;
    }

    protected virtual void FindPlayer()
    {
        if (!playerTransform)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) playerTransform = player.transform;
        }
    }

    protected virtual void ValidateSetup()
    {
        if (!data)
        {
            Debug.LogError($"Enemy {name} missing EnemyData_SO!");
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

    public virtual void TakeDamage(float amount)
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

    public virtual void TakeDamage(float amount, Vector2 knockbackDirection, Vector2 knockbackForce, float knockbackDuration)
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

    public virtual void Die()
    {
        if (isDead) return;
        Debug.Log($"[EnemyBase] Die: {name}");
        isDead = true;
        OnDeath?.Invoke();
        stateMachine.ChangeState(EnemyStateType.Die);
    }

    #endregion

    #region Movement & Detection

    public virtual bool IsGrounded()
    {
        if (!groundCheck) return true;
        // Sử dụng OverlapBox thay cho CheckCircle (fixed from original)
        return Physics2D.OverlapBox(groundCheck.position, new Vector2(0.6f, 0.3f), 0, groundMask);
    }

    public virtual bool CanSeePlayer()
    {
        if (!playerTransform) return false;

        Vector2 direction = playerTransform.position - transform.position;
        float distance = direction.magnitude;

        if (distance > data.detectionRange) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, obstacleMask);
        return hit.collider == null;
    }

    public virtual bool IsPlayerInAttackRange()
    {
        if (!playerTransform) return false;
        return Vector2.Distance(transform.position, playerTransform.position) <= data.attackRange;
    }

    public virtual bool CanAttack()
    {
        return Time.time - lastAttackTime >= data.attackCooldown;
    }

    public virtual void PerformAttack()
    {
        lastAttackTime = Time.time;
        animator.Play("Attack");
        // Attack logic will be triggered via Animation Event
    }

    public virtual void MoveTowards(Vector2 target, float speedMultiplier = 1f)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        float moveSpeed = data.moveSpeed * speedMultiplier;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        // Face the movement direction
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            FaceDirection(direction.x > 0);
        }
    }

    public virtual void StopMovement()
    {
        if (rb)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    public virtual void FaceDirection(bool facingRight)
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

    #region Animation Events
    // Gọi từ Animation Event đúng frame tấn công
    public virtual void OnAttackHit()
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
                    Vector2 knockbackForce = new Vector2(data.knockbackForceX, data.knockbackForceY);
                    playerHealth.TakeDamage(data.attackDamage, direction, knockbackForce, data.knockbackDuration);
                }
            }
        }
    }
    #endregion

    #region Gizmos

    protected virtual void OnDrawGizmosSelected()
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
        if (groundCheck != null)
        {
            Gizmos.DrawWireCube(groundCheck.position, new Vector2(0.6f, 0.3f));
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
    public EnemyData_SO Data => data;
}