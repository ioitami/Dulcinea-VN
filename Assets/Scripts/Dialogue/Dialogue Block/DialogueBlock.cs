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
public class DialogueRequirePlayerClickContinueNode : DialogueBlockNode
{
    public bool enabled = true;
}