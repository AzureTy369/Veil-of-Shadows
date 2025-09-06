using UnityEngine;

[CreateAssetMenu(menuName = "Player/Player Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Basic Stats")]
    public float maxHealth = 100f;
    public float attack = 10f;
    public float defense = 0f;
    // Thêm các chỉ số khác nếu cần
    
}
