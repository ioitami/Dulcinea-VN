using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterManager : MonoBehaviour
{
    [SerializeField]
    public Transform characterSpriteParent;

    [Header("Characters Setup")]
    public List<Character> characters = new List<Character>();

    //NOTE: MAKE IT RELATIVE TO CAMERA POSITION AND ACCOUNT FOR SCREEN SIZE OF WINDOW
    [Header("Default Anchors (Local Space)")]
    public List<CharacterPosPresets> customPositions = new List<CharacterPosPresets>();



    private void Awake()
    {
        InitializeCharacters();
    }



    public void InitializeCharacters()
    {

        foreach (Character c in characters)
        {
            GameObject container = c.ingameContainerObj;

            // Create container object
            container.transform.SetParent(characterSpriteParent, true);
            container.transform.localPosition = Vector3.zero;

            c.ingameContainerObj = container;

            c.currentMood = c.moods[0];
            SetCharacterMood(c.characterName, 0);

            HideCharacter(c.characterName);
        }

    }

    public void SetCharacterSpriteParent(Transform transform)
    {
        characterSpriteParent = transform;
    }

    public Character GetCharacter(string name)
    {
        for(int i = 0; i < characters.Count; i++)
        {
            if (characters[i].characterName.ToLower() == name.ToLower())
            {
                return characters[i];
            }
        }

        Debug.Log("No character found with name " + name);
        return null;
    }

    public Character GetCharacter(int characterID)
    {
        if (characters[characterID] == null)
        {
            Debug.Log("No character found with index " + characterID);
            return null;
        }
        else
        {
            return characters[characterID];
        }
    }

    public void ShowCharacter(string name, string mood)
    {
        Character character = GetCharacter(name);

        if (character == null) return;

        character.ingameContainerObj.SetActive(true);
        SetCharacterMood(name, mood);
    }

    public void ShowCharacter(int characterID, string mood)
    {
        Character character = GetCharacter(characterID);

        if (character == null) return;

        character.ingameContainerObj.SetActive(true);
        SetCharacterMood(name, mood);
    }

    public void ShowCharacter(string name, string mood, string positionName = null)
    {
        Character character = GetCharacter(name);

        if (character == null) return;

        character.ingameContainerObj.SetActive(true);
        SetCharacterMood(name, mood);


        if (positionName == null) return;

        Vector3 pos = Vector3.zero;
        foreach (CharacterPosPresets preset in customPositions)
        {
            if (preset.positionName.ToLower() == positionName.ToLower())
            {
                pos = preset.position;
            }
        }
        MoveCharacter(name, pos);
    }

    public void ShowCharacter(string name, string mood = null, Vector3? position = null)
    {
        Character character = GetCharacter(name);

        if (character == null) return;

        character.ingameContainerObj.SetActive(true);

        if (mood == null) return;
            
        SetCharacterMood(name, mood);

        if (position == null) return;

        MoveCharacter(name, position.Value);

    }

    public void ShowCharacter(int characterID, string mood, string positionName = null)
    {
        Character character = GetCharacter(characterID);

        if (character == null) return;

        character.ingameContainerObj.SetActive(true);
        SetCharacterMood(name, mood);


        if (positionName == null) return;

        Vector3 pos = Vector3.zero;
        foreach (CharacterPosPresets preset in customPositions)
        {
            if (preset.positionName.ToLower() == positionName.ToLower())
            {
                pos = preset.position;
            }
        }
        MoveCharacter(name, pos);
    }

    public void ShowCharacter(int characterID, string mood = null, Vector3? position = null)
    {
        Character character = GetCharacter(characterID);

        if (character == null) return;

        character.ingameContainerObj.SetActive(true);

        if (mood == null) return;

        SetCharacterMood(name, mood);

        if (position == null) return;

        MoveCharacter(name, position.Value);

    }

    public void HideCharacter(string name)
    {
        Character character = GetCharacter(name);

        if (character == null) return;

        character.ingameContainerObj.SetActive(false);

    }

    public void HideCharacter(int characterID)
    {
        Character character = GetCharacter(characterID);

        if (character == null) return;

        character.ingameContainerObj.SetActive(false);

    }


    public void HideAllCharacters()
    {
        foreach (Character character in characters)
        {
            character.ingameContainerObj.SetActive(false);
        }
    }

    public void SetCharacterMood(string name, string mood)
    {
        Character character = GetCharacter(name);
        CharacterMood currentMood = character.moods.Find(m => m.moodName.ToLower() == mood.ToLower());
        character.currentMood = currentMood;
        Sprite charMoodSprite = currentMood.sprite;

        if (character != null)
        {
            character.ingameContainerObj.GetComponentInChildren<SpriteRenderer>().sprite = charMoodSprite;
        }
    }
    public void SetCharacterMood(string name, int moodID)
    {
        Character character = GetCharacter(name);
        character.currentMood = character.moods[moodID];

        if (character != null)
        {
            character.ingameContainerObj.GetComponentInChildren<SpriteRenderer>().sprite = character.currentMood.sprite;
        }
    }
    public void SetCharacterMood(int characterID, int moodID)
    {
        Character character = GetCharacter(characterID);
        character.currentMood = character.moods[moodID];

        if (character != null)
        {
            character.ingameContainerObj.GetComponentInChildren<SpriteRenderer>().sprite = character.currentMood.sprite;
        }
    }

    public void MoveCharacter(string name, Vector3 position)
    {
        Character character = GetCharacter(name);

        if (character != null)
        {
            character.ingameContainerObj.transform.localPosition = position;
        }
    }

    public void MoveCharacter(int characterID, Vector3 position)
    {
        Character character = GetCharacter(characterID);

        if (character != null)
        {
            character.ingameContainerObj.transform.localPosition = position;
        }
    }

    public void PlayAnimationCharacter(string charName, string animName, System.Action onComplete = null)
    {
        Character character = GetCharacter(charName);

        if (character == null) return;

        SpriteAnimationManager animationManager = GameSingleton.instance.spriteAnimationManager;

        if (animationManager == null) 
        {
            Debug.Log("NO ANIMATIONMANAGER DETECTED");
            return;
        }

        animationManager.PlayAnimation(animationName:animName, spriteTransform: character.ingameContainerObj.transform, onComplete: onComplete);

    }
    public void PlayAnimationCharacter(int characterID, string animName, System.Action onComplete = null)
    {
        Character character = GetCharacter(characterID);

        if (character == null) return;

        SpriteAnimationManager animationManager = GameSingleton.instance.spriteAnimationManager;

        if (animationManager == null)
        {
            Debug.Log("NO ANIMATIONMANAGER DETECTED");
            return;
        }

        animationManager.PlayAnimation(animationName: animName, spriteTransform: character.ingameContainerObj.transform, onComplete: onComplete);

    }

}

[System.Serializable]
public class CharacterPosPresets
{
    public string positionName;
    public Vector3 position;
}

