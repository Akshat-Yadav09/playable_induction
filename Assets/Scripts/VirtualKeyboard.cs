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

    [Tooltip("The layout of your full keyboard.")]
    public string fullLayout = "1234567890QWERTYUIOPASDFGHJKLZXCVBNM";

    [Tooltip("The layout for number-only inputs.")]
    public string numPadLayout = "1234567890";

    void Awake()
    {
        // We will generate the keyboard dynamically when it's opened.
    }

    private void GenerateKeyboard(string currentLayout, bool isNumPad)
    {
        // Clear existing keys
        foreach (Transform child in keyboardContainer)
        {
            if (child.gameObject != buttonTemplate)
            {
                Destroy(child.gameObject);
            }
        }

        // Hide the template button so it doesn't show up in the grid directly
        buttonTemplate.SetActive(false);

        if (isNumPad)
        {
            // Dial pad layout: 1-9
            foreach (char c in "123456789")
            {
                string keyChar = c.ToString();
                CreateKey(keyChar, () => TypeCharacter(keyChar));
            }
            
            // Bottom row for dial pad: DEL, 0, DONE
            CreateKey("DEL", Backspace);
            CreateKey("0", () => TypeCharacter("0"));
            CreateKey("DONE", CloseKeyboard);
        }
        else
        {
            // Generate standard keys
            foreach (char c in currentLayout)
            {
                string keyChar = c.ToString();
                CreateKey(keyChar, () => TypeCharacter(keyChar));
            }

            // Generate special keys
            CreateKey("DEL", Backspace);
            CreateKey("SPACE", () => TypeCharacter(" "));
            CreateKey("DONE", CloseKeyboard);
        }

        // Adjust layout grid constraints
        GridLayoutGroup grid = keyboardContainer.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            if (isNumPad)
            {
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 3;
            }
            else
            {
                grid.constraint = GridLayoutGroup.Constraint.Flexible;
            }
        }
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
            Color originalColor = btn.targetGraphic != null ? btn.targetGraphic.color : Color.white;

            btn.onClick.AddListener(() => {
                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine(FlashButton(btn, originalColor));
                }
                action();
            });
            
            // Optional haptic feedback when pressing a key
            btn.onClick.AddListener(() => VibrationManager.Vibrate(20));
        }
    }

    private System.Collections.IEnumerator FlashButton(Button btn, Color originalColor)
    {
        if (btn == null || btn.targetGraphic == null) yield break;
        
        btn.targetGraphic.color = Color.yellow;
        
        yield return new WaitForSeconds(0.2f);
        
        if (btn != null && btn.targetGraphic != null)
        {
            btn.targetGraphic.color = originalColor;
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
        
        bool isNumPad = (input.contentType == TMP_InputField.ContentType.IntegerNumber || input.contentType == TMP_InputField.ContentType.DecimalNumber);
        
        // Choose layout based on input field type
        string targetLayout = isNumPad ? numPadLayout : fullLayout;
            
        GenerateKeyboard(targetLayout, isNumPad);
        
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
