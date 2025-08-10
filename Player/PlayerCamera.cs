using UnityEngine;

// [RequireComponent(typeof(PlayerMovement))]
public class PlayerCamera : MonoBehaviour
{
    private PlayerMovement movement;
    private Rigidbody2D rb;
    private float fallSpeedYDampingChangeThreshold;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
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
        fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;
        rb = movement.RB;
    }

    private void Update()
    {
        if (rb == null) return;
        
        if (rb.velocity.y < fallSpeedYDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping
            && !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        if (rb.velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping
            && CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
        }
    }

    private void HandleLanded()
    {
        CameraManager.instance.ResetYDamping();
    }
} 