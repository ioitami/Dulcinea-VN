public static class DialogueLookup
{
    public static bool TryFindBlockByID(string blockID, out DialogueBlock block)
    {
        return DialogueRegistry.TryGetBlock(blockID, out block);
    }

    public static bool TryFindGroupAndBlock(string groupID, string blockID, out DialogueGroup group, out DialogueBlock block)
    {
        block = null;

        if (!DialogueRegistry.TryGetGroup(groupID, out group))
            return false;

        if (!string.IsNullOrEmpty(blockID))
            DialogueRegistry.TryGetBlock(blockID, out block);

        return true;
    }
}