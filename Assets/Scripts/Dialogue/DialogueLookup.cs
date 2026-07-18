using UnityEngine;

public static class DialogueLookup
{
    public static bool TryFindBlockByID(string blockID, out DialogueBlock block)
    {
        block = null;
        if (string.IsNullOrEmpty(blockID)) return false;

        DialogueBlock[] allBlocks = Object.FindObjectsByType<DialogueBlock>(FindObjectsSortMode.None);

        foreach (DialogueBlock candidate in allBlocks)
        {
            if (candidate.ID.Trim() == blockID.Trim())
            {
                block = candidate;
                return true;
            }
        }

        return false;
    }

    public static bool TryFindGroupAndBlock(string groupID, string blockID, out DialogueGroup group, out DialogueBlock block)
    {
        group = null;
        block = null;

        if (string.IsNullOrEmpty(groupID)) return false;

        DialogueGroup[] allGroups = Object.FindObjectsByType<DialogueGroup>(FindObjectsSortMode.None);

        foreach (DialogueGroup candidate in allGroups)
        {
            if (candidate.ID.Trim() != groupID.Trim()) continue;

            group = candidate;

            if (!string.IsNullOrEmpty(blockID))
            {
                foreach (DialogueBlock candidateBlock in candidate.blocks)
                {
                    if (candidateBlock.ID.Trim() == blockID.Trim())
                    {
                        block = candidateBlock;
                        break;
                    }
                }
            }

            break;
        }

        return group != null;
    }
}
