using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxBackground : MonoBehaviour
{
    [Tooltip("How fast the background moves relative to the game speed. e.g. 0.5 for midground, 0.1 for far background.")]
    public float parallaxMultiplier = 0.5f;

    private float length;
    private Vector3 startPosition;
    private GameObject clone;

    void Start()
    {
        startPosition = transform.position;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        length = sr.bounds.size.x;

        // Create a clone for seamless looping
        clone = new GameObject(gameObject.name + "_Clone");
        clone.transform.SetParent(transform); // Make it a child
        
        // We divide by localScale.x because the child will inherit the parent's scale.
        // This ensures the clone is placed exactly at the right edge of the sprite.
        clone.transform.localPosition = new Vector3(length / transform.localScale.x, 0, 0); 
        clone.transform.localScale = Vector3.one;
        
        SpriteRenderer cloneSr = clone.AddComponent<SpriteRenderer>();
        cloneSr.sprite = sr.sprite;
        cloneSr.color = sr.color;
        cloneSr.sharedMaterial = sr.sharedMaterial; // <--- COPY THE MATERIAL SO IT GLOWS!
        cloneSr.sortingLayerID = sr.sortingLayerID;
        cloneSr.sortingOrder = sr.sortingOrder;
    }

    void Update()
    {
        // Get the current speed from the DifficultyManager (so the background speeds up as the game gets harder!)
        float gameSpeed = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentSpeed : 8f;
        
        // Apply our multiplier to create the "depth" effect
        float moveSpeed = gameSpeed * parallaxMultiplier;

        // Move the whole object left
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        // If the object has moved left completely past its own length, snap it back to create an infinite loop
        if (transform.position.x <= startPosition.x - length)
        {
            transform.position = new Vector3(transform.position.x + length, transform.position.y, transform.position.z);
        }
    }
}
