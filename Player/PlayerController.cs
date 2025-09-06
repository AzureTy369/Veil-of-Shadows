using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;

    private Vector2 moveInput;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (movement.PlayerHealth.KnockbackCounter > 0)
        {
            movement.PlayerHealth.KnockbackCounter -= Time.deltaTime;
            return;
        }
        // Nhận input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        movement.Move(moveInput);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.J))
        {
            movement.JumpInput();
        }
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.J))
        {
            movement.JumpUpInput();
        }
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K))
        {
            movement.DashInput();
        }
        movement.DampingYCamera();
    }

    private void FixedUpdate()
    {
        movement.OnFixedUpdate();
    }
} 