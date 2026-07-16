using UnityEngine;

public class MenuCubeRotation : MonoBehaviour
{
    [Header("Rotation")]
    public Vector3 rotationSpeed = new Vector3(20f, 40f, 15f);

    [Header("Floating")]
    public float floatHeight = 0.25f;
    public float floatSpeed = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;

        // Auto-fix: If you reused the Player prefab for the menu, its physics and scripts will fight the menu rotation.
        // We strip them out here so it spins freely!
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) Destroy(rb);
        
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) Destroy(pc);
    }

    void Update()
    {
        // Rotate
        transform.Rotate(rotationSpeed * Time.unscaledDeltaTime);

        // Float up and down
        Vector3 pos = startPos;
        pos.y += Mathf.Sin(Time.unscaledTime * floatSpeed) * floatHeight;
        transform.position = pos;
    }
}
