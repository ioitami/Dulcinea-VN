using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(DialogueShowCharacterNode))]
public class DialogueShowCharacterNodeDrawer : PropertyDrawer
{
    const float spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CharacterManager manager = Object.FindFirstObjectByType<CharacterManager>();

        if (manager == null)
        {
            EditorGUI.LabelField(position, "CharacterManager not found in scene");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float width = position.width;

        SerializedProperty charIndex = property.FindPropertyRelative("characterIndex");
        SerializedProperty moodIndex = property.FindPropertyRelative("moodIndex");

        SerializedProperty scaleCommand = property.FindPropertyRelative("scaleCommand");
        SerializedProperty scale = property.FindPropertyRelative("scale");

        SerializedProperty positionCommand = property.FindPropertyRelative("positionCommand");
        SerializedProperty positionMode = property.FindPropertyRelative("positionMode");
        SerializedProperty presetIndex = property.FindPropertyRelative("presetPositionIndex");
        SerializedProperty manualPos = property.FindPropertyRelative("manualPosition");

        // Character dropdown
        string[] charNames = manager.characters.Select(c => c.characterName).ToArray();

        charIndex.intValue = EditorGUI.Popup(
            new Rect(position.x, y, width, EditorGUIUtility.singleLineHeight),
            "Character",
            charIndex.intValue,
            charNames
        );

        y += EditorGUIUtility.singleLineHeight + spacing;

        // Mood dropdown
        if (charIndex.intValue >= 0 && charIndex.intValue < manager.characters.Count)
        {
            string[] moods = manager.characters[charIndex.intValue].moods
                .Select(m => m.moodName)
                .ToArray();

            moodIndex.intValue = EditorGUI.Popup(
                new Rect(position.x, y, width, EditorGUIUtility.singleLineHeight),
                "Mood",
                moodIndex.intValue,
                moods
            );
        }

        y += EditorGUIUtility.singleLineHeight + spacing;

        // Scale toggle
        float h = EditorGUI.GetPropertyHeight(scaleCommand);
        EditorGUI.PropertyField(new Rect(position.x, y, width, h), scaleCommand);
        y += h + spacing;


        h = EditorGUI.GetPropertyHeight(scale);
        EditorGUI.PropertyField(new Rect(position.x, y, width, h), scale);
        y += h + spacing;


        // Position toggle
        h = EditorGUI.GetPropertyHeight(positionCommand);
        EditorGUI.PropertyField(new Rect(position.x, y, width, h), positionCommand);
        y += h + spacing;


        h = EditorGUI.GetPropertyHeight(positionMode);
        EditorGUI.PropertyField(new Rect(position.x, y, width, h), positionMode);
        y += h + spacing;

        if ((PositionMode)positionMode.enumValueIndex == PositionMode.Preset)
        {
            string[] presetNames = manager.customPositions
                .Select(p => p.positionName)
                .ToArray();

            presetIndex.intValue = EditorGUI.Popup(
                new Rect(position.x, y, width, EditorGUIUtility.singleLineHeight),
                "Preset Position",
                presetIndex.intValue,
                presetNames
            );

            y += EditorGUIUtility.singleLineHeight + spacing;
        }
        else
        {
            h = EditorGUI.GetPropertyHeight(manualPos);
            EditorGUI.PropertyField(new Rect(position.x, y, width, h), manualPos);
            y += h + spacing;
        }


        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0;
        float spacing = 2f;

        float line = EditorGUIUtility.singleLineHeight;

        height += line + spacing; // character
        height += line + spacing; // mood

        height += line + spacing; // scale toggle

        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("scale")) + spacing;

        height += line + spacing; // position toggle


        height += line + spacing; // mode

        SerializedProperty mode = property.FindPropertyRelative("positionMode");

        if ((PositionMode)mode.enumValueIndex == PositionMode.Preset)
            height += line + spacing;
        else
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("manualPosition")) + spacing;

        height += line * 3f; // extra padding

        return height;
    }


}