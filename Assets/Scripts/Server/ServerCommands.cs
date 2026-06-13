using TMPro;
using UnityEngine;

public class ServerCommands : MonoBehaviour
{

    // ===========================
    // Server Status Commands
    // ===========================

    public void SetRequiresServer(string requiresServer)
    {
        bool value = bool.Parse(requiresServer);
        Debug.Log($"[ServerCommands] SetRequiresServer: {value}");

        if (!value)
        {
            Debug.Log("[ServerCommands] requiresServer is false. Closing client window.");
            Application.Quit();
            return;
        }
    }

    // ===========================
    // Dialogue Commands
    // ===========================

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

        if (targetGroup == null)
        {
            Debug.LogWarning($"[ServerCommands] Group '{groupID}' not found.");
            return;
        }

        GameSingleton.instance.dialogueManager.PlaySpecificBlockInGroup(targetGroup, targetBlock);
    }

    public void DialogueContinueClicked()
    {
        GameSingleton.instance.dialogueManager.DialogueContinueClicked();
    }

    public void StartFastForward()
    {
        GameSingleton.instance.dialogueManager.StartFastForward();
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

    public void RegisterVisitedBlock(string blockID)
    {
        GameSingleton.instance.gameStateManager.RegisterVisitedBlock(blockID);
    }

    // ===========================
    // Character Commands
    // ===========================

    public void ShowCharacter(string charName, string moodName, string posX, string posY, string posZ)
    {
        Vector3 position = new Vector3(
            float.Parse(posX),
            float.Parse(posY),
            float.Parse(posZ)
        );

        GameSingleton.instance.characterManager.ShowCharacter(charName, moodName, position);
    }

    public void HideCharacter(string charName)
    {
        GameSingleton.instance.characterManager.HideCharacter(charName);
    }

    public void HideAllCharacters()
    {
        GameSingleton.instance.characterManager.HideAllCharacters();
    }

    public void SetCharacterMood(string charName, string moodName)
    {
        GameSingleton.instance.characterManager.SetCharacterMood(charName, moodName);
    }

    public void MoveCharacter(string charName, string posX, string posY, string posZ)
    {
        Vector3 position = new Vector3(
            float.Parse(posX),
            float.Parse(posY),
            float.Parse(posZ)
        );

        GameSingleton.instance.characterManager.MoveCharacter(charName, position);
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
            case "SetRequiresServer":
                if (parameters.Length == 1)
                    SetRequiresServer(parameters[0]);
                break;

            case "PlayGroup":
                if (parameters.Length == 1)
                    PlayGroup(parameters[0]);
                break;

            case "PlaySpecificBlockInGroup":
                if (parameters.Length == 2)
                    PlaySpecificBlockInGroup(parameters[0], parameters[1]);
                break;

            case "DialogueContinueClicked":
                DialogueContinueClicked();
                break;

            case "StartFastForward":
                StartFastForward();
                break;

            case "StopFastForward":
                StopFastForward();
                break;

            case "SetGlobalAllowDialogueClick":
                if (parameters.Length == 1)
                    SetGlobalAllowDialogueClick(parameters[0]);
                break;

            case "RegisterVisitedBlock":
                if (parameters.Length == 1)
                    RegisterVisitedBlock(parameters[0]);
                break;

            case "ShowCharacter":
                if (parameters.Length == 5)
                    ShowCharacter(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4]);
                break;

            case "HideCharacter":
                if (parameters.Length == 1)
                    HideCharacter(parameters[0]);
                break;

            case "HideAllCharacters":
                HideAllCharacters();
                break;

            case "SetCharacterMood":
                if (parameters.Length == 2)
                    SetCharacterMood(parameters[0], parameters[1]);
                break;

            case "MoveCharacter":
                if (parameters.Length == 4)
                    MoveCharacter(parameters[0], parameters[1], parameters[2], parameters[3]);
                break;


            default:
                Debug.LogWarning($"[ServerCommands] Unknown command: '{functionName}'");
                break;
        }
    }
}