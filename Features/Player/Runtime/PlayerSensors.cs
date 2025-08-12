using UnityEngine;

public class PlayerSensors : MonoBehaviour
{
    [Header("Checks")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform frontWallCheckPoint;
    [SerializeField] private Transform backWallCheckPoint;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.5f, 1f);

    [Header("Layers & Tags")]
    [SerializeField] private LayerMask groundLayer;

    public bool IsGrounded { get; private set; }
    public bool IsFrontWall { get; private set; }
    public bool IsBackWall { get; private set; }

    public void Refresh()
    {
        IsGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer);
        IsFrontWall = Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer);
        IsBackWall = Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null || frontWallCheckPoint == null || backWallCheckPoint == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(frontWallCheckPoint.position, wallCheckSize);
        Gizmos.DrawWireCube(backWallCheckPoint.position, wallCheckSize);
    }
} 