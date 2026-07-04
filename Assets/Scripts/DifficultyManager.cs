using UnityEngine;

/// <summary>
/// Centralized difficulty controller. Ramps obstacle speed and spawn rate
/// over time so the game gets progressively harder the longer you survive.
/// Attach to the same GameObject as GameManager or any persistent object.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Obstacle Speed")]
    [Tooltip("Starting obstacle speed")]
    public float baseSpeed = 8f;
    [Tooltip("Maximum obstacle speed")]
    public float maxSpeed = 20f;
    [Tooltip("How many points until max speed is reached")]
    public float speedMaxAtScore = 300f;

    [Header("Spawn Rate")]
    [Tooltip("Starting time between spawns (seconds)")]
    public float baseSpawnInterval = 2f;
    [Tooltip("Minimum time between spawns (seconds)")]
    public float minSpawnInterval = 0.6f;
    [Tooltip("How many points until fastest spawn rate is reached")]
    public float spawnMaxAtScore = 300f;

    /// <summary>Current obstacle speed based on score.</summary>
    public float CurrentSpeed { get; private set; }

    /// <summary>Current spawn interval based on score.</summary>
    public float CurrentSpawnInterval { get; private set; }

    private float score;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        CurrentSpeed = baseSpeed;
        CurrentSpawnInterval = baseSpawnInterval;
    }

    /// <summary>
    /// Called by GameManager every frame with the latest score.
    /// </summary>
    public void UpdateDifficulty(float currentScore)
    {
        score = currentScore;

        // Lerp speed from base → max as score goes from 0 → speedMaxAtScore
        float speedT = Mathf.Clamp01(score / speedMaxAtScore);
        CurrentSpeed = Mathf.Lerp(baseSpeed, maxSpeed, speedT);

        // Lerp spawn interval from base → min as score goes from 0 → spawnMaxAtScore
        float spawnT = Mathf.Clamp01(score / spawnMaxAtScore);
        CurrentSpawnInterval = Mathf.Lerp(baseSpawnInterval, minSpawnInterval, spawnT);
    }
}
