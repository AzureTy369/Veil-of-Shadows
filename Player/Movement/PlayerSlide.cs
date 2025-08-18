using UnityEngine;

public class PlayerSlide
{
    private PlayerMovement movement;
    private Rigidbody2D RB;
    private PlayerData Data;
    public PlayerSlide(PlayerMovement movement, Rigidbody2D rb, PlayerData data)
    {
        this.movement = movement;
        this.RB = rb;
        this.Data = data;
    }
    public void Slide()
    {
        if(RB.velocity.y > 0)
        {
            RB.AddForce(-RB.velocity.y * UnityEngine.Vector2.up, UnityEngine.ForceMode2D.Impulse);
        }
        float speedDif = Data.slideSpeed - RB.velocity.y;
        float movementForce = speedDif * Data.slideAccel;
        movementForce = UnityEngine.Mathf.Clamp(movementForce, -UnityEngine.Mathf.Abs(speedDif)  * (1 / UnityEngine.Time.fixedDeltaTime), UnityEngine.Mathf.Abs(speedDif) * (1 / UnityEngine.Time.fixedDeltaTime));
        RB.AddForce(movementForce * UnityEngine.Vector2.up);
        movement.SetAnimState(PlayerAnimState.WallSlide);
    }
} 