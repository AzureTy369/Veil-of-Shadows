using UnityEngine;

public class PlayerRun
{
    private PlayerMovement movement;
    private Rigidbody2D RB;
    private PlayerData Data;
    public PlayerRun(PlayerMovement movement, Rigidbody2D rb, PlayerData data)
    {
        this.movement = movement;
        this.RB = rb;
        this.Data = data;
    }
    public void Run(float lerpAmount, Vector2 moveInput)
    {
        float targetSpeed = moveInput.x * Data.runMaxSpeed;
        targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);
        float accelRate;
        if (movement.LastOnGroundTime > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
        if ((movement.IsJumping || movement.IsWallJumping || movement._isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMult;
            targetSpeed *= Data.jumpHangMaxSpeedMult;
        }
        if(Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && movement.LastOnGroundTime < 0)
        {
            accelRate = 0; 
        }
        float speedDif = targetSpeed - RB.velocity.x;
        float movementForce = speedDif * accelRate;
        RB.AddForce(movementForce * Vector2.right, ForceMode2D.Force);
    }
} 