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

        // If it goes past the player and off-screen, recycle it
        if (transform.position.x < deadZone)
        {
            Recycle();
        }
    }

    private void Recycle()
    {
        if (ObjectPool.Instance != null)
        {
            // Return all children (blocks/spikes) to their respective pools
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.name.StartsWith("Block"))
                    ObjectPool.Instance.ReturnToPool("Block", child.gameObject);
                else if (child.name.StartsWith("Spike"))
                    ObjectPool.Instance.ReturnToPool("Spike", child.gameObject);
                else
                    Destroy(child.gameObject); // Fallback for unknown objects
            }

            // Return the container itself to the pool
            ObjectPool.Instance.ReturnToPool("PatternContainer", gameObject);
        }
        else
        {
            // Fallback if no ObjectPool is in the scene
            Destroy(gameObject);
        }
    }
}