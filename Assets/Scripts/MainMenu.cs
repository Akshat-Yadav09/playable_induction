using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
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
