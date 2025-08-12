using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    private PlayerMovement movement;

    private Vector2 moveInput;

    [Header("Input")]
    [SerializeField] private bool useDefaultInput = true;
    private IInputService inputService;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        movement.SetData(playerData);
        if (useDefaultInput || inputService == null)
        {
            inputService = new DefaultInputService();
        }
    }

    private void Update()
    {
        // Nhận input
        moveInput = inputService.GetMoveInput();
        movement.Move(moveInput);

        if (inputService.GetJumpDown())
        {
            movement.JumpInput();
        }
        if (inputService.GetJumpUp())
        {
            movement.JumpUpInput();
        }
        if (inputService.GetDashDown())
        {
            movement.DashInput();
        }
    }

    private void FixedUpdate()
    {
        movement.OnFixedUpdate();
    }
} 