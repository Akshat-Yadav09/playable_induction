using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Needed for TextMeshPro UI elements

public class MainMenu : MonoBehaviour
{
    [Tooltip("Drag the Quit Button GameObject here. It will be hidden automatically on WebGL.")]
    public GameObject quitButton;

    [Tooltip("Drag your Username TMP_InputField here to save the player's name.")]
    public TMP_InputField usernameInput;

    private const string UsernamePrefsKey = "PlayerUsername";

    void Start()
    {
        // Load the saved username if the input field is assigned
        if (usernameInput != null)
        {
            usernameInput.text = PlayerPrefs.GetString(UsernamePrefsKey, "");
        }

        // Application.Quit() does nothing on WebGL, so hide the button entirely
        #if UNITY_WEBGL && !UNITY_EDITOR
        if (quitButton != null)
            quitButton.SetActive(false);
        #endif
    }

    /// <summary>
    /// Saves the current text from the input field into PlayerPrefs.
    /// </summary>
    public void SaveUsername()
    {
        if (usernameInput != null)
        {
            PlayerPrefs.SetString(UsernamePrefsKey, usernameInput.text);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Loads the gameplay scene. Hook this to the PlayGame button's OnClick event.
    /// </summary>
    public void PlayGame()
    {
        SaveUsername(); // Save the username right before starting the game
        Time.timeScale = 1f; // Ensure time is running
        SceneManager.LoadScene("SampleScene");
    }

    /// <summary>
    /// Quits the application. Hook this to a Quit button's OnClick event.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit requested");
        Application.Quit();
    }
}
