// FlyingEnemyData_SO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Flying Enemy Data", menuName = "Enemy/Flying Enemy Data SO")]
public class FlyingEnemyData_SO : EnemyData_SO
{
    [Header("Flying Specific")]
    public float flySpeedMultiplier = 1.2f; // Tốc độ bay nhanh hơn ground một chút
    // Có thể thêm: avoidanceRange (tránh obstacle), etc.
}