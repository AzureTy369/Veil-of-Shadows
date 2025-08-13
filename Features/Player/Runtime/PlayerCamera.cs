using UnityEngine;

// [RequireComponent(typeof(PlayerMovement))]
public class PlayerCamera : MonoBehaviour
{
    private PlayerMovement movement;
    private Rigidbody2D rb;
    private float fallSpeedYDampingChangeThreshold;

    [SerializeField] private CameraManager cameraManager; // assign in inspector or auto-find
    private ICameraService cameraService;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        if (cameraManager == null)
        {
            cameraManager = FindObjectOfType<CameraManager>();
        }
        cameraService = cameraManager;
    }

    private void OnEnable()
    {
        if (movement == null)
            movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.OnLanded += HandleLanded;
    }

    private void OnDisable()
    {
        if (movement != null)
            movement.OnLanded -= HandleLanded;
    }

    private void Start()
    {
        fallSpeedYDampingChangeThreshold = cameraService.FallSpeedYDampingChangeThreshold;
        rb = movement.RB;
    }

    private void Update()
    {
        if (rb == null) return;
        
        if (rb.velocity.y < fallSpeedYDampingChangeThreshold && !cameraService.IsLerpingYDamping
            && !cameraService.LerpedFromPlayerFalling)
        {
            cameraService.LerpYDamping(true);
        }

        if (rb.velocity.y >= 0f && !cameraService.IsLerpingYDamping
            && cameraService.LerpedFromPlayerFalling)
        {
            cameraService.LerpedFromPlayerFalling = false;
            cameraService.LerpYDamping(false);
        }
    }

    private void HandleLanded()
    {
        cameraService.ResetYDamping();
    }
} 