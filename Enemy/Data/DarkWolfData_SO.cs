// DarkWolfData_SO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New DarkWolf Data", menuName = "Enemy/DarkWolf Data SO")]
public class DarkWolfData_SO : EnemyData_SO
{
    [Header("Boss Dash")]
    public int numDashes = 3;
    public float dashSpeed = 10f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 0.5f;
    public int attacksBeforeDash = 5; // Number of normal attacks before dash skill

    private void OnValidate()
    {
        base.OnValidate();
        numDashes = Mathf.Max(1, numDashes);
        dashSpeed = Mathf.Max(1f, dashSpeed);
        dashDuration = Mathf.Max(0.1f, dashDuration);
        dashCooldown = Mathf.Max(0f, dashCooldown);
        attacksBeforeDash = Mathf.Max(1, attacksBeforeDash);
    }
}