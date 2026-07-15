using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObstaclePattern
{
    public string patternName;
    [TextArea(5, 10)]
    [Tooltip("Use 'B' for Block (Obs_2) and 'S' for Spike (Obs_1). Spaces for empty.")]
    public string layout;
    [Tooltip("Higher weight = more likely to spawn. Default is 1.")]
    [Min(0.01f)]
    public float spawnWeight = 1f;
}

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Basic Prefabs (Fallback)")]
    public GameObject[] obstaclePrefabs; // Drag multiple prefabs here in the Inspector
    public int poolSizePerPrefab = 5;   // How many of each type to pre-create

    [Header("Pattern Generation")]
    [Tooltip("Assign Obs_2 (Block) here")]
    public GameObject blockPrefab; 
    [Tooltip("Assign Obs_1 (Spike) here")]
    public GameObject spikePrefab; 
    public float gridSize = 1f; // The spacing between blocks
    [Tooltip("Vertical offset to make spikes sit perfectly flush on blocks")]
    public float spikeYOffset = -0.212f;
    public ObstaclePattern[] patterns;
    
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
    private List<GameObject>[] patternPools;
    private float timer = 0f;
    private float nextSpawnInterval;
    private float calculatedSafeInterval;
    private float totalPatternWeight;

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

        // Build a separate pool for each pattern
        if (patterns != null && patterns.Length > 0)
        {
            patternPools = new List<GameObject>[patterns.Length];
            for (int p = 0; p < patterns.Length; p++)
            {
                patternPools[p] = new List<GameObject>();
                for (int i = 0; i < poolSizePerPrefab; i++) // Use same pool size
                {
                    GameObject obj = GeneratePatternObject(patterns[p]);
                    obj.SetActive(false);
                    patternPools[p].Add(obj);
                }
            }
        }

        // Pre-calculate total pattern weight for weighted random selection
        totalPatternWeight = 0f;
        if (patterns != null)
        {
            for (int i = 0; i < patterns.Length; i++)
                totalPatternWeight += patterns[i].spawnWeight;
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
        bool hasPatterns = patterns != null && patterns.Length > 0;
        bool hasPrefabs = obstaclePrefabs != null && obstaclePrefabs.Length > 0;
        
        bool spawnPattern = hasPatterns && (!hasPrefabs || Random.value > 0.5f);

        if (spawnPattern)
        {
            // Weighted random selection: pick a pattern based on its spawnWeight
            int typeIndex = GetWeightedPatternIndex();
            List<GameObject> pool = patternPools[typeIndex];

            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].activeInHierarchy)
                {
                    pool[i].transform.position = transform.position;
                    pool[i].SetActive(true);
                    return;
                }
            }

            // Pool exhausted
            GameObject obj = GeneratePatternObject(patterns[typeIndex]);
            obj.transform.position = transform.position;
            pool.Add(obj);
            Debug.LogWarning($"ObstacleSpawner: Pattern Pool '{patterns[typeIndex].patternName}' expanded to {pool.Count}.");
            return;
        }

        if (!hasPrefabs) return;

        // Pick a random prefab type
        int typeIndexPrefab = Random.Range(0, pools.Length);
        List<GameObject> poolPrefab = pools[typeIndexPrefab];

        // Find an inactive object in that pool
        for (int i = 0; i < poolPrefab.Count; i++)
        {
            if (!poolPrefab[i].activeInHierarchy)
            {
                poolPrefab[i].transform.position = transform.position;
                poolPrefab[i].SetActive(true);
                return;
            }
        }

        // Pool for this type exhausted — expand dynamically
        GameObject objPrefab = Instantiate(obstaclePrefabs[typeIndexPrefab]);
        objPrefab.transform.position = transform.position;
        poolPrefab.Add(objPrefab);
        Debug.LogWarning($"ObstacleSpawner: Pool for prefab '{obstaclePrefabs[typeIndexPrefab].name}' expanded to {poolPrefab.Count}.");
    }

    /// <summary>
    /// Picks a pattern index using weighted random selection.
    /// Patterns with higher spawnWeight are chosen more often.
    /// </summary>
    private int GetWeightedPatternIndex()
    {
        float roll = Random.Range(0f, totalPatternWeight);
        float cumulative = 0f;
        for (int i = 0; i < patterns.Length; i++)
        {
            cumulative += patterns[i].spawnWeight;
            if (roll <= cumulative)
                return i;
        }
        // Fallback (shouldn't reach here)
        return patterns.Length - 1;
    }

    private GameObject GeneratePatternObject(ObstaclePattern pattern)
    {
        // Create an empty parent object
        GameObject parentObj = new GameObject("Pattern_" + pattern.patternName);
        
        // Add ObstacleMover so the whole pattern moves together
        ObstacleMover mover = parentObj.AddComponent<ObstacleMover>();
        
        // Default values for the mover (will dynamically use DifficultyManager inside ObstacleMover)
        mover.speed = 8f; 
        mover.deadZone = -25f; // Slightly larger deadzone for big patterns

        if (string.IsNullOrEmpty(pattern.layout)) return parentObj;

        // Parse the layout (split by newlines)
        string[] rows = pattern.layout.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        // In Unity, Y goes up. In text, the first row is usually the TOP.
        // So we iterate from bottom up, or reverse the row index.
        for (int y = 0; y < rows.Length; y++)
        {
            int invertedY = rows.Length - 1 - y;
            string row = rows[y];
            
            for (int x = 0; x < row.Length; x++)
            {
                char c = row[x];
                GameObject spawned = null;
                float yOffset = 0f;
                
                if (c == 'B' || c == 'b')
                {
                    if (blockPrefab != null) spawned = Instantiate(blockPrefab);
                }
                else if (c == 'S' || c == 's')
                {
                    if (spikePrefab != null) 
                    {
                        spawned = Instantiate(spikePrefab);
                        yOffset = spikeYOffset;
                    }
                }
                
                if (spawned != null)
                {
                    spawned.transform.SetParent(parentObj.transform);
                    // Position relative to parent
                    spawned.transform.localPosition = new Vector3(x * gridSize, invertedY * gridSize + yOffset, 0f);
                    
                    // Remove ObstacleMover from children since the parent handles movement
                    ObstacleMover childMover = spawned.GetComponent<ObstacleMover>();
                    if (childMover != null) Destroy(childMover);
                }
            }
        }
        
        return parentObj;
    }
}