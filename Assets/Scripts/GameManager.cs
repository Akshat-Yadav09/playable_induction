using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("In-Game UI")]
    public TMP_Text scoreText;
    
    [Header("Game Over UI")]
    public GameObject gameOverMenu;
    [Tooltip("Drag a TextMeshPro UI element here to display the final Score on the Game Over screen.")]
    public TMP_Text gameOverScoreText;
    [Tooltip("Drag a TextMeshPro UI element here to display the High Score on the Game Over screen.")]
    public TMP_Text highScoreText;
    
    [Header("Settings")]
    [Tooltip("Points gained per second")]
    public float scoreRate = 10f;
    
    private float score = 0f;
    private int displayedScore = -1; // Track last displayed score to avoid redundant text updates
    private bool isGameOver = false;

    private int savedHighScore = 0;
    private bool hasPassedHighScore = false;
    private Color originalScoreColor;

    private const string HighScorePrefsKey = "HighScore";

    void Start()
    {
        // Ensure the game is running at normal speed when the scene starts
        Time.timeScale = 1f; 
        gameOverMenu.SetActive(false);

        // Make sure the in-game score is visible
        if (scoreText != null) 
        {
            scoreText.gameObject.SetActive(true);
            originalScoreColor = scoreText.color; // Save the original color
        }

        // Load the saved high score once at the start of the run
        savedHighScore = PlayerPrefs.GetInt(HighScorePrefsKey, 0);
    }

    void Update()
    {
        if (!isGameOver)
        {
            // Increase score over time using configurable rate
            score += Time.deltaTime * scoreRate; 

            // Feed score to difficulty system
            if (DifficultyManager.Instance != null)
                DifficultyManager.Instance.UpdateDifficulty(score);

            // Only update the UI text when the integer score actually changes
            int currentScore = Mathf.FloorToInt(score);
            if (currentScore != displayedScore)
            {
                displayedScore = currentScore;
                
                // Subway Surfers style: Check if we beat the high score during this run!
                if (savedHighScore > 0 && currentScore > savedHighScore)
                {
                    if (!hasPassedHighScore)
                    {
                        hasPassedHighScore = true;
                        if (scoreText != null)
                        {
                            scoreText.color = Color.yellow; // Make it pop visually!
                        }
                    }
                    if (scoreText != null) scoreText.text = "New High Score: " + currentScore.ToString();
                }
                else
                {
                    if (scoreText != null) scoreText.text = "Score: " + currentScore.ToString();
                }
            }
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return; // Prevent double-triggering
        
        isGameOver = true;
        
        int currentScore = Mathf.FloorToInt(score);

        // 1. Hide the top-left score counter
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(false);
            scoreText.color = originalScoreColor; // Reset color for the next time we play
        }

        // 2. Show the final Score in the center
        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "Score: " + currentScore.ToString();
        }

        // 3. Handle High Score Logic and display it in the center
        if (currentScore > savedHighScore)
        {
            PlayerPrefs.SetInt(HighScorePrefsKey, currentScore);
            PlayerPrefs.Save();
            
            if (highScoreText != null)
            {
                highScoreText.text = "New High Score: " + currentScore.ToString();
            }
        }
        else
        {
            if (highScoreText != null)
            {
                highScoreText.text = "High Score: " + savedHighScore.ToString();
            }
        }

        gameOverMenu.SetActive(true); // Show the menu
        Time.timeScale = 0f; // Freeze all movement and physics
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Explicitly restore before reload (defensive)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
