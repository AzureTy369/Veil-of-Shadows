using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance { get; private set; }

    private Dictionary<GameObject, List<GameObject>> pool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pool.ContainsKey(prefab))
            pool[prefab] = new List<GameObject>();

        foreach (var obj in pool[prefab])
        {
            if (!obj.activeSelf)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
                obj.GetComponent<IPoolable>()?.OnSpawnFromPool();
                return obj;
            }
        }

        // nếu không có object nào sẵn, tạo mới
        var newObj = Instantiate(prefab, position, rotation);
        newObj.SetActive(true);
        newObj.GetComponent<IPoolable>()?.OnSpawnFromPool();
        pool[prefab].Add(newObj);
        return newObj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.GetComponent<IPoolable>()?.OnReturnToPool();
        obj.SetActive(false);
    }

    public void Prewarm(GameObject prefab, int count)
    {
        if (!pool.ContainsKey(prefab))
            pool[prefab] = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            var g = Instantiate(prefab);
            g.SetActive(false);
            pool[prefab].Add(g);
        }
    }
}
