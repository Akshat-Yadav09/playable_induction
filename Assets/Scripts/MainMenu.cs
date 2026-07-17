using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Needed for TextMeshPro UI elements

public class MainMenu : MonoBehaviour
{
    [Tooltip("Drag your Username TMP_InputField here to save the player's name.")]
    public TMP_InputField usernameInput;

    [Tooltip("Drag your RegNo TMP_InputField here to save the player's registration number.")]
    public TMP_InputField regNoInput;

    [Tooltip("Drag a TextMeshPro UI element here to display error messages.")]
    public TMP_Text errorText;

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

    private bool isLoggingIn = false;

    public void PlayGame()
    {   
        if (isLoggingIn) return; // Prevent spamming the button

        if (errorText != null) errorText.text = ""; // Clear previous errors

        if(usernameInput.text == "" || regNoInput.text == "")
        {
            if (errorText != null) errorText.text = "Please enter your details";
            return;
        }

        // Validate registration number: must be between 200000 and 270000
        if (!int.TryParse(regNoInput.text, out int regNo) || regNo < 200000 || regNo > 270000)
        {
            if (errorText != null) errorText.text = "Please enter a valid registration number";
            return;
        }
        SaveUserDetails(); // Save details right before starting the game
        
        // Reset attempts and total score for the new session
        PlayerPrefs.SetInt("Attempts", 0);
        PlayerPrefs.SetFloat("TotalScore", 0f);
        PlayerPrefs.Save();
        
        // Ensure APIManager exists
        if (APIManager.Instance == null)
        {
            GameObject apiObj = new GameObject("APIManager");
            apiObj.AddComponent<APIManager>();
        }

        Debug.Log("Logging in to server...");
        isLoggingIn = true;
        
        // Disable play button here if needed
        APIManager.Instance.RegisterUser(usernameInput.text, regNoInput.text, (success, message) =>
        {
            if (success)
            {
                Debug.Log(message);
                Time.timeScale = 1f; // Ensure time is running
                SceneManager.LoadScene("SampleScene");
            }
            else
            {
                Debug.LogWarning("Failed to login: " + message);
                if (errorText != null) errorText.text = "Please enter a valid registration number";
                isLoggingIn = false; // Re-enable the button if login failed
            }
        });
    }
}
