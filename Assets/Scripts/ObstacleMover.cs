using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public float speed = 8f;
    public float deadZone = -15f; // The X coordinate where it disappears

    void Update()
    {
        // Use dynamic speed from DifficultyManager (falls back to local speed)
        float currentSpeed = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.CurrentSpeed
            : speed;

        // Move left at the current speed
        transform.Translate(Vector3.left * (currentSpeed * Time.deltaTime));

        // If it goes past the player and off-screen, destroy it
        if (transform.position.x < deadZone)
        {
            Destroy(gameObject);
        }
    }
}