using UnityEngine;

public interface IInputService
{
    Vector2 GetMoveInput();
    bool GetJumpDown();
    bool GetJumpUp();
    bool GetDashDown();
} 