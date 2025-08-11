using UnityEngine;

public interface IPhysicsBody2D
{
    Vector2 Velocity { get; set; }
    void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force);
    void SetGravityScale(float scale);
} 