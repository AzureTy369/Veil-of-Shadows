using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private Rigidbody2D RB;
    private float _fallSpeedYDampingChangeThreshold;
    public void Initialize(Rigidbody2D rb, float fallSpeedYDampingChangeThreshold)
    {
        this.RB = rb;
        this._fallSpeedYDampingChangeThreshold = fallSpeedYDampingChangeThreshold;
    }
    public void DampingYCamera()
    {
        if(RB.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping
        && !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        if(RB.velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping
        && CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
        }
    }
} 