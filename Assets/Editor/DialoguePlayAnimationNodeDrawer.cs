using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(DialoguePlayAnimationNode))]
public class DialoguePlayAnimationNodeDrawer : PropertyDrawer
{
    const float spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CharacterManager characterManager = Object.FindFirstObjectByType<CharacterManager>();
        SpriteAnimationManager animationManager = Object.FindFirstObjectByType<SpriteAnimationManager>();

        if (characterManager == null)
        {
            EditorGUI.LabelField(position, "CharacterManager not found in scene");
            return;
        }

        if (animationManager == null)
        {
            EditorGUI.LabelField(position, "SpriteAnimationManager not found in scene");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float w = position.width;
        float line = EditorGUIUtility.singleLineHeight;

        SerializedProperty charIndex = property.FindPropertyRelative("characterIndex");
        SerializedProperty animName = property.FindPropertyRelative("animationName");
        SerializedProperty command = property.FindPropertyRelative("command");
        SerializedProperty waitFlag = property.FindPropertyRelative("waitForCompletion");

        // Character dropdown
        string[] charNames = characterManager.characters.Select(c => c.characterName).ToArray();
        charIndex.intValue = EditorGUI.Popup(
            new Rect(position.x, y, w, line),
            "Character", charIndex.intValue, charNames
        );
        y += line + spacing;

        // Animation dropdown
        string[] animNames = animationManager.animations.Select(a => a.animationName).ToArray();

        if (animNames.Length == 0)
        {
            EditorGUI.LabelField(new Rect(position.x, y, w, line), "Animation", "No animations found");
        }
        else
        {
            int currentIndex = System.Array.IndexOf(animNames, animName.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            int selected = EditorGUI.Popup(
                new Rect(position.x, y, w, line),
                "Animation", currentIndex, animNames
            );
            animName.stringValue = animNames[selected];
        }
        y += line + spacing;

        // Command dropdown (Play / Skip)
        EditorGUI.PropertyField(new Rect(position.x, y, w, line), command);
        y += line + spacing;

        // Wait for completion — only relevant for Play
        AnimationCommand cmd = (AnimationCommand)command.enumValueIndex;
        if (cmd == AnimationCommand.Play)
        {
            EditorGUI.PropertyField(new Rect(position.x, y, w, line), waitFlag);
            y += line + spacing;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;

        SerializedProperty command = property.FindPropertyRelative("command");
        AnimationCommand cmd = (AnimationCommand)command.enumValueIndex;

        float height = (line + spacing) * 3; // character + animation + command always shown

        if (cmd == AnimationCommand.Play)
            height += line + spacing; // waitForCompletion

        return height;
    }
}