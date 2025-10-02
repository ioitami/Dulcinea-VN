using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    [Header("Characters Setup")]
    public List<Character> characters = new List<Character>();

    //NOTE: MAKE IT RELATIVE TO CAMERA POSITION AND ACCOUNT FOR SCREEN SIZE OF WINDOW
    [Header("Default Anchors (World Space)")]
    public List<Vector3> customAnchors = new List<Vector3>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        InitializeCharacters();
    }

    private void Start()
    {
        //ShowCharacter("Dulcinea", "Default", centerAnchor);
       // ShowCharacter("Io", "Default", rightAnchor);
    }

    public void InitializeCharacters()
    {
        GameObject storyCanvas = GameObject.Find("CharacterCanvas");
        Transform parent = storyCanvas.transform.Find("CharacterSpriteParent");

        foreach (Character c in characters)
        {
            GameObject container;

            // Create container object
            container = new GameObject(c.characterName + "_Container");
            container.transform.SetParent(parent, true);
            container.transform.localPosition = Vector3.zero;

            c.ingameContainerObj = container;

            // Add SpriteRenderer as child
            GameObject spriteObj = new GameObject(c + "_Sprite");
            spriteObj.transform.SetParent(container.transform, true);
            spriteObj.transform.localPosition = Vector3.zero;

            SpriteRenderer spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();
            SetCharacterMood(c.characterName, 0);
            spriteRenderer.sortingOrder = 1; // ensure in front of background

            HideCharacter(c.characterName);
        }

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

        return characters[0]; // default to first character if not found
    }
    public void ShowCharacter(string name, string mood, int positionID)
    {
        Character character = GetCharacter(name);

        if (character == null) return;

        character.ingameContainerObj.SetActive(true);

        SetCharacterMood(name, mood);

        MoveCharacter(name, customAnchors[positionID]);

    }
    public void ShowCharacter(string name, string mood = null, Vector3? position = null)
    {
        Character character = GetCharacter(name);

        if (character == null) return;

        character.ingameContainerObj.SetActive(true);
        
        if (mood != null)
        {
            SetCharacterMood(name, mood);
        }

        if(position != null)
        {
            MoveCharacter(name, position.Value);
        }
    }

    public void HideCharacter(string name)
    {
        Character character = GetCharacter(name);

        if (character == null) return;

        character.ingameContainerObj.SetActive(false);

    }

    public void SetCharacterMood(string name, string mood)
    {
        Character character = GetCharacter(name);
        Sprite charMoodSprite = character.moods.Find(m => m.moodName.ToLower() == mood.ToLower())?.sprite;

        if (character != null)
        {
            character.ingameContainerObj.GetComponentInChildren<SpriteRenderer>().sprite = charMoodSprite;
        }
    }
    public void SetCharacterMood(string name, int moodID)
    {
        Character character = GetCharacter(name);

        if (character != null)
        {
            character.ingameContainerObj.GetComponentInChildren<SpriteRenderer>().sprite = character.moods[moodID].sprite;
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

}
