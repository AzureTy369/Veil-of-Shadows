using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TeamComponent))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public PlayerStats stats;
    public float currentHealth;
    public bool isDead = false;

    private Animator animator;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    [Header("Knockback")]
    public float KnockbackCounter = 0f;


    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = stats != null ? stats.maxHealth : 100f;
        // Đảm bảo teamIndex đúng
        var team = GetComponent<TeamComponent>();
        if (team != null) team.teamIndex = TeamIndex.Player;
    }

    private void Update()
    {
    }

    public void TakeDamage(float amount)
    {
        // Default knockback values (no knockback)
        TakeDamage(amount, Vector2.zero, Vector2.zero, 0f);
    }

    public void TakeDamage(float amount, Vector2 knockbackDirection, Vector2 knockbackForce, float knockbackDuration)
    {
        if (isDead) return;
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        // if (playerMovement) playerMovement.SetAnimState(PlayerAnimState.Hit);
        animator.Play("Hit");

        KnockbackCounter = knockbackDuration;
        rb.velocity = new Vector2(0, rb.velocity.y);
        Vector2 newVelocity = new Vector2(knockbackDirection.x * knockbackForce.x, (rb.velocity.y * knockbackDirection.y) + knockbackForce.y);
        rb.velocity = newVelocity;

        // Hit stop
        StartCoroutine(HitStopCoroutine());
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitStopCoroutine()
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.15f);
        Time.timeScale = originalTimeScale;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        if (playerMovement) playerMovement.SetAnimState(PlayerAnimState.Die);

        // Disable movement, v.v. (Có thể thêm logic respawn hoặc game over ở đây)
    }
}
