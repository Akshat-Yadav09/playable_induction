using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float jumpForce = 14f;
    [Header("GD-Style Fall")]
    public float fallMultiplier = 3.5f;   // how much harder gravity pulls you down

    [Header("GD-Style Rotation")]
    [Tooltip("Drag the child sprite/visual object here. Leave empty to rotate the whole GameObject.")]
    public Transform visualTransform;
    public float rotationSpeed = 400f;    // degrees per second while airborne

    private Rigidbody2D rb;
    private GameManager gm;
    private PlayerTrail trail;
    private bool isGrounded = true;
    private bool wasGrounded = true;
    private bool jumpRequested = false;
    private float targetRotationZ = 0f;   // the next 90° snap target

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gm = FindAnyObjectByType<GameManager>();
        if (gm == null) Debug.LogWarning("PlayerController: GameManager not found in scene!");

        trail = GetComponent<PlayerTrail>();

        // Lock X so friction from obstacles can't push the player sideways.
        // Also freeze rigidbody rotation since we handle rotation visually.
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        // If no visual assigned, rotate this transform directly
        if (visualTransform == null)
            visualTransform = transform;
    }

    /// <summary>
    /// Call this from a UI Button's OnClick event for mobile jump.
    /// </summary>
    public void Jump()
    {
        jumpRequested = true;
    }

    void Update()
    {
        // Allow holding spacebar, clicking/holding anywhere on screen (mobile touch), 
        // OR using the UI button to jump.
        bool isHoldingJump = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space) || jumpRequested;

        if (isHoldingJump && isGrounded)
        {
            rb.linearVelocity = Vector2.up * jumpForce;
            isGrounded = false;
            if (trail != null) trail.SetGrounded(false);

            // Set the next 90° rotation target (clockwise = negative Z)
            targetRotationZ -= 90f;
        }
        
        // Reset the UI button request
        jumpRequested = false;

        // Smoothly rotate toward the target while airborne
        if (!isGrounded)
        {
            float currentZ = visualTransform.localEulerAngles.z;
            // Convert to signed angle for smooth Lerp
            if (currentZ > 180f) currentZ -= 360f;
            float newZ = Mathf.MoveTowards(currentZ, targetRotationZ, rotationSpeed * Time.deltaTime);
            visualTransform.localEulerAngles = new Vector3(0f, 0f, newZ);
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
            if (trail != null) trail.SetGrounded(false);
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
            if (trail != null) trail.SetGrounded(true);
            SnapRotation();
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
                if (trail != null) trail.SetGrounded(true);
                SnapRotation();
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

    /// <summary>
    /// Snap the visual to the nearest 90° on landing (just like GD).
    /// </summary>
    private void SnapRotation()
    {
        targetRotationZ = Mathf.Round(targetRotationZ / 90f) * 90f;
        visualTransform.localEulerAngles = new Vector3(0f, 0f, targetRotationZ);
    }
}