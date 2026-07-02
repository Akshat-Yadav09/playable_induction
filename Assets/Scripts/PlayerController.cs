using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float jumpForce = 14f;
    [Header("GD-Style Fall")]
    public float fallMultiplier = 3.5f;   // how much harder gravity pulls you down
    private Rigidbody2D rb;
    private GameManager gm;
    private bool isGrounded = true;
    private bool wasGrounded = true;

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

    void LateUpdate()
    {
        wasGrounded = isGrounded;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Reset isGrounded when leaving any surface (falling off edges)
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Obstacle"))
        {
            isGrounded = false;
        }
    }

    private void HandleCollision(Collision2D collision)
    {
        // 1. Hitting the ground is always safe
        if (collision.gameObject.CompareTag("Ground")) 
        { 
            if (!wasGrounded && CameraShake.Instance != null)
                CameraShake.Instance.Shake();
            isGrounded = true; 
        }
        
        // 2. Hitting an obstacle requires some math
        else if (collision.gameObject.CompareTag("Obstacle")) 
        { 
            // Check ALL contact normals — use the highest Y to avoid
            // false deaths when clipping a corner
            float maxNormalY = float.MinValue;
            for (int i = 0; i < collision.contactCount; i++)
            {
                maxNormalY = Mathf.Max(maxNormalY, collision.GetContact(i).normal.y);
            }

            // If the best normal is pointing UP (y > 0.5), we landed on top!
            if (maxNormalY > 0.5f) 
            {
                if (!wasGrounded && CameraShake.Instance != null)
                    CameraShake.Instance.Shake();
                isGrounded = true; // Safe to jump again
            }
            // Otherwise we hit the side — game over
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