using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.UI;

// TODO: NextIcon position/size scale based on screen resolution

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    private bool allowStoryClicks = true;
    [SerializeField]
    private bool isAVL = true;

    [Header("UI Components - Basic")]
    public TextMeshProUGUI dialogueTextAVL;
    public Image nextIconAVL; // sprite icon for "continue"

    [Header("UI Components - Choices")]
    [SerializeField]
    private VerticalLayoutGroup choiceButtonContainer;
    [SerializeField]
    private Button choiceButtonPrefab;


    [Header("SaveLoad")]
    private string loadedState;


    [Header("Typing Settings")]
    public float typingSpeed = 0.03f; // delay per character
    public bool isTyping = false;
    public string currentTypingPart = ""; // <- track the current segment being typed
    private bool appendMode = false;      // <- track if we're appending

    public bool isFastForwarding = false;
    public float FastForwardDelay = 0.01f;
    private Coroutine fastForwardRoutine;

    [Header("Next Icon Blink Settings")]
    public float blinkSpeed = 0.5f; // seconds between fade

    [Header("Ink Configuration")]
    [Tooltip("Delimiter used in Ink files to split a line into segments (e.g., '||').")]
    private const string SEGMENT_DELIMITER = "(SPLIT)";

    [Header("Story Variables")]
    public string playerName;
    public bool needSecondWindow = false;

    public bool IsAVL
    {
        get => isAVL;
        private set
        {
            isAVL = value;
        }
    }

    public string PlayerName
    {
        get => playerName;
        private set
        {
            Debug.Log($"Updating RelationshipStrength value. Old value: {playerName}, new value: {value}");
            playerName = value;
        }
    }

    public bool AllowStoryClicks
    {
        get => allowStoryClicks;
        set => allowStoryClicks = value;
    }

    public void LoadState(string state)
    {
        loadedState = state;
        StartStory(false);
    }


    public void SetAVL(bool x)
    {
        isAVL = x;
    }

    public void StartStory(bool instant)
    {

        // IF LOADING A SAVE FILE



    }

    public void ResetStory()
    {
        loadedState = null;
    }

  


    public void DisplayFullCurrentLine()
    {

    }

    public void ShowFullSentenceInstant(string sentence)
    {
        //if(typingCoroutine != null)
        //{
        //    StopCoroutine(typingCoroutine);
        //    typingCoroutine = null;
        //}

        GameSingleton.instance.spriteAnimationManager.StopAllAnimations();

        //nextIconAVL.gameObject.SetActive(true);
        sentence = sentence.Trim().Replace(SEGMENT_DELIMITER, "");
        dialogueTextAVL.text = sentence;

        // ensure the layout updates before placing the next icon
        dialogueTextAVL.ForceMeshUpdate();


        // Mark as fully typed
        isTyping = false;

    }

    public void DisplayCurrentLine(bool instant)
    {

        // Handle any modifiers here

        // ===

        if (instant)
        {
 
        }
        else
        {
            // Split with delimiter


            // reset for new line
            dialogueTextAVL.text = "";

            // Show next part
            //ShowSentencePart(currentSentenceParts[currentPartIndex], append: false);
        }

        // If there are choices available...

    }

    public void DisplayNextLine()
    {


    }

    public void StartFastForward()
    {
        if (GameSingleton.instance.cameraManager.overlayCameraList[(int)OverlayCameraID.AVL].gameObject.activeSelf == false)
        {
            Debug.Log("[DialogueManager] AVL not active. Cannot fast forward.");
            return;
        }

        if (fastForwardRoutine != null)
        {
            StopCoroutine(fastForwardRoutine);
        }

        isFastForwarding = true;
        //allowStoryClicks = false;

        fastForwardRoutine = StartCoroutine(FastForwardRoutine());
    }

    public void StopFastForward()
    {
        isFastForwarding = false;
        //allowStoryClicks = true;

        if (fastForwardRoutine != null)
        {
            StopCoroutine(fastForwardRoutine);
            fastForwardRoutine = null;

            Debug.Log("[DialogueManager] Fast-forward finished (end of story).");
        }
    }

    private IEnumerator FastForwardRoutine()
    {
        Debug.Log("[DialogueManager] Starting fast-forward...");

        while (isFastForwarding)
        {


            yield return new WaitForSeconds(FastForwardDelay);
        }

    }

    private void ShowSentencePart(string textPart, bool append)
    {

    }


    private IEnumerator TypeText(string textPart, bool append)
    {
        isTyping = true;

        if (!append)
        {
            dialogueTextAVL.text = "";
        }

        string displayedText = "";

        if (append == true)
        {
            displayedText = dialogueTextAVL.text;
        }
        else
        {
            dialogueTextAVL.text = "";
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
                    dialogueTextAVL.text = displayedText;
                    continue;
                }
            }

            // Append normal character
            displayedText += textPart[i];
            dialogueTextAVL.text = displayedText;
            i++;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

    }


    public void HandleTags(List<string> tags)
    {
        foreach (string tag in tags)
        {
            // Each tag might look like "show Luna happy" or "bg forest"
            string[] parts = tag.Split(' ');

            if (parts.Length == 0) continue;

            string command = parts[0].ToLower();

            switch (command)
            {
                case "command":
                    // Format: #Command hide CharacterName
                    if (parts.Length >= 2)
                    {

                    }
                    break;


                case "scommand":
                    // SPECIAL COMMANDS FOR ONE TIME USE AND STUFF

                    break;


                case "id":
                    break;

                default:
                    Debug.LogWarning($"Unknown tag: {tag}");
                    break;
            }
        }
    }

    public void HandleIDTag(List<string> tags)
    {
        foreach (string tag in tags)
        {
            // Each tag might look like "show Luna happy" or "bg forest"
            string[] parts = tag.Split(' ');

            if (parts.Length == 0) continue;

            string command = parts[0].ToLower();

            if(command == "id")
            {
                // Format: #id someIDValue
                if (parts.Length == 2)
                {
  
                }
                else
                {
                    Debug.LogWarning($"Invalid 'id' tag format: {tag}");
                }
            }
            else
            {
                continue;
            }
        }
    }

    public string GetLineID(List<string> tags)
    {
        foreach (string tag in tags)
        {
            // Each tag might look like "show Luna happy" or "bg forest"
            string[] parts = tag.Split(' ');

            if (parts.Length == 0) continue;

            string command = parts[0].ToLower();

            if(command == "id")
            {
                return parts[1];
            }

        }

        return "";
    }


    public void OnContinueClicked()
    {
        if (isFastForwarding == true)
        {
            StopFastForward();

        }

      
    }

    public void OnContinueClickedFastForward()
    {

       
    }

    private void DisplayChoices()
    {
        // checks if choices are already being displaye
        if (choiceButtonContainer.GetComponentsInChildren<Button>().Length > 0) return;


    }

    Button CreateChoiceButton(VerticalLayoutGroup choiceButtonContainer, string text)
    {
        // creates the button from a prefab
        var choiceButton = Instantiate(choiceButtonPrefab, choiceButtonContainer.transform);

        // sets text on the button
        var buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = text;

        return choiceButton;
    }

    public void RefreshChoiceView()
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
        //HideNextIcon();
    }

    public void ChangeTypingSpeed(float speed)
    {
        if (speed <= 0f)
        {
            speed = 0.0001f;
        }

        typingSpeed = speed;
    }


}


