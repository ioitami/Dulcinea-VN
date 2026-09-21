using System.Collections.Generic;

public static class DialogueRegistry
{
    private static readonly Dictionary<string, DialogueGroup> groupsByID = new Dictionary<string, DialogueGroup>();
    private static readonly Dictionary<string, DialogueBlock> blocksByID = new Dictionary<string, DialogueBlock>();

    public static void RegisterGroup(DialogueGroup group)
    {
        if (group == null || string.IsNullOrEmpty(group.ID)) return;
        groupsByID[group.ID.Trim()] = group;
    }

    public static void UnregisterGroup(DialogueGroup group)
    {
        if (group == null || string.IsNullOrEmpty(group.ID)) return;

        if (groupsByID.TryGetValue(group.ID.Trim(), out DialogueGroup current) && current == group)
            groupsByID.Remove(group.ID.Trim());
    }

    public static void RegisterBlock(DialogueBlock block)
    {
        if (block == null || string.IsNullOrEmpty(block.ID)) return;
        blocksByID[block.ID.Trim()] = block;
    }

    public static void UnregisterBlock(DialogueBlock block)
    {
        if (block == null || string.IsNullOrEmpty(block.ID)) return;

        if (blocksByID.TryGetValue(block.ID.Trim(), out DialogueBlock current) && current == block)
            blocksByID.Remove(block.ID.Trim());
    }

    public static bool TryGetGroup(string groupID, out DialogueGroup group)
    {
        group = null;
        if (string.IsNullOrEmpty(groupID)) return false;
        return groupsByID.TryGetValue(groupID.Trim(), out group);
    }

    public static bool TryGetBlock(string blockID, out DialogueBlock block)
    {
        block = null;
        if (string.IsNullOrEmpty(blockID)) return false;
        return blocksByID.TryGetValue(blockID.Trim(), out block);
    }
}