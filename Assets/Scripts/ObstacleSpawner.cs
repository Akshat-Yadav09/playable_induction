using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs; // Drag multiple prefabs here in the Inspector
    public int poolSizePerPrefab = 5;   // How many of each type to pre-create
    public float spawnRate = 2f;

    [Header("Spawn Randomness")]
    [Tooltip("Minimum multiplier on the base interval (closer obstacles)")]
    public float minIntervalMultiplier = 0.5f;
    [Tooltip("Maximum multiplier on the base interval (further obstacles)")]
    public float maxIntervalMultiplier = 1.2f;

    [Header("Safety")]
    [Tooltip("Minimum seconds between obstacles reaching the player. Ensures every pattern is survivable. Set to 0 to auto-calculate from jump physics.")]
    public float minSafeInterval = 0f;

    // One pool per prefab type so we recycle the correct mesh/collider
    private List<GameObject>[] pools;
    private float timer = 0f;
    private float nextSpawnInterval;
    private float calculatedSafeInterval;

    void Start()
    {
        // Calculate the player's jump duration from physics so we know the
        // minimum gap needed between obstacles for the player to land + jump again.
        if (minSafeInterval <= 0f)
            calculatedSafeInterval = CalculateJumpDuration();
        else
            calculatedSafeInterval = minSafeInterval;

        Debug.Log($"ObstacleSpawner Start: calculatedSafeInterval = {calculatedSafeInterval}");

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

        // Pick a random interval for the first spawn
        nextSpawnInterval = GetRandomInterval();
    }

    void Update()
    {
        if (float.IsNaN(nextSpawnInterval) || float.IsInfinity(nextSpawnInterval))
        {
            nextSpawnInterval = spawnRate;
        }
        
        timer += Time.deltaTime;
        // Debug.Log($"Spawner timer: {timer} / {nextSpawnInterval}");
        if (timer >= nextSpawnInterval)
        {
            SpawnObstacle();
            timer = 0f;
            // Pick a new random interval for the next spawn
            nextSpawnInterval = GetRandomInterval();
            Debug.Log($"Spawned obstacle! Next interval: {nextSpawnInterval}");
        }
    }

    /// <summary>
    /// Returns a randomized spawn interval, clamped to never go below the safe minimum.
    /// </summary>
    private float GetRandomInterval()
    {
        float baseInterval = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.CurrentSpawnInterval
            : spawnRate;

        float randomInterval = baseInterval * Random.Range(minIntervalMultiplier, maxIntervalMultiplier);

        // Never spawn faster than the player can physically react
        return Mathf.Max(randomInterval, calculatedSafeInterval);
    }

    /// <summary>
    /// Calculates the minimum time the player needs to land and jump again.
    /// Only uses fall time (not full jump), since the player jumps BEFORE
    /// the obstacle arrives — they only need to land and react.
    /// </summary>
    private float CalculateJumpDuration()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        float jumpForce = player != null ? player.jumpForce : 14f;
        float fallMult = player != null ? player.fallMultiplier : 3.5f;
        float gravity = Mathf.Abs(Physics2D.gravity.y);
        
        // Failsafes to prevent NaN
        if (gravity <= 0.01f) gravity = 9.81f;
        if (fallMult <= 0.01f) fallMult = 3.5f;

        // Time to reach peak: v = v0 - g*t → t_up = jumpForce / gravity
        float timeUp = jumpForce / gravity;

        // Fall time with multiplier (this is the bottleneck — player can't act until they land)
        float peakHeight = 0.5f * gravity * timeUp * timeUp;
        float timeDown = Mathf.Sqrt(Mathf.Max(0f, 2f * peakHeight / (gravity * fallMult)));

        // Player only needs: fall time + small reaction buffer to tap jump
        return timeDown + 0.15f;
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