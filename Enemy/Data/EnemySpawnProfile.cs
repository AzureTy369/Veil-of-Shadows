using UnityEngine;

[System.Serializable]
public struct EnemySpawnEntry
{
    public string poolKey;
    public int weight;
    public Vector2 localOffset;
    [Range(0f,1f)] public float spawnChance;
    public string bossDoorGroupName; // Thêm trường này
}

[CreateAssetMenu(menuName = "Enemy/Spawn Profile")]
public class EnemySpawnProfile : ScriptableObject
{
    public EnemySpawnEntry[] entries;
    public float spawnInterval = 2f;
    public int spawnCount = 1;
    public bool randomizeOrder = true;
}
