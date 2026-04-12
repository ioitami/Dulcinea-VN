using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueChoiceNode))]
public class DialogueChoiceNodeDrawer : PropertyDrawer
{
    const float spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float w = position.width;
        float line = EditorGUIUtility.singleLineHeight;

        SerializedProperty prefab = property.FindPropertyRelative("choicePrefab");
        SerializedProperty containerParent = property.FindPropertyRelative("choiceContainerParent");
        SerializedProperty choices = property.FindPropertyRelative("choices");

        // Prefab field
        EditorGUI.PropertyField(new Rect(position.x, y, w, line), prefab, new GUIContent("Choice Prefab"));
        y += line + spacing;

        // Container parent field
        EditorGUI.PropertyField(new Rect(position.x, y, w, line), containerParent, new GUIContent("Choice Container Parent"));
        y += line + spacing;

        // Choices list header
        EditorGUI.LabelField(new Rect(position.x, y, w, line), "Choices", EditorStyles.boldLabel);
        y += line + spacing;

        // Draw each choice
        for (int i = 0; i < choices.arraySize; i++)
        {
            SerializedProperty choice = choices.GetArrayElementAtIndex(i);
            SerializedProperty text = choice.FindPropertyRelative("text");
            SerializedProperty onSelect = choice.FindPropertyRelative("onSelected");

            // Choice box
            float choiceHeight = GetChoiceHeight(choice);
            EditorGUI.HelpBox(new Rect(position.x, y, w, choiceHeight), "", MessageType.None);

            float innerX = position.x + 4f;
            float innerW = w - 8f;
            float innerY = y + 4f;

            // Choice label + remove button
            EditorGUI.LabelField(new Rect(innerX, innerY, innerW - 30f, line), $"Choice {i + 1}", EditorStyles.boldLabel);

            if (GUI.Button(new Rect(innerX + innerW - 26f, innerY, 26f, line), "X"))
            {
                choices.DeleteArrayElementAtIndex(i);
                break;
            }

            innerY += line + spacing;

            // Text field
            EditorGUI.PropertyField(new Rect(innerX, innerY, innerW, line), text, new GUIContent("Text"));
            innerY += line + spacing;

            // Linked group field
            SerializedProperty linkedGroup = choice.FindPropertyRelative("linkedGroup");
            EditorGUI.PropertyField(new Rect(innerX, innerY, innerW, line), linkedGroup, new GUIContent("Linked Group"));
            innerY += line + spacing;

            // Linked block field
            SerializedProperty linkedBlock = choice.FindPropertyRelative("linkedBlock");
            EditorGUI.PropertyField(new Rect(innerX, innerY, innerW, line), linkedBlock, new GUIContent("Linked Block"));
            innerY += line + spacing;

            // OnSelected event
            float eventHeight = EditorGUI.GetPropertyHeight(onSelect);
            EditorGUI.PropertyField(new Rect(innerX, innerY, innerW, eventHeight), onSelect, new GUIContent("On Selected"));

            y += choiceHeight + spacing;
        }

        // Add choice button
        if (GUI.Button(new Rect(position.x, y, w, line), "+ Add Choice"))
        {
            choices.InsertArrayElementAtIndex(choices.arraySize);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float height = (line + spacing) * 3; // prefab + parent + choices header

        SerializedProperty choices = property.FindPropertyRelative("choices");
        for (int i = 0; i < choices.arraySize; i++)
        {
            height += GetChoiceHeight(choices.GetArrayElementAtIndex(i)) + spacing;
        }

        height += line + spacing; // add choice button

        return height;
    }

    private float GetChoiceHeight(SerializedProperty choice)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float padding = 8f;

        float eventHeight = EditorGUI.GetPropertyHeight(choice.FindPropertyRelative("onSelected"));

        return padding + line + spacing  // header row
                       + line + spacing  // text field
                       + line + spacing  // linked group
                       + line + spacing  // linked block
                       + eventHeight + spacing;
    }
}