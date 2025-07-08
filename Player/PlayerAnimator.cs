using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement movement;
    private PlayerAnimState lastAnimState;

    private void Awake()
    {
        if (movement == null)
            movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        var state = movement.CurrentAnimState;
        if (state != lastAnimState)
        {
            // Reset all parameters to false
            // animator.SetBool("IsIdle", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsWallSliding", false);
            animator.SetBool("IsFalling", false);

            // Set only the current state to true
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
            }
            lastAnimState = state;
        }
    }
}