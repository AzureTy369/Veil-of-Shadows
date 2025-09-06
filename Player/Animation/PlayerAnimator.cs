using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private PlayerMovement movement;

    private void Awake()
    {
        if (movement == null)
            movement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        if (movement == null)
            movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.OnAnimStateChanged += HandleStateChanged;
            HandleStateChanged(movement.CurrentAnimState);
        }
    }

    private void OnDisable()
    {
        if (movement != null)
            movement.OnAnimStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(PlayerAnimState state)
    {
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsWallSliding", false);
        animator.SetBool("IsFalling", false);

        switch (state)
        {
            case PlayerAnimState.Run:
                animator.SetBool("IsRunning", true);
                break;
            case PlayerAnimState.Jump:
                animator.SetBool("IsJumping", true);
                break;
            case PlayerAnimState.Fall:
                animator.SetBool("IsFalling", true);
                break;
            case PlayerAnimState.WallSlide:
                animator.SetBool("IsWallSliding", true);
                break;
            case PlayerAnimState.Hit:
                animator.SetTrigger("Hit");
                break;
            case PlayerAnimState.Die:
                animator.SetTrigger("Die");
                break;
        }
    }
}