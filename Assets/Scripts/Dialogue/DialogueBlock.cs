using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public enum DialogueBlockWindow
{
    AVL,
    NVL
}

public class DialogueBlock : MonoBehaviour
{
    public string ID;
    public string saveDescription;
    public TextMeshProUGUI textBox;
    public DialogueBlockWindow window;

    [Header("Dialogue UI")]
    public Image dialogueBoxImage;
    public Image dialogueBoxCharIconImage;

    [SerializeReference]
    public DialogueBlockNode[] nodes;

    private void Awake()
    {
        DialogueRegistry.RegisterBlock(this);
    }

    private void OnDestroy()
    {
        DialogueRegistry.UnregisterBlock(this);
    }
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

        string localizedText = GetLocalizedText(manager);

        // Add change textbox background UI and character icon (used instead of name) here based on the characterindex
        ApplyCharacterTextUI(manager);
        UpdateDialogueLogHistory(manager, localizedText);

        manager.StartTyping(localizedText, speed, appendText, requirePlayerClickContinue, onComplete);
    }

    // Looks up this line's own text by a key derived from the block it's
    // in and its position within that block — no new serialized field
    // needed, and it's a no-op returning the authored text until a string
    // table actually exists for the current language.
    private string GetLocalizedText(DialogueManager manager)
    {
        if (LocalizationManager.instance == null) return text;
        if (manager.currentBlock == null) return text;

        string key = LocalizationManager.MakeDialogueTextKey(manager.currentBlock.ID, manager.CurrentNodeIndex - 1);
        return LocalizationManager.instance.Get(key, text);
    }

    private void UpdateDialogueLogHistory(DialogueManager manager, string displayedText)
    {
        DialogueLogHistory logHistory = GameSingleton.instance.sceneLoaderManager.uiController.dialogueLogHistory;
        if (logHistory == null) return;

        string characterName = "";

        if (characterIndex != -1)
        {
            CharacterManager characterManager = GameSingleton.instance.characterManager;
            if (characterManager != null && characterIndex < characterManager.characters.Count)
            {
                Character character = characterManager.characters[characterIndex];
                string nameKey = character.GetStableID();
                characterName = LocalizationManager.instance != null
                    ? LocalizationManager.instance.Get(nameKey, character.characterName)
                    : character.characterName;
            }
        }

        logHistory.LogDialogueText(manager.currentBlock, displayedText, characterName);
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

[Serializable]
public class DialogueWaitForClickNode : DialogueBlockNode
{
    public override void Execute(DialogueManager manager, Action onComplete)
    {
        manager.ActiveTrack.WaitForClick(onComplete);
    }
}

[Serializable]
public class ToggleBothWindowContinueClicksNode : DialogueBlockNode
{
    public bool enabled = true;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        manager.SetIgnorePrimaryTrackWindowMatch(enabled);
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

        DialogueTrack track = manager.ActiveTrack;

        manager.RegisterActiveChoice(this, onComplete);

        if (track.isFastForwarding)
        {
            track.StopFastForward();
        }

        int nodeIndex = manager.CurrentNodeIndex - 1;

        if (NVLNetworkPlayer.hostInstance != null)
        {
            NVLNetworkPlayer.hostInstance.RpcShowChoiceUI(track.windowNumber, manager.currentBlock.ID, nodeIndex);
        }
        else
        {
            DisplayChoicesLocally(manager, track.windowNumber, manager.currentBlock.ID, nodeIndex);
        }
    }

    public void DisplayChoicesLocally(DialogueManager manager, int windowNumber, string blockID, int nodeIndex)
    {
        CleanupChoicesLocally();

        for (int i = 0; i < choices.Count; i++)
        {
            int capturedIndex = i;
            DialogueChoice choice = choices[i];

            GameObject choiceObj = GameObject.Instantiate(choicePrefab, choiceContainerParent);

            TextMeshProUGUI label = choiceObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                string key = LocalizationManager.MakeChoiceKey(blockID, nodeIndex, i);
                label.text = LocalizationManager.instance != null
                    ? LocalizationManager.instance.Get(key, choice.text)
                    : choice.text;
            }

            Button button = choiceObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    if (NVLNetworkPlayer.localPlayer != null)
                        NVLNetworkPlayer.localPlayer.CmdSelectChoice(windowNumber, capturedIndex);
                    else
                        manager.ResolveActiveChoiceByIndex(windowNumber, capturedIndex);
                });
            }
        }
    }

    public void ResolveChoice(int index, DialogueManager manager, DialogueTrack track, Action onComplete)
    {
        if (choices == null || index < 0 || index >= choices.Count) return;

        DialogueChoice chosen = choices[index];

        chosen.onSelected?.Invoke();

        if (NVLNetworkPlayer.hostInstance != null)
            NVLNetworkPlayer.hostInstance.RpcHideChoiceUI(track.windowNumber);
        else
            manager.HideChoiceUILocally(track.windowNumber);

        if (track.currentBlock != null)
        {
            GameSingleton.instance.gameStateManager.RegisterVisitedBlock(track.currentBlock.ID);
        }

        if (chosen.linkedGroup != null)
            track.PlaySpecificBlockInGroup(chosen.linkedGroup, chosen.linkedBlock);
        else
            onComplete?.Invoke();
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
        string charName = character.characterName;
        string moodName = character.moods[moodIndex].moodName;

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


// does NOT pause the text node. Just sets a flag that affects all future requireplayerclicks for text nodes
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

public enum BackgroundTarget
{
    MainMenu,
    Window1,
    Window2
}

[Serializable]
public class DialogueSetBackgroundNode : DialogueBlockNode
{
    public BackgroundTarget target = BackgroundTarget.Window1;
    public string backgroundName;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        BackgroundManager backgroundManager = GameSingleton.instance.backgroundManager;

        if (backgroundManager == null)
        {
            Debug.LogWarning("[DialogueSetBackgroundNode] No BackgroundManager found.");
            onComplete?.Invoke();
            return;
        }

        switch (target)
        {
            case BackgroundTarget.MainMenu:
                backgroundManager.SetMainMenuBackground(backgroundName);
                break;
            case BackgroundTarget.Window1:
                backgroundManager.SetBackground(backgroundName, 1);
                break;
            case BackgroundTarget.Window2:
                backgroundManager.SetBackground(backgroundName, 2);
                break;
        }

        onComplete?.Invoke();
    }

    [Serializable]
    public class SplitPlayGroupNode : DialogueBlockNode
    {
        [Header("Window 1 Path")]
        public DialogueGroup window1Group;
        public DialogueBlock window1Block;

        [Header("Window 2 Path")]
        public DialogueGroup window2Group;
        public DialogueBlock window2Block;

        public override void Execute(DialogueManager manager, Action onComplete)
        {
            if (window1Group == null || window2Group == null)
            {
                Debug.LogWarning("[SplitPlayGroupNode] Both window paths must be assigned.");
                onComplete?.Invoke();
                return;
            }

            if (window1Block != null && window2Block != null && window1Block == window2Block)
            {
                Debug.LogError("[SplitPlayGroupNode] Window 1 and Window 2 paths point to the same DialogueBlock — split ignored.");
                onComplete?.Invoke();
                return;
            }

            if (window1Block != null && window2Block != null && window1Block.textBox != null && window1Block.textBox == window2Block.textBox)
            {
                Debug.LogError("[SplitPlayGroupNode] Window 1 and Window 2 blocks share the same textBox — split ignored.");
                onComplete?.Invoke();
                return;
            }

            // Does NOT call onComplete, prim track pauses ReportSplitEnd converges both split tracks
            manager.BeginSplit(window1Group, window1Block, window2Group, window2Block);
        }
    }


    [Serializable]
    public class EndSplitGroupNode : DialogueBlockNode
    {
        public string splitID;
        public DialogueGroup nextGroup;
        public DialogueBlock nextBlock;

        public override void Execute(DialogueManager manager, Action onComplete)
        {
            manager.ReportSplitEnd(splitID, nextGroup, nextBlock);
        }
    }

    [Serializable]
    public class DialogueForceEndSplitNode : DialogueBlockNode
    {
        public DialogueGroup nextGroup;
        public DialogueBlock nextBlock;

        public override void Execute(DialogueManager manager, Action onComplete)
        {
            manager.ForceEndSplit(nextGroup, nextBlock);
        }
    }

    [Serializable]
    public class DialogueSetLinkedSplitContinueNode : DialogueBlockNode
    {
        public bool linked = true;

        public override void Execute(DialogueManager manager, Action onComplete)
        {
            manager.SetLinkedSplitContinue(linked);
            onComplete?.Invoke();
        }
    }
}