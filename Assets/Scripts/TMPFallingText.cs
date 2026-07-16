using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TMPFallingText : MonoBehaviour
{
    private TMP_Text textComponent;

    [Header("Animation Settings")]
    [Tooltip("How high above their normal position the letters start falling from.")]
    public float dropHeight = 500f; 
    
    [Tooltip("How long it takes for a single letter to fall into place.")]
    public float dropDuration = 0.5f; 
    
    [Tooltip("The time delay before the next letter starts falling.")]
    public float delayBetweenCharacters = 0.1f; 

    [Tooltip("Delay before the animation starts. Helps prevent the animation from skipping due to game load lag.")]
    public float startupDelay = 0.5f;

    [Tooltip("If true, plays automatically. If false, waits for you to call StartAnimation().")]
    public bool playOnAwake = false; // Default to false so it waits for the click!

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (playOnAwake)
        {
            StartCoroutine(AnimateText());
        }
        else
        {
            // Push text up out of view while waiting for the player to click start
            PushTextUp();
        }
    }

    private void PushTextUp()
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vertIndex = charInfo.vertexIndex;

            Vector3[] source = cachedMeshInfo[matIndex].vertices;
            Vector3[] dest = textInfo.meshInfo[matIndex].vertices;

            Vector3 offset = new Vector3(0, dropHeight, 0);
            dest[vertIndex + 0] = source[vertIndex + 0] + offset;
            dest[vertIndex + 1] = source[vertIndex + 1] + offset;
            dest[vertIndex + 2] = source[vertIndex + 2] + offset;
            dest[vertIndex + 3] = source[vertIndex + 3] + offset;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    public void StartAnimation()
    {
        StartCoroutine(AnimateText());
    }

    IEnumerator AnimateText()
    {
        // Wait a bit to let the game finish loading so the animation doesn't skip
        if (startupDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(startupDelay);
        }

        // Force the text mesh to update so we have accurate character data
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        // Cache the original vertex data so we know where the letters are supposed to end up
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
        int characterCount = textInfo.characterCount;
        
        float[] characterProgress = new float[characterCount];
        bool isAnimating = true;
        float timePassed = 0f;

        while (isAnimating)
        {
            timePassed += Time.unscaledDeltaTime; // Use unscaled time so it works even if game is paused
            isAnimating = false;

            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                
                // Skip invisible characters (spaces, etc.)
                if (!charInfo.isVisible) continue;

                // Calculate when this specific character should start dropping
                float startTime = i * delayBetweenCharacters;
                
                if (timePassed >= startTime)
                {
                    if (characterProgress[i] < 1f)
                    {
                        characterProgress[i] += Time.unscaledDeltaTime / dropDuration;
                        characterProgress[i] = Mathf.Clamp01(characterProgress[i]);
                        isAnimating = true;
                    }
                }
                else
                {
                    isAnimating = true; // Still waiting for this character to start
                }

                // Get the vertices for this character
                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                // Smooth cubic ease-out calculation (slows down as it hits the bottom)
                float t = characterProgress[i];
                float easeOut = 1f - Mathf.Pow(1f - t, 3f); 
                
                // If it hasn't started yet, keep it hovering at dropHeight. Otherwise, ease it down to 0.
                float currentOffset = (timePassed < startTime) ? dropHeight : Mathf.Lerp(dropHeight, 0f, easeOut);
                Vector3 offset = new Vector3(0, currentOffset, 0);

                // Apply the offset to all 4 corners (vertices) of the letter
                destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] + offset;
                destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] + offset;
                destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] + offset;
                destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] + offset;
            }

            // Push the changed vertices back to the mesh to actually render them on screen
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return null; // Wait for the next frame
        }
    }
}
