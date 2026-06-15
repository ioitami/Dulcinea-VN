using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class DialogueBlock : MonoBehaviour
{
    public string ID;
    public string saveDescription;
    public TextMeshProUGUI textBox;

    [Header("Dialogue UI")]
    public Image dialogueBoxImage;
    public Image dialogueBoxCharIconImage;

    [SerializeReference]
    public DialogueBlockNode[] nodes;
}


[Serializable]
public abstract class DialogueBlockNode
{
    public abstract void Execute(DialogueManager manager, Action onComplete);
}


[Serializable]
public class DialogueTextNode : DialogueBlockNode
{
    [TextArea(2, 5)]
    public string text;

    public bool appendText = true;
    public bool requirePlayerClickContinue;
    public bool overwriteTextSpeed;
    public float textSpeed = 0.04f;

    public int characterIndex = -1; // -1 = None


    public override void Execute(DialogueManager manager, Action onComplete)
    {
        float speed;
        if (overwriteTextSpeed)
            speed = textSpeed;
        else
            speed = manager.typingSpeed;


        // Add change textbox background UI and character icon (used instead of name) here based on the characterindex
        ApplyCharacterTextUI(manager);
        UpdateDialogueLogHistory(manager);

        manager.StartTyping(text, speed, appendText, requirePlayerClickContinue, onComplete);
    }

    private void UpdateDialogueLogHistory(DialogueManager manager)
    {
        DialogueLogHistory logHistory = GameSingleton.instance.sceneLoaderManager.uiController.dialogueLogHistory;
        if (logHistory == null) return;

        string characterName = "";

        if (characterIndex != -1)
        {
            CharacterManager characterManager = GameSingleton.instance.characterManager;
            if (characterManager != null && characterIndex < characterManager.characters.Count)
                characterName = characterManager.characters[characterIndex].characterName;
        }

        logHistory.LogDialogueText(manager.currentBlock, text, characterName);
    }

    private void ApplyCharacterTextUI(DialogueManager manager)
    {
        if (characterIndex == -1) return;

        CharacterManager characterManager = GameSingleton.instance.characterManager;
        if (characterManager == null) return;
        if (characterIndex < 0 || characterIndex >= characterManager.characters.Count) return;

        Character character = characterManager.characters[characterIndex];
        DialogueBlock block = manager.currentBlock;

        if (block == null) return;

        Debug.Log("Applying dialogue box UI");

        if (block.dialogueBoxImage != null && character.dialogueBoxImage != null)
            block.dialogueBoxImage.sprite = character.dialogueBoxImage;

        if (block.dialogueBoxCharIconImage != null && character.dialogueBoxCharIconImage != null)
            block.dialogueBoxCharIconImage.sprite = character.dialogueBoxCharIconImage;
    }
}


[Serializable]
public class DialogueChangeTextBoxUINode : DialogueBlockNode
{
    public Image targetDialogueBoxImage;
    public Image targetDialogueBoxCharIconImage;
    public Sprite sourceDialogueBoxImage;
    public Sprite sourceDialogueBoxCharIconImage;

    public bool changeDialogueBoxColor;
    public Color dialogueBoxColor = Color.white;

    public bool changeCharIconColor;
    public Color charIconColor = Color.white;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        if (targetDialogueBoxImage != null)
        {
            if (sourceDialogueBoxImage != null)
                targetDialogueBoxImage.sprite = sourceDialogueBoxImage;

            if (changeDialogueBoxColor)
                targetDialogueBoxImage.color = dialogueBoxColor;
        }

        if (targetDialogueBoxCharIconImage != null)
        {
            if (sourceDialogueBoxCharIconImage != null)
                targetDialogueBoxCharIconImage.sprite = sourceDialogueBoxCharIconImage;

            if (changeCharIconColor)
                targetDialogueBoxCharIconImage.color = charIconColor;
        }

        onComplete?.Invoke();
    }
}


public class DialoguePauseNode : DialogueBlockNode
{
    public float pauseDuration = 0.5f;

