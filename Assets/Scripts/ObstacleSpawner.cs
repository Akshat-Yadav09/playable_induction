using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs; // Drag multiple prefabs here in the Inspector
    public int poolSizePerPrefab = 3;   // How many of each type to pre-create
    public float spawnRate = 2f;

    // One pool per prefab type so we recycle the correct mesh/collider
    private List<GameObject>[] pools;
    private float timer = 0f;

    void Start()
    {
        // Build a separate pool for each prefab
        pools = new List<GameObject>[obstaclePrefabs.Length];
        for (int p = 0; p < obstaclePrefabs.Length; p++)
        {
            pools[p] = new List<GameObject>();
            for (int i = 0; i < poolSizePerPrefab; i++)
            {
                GameObject obj = Instantiate(obstaclePrefabs[p]);
                obj.SetActive(false);
                pools[p].Add(obj);
            }
        }
    }

    void Update()
    {
        // Use dynamic spawn interval from DifficultyManager (falls back to spawnRate)
        float interval = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.CurrentSpawnInterval
            : spawnRate;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            SpawnObstacle();
            timer = 0f;
        }
    }

    void SpawnObstacle()
    {
        // Pick a random prefab type
        int typeIndex = Random.Range(0, pools.Length);
        List<GameObject> pool = pools[typeIndex];

        // Find an inactive object in that pool
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                pool[i].transform.position = transform.position;
                pool[i].SetActive(true);
                return;
            }
        }

        // Pool for this type exhausted — expand dynamically
        GameObject obj = Instantiate(obstaclePrefabs[typeIndex]);
        obj.transform.position = transform.position;
        pool.Add(obj);
        Debug.LogWarning($"ObstacleSpawner: Pool for prefab '{obstaclePrefabs[typeIndex].name}' expanded to {pool.Count}.");
    }
}