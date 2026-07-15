using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the Player GameObject.
/// On death, hides the player sprite and spawns a burst of square fragments.
/// Uses unscaled time so it works even after Time.timeScale = 0.
/// </summary>
public class PlayerExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [Tooltip("How many square pieces to spawn")]
    public int fragmentCount = 12;
    [Tooltip("How fast fragments fly outward")]
    public float explosionForce = 6f;
    [Tooltip("How long before fragments disappear")]
    public float fragmentLifetime = 0.8f;
    [Tooltip("Size of each fragment relative to the player")]
    public float fragmentSize = 0.3f;
    [Tooltip("If true, uses the player's SpriteRenderer color automatically")]
    public bool usePlayerColor = true;
    [Tooltip("Fallback color if not using player color")]
    public Color fragmentColor = Color.white;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Awake()
    {
        // Search children too, since the visual may be on a child object
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public void Explode()
    {
        Color color = (usePlayerColor && spriteRenderer != null)
            ? spriteRenderer.color
            : fragmentColor;

        // Hide the player sprite and disable collider
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (col != null) col.enabled = false;

        // Spawn fragments
        for (int i = 0; i < fragmentCount; i++)
        {
            // Spread fragments evenly in a circle + some randomness
            float angle = (360f / fragmentCount) * i + Random.Range(-15f, 15f);
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            float speed = Random.Range(explosionForce * 0.5f, explosionForce);

            GameObject frag = CreateFragment(color);
            frag.transform.position = (Vector2)transform.position + dir * 0.1f;
            frag.transform.localScale = Vector3.one * fragmentSize * Random.Range(0.7f, 1.3f);

            float spin = Random.Range(-360f, 360f);
            StartCoroutine(AnimateFragment(frag, dir * speed, spin, fragmentLifetime));
        }
    }

    private IEnumerator AnimateFragment(GameObject obj, Vector2 velocity, float spin, float duration)
    {
        if (obj == null) yield break;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Color startColor = sr != null ? sr.color : Color.white;
        float gravity = 9.8f;
        float elapsed = 0f;

        while (elapsed < duration && obj != null)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;

            // Apply manual gravity
            velocity.y -= gravity * dt;

            // Move
            obj.transform.position += (Vector3)(velocity * dt);

            // Spin
            obj.transform.Rotate(0f, 0f, spin * dt);

            // Fade out
            if (sr != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            yield return null;
        }

        if (obj != null) Destroy(obj);
    }

    private GameObject CreateFragment(Color color)
    {
        GameObject frag = new GameObject("ExplosionFragment");

        SpriteRenderer sr = frag.AddComponent<SpriteRenderer>();
        sr.sprite = GetOrCreateSquareSprite();
        sr.color = color;
        sr.sortingOrder = 10;

        return frag;
    }

    private Sprite _squareSprite;
    private Sprite GetOrCreateSquareSprite()
    {
        // Reuse player sprite if available (looks better)
        if (spriteRenderer != null && spriteRenderer.sprite != null)
            return spriteRenderer.sprite;

        // Otherwise generate a simple white square
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
