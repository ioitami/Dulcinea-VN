
using Ink.Runtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO: NextIcon position/size scale based on screen resolution

public class DialogueManager : MonoBehaviour
{
    [Header("UI Components - Basic")]
    public TextMeshProUGUI dialogueText;
    public Image nextIcon; // sprite icon for "continue"

    [Header("UI Components - Choices")]
    [SerializeField]
    private VerticalLayoutGroup choiceButtonContainer;
    [SerializeField]
    private Button choiceButtonPrefab;

    [Header("Ink")]
    public TextAsset inkJSONAsset;
    private Story story;

    private string[] currentSentenceParts;
    private int currentPartIndex = 0;
    private Coroutine typingCoroutine;
    private Coroutine iconBlinkCoroutine;


    [Header("Typing Settings")]
    public float typingSpeed = 0.03f; // delay per character
    private bool isTyping = false;
    private string currentTypingPart = ""; // <- track the current segment being typed
    private bool appendMode = false;      // <- track if we're appending

    [Header("Next Icon Blink Settings")]
    public float blinkSpeed = 0.5f; // seconds between fade

    [Header("Ink Configuration")]
    [Tooltip("Delimiter used in Ink files to split a line into segments (e.g., '||').")]
    private const string SEGMENT_DELIMITER = "(SPLIT)";

    [Header("CharacterManager")]
    public CharacterManager characterManager;

    [Header("Story Variables")]
    public string playerName;
    public int healthPoints;

    public string PlayerName
    {
        get => playerName;
        private set
        {
            Debug.Log($"Updating RelationshipStrength value. Old value: {playerName}, new value: {value}");
            playerName = value;
        }
    }

    public int HealthPoints
    {
        get => healthPoints;
        private set
        {
            Debug.Log($"Updating HealthPoints value. Old value: {healthPoints}, new value: {value}");
            healthPoints = value;
        }
    }

    private void Start()
    {
        story = new Story(inkJSONAsset.text);

        StartStory();
        InitializeVariables();
    }

    void Update()
    {
        // Only update the icon position if sentence finished
        if (isTyping == false && nextIcon != null && dialogueText != null)
        {
            UpdateNextIconPosition();
        }
    }

    private void StartStory()
    {
        story = new Story(inkJSONAsset.text);

        // Link unity functions to Story in Ink

        story.BindExternalFunction("ShowCharacter", (string name, string mood, int positionID)
             => characterManager.ShowCharacter(name, mood, positionID));

        story.BindExternalFunction("ChangeTypingSpeed", (float speed)
             => ChangeTypingSpeed(speed));

        //story.BindExternalFunction("ChangeMood", (string name, string mood)
        //     => _characterManager.ChangeMood(name, mood));

        DisplayNextLine();
    }

    // ====================================================================================================================
    // Initialize after StartStory, will update whenever changes are made to these values. Can add functions to trigger on change here.
    // To update variable from Unity to Ink, use: story.variablesState["variableName"] = newValue;
    // ====================================================================================================================
    private void InitializeVariables()
    {
        PlayerName = (string)story.variablesState["PlayerName"];
        HealthPoints = (int)story.variablesState["HealthPoints"];

        story.ObserveVariable("PlayerName", (arg, value) =>
        {
            playerName = (string)value;
        });

        story.ObserveVariable("HealthPoints", (arg, value) =>
        {
            HealthPoints = (int)value;
        });

       
    }

    // ====================================================================================================================
    // Add functions to edit variables here from Unity to Ink, probably at the start of Ink scripts for save/load files
    // ====================================================================================================================
    public void UpdatePlayerName(string name)
    {
        story.variablesState["PlayerName"] = name;
    }

    private void DisplayNextLine()
    {
        if (story.canContinue)
        {
            string rawLine = story.Continue().Trim();

            // Split with delimiter
            currentSentenceParts = rawLine.Split(SEGMENT_DELIMITER);
            currentPartIndex = 0;

            // reset for new Ink line
            dialogueText.text = "";

            // Show next part
            ShowSentencePart(currentSentenceParts[currentPartIndex], append: false);

        }
        else if (story.currentChoices.Count > 0)
        {
            DisplayChoices();
        }
        else
        {
            // Put function here like continuing to next scene or ink script
            // ===============================================================================
            EndDialogue();
        }

    }

    private void ShowSentencePart(string textPart, bool append)
    {
        // If already typing, stop the previous coroutine to prevent two coroutines running at the same time
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        appendMode = append;
        currentTypingPart = textPart;
        typingCoroutine = StartCoroutine(TypeText(currentTypingPart, appendMode));
    }


