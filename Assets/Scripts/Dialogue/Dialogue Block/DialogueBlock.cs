using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;

public class DialogueBlock : MonoBehaviour
{
    public string ID;
    public TextMeshProUGUI textBox;

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

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        float speed;
        if (overwriteTextSpeed)
            speed = textSpeed;
        else
            speed = manager.typingSpeed;


        manager.StartTyping(text, speed, appendText, requirePlayerClickContinue, onComplete);
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
    public string[] choices;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        // Placeholder — wire up choice UI here, call onComplete when a choice is picked
        Debug.Log("[DialogueChoiceNode] Choice UI not yet implemented.");
    }
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

        if (command == AnimationCommand.Skip)
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
        else if (command == AudioCommand.Stop)
        {
            switch (category)
            {
                case AudioCategory.BGM: audio.StopAllBGM(); break;
                case AudioCategory.SFX: audio.StopAllSFX(); break;
                case AudioCategory.Character: audio.StopAllVoices(); break;
            }
        }

        onComplete?.Invoke();
    }
}


[Serializable]
public class DialogueRequirePlayerClickContinueNode : DialogueBlockNode
{
    public bool enabled = true;

    public override void Execute(DialogueManager manager, Action onComplete)
    {
        manager.requireClickToContinue = enabled;
        onComplete?.Invoke();
    }
}