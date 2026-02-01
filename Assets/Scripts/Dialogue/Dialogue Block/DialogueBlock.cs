using UnityEngine;
using UnityEngine.Events;


public class DialogueBlock : MonoBehaviour
{
    public string id;
    public DialogueEntry[] entries;
}

[System.Serializable]
public class DialogueEntry
{
    public EntryType entryType;

    [TextArea(2, 5)]
    public string text;

    public DialogueCommand command;

    public float overwriteTextSpeed;

    public UnityEvent scriptEvent;
}

public enum EntryType
{
    Text,
    Command
}

public enum DialogueCommand
{
    Pause,
    Choice,
    OverwriteTextSpeed,
    Script
    // Add more commands as needed
}