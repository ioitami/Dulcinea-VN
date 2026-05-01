using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadGameButton : MonoBehaviour
{
    public int loadSlotNumber;
    public Image loadSpritePreview;
    public TextMeshProUGUI saveID_Text;
    public Sprite emptySaveSprite;
    //public TextMeshProUGUI chapterName_Text;
    //public TextMeshProUGUI saveTimeStamp_Text;

    private void Start()
    {
        RefreshButton();
    }

    private void RefreshButton()
    {
        SaveData data = GameSingleton.instance.gameStateManager.Load(loadSlotNumber);

        if (data == null)
        {
            loadSpritePreview.sprite = emptySaveSprite;
            saveID_Text.text = "Empty";
            return;
        }

        Sprite screenshot = GameSingleton.instance.gameStateManager.GetSaveScreenshotSprite(loadSlotNumber);

        if (screenshot != null)
            loadSpritePreview.sprite = screenshot;
        else
            loadSpritePreview.sprite = emptySaveSprite;

        saveID_Text.text = "Save " + data.saveID.ToString();
    }

    public void LoadGame()
    {
        SaveData data = GameSingleton.instance.gameStateManager.Load(loadSlotNumber);
        Debug.Log("data:" + data.saveID + "," + loadSlotNumber);

        if (data == null)
        {
            Debug.LogWarning($"[LoadGameButton] No save data found for slot {loadSlotNumber}.");
            return;
        }

        // Load the game scene
        GameSingleton.instance.sceneLoaderManager.LoadWindow1();

        // Close the save/load menu
        GameSingleton.instance.sceneLoaderManager.CloseSaveLoadOptionsMenu();

        // Restore characters on screen
        RestoreCharacters(data);

        // Find and play the correct dialogue group and block
        FindAndPlayDialogue(data);
    }

    private void RestoreCharacters(SaveData data)
    {
        CharacterManager characterManager = GameSingleton.instance.characterManager;
        if (characterManager == null) return;

        characterManager.HideAllCharacters();

        for (int i = 0; i < data.charactersOnScreen.Count; i++)
        {
            string charName = data.charactersOnScreen[i];
            string moodName = data.charactersMood[i];
            Vector3 position = data.charactersPosition[i].ToVector3();

            characterManager.ShowCharacter(charName, moodName, position);
        }
    }

    private void FindAndPlayDialogue(SaveData data)
    {
        DialogueGroup[] allGroups = GameObject.FindObjectsByType<DialogueGroup>(FindObjectsSortMode.None);

        Debug.Log($"[LoadGameButton] Looking for Group ID: '{data.dialogueGroupID}' (length: {data.dialogueGroupID.Length})");
        Debug.Log($"[LoadGameButton] Looking for Block ID: '{data.dialogueBlockID}' (length: {data.dialogueBlockID.Length})");

        DialogueGroup targetGroup = null;
        DialogueBlock targetBlock = null;

        foreach (DialogueGroup group in allGroups)
        {
            Debug.Log($"[LoadGameButton] Checking group: '{group.ID}' (length: {group.ID.Length})");

            if (group.ID.Trim() == data.dialogueGroupID.Trim())
            {
                targetGroup = group;

                foreach (DialogueBlock block in group.blocks)
                {
                    Debug.Log($"[LoadGameButton] Checking block: '{block.ID}' (length: {block.ID.Length})");

                    if (block.ID.Trim() == data.dialogueBlockID.Trim())
                    {
                        targetBlock = block;
                        break;
                    }
                }
                break;
            }
        }

        if (targetGroup == null)
        {
            Debug.LogWarning($"[LoadGameButton] DialogueGroup '{data.dialogueGroupID}' not found.");
            return;
        }

        if (targetBlock == null)
        {
            Debug.LogWarning($"[LoadGameButton] DialogueBlock '{data.dialogueBlockID}' not found.");
            return;
        }

        GameSingleton.instance.dialogueManager.PlaySpecificBlockInGroup(targetGroup, targetBlock);
    }
}