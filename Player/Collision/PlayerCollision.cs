using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer;

    public bool IsGrounded()
    {
        return Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer);
    }
    public bool IsOnWall(bool isFacingRight, bool isWallJumping)
    {
        if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && isFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !isFacingRight)) && !isWallJumping)
            return true;
        return false;
    }
    public bool IsOnWallLeft(bool isFacingRight, bool isWallJumping)
    {
        if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !isFacingRight)
            || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && isFacingRight)) && !isWallJumping)
            return true;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheckPoint == null || _frontWallCheckPoint == null || _backWallCheckPoint == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
        Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
    }
} 