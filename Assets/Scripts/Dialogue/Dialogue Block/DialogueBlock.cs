using System;
using UnityEngine;
using UnityEngine.Events;


public class DialogueBlock : MonoBehaviour
{
    public int ID;

    [SerializeReference]
    public DialogueBlockNode[] nodes;
}


[System.Serializable]
public class DialogueEntry
{
    public EntryType entryType;

    [TextArea(2, 5)]
    public string text;
    

    public bool overwriteTextSpeed;
    public float textSpeed;

    public DialogueCommand command;

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
    Script,
    PlayerCanClick
    // Add more commands as needed
}