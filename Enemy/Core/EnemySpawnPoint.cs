using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public enum AreaShape { Point, Circle, Box }

public class EnemySpawnPoint : MonoBehaviour
{
    public EnemySpawnProfile profile;
    public AreaShape areaShape = AreaShape.Point;

    public float circleRadius = 1f;
    public Vector2 boxSize = Vector2.one;

    public bool activeOnStart = true;
    public bool useRandomRotation = false;
    
    [Header("Patrol Points")]
    [SerializeField] private bool autoAssignPatrolPoints = true;
    [SerializeField] private float patrolSearchRadius = 5f;
    [SerializeField] private string patrolPointTag = "PatrolPoint";
    [SerializeField] private Transform patrolPointParent;

    private Coroutine spawnLoop;

    private void Start()
    {
        if (activeOnStart && profile != null)
            spawnLoop = StartCoroutine(SpawnLoop());
    }

    public void StartSpawning()
    {
        if (spawnLoop == null && profile != null)
            spawnLoop = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnLoop != null)
        {
            StopCoroutine(spawnLoop);
            spawnLoop = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            DoSpawnEvent();
            yield return new WaitForSeconds(profile.spawnInterval);
        }
    }

    private void DoSpawnEvent()
    {
        if (profile == null || profile.entries == null || profile.entries.Length == 0) return;

        for (int i = 0; i < profile.spawnCount; i++)
        {
            var entry = PickWeighted(profile.entries);
            if (entry.poolKey == null || entry.poolKey == "") continue;
            if (Random.value > entry.spawnChance) continue;

            Vector3 pos = ChoosePosition(entry.localOffset);
            Quaternion rot = useRandomRotation ? Quaternion.Euler(0, 0, Random.Range(0f, 360f)) : Quaternion.identity;

            // Spawn enemy từ pool
            GameObject enemy = PoolManager.Instance.Spawn(entry.poolKey, pos, rot, null);
            
            // Tự động gán patrol points nếu cần
            if (enemy != null && autoAssignPatrolPoints)
            {
                AssignPatrolPointsToEnemy(enemy);
            }
        }
    }

    private void AssignPatrolPointsToEnemy(GameObject enemy)
    {
        // Chỉ gán patrol points cho enemy cần thiết
        if (enemy.GetComponent<DarkWolf>() != null)
        {
            return; // Boss không cần patrol points
        }

        var groundEnemy = enemy.GetComponent<GroundEnemy>();
        var flyingEnemy = enemy.GetComponent<FlyingEnemy>();

        if (groundEnemy == null && flyingEnemy == null)
        {
            return; // Không phải enemy cần patrol
        }

        Transform[] patrolPoints = FindPatrolPoints();

        if (patrolPoints.Length == 0)
        {
            return; // Không có patrol points, không báo lỗi
        }

        // Gán patrol points
        if (groundEnemy != null)
        {
            groundEnemy.patrolPoints = patrolPoints;
            groundEnemy.patrolIndex = 0;
            groundEnemy.patrolWaitTimer = 0f;
            groundEnemy.isWaitingAtPoint = false;
        }
        else if (flyingEnemy != null)
        {
            flyingEnemy.flyPoints = patrolPoints;
            flyingEnemy.flyIndex = 0;
            flyingEnemy.flyWaitTimer = 0f;
            flyingEnemy.isWaitingAtFlyPoint = false;
        }

        Debug.Log($"[EnemySpawnPoint] Assigned {patrolPoints.Length} patrol points to {enemy.name}");
    }

    private Transform[] FindPatrolPoints()
    {
        List<Transform> points = new List<Transform>();

        if (patrolPointParent != null)
        {
            // Sử dụng patrol point parent
            for (int i = 0; i < patrolPointParent.childCount; i++)
            {
                points.Add(patrolPointParent.GetChild(i));
            }
        }
        else
        {
            // Tìm patrol points trong radius
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, patrolSearchRadius);

            foreach (var col in colliders)
            {
                if (col.CompareTag(patrolPointTag))
                {
                    points.Add(col.transform);
                }
            }
        }

        return points.ToArray();
    }

    private Vector3 ChoosePosition(Vector2 localOffset)
    {
        Vector2 worldOff = localOffset;
        switch (areaShape)
        {
            case AreaShape.Point:
                worldOff = localOffset;
                break;
            case AreaShape.Circle:
                float ang = Random.Range(0f, Mathf.PI * 2f);
                float r = Random.Range(0f, circleRadius);
                worldOff = new Vector2(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r) + localOffset;
                break;
            case AreaShape.Box:
                float rx = Random.Range(-boxSize.x * 0.5f, boxSize.x * 0.5f);
                float ry = Random.Range(-boxSize.y * 0.5f, boxSize.y * 0.5f);
                worldOff = new Vector2(rx, ry) + localOffset;
                break;
        }
        return transform.TransformPoint(worldOff);
    }

    private EnemySpawnEntry PickWeighted(EnemySpawnEntry[] arr)
    {
        int total = 0;
        foreach (var e in arr) total += Mathf.Max(0, e.weight);
        if (total <= 0) return arr[Random.Range(0, arr.Length)];
        int r = Random.Range(0, total);
        int acc = 0;
        foreach (var e in arr)
        {
            acc += Mathf.Max(0, e.weight);
            if (r < acc) return e;
        }
        return arr[0];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        switch (areaShape)
        {
            case AreaShape.Point:
                Gizmos.DrawSphere(transform.position, 0.1f);
                break;
            case AreaShape.Circle:
                Gizmos.DrawWireSphere(transform.position, circleRadius);
                break;
            case AreaShape.Box:
                Gizmos.DrawWireCube(transform.position, boxSize);
                break;
        }

        // Vẽ patrol points
        if (autoAssignPatrolPoints)
        {
            Transform[] patrolPoints = FindPatrolPoints();
            Gizmos.color = Color.blue;
            foreach (var point in patrolPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.3f);
                    Gizmos.DrawLine(transform.position, point.position);
                }
            }
        }
    }
}
