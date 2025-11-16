using Ink.Runtime;
using System.Globalization;
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

            case "playsound":
                // Format: #command playsound bgm mainmenuBGM
                if (tags.Length == 4)
                {
                    string playcommand = tags[2].ToLower();

                    string AudioName = tags[3];

                    if (playcommand == "bgm")
                    {
                        GameSingleton.instance.audioManager.PlayBGM(AudioName);
                    }
                    else if (playcommand == "sfx")
                    {
                        GameSingleton.instance.audioManager.PlaySFX(AudioName);
                    }
                    else if (playcommand == "character")
                    {
                        GameSingleton.instance.audioManager.PlayCharacter(AudioName);
                    }
                }
                break;

            case "adjustvolume":
                // Format: #command adjustvolume bgm 0.5 (0-1 volume)
                if (tags.Length == 3)
                {
                    string playcommand = tags[2].ToLower();

                    float volume = 1f;

                    try
                    {
                        volume = float.Parse(tags[3]);
                    }
                    catch (System.FormatException e)
                    {
                        Debug.LogError("Invalid string format: " + e.Message);
                    }
                    catch (System.OverflowException e)
                    {
                        Debug.LogError("Value too large or too small for a float: " + e.Message);
                    }


                    if (playcommand == "bgm")
                    {
                        GameSingleton.instance.audioManager.BGMVolume = volume;
                    }
                    else if (playcommand == "sfx")
                    {
                        GameSingleton.instance.audioManager.SFXVolume = volume;
                    }
                    else if (playcommand == "character")
                    {
                        GameSingleton.instance.audioManager.CharacterVolume = volume;
                    }
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