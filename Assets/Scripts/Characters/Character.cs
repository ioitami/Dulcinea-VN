using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterMood
{
    public string moodName;
    public Sprite sprite;
}

public class Character : MonoBehaviour
{
    [Header("Character Settings")]
    public string characterName;

    [Header("Moods")]
    public List<CharacterMood> moods = new List<CharacterMood>();

    [HideInInspector] public GameObject container;
    [HideInInspector] public SpriteRenderer spriteRenderer;

    public GameObject ingameContainerObj;




}
