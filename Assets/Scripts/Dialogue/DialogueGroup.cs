using System.Collections.Generic;
using UnityEngine;

public class DialogueGroup : MonoBehaviour
{
    public string ID;
    public string chapterName;

    public List<DialogueBlock> blocks = new List<DialogueBlock>();

    private void Awake()
    {
        DialogueRegistry.RegisterGroup(this);
    }

    private void OnDestroy()
    {
        DialogueRegistry.UnregisterGroup(this);
    }

    private void OnValidate()
    {
        // Auto-populate blocks from children in order
        blocks.Clear();
        foreach (Transform child in transform)
        {
            DialogueBlock block = child.GetComponent<DialogueBlock>();
            if (block != null)
                blocks.Add(block);
        }
    }
}