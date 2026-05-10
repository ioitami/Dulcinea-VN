using TMPro;
using UnityEngine;

public class ServerCommands : MonoBehaviour
{
    public TextMeshProUGUI NVLText;

    // ===========================
    // Command List
    // ===========================

    public void NVLTest1()
    {
        NVLText.text += "test1here";
    }

    public void NVLTest2(string text)
    {
        NVLText.text = text;
    }

    public void PlayGroup(string groupID)
    {
        DialogueGroup[] allGroups = GameObject.FindObjectsByType<DialogueGroup>(FindObjectsSortMode.None);

        foreach (DialogueGroup group in allGroups)
        {
            if (group.ID.Trim() == groupID.Trim())
            {
                GameSingleton.instance.dialogueManager.PlayGroup(group);
                return;
            }
        }

        Debug.LogWarning($"[ServerCommands] Group '{groupID}' not found.");
    }

    public void PlaySpecificBlockInGroup(string groupID, string blockID)
    {
        DialogueGroup[] allGroups = GameObject.FindObjectsByType<DialogueGroup>(FindObjectsSortMode.None);

        DialogueGroup targetGroup = null;
        DialogueBlock targetBlock = null;

        foreach (DialogueGroup group in allGroups)
        {
            if (group.ID.Trim() == groupID.Trim())
            {
                targetGroup = group;

                foreach (DialogueBlock block in group.blocks)
                {
                    if (block.ID.Trim() == blockID.Trim())
                    {
                        targetBlock = block;
                        break;
                    }
                }
                break;
            }
        }

        GameSingleton.instance.dialogueManager.PlaySpecificBlockInGroup(targetGroup, targetBlock);
    }

    public void StopFastForward()
    {
        GameSingleton.instance.dialogueManager.StopFastForward();
    }

    public void SetGlobalAllowDialogueClick(string allow)
    {
        bool value = bool.Parse(allow);
        GameSingleton.instance.dialogueManager.SetGlobalAllowDialogueClick(value);
    }

    // ===========================
    // Command Parser
    // ===========================

    public void ParseAndExecute(string rawCommand)
    {
        Debug.Log($"[ServerCommands] Received command: {rawCommand}");

        if (!rawCommand.StartsWith("<") || !rawCommand.EndsWith(">"))
        {
            Debug.LogWarning($"[ServerCommands] Invalid command format: '{rawCommand}'");
            return;
        }

        string inner = rawCommand.Substring(1, rawCommand.Length - 2);

        string functionName;
        string[] parameters;

        if (inner.Contains("("))
        {
            int paramStart = inner.IndexOf('(');
            functionName = inner.Substring(0, paramStart);

            string paramString = inner.Substring(paramStart + 1, inner.LastIndexOf(')') - paramStart - 1);
            parameters = paramString.Split(',');

            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = parameters[i].Trim();
        }
        else
        {
            functionName = inner;
            parameters = new string[0];
        }

        ExecuteCommand(functionName, parameters);
    }

    private void ExecuteCommand(string functionName, string[] parameters)
    {
        switch (functionName)
        {
            case "NVLTest1":
                NVLTest1();
                break;

            case "NVLTest2":
                if (parameters.Length == 1)
                    NVLTest2(parameters[0]);
                break;

            case "PlayGroup":
                if (parameters.Length == 1)
                    PlayGroup(parameters[0]);
                break;

            case "PlaySpecificBlockInGroup":
                if (parameters.Length == 2)
                    PlaySpecificBlockInGroup(parameters[0], parameters[1]);
                break;

            case "StopFastForward":
                StopFastForward();
                break;

            case "SetGlobalAllowDialogueClick":
                if (parameters.Length == 1)
                    SetGlobalAllowDialogueClick(parameters[0]);
                break;

            default:
                Debug.LogWarning($"[ServerCommands] Unknown command: '{functionName}'");
                break;
        }
    }
}