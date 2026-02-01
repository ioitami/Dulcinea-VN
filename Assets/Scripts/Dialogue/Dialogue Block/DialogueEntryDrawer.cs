using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueEntry))]
public class DialogueEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty entryType = property.FindPropertyRelative("entryType");
        SerializedProperty text = property.FindPropertyRelative("text");
        SerializedProperty command = property.FindPropertyRelative("command");
        SerializedProperty overwriteTextSpeed = property.FindPropertyRelative("overwriteTextSpeed");
        SerializedProperty scriptEvent = property.FindPropertyRelative("scriptEvent");

        float y = position.y;
        float width = position.width;

        // Entry Type
        Rect entryTypeRect = new Rect(position.x, y, width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(entryTypeRect, entryType);
        y += entryTypeRect.height + EditorGUIUtility.standardVerticalSpacing;

        // TEXT
        if ((EntryType)entryType.enumValueIndex == EntryType.Text)
        {
            float textHeight = EditorGUI.GetPropertyHeight(text, true);
            Rect textRect = new Rect(position.x, y, width, textHeight);
            EditorGUI.PropertyField(textRect, text, true);
        }
        // COMMAND
        else
        {
            // Command dropdown
            Rect commandRect = new Rect(position.x, y, width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(commandRect, command);
            y += commandRect.height + EditorGUIUtility.standardVerticalSpacing;

            DialogueCommand cmd = (DialogueCommand)command.enumValueIndex;

            // OverwriteTextSpeed
            if (cmd == DialogueCommand.OverwriteTextSpeed)
            {
                Rect speedRect = new Rect(position.x, y, width, EditorGUIUtility.singleLineHeight);

                EditorGUI.BeginChangeCheck();
                float newValue = EditorGUI.FloatField(
                    speedRect,
                    "Text Speed",
                    overwriteTextSpeed.floatValue
                );
                if (EditorGUI.EndChangeCheck())
                {
                    overwriteTextSpeed.floatValue = Mathf.Max(0f, newValue);
                }
            }
            // Script Event
            else if (cmd == DialogueCommand.Script)
            {
                float eventHeight = EditorGUI.GetPropertyHeight(scriptEvent, true);
                Rect eventRect = new Rect(position.x, y, width, eventHeight);
                EditorGUI.PropertyField(eventRect, scriptEvent, true);
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty entryType = property.FindPropertyRelative("entryType");
        SerializedProperty text = property.FindPropertyRelative("text");
        SerializedProperty command = property.FindPropertyRelative("command");
        SerializedProperty scriptEvent = property.FindPropertyRelative("scriptEvent");

        float height = EditorGUIUtility.singleLineHeight
                     + EditorGUIUtility.standardVerticalSpacing;

        if ((EntryType)entryType.enumValueIndex == EntryType.Text)
        {
            height += EditorGUI.GetPropertyHeight(text, true);
        }
        else
        {
            height += EditorGUIUtility.singleLineHeight
                    + EditorGUIUtility.standardVerticalSpacing;

            DialogueCommand cmd = (DialogueCommand)command.enumValueIndex;

            if (cmd == DialogueCommand.OverwriteTextSpeed)
            {
                height += EditorGUIUtility.singleLineHeight;
            }
            else if (cmd == DialogueCommand.Script)
            {
                height += EditorGUI.GetPropertyHeight(scriptEvent, true);
            }
        }

        return height;
    }
}
