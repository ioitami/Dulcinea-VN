using System.Collections.Generic;
using UnityEngine;

public class DialogueGroup : MonoBehaviour
{
    public string ID;

    public List<DialogueBlock> blocks = new List<DialogueBlock>();

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