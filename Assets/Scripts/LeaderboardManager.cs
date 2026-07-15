using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text leaderboardText;

    private const string AttemptsPrefsKey = "Attempts";
    private const string TotalScorePrefsKey = "TotalScore";

    void Start()
    {
        // 1. Calculate the player's recent average score
        float totalScore = PlayerPrefs.GetFloat(TotalScorePrefsKey, 0f);
        int avgScore = Mathf.FloorToInt(totalScore / 3f);

        // 2. Clear the attempts so they can play again!
        PlayerPrefs.SetInt(AttemptsPrefsKey, 0);
        PlayerPrefs.SetFloat(TotalScorePrefsKey, 0f);
        PlayerPrefs.Save();

        // 3. Start Polling the Leaderboard
        if (leaderboardText != null)
        {
            leaderboardText.text = "Loading Leaderboard...";
            InvokeRepeating(nameof(RefreshLeaderboard), 0f, 5f);
        }
    }

    private void RefreshLeaderboard()
    {
        if (APIManager.Instance != null)
        {
            APIManager.Instance.GetLeaderboard((success, topPlayers) =>
            {
                if (success && topPlayers != null && leaderboardText != null)
                {
                    string lbText = "--- LEADERBOARD ---\n\n";
                    for (int i = 0; i < topPlayers.Length; i++)
                    {
                        lbText += $"{i + 1}. {topPlayers[i].name}: {topPlayers[i].score}\n";
                    }
                    if (topPlayers.Length == 0)
                    {
                        lbText += "No scores yet!\n";
                    }
                    leaderboardText.text = lbText;
                }
                else if (leaderboardText != null && leaderboardText.text == "Loading Leaderboard...")
                {
                    leaderboardText.text = "Failed to load leaderboard.\nCheck internet connection.";
                }
            });
        }
    }

    /// <summary>
    /// Call this from a "Main Menu" or "Play Again" button in the Leaderboard Scene.
    /// </summary>
    public void GoToMainMenu()
    {
        // Make sure "SampleScene" matches your actual gameplay/menu scene name
        SceneManager.LoadScene("SampleScene"); 
    }
}
