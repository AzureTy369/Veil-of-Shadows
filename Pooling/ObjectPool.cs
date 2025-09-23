using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private GameObject prefab;
    private Transform parent;
    private Queue<GameObject> queue = new Queue<GameObject>();
    private bool allowExpand;

    public int CreatedCount { get; private set; }
    public int AvailableCount => queue.Count;

    public ObjectPool(GameObject prefab, int initialSize, bool allowExpand, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.allowExpand = allowExpand;
        Prewarm(initialSize);
    }

    private void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var go = CreateNew();
            ReturnToPool(go);
        }
    }

    private GameObject CreateNew()
    {
        var go = GameObject.Instantiate(prefab, parent);
        go.name = prefab.name + " (pooled)";
        go.SetActive(false);
        CreatedCount++;
        return go;
    }

    public GameObject Get()
    {
        GameObject go = null;
        while (queue.Count > 0)
        {
            go = queue.Dequeue();
            if (go != null) break;
            go = null;
        }

        if (go != null)
        {
            go.SetActive(true);
            InvokeOnSpawn(go);
            return go;
        }

        if (allowExpand)
        {
            go = CreateNew();
            go.SetActive(true);
            InvokeOnSpawn(go);
            return go;
        }

        return null;
    }

    public void ReturnToPool(GameObject go)
    {
        if (go == null) return;
        InvokeOnReturn(go);
        go.SetActive(false);
        if (parent != null)
            go.transform.SetParent(parent, false);
        queue.Enqueue(go);
    }

    private void InvokeOnSpawn(GameObject go)
    {
        var poolable = go.GetComponent<IPoolable>();
        if (poolable != null) poolable.OnSpawnFromPool();
    }

    private void InvokeOnReturn(GameObject go)
    {
        var poolable = go.GetComponent<IPoolable>();
        if (poolable != null) poolable.OnReturnToPool();
    }
}


