using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RGBMaterials : MonoBehaviour
{
    [Header("RGB Settings")]
    [Tooltip("How fast the colors cycle")]
    public float cycleSpeed = 0.5f;
    [Tooltip("Saturation (0-1)")]
    public float saturation = 1f;
    [Tooltip("Value/Brightness (0-1)")]
    public float value = 1f;

    private Renderer rend;
    private Material mat;
    private float hue = 0f;

    void Start()
    {
        rend = GetComponent<Renderer>();
        
        // Generate a custom Unlit material at runtime to guarantee colors work regardless of lighting or 2D/3D project settings
        Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
        
        // Fallback for standard 3D if URP isn't installed
        if (unlitShader == null) unlitShader = Shader.Find("Unlit/Color"); 
        
        if (unlitShader != null)
        {
            mat = new Material(unlitShader);
            rend.material = mat;
        }
        else
        {
            mat = rend.material; // Final fallback
            mat.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        // Shift hue over time
        hue += cycleSpeed * Time.deltaTime;
        if (hue > 1f) hue -= 1f;

        // Convert HSV to RGB
        Color rgbColor = Color.HSVToRGB(hue, saturation, value);
        
        // Apply to the material universally
        if (mat != null)
        {
            mat.color = rgbColor;

            // Force update all common shader color properties
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", rgbColor);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", rgbColor);
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", rgbColor * 1.5f);
            if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", rgbColor);
        }
        
        if (rend is SpriteRenderer sr)
        {
            sr.color = rgbColor;
        }
    }
    
    void OnDestroy()
    {
        if (mat != null)
        {
            Destroy(mat); // Cleanup material instance
        }
    }
}
