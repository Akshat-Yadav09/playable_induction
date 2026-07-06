using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Drag the Quit Button GameObject here. It will be hidden automatically on WebGL.")]
    public GameObject quitButton;

    void Start()
    {
        // Application.Quit() does nothing on WebGL, so hide the button entirely
        #if UNITY_WEBGL && !UNITY_EDITOR
        if (quitButton != null)
            quitButton.SetActive(false);
        #endif
    }

    /// <summary>
    /// Loads the gameplay scene. Hook this to the PlayGame button's OnClick event.
    /// </summary>
    public void PlayGame()
    {
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
