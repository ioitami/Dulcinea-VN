using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;

    [SerializeField]
    public Transform characterSpriteParent_Window1;
    public Transform characterSpriteParent_Window2;

    [Header("Characters Setup")]
    public GameObject characterPrefab;
    public List<Character> characters = new List<Character>();

    [Header("Default Anchors (Local Space)")]
    public List<CharacterPosPresets> customPositions = new List<CharacterPosPresets>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (NetworkServer.active)
        {
            InitializeCharacters();
        }
    }

    public void InitializeCharacters()
    {
        foreach (Character c in characters)
        {
            SpawnCharacterContainer(c);
        }
    }

    private void SpawnCharacterContainer(Character c)
    {
        GameObject containerInstance = Instantiate(characterPrefab, Vector3.zero, Quaternion.identity);
        containerInstance.name = c.characterName + "_Container";

        NVLCharacterContainer networkedContainer = containerInstance.GetComponent<NVLCharacterContainer>();

        if (networkedContainer == null)
        {
            Debug.LogWarning($"[CharacterManager] characterPrefab is missing NVLCharacterContainer for '{c.characterName}'.");
            Destroy(containerInstance);
            return;
        }

        networkedContainer.characterStableID = c.GetStableID();
        networkedContainer.windowNumber = c.windowNumber;

        NetworkServer.Spawn(containerInstance);
    }

    public void RegisterSpawnedContainer(string stableID, GameObject container)
    {
        Character character = characters.Find(c => c.GetStableID() == stableID);

        if (character == null)
        {
            Debug.LogWarning($"[CharacterManager] No Character entry found for stable ID '{stableID}'.");
            return;
        }

        character.ingameContainerObj = container;

        NVLSyncSpriteRenderer spriteSync = container.GetComponentInChildren<NVLSyncSpriteRenderer>(true);

        if (spriteSync != null)
        {
            spriteSync.availableSprites = character.moods.ConvertAll(m => m.sprite).ToArray();
            spriteSync.ApplyCurrentSprite();
        }

        if (NetworkServer.active)
        {
            character.currentMood = character.moods[0];

            SpriteRenderer spriteRenderer = container.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = character.currentMood.sprite;
            }

            NVLCharacterContainer networkContainer = container.GetComponent<NVLCharacterContainer>();
            if (networkContainer != null)
            {
                networkContainer.SetVisualActive(false);
            }
        }
    }

    public Character GetCharacter(string name)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].GetStableID().ToLower() == name.ToLower())
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

        SetVisualActive(character, true);
        SetCharacterMood(name, mood);
    }

    public void ShowCharacter(int characterID, string mood)
    {
        Character character = GetCharacter(characterID);

        if (character == null) return;

        SetVisualActive(character, true);
        SetCharacterMood(name, mood);
    }

    public void ShowCharacter(string name, string mood, string positionName = null)
    {
        Character character = GetCharacter(name);

        if (character == null) return;

        SetVisualActive(character, true);
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

        SetVisualActive(character, true);

        if (mood == null) return;

        SetCharacterMood(name, mood);

        if (position == null) return;

        MoveCharacter(name, position.Value);
    }

    public void ShowCharacter(int characterID, string mood, string positionName = null)
    {
        Character character = GetCharacter(characterID);

        if (character == null) return;

        SetVisualActive(character, true);
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

        SetVisualActive(character, true);

        if (mood == null) return;

        SetCharacterMood(name, mood);

        if (position == null) return;

        MoveCharacter(name, position.Value);
    }

    public void HideCharacter(string name)
    {
        Character character = GetCharacter(name);

        if (character == null) return;

        SetVisualActive(character, false);
    }

    public void HideCharacter(int characterID)
    {
        Character character = GetCharacter(characterID);

        if (character == null) return;

        SetVisualActive(character, false);
    }

    public void HideAllCharacters()
    {
        foreach (Character character in characters)
        {
            SetVisualActive(character, false);
        }
    }

    private void SetVisualActive(Character character, bool active)
    {
        if (character.ingameContainerObj == null) return;

        NVLCharacterContainer container = character.ingameContainerObj.GetComponent<NVLCharacterContainer>();

        if (container != null)
        {
            container.SetVisualActive(active);
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

    public void ScaleCharacter(string name, Vector3 scale)
    {
        Character character = GetCharacter(name);

        if (character != null)
        {
            character.ingameContainerObj.transform.localScale = scale;
        }
    }

    public void ScaleCharacter(int characterID, Vector3 scale)
    {
        Character character = GetCharacter(characterID);

        if (character != null)
        {
            character.ingameContainerObj.transform.localScale = scale;
        }
    }

    public void RotateCharacter(string name, Quaternion rotation)
    {
        Character character = GetCharacter(name);

        if (character != null)
        {
            character.ingameContainerObj.transform.localRotation = rotation;
        }
    }

    public void RotateCharacter(int characterID, Quaternion rotation)
    {
        Character character = GetCharacter(characterID);

        if (character != null)
        {
            character.ingameContainerObj.transform.localRotation = rotation;
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

        animationManager.PlayAnimation(animationName: animName, spriteTransform: character.ingameContainerObj.transform, onComplete: onComplete);
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