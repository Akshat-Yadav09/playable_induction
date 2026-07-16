using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public float jumpForce = 14f;
    [Header("GD-Style Fall")]
    public float fallMultiplier = 3.5f;   // how much harder gravity pulls you down

    [Header("GD-Style Rotation")]
    [Tooltip("Drag the child sprite/visual object here. Leave empty to rotate the whole GameObject.")]
    public Transform visualTransform;
    [Tooltip("Fallback rotation speed. In practice, speed is auto-calculated from jump physics.")]
    public float rotationSpeed = 400f;

    private Rigidbody2D rb;
    private GameManager gm;
    private PlayerTrail trail;
    private PlayerExplosion explosion;
    private bool isGrounded = true;
    private bool wasGrounded = true;
    private bool jumpRequested = false;
    private bool isHoldingUIJump = false;
    private float targetRotationZ = 0f;   // the next snap target
    private float currentRotationZ = 0f;
    private float dynamicRotationSpeed = 400f; // auto-calculated per jump from physics
    private float collisionGraceTimer = 0f; // blocks ghost collisions after landing
    private const float COLLISION_GRACE_DURATION = 0.05f; // 50ms grace window

    void Awake()
    {
        // We ALWAYS need a CHILD transform to rotate, because the root Rigidbody2D
        // has FreezeRotation which blocks localEulerAngles on the root.
        
        bool needsAutoDetect = (visualTransform == null || visualTransform == transform);
        
        if (!needsAutoDetect) return;

        // Auto-detect: find the first SpriteRenderer (preferring children over root)
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        SpriteRenderer childSR = null;
        SpriteRenderer rootSR = null;
        
        foreach (SpriteRenderer sr in renderers)
        {
            if (sr.gameObject == gameObject)
                rootSR = sr;
            else if (childSR == null)
                childSR = sr;
        }

        if (childSR != null)
        {
            visualTransform = childSR.transform;
        }
        else if (rootSR != null)
        {
            // SR is only on the root — migrate it to a new child
            GameObject child = new GameObject("PlayerVisual");
            child.transform.SetParent(transform, false);
            SpriteRenderer newSR = child.AddComponent<SpriteRenderer>();
            newSR.sprite = rootSR.sprite;
            newSR.color = rootSR.color;
            newSR.material = rootSR.material;
            newSR.sortingLayerID = rootSR.sortingLayerID;
            newSR.sortingOrder = rootSR.sortingOrder;
            DestroyImmediate(rootSR);
            visualTransform = child.transform;
        }
        else
        {
            // No SR at all — create an empty child
            GameObject child = new GameObject("PlayerVisual");
            child.transform.SetParent(transform, false);
            visualTransform = child.transform;
        }
    }

    [Header("Screen Positioning")]
    [Tooltip("If true, automatically places the player at a safe distance from the left edge of the screen.")]
    public bool autoPositionX = true;
    [Tooltip("Distance from the left edge in world units.")]
    public float distanceFromLeftEdge = 2.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gm = FindAnyObjectByType<GameManager>();
        if (gm == null) Debug.LogWarning("PlayerController: GameManager not found in scene!");

        trail = GetComponent<PlayerTrail>();
        explosion = GetComponent<PlayerExplosion>();

        // Dynamically adjust player X position based on aspect ratio
        if (autoPositionX && Camera.main != null)
        {
            float leftEdgeX = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
            transform.position = new Vector3(leftEdgeX + distanceFromLeftEdge, transform.position.y, transform.position.z);
        }

        // Lock X so friction from obstacles can't push the player sideways.
        // Also freeze rigidbody rotation since we handle rotation visually.
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        
        // Continuous collision detection prevents tunneling
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    /// <summary>
    /// Call this from a UI Button's OnClick event for mobile jump.
    /// </summary>
    public void Jump()
    {
        jumpRequested = true;
    }

    /// <summary>
    /// Call this from a UI EventTrigger (PointerDown) for continuous jumping.
    /// </summary>
    public void PointerDownJump()
    {
        isHoldingUIJump = true;
    }

    /// <summary>
    /// Call this from a UI EventTrigger (PointerUp) to stop continuous jumping.
    /// </summary>
    public void PointerUpJump()
    {
        isHoldingUIJump = false;
    }

    void Update()
    {
        if (collisionGraceTimer > 0f)
            collisionGraceTimer -= Time.deltaTime;

        // Only allow jump via UI button or explicit Jump() call
        bool shouldJump = isHoldingUIJump || jumpRequested;

        if (shouldJump && isGrounded)
        {
            VibrationManager.Vibrate(30); // Tiny haptic tick on jump
            
            rb.linearVelocity = Vector2.up * jumpForce;
            isGrounded = false;
            if (trail != null) trail.SetGrounded(false);

            // GD-style: 180° rotation per jump, speed synced to arc duration
            float jumpDuration = CalculateJumpArcDuration(jumpForce);
            dynamicRotationSpeed = 180f / Mathf.Max(jumpDuration, 0.1f);
            targetRotationZ -= 180f;
        }

        jumpRequested = false;

        // Smoothly rotate toward the target while airborne (speed synced to jump arc)
        if (!isGrounded && visualTransform != null)
        {
            currentRotationZ = Mathf.MoveTowards(currentRotationZ, targetRotationZ, dynamicRotationSpeed * Time.deltaTime);
            Vector3 currentEuler = visualTransform.localEulerAngles;
            visualTransform.localEulerAngles = new Vector3(currentEuler.x, currentEuler.y, currentRotationZ);
        }
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.y < 0 && fallMultiplier > 1f)
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
        // Skip ghost collisions during the grace period after a landing
        if (collisionGraceTimer > 0f && !collision.gameObject.CompareTag("Ground") && !collision.gameObject.CompareTag("Spike"))
            return;

        HandleCollision(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Obstacle"))
        {
            // Only un-ground if we aren't currently touching another ground/obstacle block
            if (!IsTouchingGroundOrObstacle())
            {
                isGrounded = false;
                if (trail != null) trail.SetGrounded(false);
            }
        }
    }

    private bool IsTouchingGroundOrObstacle()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return false;
        
        ContactPoint2D[] contacts = new ContactPoint2D[10];
        int count = col.GetContacts(contacts);
        for (int i = 0; i < count; i++)
        {
            GameObject obj = contacts[i].collider.gameObject;
            if (obj.CompareTag("Ground") || obj.CompareTag("Obstacle"))
            {
                // Ensure we are resting on top of it, not just grazing a wall
                if (contacts[i].normal.y > 0.1f) return true;
            }
        }
        return false;
    }

    private void HandleCollision(Collision2D collision)
    {
        // 1. Hitting the ground is always safe
        if (collision.gameObject.CompareTag("Ground")) 
        { 
            if (!wasGrounded)
            {
                if (CameraShake.Instance != null) CameraShake.Instance.Shake();
                PlayLandingSplash();
            }
            isGrounded = true;
            if (trail != null) trail.SetGrounded(true);
            SnapRotation();
        }
        
        // 2. Hitting a SPIKE (Instant death from any angle)
        else if (collision.gameObject.CompareTag("Spike"))
        {
            Die();
        }
        // 3. Hitting ANYTHING ELSE (Obstacles)
        else 
        { 
            Collider2D playerCol = GetComponent<Collider2D>();
            float playerBottom = playerCol != null ? playerCol.bounds.min.y : transform.position.y;
            float obsTop = collision.collider.bounds.max.y;

            // --- GHOST COLLISION SEAM FIX ---
            // If the player hits the vertical seam between two blocks while already sliding, 
            // the hit point will be near their feet. We ignore it.
            if (isGrounded && playerBottom >= obsTop - 0.15f)
            {
                return;
            }

            float maxNormalY = float.MinValue;
            for (int i = 0; i < collision.contactCount; i++)
            {
                maxNormalY = Mathf.Max(maxNormalY, collision.GetContact(i).normal.y);
            }

            // STRICTER CHECK
            bool isSafelyOnTop = maxNormalY > 0.5f && (playerBottom >= obsTop - 0.2f);

            if (isSafelyOnTop) 
            {
                if (!wasGrounded)
                {
                    if (CameraShake.Instance != null) CameraShake.Instance.Shake();
                    PlayLandingSplash();
                }
                isGrounded = true; 
                if (trail != null) trail.SetGrounded(true);
                SnapRotation();
            }
            else 
            {
                Die();
            }
        }
    }

    private void Die()
    {
        if (trail != null) trail.ClearOnDeath();
        if (explosion != null) explosion.Explode();
        if (gm != null) gm.TriggerGameOver();
    }

    private void SnapRotation()
    {
        targetRotationZ = Mathf.Round(targetRotationZ / 90f) * 90f;

        // Normalize
        targetRotationZ = targetRotationZ % 360f;
        currentRotationZ = targetRotationZ;
        
        if (visualTransform != null)
        {
            Vector3 currentEuler = visualTransform.localEulerAngles;
            visualTransform.localEulerAngles = new Vector3(currentEuler.x, currentEuler.y, targetRotationZ);
        }

        collisionGraceTimer = COLLISION_GRACE_DURATION;
    }

    private float CalculateJumpArcDuration(float velocity)
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y);
        if (gravity <= 0.01f) gravity = 9.81f;

        float gravityScale = (rb != null) ? rb.gravityScale : 1f;
        if (gravityScale <= 0.01f) gravityScale = 1f;
        float effectiveGravityUp = gravity * gravityScale;

        float timeUp = velocity / effectiveGravityUp;
        float peakHeight = 0.5f * effectiveGravityUp * timeUp * timeUp;

        float safeFallMult = Mathf.Max(fallMultiplier, 1f);
        float effectiveGravityDown = effectiveGravityUp * safeFallMult;
        float timeDown = Mathf.Sqrt(Mathf.Max(0f, 2f * peakHeight / effectiveGravityDown));

        return timeUp + timeDown;
    }

    private ParticleSystem landingParticles;

    private void PlayLandingSplash()
    {
        if (landingParticles == null)
        {
            GameObject pObj = new GameObject("LandingParticleSystem");
            pObj.transform.SetParent(transform, false);
            // Position at the bottom of the player
            Collider2D col = GetComponent<Collider2D>();
            float bottomY = col != null ? col.bounds.min.y - transform.position.y : -0.5f;
            pObj.transform.localPosition = new Vector3(0, bottomY, 0);

            landingParticles = pObj.AddComponent<ParticleSystem>();
            landingParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = landingParticles.main;
            main.duration = 1f;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.35f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            main.gravityModifier = 1.5f; // Pull them down
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = landingParticles.emission;
            emission.rateOverTime = 0; // Only burst manually

            var shape = landingParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(0.5f, 0.1f, 1f);
            shape.rotation = new Vector3(-90f, 0f, 0f); // Point upwards so they shoot up and fall

            var colorOverLife = landingParticles.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLife.color = grad;

            var renderer = landingParticles.GetComponent<ParticleSystemRenderer>();
            // Use default material to avoid WebGL shader stripping
        }

        // Inherit player color dynamically
        SpriteRenderer playerSR = visualTransform != null ? visualTransform.GetComponent<SpriteRenderer>() : null;
        if (playerSR == null) playerSR = GetComponent<SpriteRenderer>();
        Color playerColor = playerSR != null ? playerSR.color : Color.white;

        var mainModule = landingParticles.main;
        mainModule.startColor = playerColor;

        landingParticles.Emit(Random.Range(4, 7));
    }


    private Sprite _squareSprite;
    private Sprite GetOrCreateSquareSprite()
    {
        SpriteRenderer playerSR = visualTransform != null ? visualTransform.GetComponent<SpriteRenderer>() : null;
        if (playerSR == null) playerSR = GetComponent<SpriteRenderer>();
        if (playerSR != null && playerSR.sprite != null) return playerSR.sprite;

        if (_squareSprite == null)
        {
            Texture2D tex = new Texture2D(4, 4);
            Color[] pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            _squareSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }
        return _squareSprite;
    }
}