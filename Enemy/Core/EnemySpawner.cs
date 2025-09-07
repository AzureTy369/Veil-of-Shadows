// EnemySpawner.cs
using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab; // Prefab của GroundEnemy hoặc base
    [SerializeField] private int poolSize = 10;
    [SerializeField] private Transform[] spawnPoints;

    private List<GameObject> enemyPool = new List<GameObject>();

    private void Start()
    {
        InitializePool();
        // Spawn initial enemies if needed
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, transform);
            enemy.SetActive(false);
            enemyPool.Add(enemy);
        }
    }

    public GameObject SpawnEnemy(Vector3 position)
    {
        foreach (GameObject enemy in enemyPool)
        {
            if (!enemy.activeSelf)
            {
                enemy.transform.position = position;
                enemy.SetActive(true);
                return enemy;
            }
        }
        // Nếu pool hết, tạo mới
        GameObject newEnemy = Instantiate(enemyPrefab, position, Quaternion.identity, transform);
        enemyPool.Add(newEnemy);
        return newEnemy;
    }

    // Có thể thêm method để spawn tại spawnPoints ngẫu nhiên, etc.
}