using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Needed for TextMeshPro UI elements

public class MainMenu : MonoBehaviour
{
    [Tooltip("Drag your Username TMP_InputField here to save the player's name.")]
    public TMP_InputField usernameInput;

    [Tooltip("Drag your RegNo TMP_InputField here to save the player's registration number.")]
    public TMP_InputField regNoInput;

    private const string UsernamePrefsKey = "PlayerUsername";
    private const string RegNoPrefsKey = "PlayerRegNo";

    void Start()
    {
        // Load the saved username if the input field is assigned
        if (usernameInput != null)
        {
            usernameInput.text = PlayerPrefs.GetString(UsernamePrefsKey, "");
        }
        
        if (regNoInput != null)
        {
            regNoInput.text = PlayerPrefs.GetString(RegNoPrefsKey, "");
        }
    }

    /// <summary>
    /// Saves the current text from the input fields into PlayerPrefs.
    /// </summary>
    public void SaveUserDetails()
    {
        if (usernameInput != null)
        {
            PlayerPrefs.SetString(UsernamePrefsKey, usernameInput.text);
        }
        if (regNoInput != null)
        {
            PlayerPrefs.SetString(RegNoPrefsKey, regNoInput.text);
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the gameplay scene. Hook this to the PlayGame button's OnClick event.
    /// </summary>
    public void PlayGame()
    {
        SaveUserDetails(); // Save details right before starting the game
        
        // Reset attempts and total score for the new session
        PlayerPrefs.SetInt("Attempts", 0);
        PlayerPrefs.SetFloat("TotalScore", 0f);
        PlayerPrefs.Save();
        
        Time.timeScale = 1f; // Ensure time is running
        SceneManager.LoadScene("SampleScene");
    }
}
