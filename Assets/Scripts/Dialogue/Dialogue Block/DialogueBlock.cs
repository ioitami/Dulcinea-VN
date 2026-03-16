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
}


[Serializable]
public class DialogueTextNode : DialogueBlockNode
{
    [TextArea(2, 5)]
    public string text;

    public bool requirePlayerClickContinue;
    public bool overwriteTextSpeed;
    public float textSpeed = 0.04f;
}


public class DialoguePauseNode : DialogueBlockNode
{
    public float pauseDuration = 0.5f;
}


[Serializable]
public class DialogueChoiceNode : DialogueBlockNode
{
    public string[] choices;
}


[Serializable]
public class DialogueScriptNode : DialogueBlockNode
{
    public UnityEvent scriptEvent;
}


[Serializable]
public class DialogueChangeFontNode : DialogueBlockNode
{
    public TMP_FontAsset fontAsset;
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

    public void Execute(CharacterManager manager)
    {
        if (manager == null) return;

        if (characterIndex < 0 || characterIndex >= manager.characters.Count)
            return;

        Character character = manager.characters[characterIndex];

        string charName = character.characterName;
        string moodName = character.moods[moodIndex].moodName;

        if (positionCommand)
        {
            if (positionMode == PositionMode.Preset)
            {
                var preset = manager.customPositions[presetPositionIndex];
                manager.ShowCharacter(charName, moodName, preset.positionName);
            }
            else
            {
                manager.ShowCharacter(charName, moodName, manualPosition);
            }
        }
        else
        {
            manager.ShowCharacter(charName, moodName, charName = null);
        }

        if (scaleCommand)
        {
            character.ingameContainerObj.transform.localScale = scale;
        }
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

    public void Execute(CharacterManager manager)
    {
        if (manager == null) return;

        if (characterIndex < 0 || characterIndex >= manager.characters.Count)
            return;

        manager.HideCharacter(manager.characters[characterIndex].characterName);
    }
}


[Serializable]
public class DialogueRequirePlayerClickContinueNode : DialogueBlockNode
{
    public bool enabled = true;
}