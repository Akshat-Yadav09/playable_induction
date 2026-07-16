using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIFloat : MonoBehaviour
{
    [Header("Floating Settings")]
    [Tooltip("How fast the UI element bobs up and down")]
    public float floatSpeed = 2f;
    
    [Tooltip("How high it floats (in UI pixels)")]
    public float floatHeight = 10f; 
    
    [Tooltip("Randomizes the starting phase so multiple buttons don't float in perfect sync")]
    public bool randomizeOffset = true;

    private RectTransform rectTransform;
    private Vector2 startPos;
    private float timeOffset;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
        
        if (randomizeOffset)
        {
            // Pick a random spot in the sine wave so buttons bob independently
            timeOffset = Random.Range(0f, 2f * Mathf.PI);
        }
    }

    void Update()
    {
        Vector2 pos = startPos;
        
        // Use unscaledTime so the buttons keep floating even if the game is paused (Time.timeScale = 0)
        pos.y += Mathf.Sin(Time.unscaledTime * floatSpeed + timeOffset) * floatHeight;
        
        rectTransform.anchoredPosition = pos;
    }
}