    private IEnumerator TypeText(string textPart, bool append)
    {
        isTyping = true;
        HideNextIcon();

        if (!append)
        {
            dialogueText.text = "";
        }

        string displayedText = "";

        if (append == true)
        {
            displayedText = dialogueText.text;
        }
        else
        {
            dialogueText.text = "";
        }


        int i = 0;

        while (i < textPart.Length)
        {
            // Check if starting a tag
            if (textPart[i] == '<')
            {
                int tagEnd = textPart.IndexOf('>', i);
                if (tagEnd != -1)
                {
                    // Append full tag immediately
                    displayedText += textPart.Substring(i, tagEnd - i + 1);
                    i = tagEnd + 1;
                    dialogueText.text = displayedText;
                    continue;
                }
            }

            // Append normal character
            displayedText += textPart[i];
            dialogueText.text = displayedText;
            i++;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        ShowNextIcon();
    }


    public void OnContinueClicked()
    {
        if (isTyping == true)
        {
            // if already typing, skip typing and show the full segment immediately
            if (typingCoroutine != null)
            {
                 StopCoroutine(typingCoroutine);
            }

            if (appendMode == true)
            {
                dialogueText.text += currentTypingPart; // append full part
            }
            else
            {
                dialogueText.text = currentTypingPart; // replace with full part
            }

            isTyping = false;

            // Show icon
            ShowNextIcon();
        }
        else if (currentSentenceParts != null && currentPartIndex < currentSentenceParts.Length - 1)
        {
            // Append next part instead of clearing
            currentPartIndex++;
            ShowSentencePart(currentSentenceParts[currentPartIndex], append: true);
        }
        else
        {
            // Go to next Ink line
            DisplayNextLine();
        }
    }

    private void DisplayChoices()
    {
        // checks if choices are already being displaye
        if (choiceButtonContainer.GetComponentsInChildren<Button>().Length > 0) return;

        for (int i = 0; i < story.currentChoices.Count; i++) // iterates through all choices
        {

            var choice = story.currentChoices[i];
            var button = CreateChoiceButton(choice.text); // creates a choice button

            button.onClick.AddListener(() => OnClickChoiceButton(choice));
        }
    }

    Button CreateChoiceButton(string text)
    {
        // creates the button from a prefab
        var choiceButton = Instantiate(choiceButtonPrefab, choiceButtonContainer.transform);

        // sets text on the button
        var buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = text;

        return choiceButton;
    }

    // CAN EDIT TO IMPLEMENT CUSTOM EVENT ACTIONS ON EACH CHOICE HERE
    // ==================================================================
    void OnClickChoiceButton(Choice choice)
    {
        story.ChooseChoiceIndex(choice.index); // tells ink which choice was selected
        RefreshChoiceView(); // removes choices from the screen
        DisplayNextLine();
    }
    void RefreshChoiceView()
    {
        if (choiceButtonContainer != null)
        {
            foreach (var button in choiceButtonContainer.GetComponentsInChildren<Button>())
            {
                Destroy(button.gameObject);
            }
        }
    }

    private void EndDialogue()
    {
        HideNextIcon();
    }

    public void ChangeTypingSpeed(float speed)
    {
        if (speed <= 0f)
        {
            speed = 0.01f;
        }

        typingSpeed = speed;
    }


    // ================================
    // Next Icon Control
    // ================================
    private void ShowNextIcon()
    {
        UpdateNextIconPosition();
        nextIcon.gameObject.SetActive(true);

        if (iconBlinkCoroutine != null) StopCoroutine(iconBlinkCoroutine);
        iconBlinkCoroutine = StartCoroutine(BlinkNextIcon());
    }

    private void HideNextIcon()
    {
        if (iconBlinkCoroutine != null) StopCoroutine(iconBlinkCoroutine);
        nextIcon.gameObject.SetActive(false);
    }
    private IEnumerator BlinkNextIcon()
    {
        Color baseColor = nextIcon.color;

        while (true)
        {
            // Fade out
            for (float t = 0; t < 1; t += Time.deltaTime / blinkSpeed)
            {
                nextIcon.color = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(1f, 0f, t));
                yield return null;
            }

            // Fade in
            for (float t = 0; t < 1; t += Time.deltaTime / blinkSpeed)
            {
                nextIcon.color = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(0f, 1f, t));
                yield return null;
            }
        }
    }

    // Position the next icon at the end of the current text.
    private void UpdateNextIconPosition()
    {
        int lastIndex = dialogueText.textInfo.characterCount - 1;
        if (lastIndex < 0) return;

        var charInfo = dialogueText.textInfo.characterInfo[lastIndex];

        // Use bottomRight for a clean attachment point
        Vector3 worldPos = dialogueText.transform.TransformPoint(charInfo.bottomRight);

        // Convert world -> local canvas space
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dialogueText.canvas.transform as RectTransform,
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out localPos
        );

        // Apply small padding to the right
        nextIcon.GetComponent<RectTransform>().localPosition = new Vector3(10f + localPos.x, localPos.y, 0);
    }
}
