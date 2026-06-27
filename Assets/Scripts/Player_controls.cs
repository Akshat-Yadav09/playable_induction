using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float jumpForce = 14f;
    [Header("GD-Style Fall")]
    public float fallMultiplier = 3.5f;   // how much harder gravity pulls you down
    private Rigidbody2D rb;
    private GameManager gm;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gm = FindAnyObjectByType<GameManager>();
        if (gm == null) Debug.LogWarning("PlayerController: GameManager not found in scene!");
    }

    void Update()
    {
        // Jump with Spacebar or Left Mouse Click
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && isGrounded)
        {
            rb.linearVelocity = Vector2.up * jumpForce;
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        // When falling, crank up gravity so the drop feels snappy like GD
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collision2D collision)
    {
        // 1. Hitting the ground is always safe
        if (collision.gameObject.CompareTag("Ground")) 
        { 
            isGrounded = true; 
        }
        
        // 2. Hitting an obstacle requires some math
        else if (collision.gameObject.CompareTag("Obstacle")) 
        { 
            // Get the direction of the collision
            Vector2 contactNormal = collision.GetContact(0).normal;

            // If the normal is pointing UP (y is close to 1), we landed on top!
            if (contactNormal.y > 0.5f) 
            {
                isGrounded = true; // Safe to jump again
            }
            // If the normal is pointing left or right, we hit the side!
            else 
            {
                if (gm != null) gm.TriggerGameOver();
            }
        }

        // 3. Hitting a SPIKE (Instant death from any angle)
        else if (collision.gameObject.CompareTag("Spike"))
        {
            if (gm != null) gm.TriggerGameOver();
        }
    }
}