using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VirtualKeyboard : MonoBehaviour
{
    [Header("Input Target")]
    [Tooltip("The InputField that is currently being typed into.")]
    public TMP_InputField activeInputField;
    public int maxCharacters = 15;

    [Header("Auto-Generate Settings")]
    [Tooltip("Drag the panel containing a Grid Layout Group here.")]
    public Transform keyboardContainer;
    
    [Tooltip("Create ONE button inside the container, style it, and drag it here. The script will duplicate it.")]
    public GameObject buttonTemplate;

    [Tooltip("The layout of your keys.")]
    public string layout = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private bool hasGenerated = false;

    void Awake()
    {
        if (keyboardContainer != null && buttonTemplate != null && !hasGenerated)
        {
            GenerateKeyboard();
        }
    }

    private void GenerateKeyboard()
    {
        hasGenerated = true;

        // Hide the template button so it doesn't show up in the grid directly
        buttonTemplate.SetActive(false);

        // Generate standard keys
        foreach (char c in layout)
        {
            string keyChar = c.ToString();
            CreateKey(keyChar, () => TypeCharacter(keyChar));
        }

        // Generate special keys
        CreateKey("DEL", Backspace);
        CreateKey("SPACE", () => TypeCharacter(" "));
        CreateKey("DONE", CloseKeyboard); // Added a Done button to close it!
    }

    private void CreateKey(string text, UnityEngine.Events.UnityAction action)
    {
        GameObject newBtnObj = Instantiate(buttonTemplate, keyboardContainer);
        newBtnObj.SetActive(true);
        newBtnObj.name = "Key_" + text;

        // Set the text on the button
        TMP_Text btnText = newBtnObj.GetComponentInChildren<TMP_Text>();
        if (btnText != null)
        {
            btnText.text = text;
            btnText.enableWordWrapping = false; // Prevent vertical stacking
            btnText.enableAutoSizing = true;    // Shrink text if needed to fit "SPACE"
            btnText.fontSizeMin = 10;
            btnText.fontSizeMax = 60;
            btnText.alignment = TextAlignmentOptions.Center;
        }

        // Bind the click event
        Button btn = newBtnObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(action);
            
            // Optional haptic feedback when pressing a key
            btn.onClick.AddListener(() => VibrationManager.Vibrate(20));
        }
    }

    // --- KEY ACTIONS ---

    public void TypeCharacter(string character)
    {
        if (activeInputField == null) return;
        
        if (activeInputField.text.Length < maxCharacters)
        {
            activeInputField.text += character;
        }
    }

    public void Backspace()
    {
        if (activeInputField == null || string.IsNullOrEmpty(activeInputField.text)) return;
        
        activeInputField.text = activeInputField.text.Substring(0, activeInputField.text.Length - 1);
    }

    // --- SELECTION ---

    /// <summary>
    /// Opens the keyboard and assigns the target input field simultaneously.
    /// </summary>
    public void OpenKeyboardFor(TMP_InputField input)
    {
        activeInputField = input;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Closes the keyboard.
    /// </summary>
    public void CloseKeyboard()
    {
        gameObject.SetActive(false);
    }
}
