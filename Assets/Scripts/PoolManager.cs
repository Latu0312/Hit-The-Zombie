using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple global pooling manager.
/// Attach this script to an empty GameObject (e.g. "PoolManager") in scene.
/// Use PoolManager.Instance.Spawn(prefab, pos, rot) and PoolManager.Instance.Despawn(obj).
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Prewarm pool for prefab with count (optional).
    /// </summary>
    public void Prewarm(GameObject prefab, int count)
    {
        if (!pools.ContainsKey(prefab)) pools[prefab] = new Queue<GameObject>();
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab);
            go.SetActive(false);
            pools[prefab].Enqueue(go);
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!pools.ContainsKey(prefab)) pools[prefab] = new Queue<GameObject>();

        GameObject obj;
        if (pools[prefab].Count > 0)
        {
            obj = pools[prefab].Dequeue();
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, pos, rot);
        }

        // If pooled object has IPooledObject, call OnSpawned
        var pooled = obj.GetComponent<IPooledObject>();
        if (pooled != null) pooled.OnSpawned();

        return obj;
    }

    public void Despawn(GameObject obj, GameObject prefabReference = null)
    {
        // If object implements IPooledObject, call OnDespawned
        var pooled = obj.GetComponent<IPooledObject>();
        if (pooled != null) pooled.OnDespawned();

        obj.SetActive(false);

        // If prefabReference provided, enqueue to that pool. Otherwise try to find original prefab by name (best-effort).
        if (prefabReference != null)
        {
            if (!pools.ContainsKey(prefabReference)) pools[prefabReference] = new Queue<GameObject>();
            pools[prefabReference].Enqueue(obj);
        }
        else
        {
            // fallback: store per prefab by trying to find a matching pool by name
            foreach (var kv in pools)
            {
                if (kv.Key.name == obj.name.Replace("(Clone)", "").Trim())
                {
                    kv.Value.Enqueue(obj);
                    return;
                }
            }

            // else store in a generic pool for its own type
            var key = obj; // not good, but avoid losing object
            if (!pools.ContainsKey(key)) pools[key] = new Queue<GameObject>();
            pools[key].Enqueue(obj);
        }
    }
}

public interface IPooledObject
{
    void OnSpawned();
    void OnDespawned();
}
