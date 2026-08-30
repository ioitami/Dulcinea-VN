using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(DialogueSetBackgroundNode))]
public class DialogueSetBackgroundNodeDrawer : PropertyDrawer
{
    const float spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        BackgroundManager backgroundManager = Object.FindFirstObjectByType<BackgroundManager>();

        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float w = position.width;
        float line = EditorGUIUtility.singleLineHeight;

        SerializedProperty target = property.FindPropertyRelative("target");
        SerializedProperty backgroundName = property.FindPropertyRelative("backgroundName");

        // Target (MainMenu / Window1 / Window2)
        EditorGUI.PropertyField(new Rect(position.x, y, w, line), target);
        y += line + spacing;

        // Background name — dropdown sourced from BackgroundManager's list
        // in scene, falls back to a plain text field if none is found.
        if (backgroundManager == null || backgroundManager.backgrounds == null || backgroundManager.backgrounds.Count == 0)
        {
            EditorGUI.PropertyField(new Rect(position.x, y, w, line), backgroundName, new GUIContent("Background Name"));
        }
        else
        {
            string[] names = backgroundManager.backgrounds.Select(b => b.backgroundName).ToArray();

            int currentIndex = System.Array.IndexOf(names, backgroundName.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            int selected = EditorGUI.Popup(new Rect(position.x, y, w, line), "Background Name", currentIndex, names);
            backgroundName.stringValue = names[selected];
        }
        y += line + spacing;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float s = spacing;

        return (line + s) * 2; // target + background name
    }
}