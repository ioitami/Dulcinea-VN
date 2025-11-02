using UnityEngine;

public class DialogueTagManager : MonoBehaviour
{
    public void HandleCommandTags(string[] tags)
    {
        string command = tags[1].ToLower();

        switch (command)
        {
            case "showcharacter":
                // Format: #command showcharacter CharacterName Mood positionName
                if (tags.Length >= 3)
                {
                    string characterName = tags[2];
                    string mood = tags[3];
                    string positionName = tags[4];
                    GameSingleton.instance.characterManager.ShowCharacter(characterName, mood, positionName);
                }
                break;

            case "playanimationcharacter":
                // Format: #command animate CharacterName AnimationName action(optional)
                if (tags.Length >= 3)
                {
                    string characterName = tags[2];
                    string animationName = tags[3];

                    GameSingleton.instance.characterManager.PlayAnimationCharacter(characterName,animationName);

                }
                break;

            default:
                Debug.LogWarning($"Unknown command tag: {command}");
                break;
        }

    }

    public void HandleSCommandTags(string[] tags)
    {
        string command = tags[1].ToLower();

        switch (command)
        {
            default:
                Debug.LogWarning($"Unknown command tag: {command}");
                break;
        }

    }

    public void HandleIDTags(string[] tags)
    {
        string id = tags[1];

        GameSingleton.instance.gameStateManager.readLineSave.MarkAsRead(id);
    }
}