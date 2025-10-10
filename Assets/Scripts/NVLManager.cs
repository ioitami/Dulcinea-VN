using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;

public class NVLManager : MonoBehaviour
{
    public VerticalLayoutGroup choiceButtonContainer;
    public TextMeshProUGUI dialogueText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameSingleton.instance.dialogueManager.StartStory(choiceButtonContainer, dialogueText);
    }

    // Update is called once per frame
    void Update()
    {
        // ADD TO NVLMANAGER
        // Only update the icon position if sentence finished

        if (GameSingleton.instance.dialogueManager.isTyping == false && 
            GameSingleton.instance.dialogueManager.nextIcon != null && 
            dialogueText != null)
        {
            GameSingleton.instance.dialogueManager.UpdateNextIconPosition(dialogueText);
        }
    }

    public void ClickContinueStory()
    {
        GameSingleton.instance.dialogueManager.OnContinueClicked(choiceButtonContainer,dialogueText);
    }
}
