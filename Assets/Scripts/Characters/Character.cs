using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

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

    public CharacterMood currentMood;
    public GameObject ingameContainerObj;
}

[System.Serializable]
public struct CharacterTransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public CharacterTransformData(Vector3 pos, Quaternion rot, Vector3 scl)
    {
        position = pos;
        rotation = rot;
        scale = scl;
    }
}
