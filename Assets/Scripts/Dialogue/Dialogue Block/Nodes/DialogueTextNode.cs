using System;
using UnityEngine;

[Serializable]
public class DialogueTextNode : DialogueBlockNode
{
    [TextArea(2, 5)]
    public string text;

    public bool overwriteTextSpeed;
    public float textSpeed = 0.04f;
}