    public override void Execute(DialogueManager manager, Action onComplete)
    {

        manager.StartCoroutine(PauseRoutine(onComplete));
    }

    private System.Collections.IEnumerator PauseRoutine(Action onComplete)
    {
        yield return new UnityEngine.WaitForSeconds(pauseDuration);
        onComplete?.Invoke();
    }
}


[Serializable]
public class DialogueChoiceNode : DialogueBlockNode
{
    public GameObject choicePrefab;
    public Transform choiceContainerParent;
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        if (choicePrefab == null)
        {
            Debug.LogWarning("[DialogueChoiceNode] No choice prefab assigned.");
            onComplete?.Invoke();
            return;
        }

        if (choiceContainerParent == null)
        {
            Debug.LogWarning("[DialogueChoiceNode] No choice container parent assigned.");
            onComplete?.Invoke();
            return;
        }

        foreach (DialogueChoice choice in choices)
        {
            GameObject choiceObj = GameObject.Instantiate(choicePrefab, choiceContainerParent);

            // Set button text on the child text component
            TextMeshProUGUI label = choiceObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = choice.text;

            // Wire up the button onclick
            Button button = choiceObj.GetComponent<Button>();
            if (button != null)
            {
                // Capture local reference for the lambda
                DialogueChoice capturedChoice = choice;

                button.onClick.AddListener(() =>
                {
                    capturedChoice.onSelected?.Invoke();
                    CleanupChoices();

                    // Register current block as visited before jumping to new node
                    if (manager.currentBlock != null)
                    {
                        GameSingleton.instance.gameStateManager.RegisterVisitedBlock(manager.currentBlock.ID);
                    }


                    if (capturedChoice.linkedGroup != null)
                    {
                        manager.PlaySpecificBlockInGroup(capturedChoice.linkedGroup, capturedChoice.linkedBlock);
                    }
                    else
                    {
                        onComplete?.Invoke();
                    }

                });
            }
        }


        if (manager.isFastForwarding)
        {
            manager.StopFastForward();
        }

    }

    private void CleanupChoices()
    {
        if (choiceContainerParent == null) return;

        foreach (Transform child in choiceContainerParent)
            GameObject.Destroy(child.gameObject);
    }
}

[Serializable]
public class DialogueChoice
{
    public string text;
    public DialogueGroup linkedGroup;
    public DialogueBlock linkedBlock;
    public UnityEvent onSelected;
}


[Serializable]
public class DialogueScriptNode : DialogueBlockNode
{
    public UnityEvent scriptEvent;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        scriptEvent?.Invoke();
        onComplete?.Invoke();
    }
}


[Serializable]
public class DialogueChangeFontNode : DialogueBlockNode
{
    public TMP_FontAsset fontAsset;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        if (manager.currentBlock?.textBox != null && fontAsset != null)
            manager.currentBlock.textBox.font = fontAsset;

        onComplete?.Invoke();
    }
}


[Serializable]
public class DialogueShowCharacterNode : DialogueBlockNode
{
    public int characterIndex;
    public int moodIndex;

    [Header("Scale")]
    public bool scaleCommand;
    public Vector3 scale = Vector3.one;

    [Header("Position")]
    public bool positionCommand;

    public PositionMode positionMode = PositionMode.Preset;

    public int presetPositionIndex;
    public Vector3 manualPosition;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        CharacterManager characterManager = GameSingleton.instance.characterManager;
        if (characterManager == null) { onComplete?.Invoke(); return; }
        if (characterIndex < 0 || characterIndex >= characterManager.characters.Count) { onComplete?.Invoke(); return; }

        Character character = characterManager.characters[characterIndex];
        string charName  = character.characterName;
        string moodName  = character.moods[moodIndex].moodName;

        if (positionCommand)
        {
            if (positionMode == PositionMode.Preset)
            {
                var preset = characterManager.customPositions[presetPositionIndex];
                characterManager.ShowCharacter(charName, moodName, preset.positionName);
            }
            else
            {
                characterManager.ShowCharacter(charName, moodName, manualPosition);
            }
        }
        else
        {
            characterManager.ShowCharacter(charName, moodName);
        }

