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
    [Tooltip("Drag a TextMeshPro UI element here to display the Average Score on the final Game Over screen.")]
    public TMP_Text averageScoreText;
    [Tooltip("Drag the Continue (Restart) button here to hide it after the 3rd attempt.")]
    public GameObject continueButton;
    
    [Header("Settings")]
    [Tooltip("Points gained per second")]
    public float scoreRate = 10f;
    
    private float score = 0f;
    private int displayedScore = -1; // Track last displayed score to avoid redundant text updates
    private bool isGameOver = false;

    private const string AttemptsPrefsKey = "Attempts";
    private const string TotalScorePrefsKey = "TotalScore";

    void Awake()
    {
        // Lock to 60 FPS to save battery on mobile and keep physics consistent across all devices
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        // Ensure the game is running at normal speed when the scene starts
        Time.timeScale = 1f; 
        gameOverMenu.SetActive(false);

        // Make sure the in-game score is visible
        if (scoreText != null) 
        {
            scoreText.gameObject.SetActive(true);
        }

        if (SoundManager.Instance != null) SoundManager.Instance.PlayGameplayMusic();
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
                if (scoreText != null) scoreText.text = "Score: " + currentScore.ToString();
            }
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return; // Prevent double-triggering
        
        isGameOver = true;

        if (SoundManager.Instance != null) SoundManager.Instance.PlayDeathSound();
        
        int currentScore = Mathf.FloorToInt(score);

        // 1. Hide the top-left score counter
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(false);
        }

        // Get attempts and total score
        int attempts = PlayerPrefs.GetInt(AttemptsPrefsKey, 0) + 1;
        float totalScore = PlayerPrefs.GetFloat(TotalScorePrefsKey, 0f) + currentScore;
        
        PlayerPrefs.SetInt(AttemptsPrefsKey, attempts);
        PlayerPrefs.SetFloat(TotalScorePrefsKey, totalScore);
        PlayerPrefs.SetInt("Score_Attempt_" + attempts, currentScore);
        PlayerPrefs.Save();

        // 2. Show the final Score in the center
        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "Attempt " + attempts + "/3 Score: " + currentScore.ToString();
        }

        // 3. Handle 3rd attempt logic
        if (attempts >= 3)
        {
            int avgScore = Mathf.FloorToInt(totalScore / 3f);

            // Only submit on exactly the 3rd attempt
            if (attempts == 3 && APIManager.Instance != null)
            {
                Debug.Log("Submitting average score to server: " + avgScore);
                APIManager.Instance.SubmitScore(avgScore, (success, message) => 
                {
                    if (success) Debug.Log("API: " + message);
                    else Debug.LogError("API Error: " + message);
                });
            }

            if (averageScoreText != null)
            {
                averageScoreText.text = "Attempt 3/3 Score: " + currentScore.ToString() + "\nFinal Average Score: " + avgScore.ToString() + "\n\n<color=red>Disclaimer: Further scores are not counted. You can continue if you want to.</color>";
                averageScoreText.gameObject.SetActive(true);
            }
            if (continueButton != null)
            {
                continueButton.SetActive(true);
                TMP_Text btnText = continueButton.GetComponentInChildren<TMP_Text>();
                if (btnText != null) btnText.text = "RESTART";
            }
        }
        else
        {
            if (averageScoreText != null)
            {
                averageScoreText.text = "Attempt " + attempts + "/3 Score: " + currentScore.ToString();
                averageScoreText.gameObject.SetActive(true); // Show score for attempt 1 and 2
            }
            if (continueButton != null)
            {
                continueButton.SetActive(true);
                TMP_Text btnText = continueButton.GetComponentInChildren<TMP_Text>();
                if (btnText != null) btnText.text = "RESTART";
            }
        }

        gameOverMenu.SetActive(true); // Show the menu
        Time.timeScale = 0f; // Freeze all movement and physics
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Explicitly restore before reload (defensive)
        
        // Just reload the current scene to restart, regardless of attempts
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
