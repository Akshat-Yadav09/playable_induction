using UnityEngine;
using UnityEngine.UI; // Required for UI elements
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public GameObject gameOverMenu;
    
    private float score = 0f;
    private int displayedScore = -1; // Track last displayed score to avoid redundant text updates
    private bool isGameOver = false;

    void Start()
    {
        // Ensure the game is running at normal speed when the scene starts
        Time.timeScale = 1f; 
        gameOverMenu.SetActive(false);
    }

    void Update()
    {
        if (!isGameOver)
        {
            // Increase score over time (e.g., 10 points per second)
            score += Time.deltaTime * 10f; 

            // Only update the UI text when the integer score actually changes
            int currentScore = Mathf.FloorToInt(score);
            if (currentScore != displayedScore)
            {
                displayedScore = currentScore;
                scoreText.text = "Score: " + currentScore.ToString();
            }
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return; // Prevent double-triggering
        
        isGameOver = true;
        gameOverMenu.SetActive(true); // Show the menu
        Time.timeScale = 0f; // Freeze all movement and physics
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Explicitly restore before reload (defensive)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
