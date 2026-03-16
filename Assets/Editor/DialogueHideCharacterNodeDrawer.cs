using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(DialogueHideCharacterNode))]
public class DialogueHideCharacterNodeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CharacterManager manager = GameObject.FindObjectOfType<CharacterManager>();

        if (manager == null)
        {
            EditorGUI.LabelField(position, "CharacterManager not found");
            return;
        }

        SerializedProperty charIndex = property.FindPropertyRelative("characterIndex");

        string[] names = manager.characters.Select(c => c.characterName).ToArray();

        charIndex.intValue = EditorGUI.Popup(
            position,
            "Character",
            charIndex.intValue,
            names
        );
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}