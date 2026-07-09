using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Shake Settings")]
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.15f;
    [Tooltip("How quickly the shake fades out (higher = faster fade)")]
    public float dampingSpeed = 8f;

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    void OnDisable()
    {
        // Safety net: reset position if disabled or frozen mid-shake
        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        originalPosition = transform.localPosition;
    }

    /// <summary>
    /// Trigger a camera shake with the default settings.
    /// </summary>
    public void Shake()
    {
        Shake(shakeDuration, shakeMagnitude);
    }

    /// <summary>
    /// Trigger a camera shake with custom duration and magnitude.
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0f);

            elapsed += Time.unscaledDeltaTime;

            // Fade out the magnitude over time
            magnitude = Mathf.Lerp(magnitude, 0f, dampingSpeed * Time.unscaledDeltaTime);

            yield return null;
        }

        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
}
