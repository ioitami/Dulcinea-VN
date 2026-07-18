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

        if (choices == null || choices.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // Only runs here on the authoritative (host/offline) instance —
        // DialogueManager gates node execution to that context already.
        manager.RegisterActiveChoice(this, onComplete);

        if (manager.isFastForwarding)
        {
            manager.StopFastForward();
        }

        // Broadcast to every window (including this one, if networked) so
        // they all instantiate identical choice UI from their own local
        // scene data. Falls back to a direct local display when no
        // networking is running at all (editor/offline testing).
        if (NVLNetworkPlayer.hostInstance != null)
        {
            int nodeIndex = manager.CurrentNodeIndex - 1;
            NVLNetworkPlayer.hostInstance.RpcShowChoiceUI(manager.currentBlock.ID, nodeIndex);
        }
        else
        {
            DisplayChoicesLocally(manager);
        }
    }

    // Display-only: instantiates the choice buttons and wires clicks to
    // route through the network (or directly, offline) rather than
    // resolving the outcome inline — the outcome is only ever resolved on
    // the authoritative instance via ResolveChoice.
    public void DisplayChoicesLocally(DialogueManager manager)
    {
        CleanupChoicesLocally();

        for (int i = 0; i < choices.Count; i++)
        {
            int capturedIndex = i;
            DialogueChoice choice = choices[i];

            GameObject choiceObj = GameObject.Instantiate(choicePrefab, choiceContainerParent);

            TextMeshProUGUI label = choiceObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = choice.text;

            Button button = choiceObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    if (NVLNetworkPlayer.localPlayer != null)
                        NVLNetworkPlayer.localPlayer.CmdSelectChoice(capturedIndex);
                    else
                        manager.ResolveActiveChoiceByIndex(capturedIndex);
                });
            }
        }
    }

    // Authoritative resolution — only ever invoked on the host/offline
    // instance, either directly (offline) or via CmdSelectChoice (networked).
    public void ResolveChoice(int index, DialogueManager manager, Action onComplete)
    {
        if (choices == null || index < 0 || index >= choices.Count) return;

        DialogueChoice chosen = choices[index];

        chosen.onSelected?.Invoke();

        if (NVLNetworkPlayer.hostInstance != null)
            NVLNetworkPlayer.hostInstance.RpcHideChoiceUI();
        else
            manager.HideChoiceUILocally();

        // Register current block as visited before jumping to new node
        if (manager.currentBlock != null)
        {
            GameSingleton.instance.gameStateManager.RegisterVisitedBlock(manager.currentBlock.ID);
        }

        if (chosen.linkedGroup != null)
        {
            manager.PlaySpecificBlockInGroup(chosen.linkedGroup, chosen.linkedBlock);
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    public void CleanupChoicesLocally()
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


// Marks whether the story from this point onward needs both windows open.
// NVLNetworkManager reads this to decide when to show the "waiting for
// second window" / "please close the second window" prompts.
[Serializable]
public class DialogueRequireServerNode : DialogueBlockNode
{
    public bool requiresServer = true;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        manager.SetRequiresServer(requiresServer);
        onComplete?.Invoke();
    }
}


// A narrative checkpoint where the story branches based on which window
// the player physically closes. Pauses playback (does not call
// onComplete) until either window closes; NVLNetworkManager resolves the
// outcome — jumping the surviving/host window to the matching block.
[Serializable]
public class DialogueWindowCloseChoiceNode : DialogueBlockNode
{
    [Header("If window 1 (host) is closed")]
    public DialogueGroup groupIfHostCloses;
    public DialogueBlock blockIfHostCloses;

    [Header("If window 2 (client) is closed")]
    public DialogueGroup groupIfClientCloses;
    public DialogueBlock blockIfClientCloses;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        if (groupIfHostCloses == null || groupIfClientCloses == null)
        {
            Debug.LogWarning("[DialogueWindowCloseChoiceNode] Both outcomes must be assigned.");
            onComplete?.Invoke();
            return;
        }

        manager.BeginWindowCloseChoice(
            groupIfHostCloses.ID, blockIfHostCloses != null ? blockIfHostCloses.ID : "",
            groupIfClientCloses.ID, blockIfClientCloses != null ? blockIfClientCloses.ID : "");

        // Intentionally does not call onComplete — playback stays paused
        // here until NVLNetworkManager resolves the outcome itself via
        // PlaySpecificBlockInGroup once a window actually closes.
    }
}

