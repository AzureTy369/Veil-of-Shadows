using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolConfig
{
    public string key;
    public GameObject prefab;
    public int initialSize = 10;
    public bool expandable = true;
    public Transform parent;
}

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    public List<PoolConfig> pools = new List<PoolConfig>();

    private Dictionary<string, ObjectPool> poolDict = new Dictionary<string, ObjectPool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (var cfg in pools)
        {
            if (cfg == null || cfg.prefab == null || string.IsNullOrEmpty(cfg.key)) continue;
            if (poolDict.ContainsKey(cfg.key)) continue;

            if (cfg.parent == null)
            {
                var parentGO = new GameObject(cfg.key + "_Pool");
                parentGO.transform.SetParent(this.transform, false);
                cfg.parent = parentGO.transform;
            }

            poolDict[cfg.key] = new ObjectPool(cfg.prefab, Mathf.Max(0, cfg.initialSize), cfg.expandable, cfg.parent);
        }
    }

    public bool HasPool(string key) => poolDict.ContainsKey(key);

    public void CreatePool(string key, GameObject prefab, int initialSize = 5, bool expandable = true, Transform parent = null)
    {
        if (string.IsNullOrEmpty(key) || prefab == null) return;
        if (poolDict.ContainsKey(key)) return;
        if (parent == null)
        {
            var parentGO = new GameObject(key + "_Pool");
            parentGO.transform.SetParent(this.transform, false);
            parent = parentGO.transform;
        }
        poolDict[key] = new ObjectPool(prefab, Mathf.Max(0, initialSize), expandable, parent);
    }

    public GameObject Spawn(string key, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!poolDict.ContainsKey(key))
        {
            Debug.LogWarning("Pool key not found: " + key);
            return null;
        }

        var pool = poolDict[key];
        var go = pool.Get();
        if (go == null)
        {
            Debug.LogWarning("Pool " + key + " returned null (empty and non-expandable)");
            return null;
        }

        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.rotation = rotation;

        var info = go.GetComponent<PooledObjectInfo>();
        if (info == null) info = go.AddComponent<PooledObjectInfo>();
        info.poolKey = key;

        return go;
    }

    public void ReturnToPool(GameObject go)
    {
        if (go == null) return;
        var info = go.GetComponent<PooledObjectInfo>();
        if (info == null || string.IsNullOrEmpty(info.poolKey))
        {
            Destroy(go);
            return;
        }

        if (!poolDict.ContainsKey(info.poolKey))
        {
            Destroy(go);
            return;
        }

        poolDict[info.poolKey].ReturnToPool(go);
    }

    public int GetCreatedCount(string key)
    {
        if (!poolDict.ContainsKey(key)) return 0;
        return poolDict[key].CreatedCount;
    }

    public int GetAvailableCount(string key)
    {
        if (!poolDict.ContainsKey(key)) return 0;
        return poolDict[key].AvailableCount;
    }
}

public static class PoolManagerExtensions
{
    public static GameObject SpawnIfExists(this PoolManager pm, string key, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        if (pm == null) return null;
        if (!pm.HasPool(key)) return null;
        return pm.Spawn(key, pos, rot, parent);
    }
}


