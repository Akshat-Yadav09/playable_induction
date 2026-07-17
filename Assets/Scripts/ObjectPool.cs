using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for Unity. Pre-instantiates objects at startup and recycles them
/// instead of using Instantiate/Destroy, which eliminates GC spikes on WebGL.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [Header("Pools")]
    public List<Pool> pools;

    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary[pool.tag] = objectPool;
        }
    }

    /// <summary>
    /// Grab an object from the pool. Returns null if pool is empty or tag doesn't exist.
    /// </summary>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag) || poolDictionary[tag].Count == 0)
        {
            // Pool exhausted — expand by creating one more (safety net)
            foreach (Pool pool in pools)
            {
                if (pool.tag == tag)
                {
                    GameObject extra = Instantiate(pool.prefab, transform);
                    extra.SetActive(false);
                    poolDictionary[tag].Enqueue(extra);
                    break;
                }
            }

            if (!poolDictionary.ContainsKey(tag) || poolDictionary[tag].Count == 0)
                return null;
        }

        GameObject obj = poolDictionary[tag].Dequeue();

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        return obj;
    }

    /// <summary>
    /// Return an object to its pool. Deactivates and re-parents it.
    /// </summary>
    public void ReturnToPool(string tag, GameObject obj)
    {
        if (obj == null) return;

        if (poolDictionary.ContainsKey(tag))
        {
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            poolDictionary[tag].Enqueue(obj);
        }
        else
        {
            // If the pool doesn't exist, just destroy it to prevent memory leaks
            Destroy(obj);
        }
    }
}
