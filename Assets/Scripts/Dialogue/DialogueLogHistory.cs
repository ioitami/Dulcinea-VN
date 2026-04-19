using TMPro;
using UnityEngine;

public class DialogueLogHistory : MonoBehaviour
{
    public Transform dialogueLogHistoryCameraParent;
    public Canvas dialogueLogHistoryCanvas;
    public Transform dialogueLogHistoryContent;

    public GameObject dialogueLogHistoryTextbox_WithName_Prefab;
    public GameObject dialogueLogHistoryTextbox_NoName_Prefab;

    // Track the current active log entry
    private DialogueBlock currentLogBlock;
    private TextMeshProUGUI currentLogText;

    public void LogDialogueText(DialogueBlock block, string text, string characterName)
    {
        // If this is a new block, spawn a new prefab
        if (block != currentLogBlock)
        {
            currentLogBlock = block;
            SpawnLogEntry(characterName);
        }

        // Append text to the current log entry
        if (currentLogText != null)
            currentLogText.text += text;
    }

    private void SpawnLogEntry(string characterName)
    {
        bool hasName = !string.IsNullOrEmpty(characterName);

        if (hasName)
        {
            GameObject obj = Instantiate(
                dialogueLogHistoryTextbox_WithName_Prefab,
                dialogueLogHistoryContent
            );

            DialogueLogHistoryTextbox_WithName entry = obj.GetComponent<DialogueLogHistoryTextbox_WithName>();
            if (entry != null)
            {
                entry.logDialogueName.text = characterName;
                currentLogText = entry.logDialogueText;
                currentLogText.text = "";
            }
        }
        else
        {
            GameObject obj = Instantiate(
                dialogueLogHistoryTextbox_NoName_Prefab,
                dialogueLogHistoryContent
            );

            DialogueLogHistoryTextbox_NoName entry = obj.GetComponent<DialogueLogHistoryTextbox_NoName>();
            if (entry != null)
            {
                currentLogText = entry.logDialogueText;
                currentLogText.text = "";
            }
        }
    }

    public void ClearLog()
    {
        foreach (Transform child in dialogueLogHistoryContent)
            Destroy(child.gameObject);

        currentLogBlock = null;
        currentLogText = null;
    }
}