        if (scaleCommand)
            character.ingameContainerObj.transform.localScale = scale;

        onComplete?.Invoke();
    }
}

public enum PositionMode
{
    Preset,
    Manual
}


[Serializable]
public class DialogueHideCharacterNode : DialogueBlockNode
{
    public int characterIndex;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        CharacterManager characterManager = GameSingleton.instance.characterManager;

        if (characterManager == null) { onComplete?.Invoke(); return; }

        if (characterIndex < 0 || characterIndex >= characterManager.characters.Count) { onComplete?.Invoke(); return; }

        characterManager.HideCharacter(characterManager.characters[characterIndex].characterName);
        onComplete?.Invoke();
    }
}

public enum AnimationCommand { Play, Skip }

[Serializable]
public class DialoguePlayAnimationNode : DialogueBlockNode
{
    public int characterIndex;
    public string animationName;
    public AnimationCommand command = AnimationCommand.Play;
    public bool waitForCompletion = true;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        CharacterManager characterManager = GameSingleton.instance.characterManager;
        SpriteAnimationManager animManager = GameSingleton.instance.spriteAnimationManager;

        if (characterManager == null || animManager == null) { onComplete?.Invoke(); return; }
        if (characterIndex < 0 || characterIndex >= characterManager.characters.Count) { onComplete?.Invoke(); return; }

        Character character = characterManager.characters[characterIndex];


        if (command == AnimationCommand.Skip || manager.isFastForwarding)
        {
            animManager.SkipToEnd(animationName, character.ingameContainerObj.transform);
            onComplete?.Invoke();
            return;
        }

        if (waitForCompletion)
            characterManager.PlayAnimationCharacter(characterIndex, animationName, onComplete);
        else
        {
            characterManager.PlayAnimationCharacter(characterIndex, animationName);
            onComplete?.Invoke();
        }
    }
}

public enum AudioCategory { BGM, SFX, Character }
public enum AudioCommand { Play, Stop }

[Serializable]
public class DialoguePlaySoundNode : DialogueBlockNode
{
    public AudioCommand command = AudioCommand.Play;
    public AudioCategory category = AudioCategory.SFX;
    public string clipName;
    public float fadeOutDuration = 0.5f;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        AudioManager audio = GameSingleton.instance.audioManager;
        if (audio == null) { onComplete?.Invoke(); return; }

        if (command == AudioCommand.Play)
        {
            switch (category)
            {
                case AudioCategory.BGM: audio.PlayBGM(clipName); break;
                case AudioCategory.SFX: audio.PlaySFX(clipName); break;
                case AudioCategory.Character: audio.PlayCharacter(clipName); break;
            }
        }
        else
        {
            switch (category)
            {
                case AudioCategory.BGM: audio.StopAllBGM(fadeOutDuration); break;
                case AudioCategory.SFX: audio.StopAllSFX(fadeOutDuration); break;
                case AudioCategory.Character: audio.StopAllVoices(fadeOutDuration); break;
            }
        }

        onComplete?.Invoke();
    }
}


[Serializable]
public class DialoguePlayGroupNode : DialogueBlockNode
{
    public DialogueGroup group;
    public DialogueBlock block;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        if (group == null)
        {
            Debug.LogWarning("[DialoguePlayGroupNode] No group assigned.");
            onComplete?.Invoke();
            return;
        }

        if (manager.currentBlock != null)
        {
            GameSingleton.instance.gameStateManager.RegisterVisitedBlock(manager.currentBlock.ID);
        }

        manager.PlaySpecificBlockInGroup(group, block);
    }
}


[Serializable]
public class DialogueRequirePlayerClickContinueNode : DialogueBlockNode
{
    public bool enabled = true;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        manager.clickToContinueEnabled = enabled;
        onComplete?.Invoke();
    }
}


[Serializable]
public class DialogueSetDialogueClickRightsNode : DialogueBlockNode
{
    public bool allow = true;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        manager.SetGlobalAllowDialogueClick(allow);
        onComplete?.Invoke();
    }
}

