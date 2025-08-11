using UnityEngine;

public class Rigidbody2DAdapter : IPhysicsBody2D
{
    private readonly Rigidbody2D _rb;

    public Rigidbody2DAdapter(Rigidbody2D rb)
    {
        _rb = rb;
    }

    public Vector2 Velocity
    {
        get => _rb.velocity;
        set => _rb.velocity = value;
    }

    public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
    {
        _rb.AddForce(force, mode);
    }

    public void SetGravityScale(float scale)
    {
        _rb.gravityScale = scale;
    }
} 