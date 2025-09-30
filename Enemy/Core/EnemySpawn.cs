using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IBossWithDoor
{
    DoorController doorController { get; set; }
    string doorGroupName { get; set; }
}

[System.Serializable]
public class EnemySpawnConfig
{
    public GameObject prefab;
    public Transform spawnPoint;
    public int count = 1;
    public int prewarm = 0;
    public bool isBoss = false;
    public string doorGroupName; // Thêm lại trường này
}

public class EnemySpawn : MonoBehaviour
{
    [Header("Enemy Configs")]
    public List<EnemySpawnConfig> enemyConfigs;
    private List<GameObject> spawnedEnemies = new();

    private void Awake()
    {
        foreach (var config in enemyConfigs)
        {
            if (config == null || config.prefab == null) continue;
            if (ObjectPooling.Instance == null)
            {
                Debug.LogError($"[EnemySpawn] ObjectPooling.Instance is null! Bạn cần đặt ObjectPooling vào scene.", this);
                return;
            }
            if (config.prewarm > 0)
                ObjectPooling.Instance.Prewarm(config.prefab, config.prewarm);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            SpawnEnemies();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            DespawnEnemies();
    }

    public void SpawnEnemies()
    {
        if (spawnedEnemies.Count > 0) return;
        if (ObjectPooling.Instance == null)
        {
            Debug.LogError("[EnemySpawn] ObjectPooling.Instance is null! Bạn cần đặt ObjectPooling vào scene.", this);
            return;
        }
        foreach (var config in enemyConfigs)
        {
            if (config == null)
            {
                Debug.LogWarning("[EnemySpawn] Một config trong enemyConfigs bị null.", this);
                continue;
            }
            if (config.prefab == null)
            {
                Debug.LogWarning("[EnemySpawn] Prefab trong config bị null.", this);
                continue;
            }
            if (config.isBoss) continue;
            for (int i = 0; i < config.count; i++)
            {
                var obj = ObjectPooling.Instance.GetObject(
                    config.prefab,
                    config.spawnPoint != null ? config.spawnPoint.position : transform.position,
                    Quaternion.identity
                );
                if (obj == null)
                {
                    Debug.LogError($"[EnemySpawn] ObjectPooling.Instance.GetObject trả về null! Prefab: {config.prefab.name}", this);
                    continue;
                }
                // Gán patrol point nếu spawnPoint có child
                Transform[] patrolPoints = null;
                if (config.spawnPoint != null && config.spawnPoint.childCount > 0)
                {
                    patrolPoints = new Transform[config.spawnPoint.childCount];
                    for (int c = 0; c < config.spawnPoint.childCount; c++)
                        patrolPoints[c] = config.spawnPoint.GetChild(c);
                }
                var groundEnemy = obj.GetComponent<GroundEnemy>();
                if (groundEnemy != null && patrolPoints != null && patrolPoints.Length > 0)
                    groundEnemy.patrolPoints = patrolPoints;
                var flyingEnemy = obj.GetComponent<FlyingEnemy>();
                if (flyingEnemy != null && patrolPoints != null && patrolPoints.Length > 0)
                    flyingEnemy.flyPoints = patrolPoints;
                var bossWithDoor = obj.GetComponent<IBossWithDoor>();
                if (bossWithDoor != null)
                {
                    bossWithDoor.doorController = Object.FindFirstObjectByType<DoorController>();
                    bossWithDoor.doorGroupName = config.doorGroupName;
                }
                spawnedEnemies.Add(obj);
            }
        }
    }

    public void SpawnBoss()
    {
        if (ObjectPooling.Instance == null)
        {
            Debug.LogError("[EnemySpawn] ObjectPooling.Instance is null! Bạn cần đặt ObjectPooling vào scene.", this);
            return;
        }
        foreach (var config in enemyConfigs)
        {
            if (config == null)
            {
                Debug.LogWarning("[EnemySpawn] Một config trong enemyConfigs bị null.", this);
                continue;
            }
            if (config.prefab == null)
            {
                Debug.LogWarning("[EnemySpawn] Prefab trong config bị null.", this);
                continue;
            }
            if (!config.isBoss) continue;
            for (int i = 0; i < config.count; i++)
            {
                var obj = ObjectPooling.Instance.GetObject(
                    config.prefab,
                    config.spawnPoint != null ? config.spawnPoint.position : transform.position,
                    Quaternion.identity
                );
                if (obj == null)
                {
                    Debug.LogError($"[EnemySpawn] ObjectPooling.Instance.GetObject trả về null! Prefab: {config.prefab.name}", this);
                    continue;
                }
                // Gán patrol point nếu spawnPoint có child
                Transform[] patrolPoints = null;
                if (config.spawnPoint != null && config.spawnPoint.childCount > 0)
                {
                    patrolPoints = new Transform[config.spawnPoint.childCount];
                    for (int c = 0; c < config.spawnPoint.childCount; c++)
                        patrolPoints[c] = config.spawnPoint.GetChild(c);
                }
                var groundEnemy = obj.GetComponent<GroundEnemy>();
                if (groundEnemy != null && patrolPoints != null && patrolPoints.Length > 0)
                    groundEnemy.patrolPoints = patrolPoints;
                var flyingEnemy = obj.GetComponent<FlyingEnemy>();
                if (flyingEnemy != null && patrolPoints != null && patrolPoints.Length > 0)
                    flyingEnemy.flyPoints = patrolPoints;
                var bossWithDoor = obj.GetComponent<IBossWithDoor>();
                if (bossWithDoor != null)
                {
                    bossWithDoor.doorController = Object.FindFirstObjectByType<DoorController>();
                    bossWithDoor.doorGroupName = config.doorGroupName;
                }
                spawnedEnemies.Add(obj);
            }
        }
    }

    void DespawnEnemies()
    {
        foreach (var e in spawnedEnemies)
        {
            if (e != null)
                ObjectPooling.Instance.ReturnToPool(e);
        }
        spawnedEnemies.Clear();
    }
}
