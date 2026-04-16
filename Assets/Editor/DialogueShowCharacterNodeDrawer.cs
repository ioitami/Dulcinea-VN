using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(DialogueTextNode))]
public class DialogueTextNodeDrawer : PropertyDrawer
{
    const float spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CharacterManager manager = Object.FindFirstObjectByType<CharacterManager>();

        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float w = position.width;
        float line = EditorGUIUtility.singleLineHeight;

        SerializedProperty text = property.FindPropertyRelative("text");
        SerializedProperty appendText = property.FindPropertyRelative("appendText");
        SerializedProperty requireClick = property.FindPropertyRelative("requirePlayerClickContinue");
        SerializedProperty overwriteSpeed = property.FindPropertyRelative("overwriteTextSpeed");
        SerializedProperty textSpeed = property.FindPropertyRelative("textSpeed");
        SerializedProperty characterIndex = property.FindPropertyRelative("characterIndex");

        // Character dropdown with None option
        if (manager == null)
        {
            EditorGUI.LabelField(new Rect(position.x, y, w, line), "Character", "CharacterManager not found in scene");
        }
        else
        {
            // Build names list with None as first entry
            string[] characterNames = manager.characters.Select(c => c.characterName).ToArray();
            string[] options = new string[characterNames.Length + 1];
            options[0] = "None";
            for (int i = 0; i < characterNames.Length; i++)
                options[i + 1] = characterNames[i];

            // characterIndex -1 maps to dropdown index 0 (None)
            // characterIndex 0 maps to dropdown index 1, etc.
            int dropdownIndex = characterIndex.intValue + 1;
            if (dropdownIndex < 0 || dropdownIndex >= options.Length)
                dropdownIndex = 0;

            int selected = EditorGUI.Popup(
                new Rect(position.x, y, w, line),
                "Character", dropdownIndex, options
            );

            characterIndex.intValue = selected - 1;
        }

        y += line + spacing;

        // Text area
        float textHeight = EditorGUI.GetPropertyHeight(text);
        EditorGUI.PropertyField(new Rect(position.x, y, w, textHeight), text, new GUIContent("Text"));
        y += textHeight + spacing;

        // Append text toggle
        EditorGUI.PropertyField(new Rect(position.x, y, w, line), appendText);
        y += line + spacing;

        // Require click toggle
        EditorGUI.PropertyField(new Rect(position.x, y, w, line), requireClick);
        y += line + spacing;

        // Overwrite speed toggle
        EditorGUI.PropertyField(new Rect(position.x, y, w, line), overwriteSpeed);
        y += line + spacing;

        // Text speed — only show if overwrite is enabled
        if (overwriteSpeed.boolValue)
        {
            EditorGUI.PropertyField(new Rect(position.x, y, w, line), textSpeed);
            y += line + spacing;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float height = 0f;

        height += line + spacing; // character dropdown
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("text")) + spacing;
        height += line + spacing; // appendText
        height += line + spacing; // requireClick
        height += line + spacing; // overwriteSpeed

        if (property.FindPropertyRelative("overwriteTextSpeed").boolValue)
            height += line + spacing; // textSpeed

        return height;
    }
}