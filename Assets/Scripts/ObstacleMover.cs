using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public float speed = 8f;
    public float deadZone = -15f; // The X coordinate where it disappears

    void Update()
    {
        // Move left at a constant speed
        transform.Translate(Vector3.left * (speed * Time.deltaTime));

        // If it goes past the player and off-screen, deactivate it
        if (transform.position.x < deadZone)
        {
            gameObject.SetActive(false);
        }
    }
}