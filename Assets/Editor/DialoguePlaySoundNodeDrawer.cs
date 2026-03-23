using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(DialoguePlaySoundNode))]
public class DialoguePlaySoundNodeDrawer : PropertyDrawer
{
    const float spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        AudioManager audio = Object.FindFirstObjectByType<AudioManager>();

        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float w = position.width;
        float line = EditorGUIUtility.singleLineHeight;

        SerializedProperty command = property.FindPropertyRelative("command");
        SerializedProperty category = property.FindPropertyRelative("category");
        SerializedProperty clipName = property.FindPropertyRelative("clipName");
        SerializedProperty fadeOut = property.FindPropertyRelative("fadeOutDuration");

        // Command (Play / Stop)
        EditorGUI.PropertyField(new Rect(position.x, y, w, line), command);
        y += line + spacing;

        // Category (BGM / SFX / Character)
        EditorGUI.PropertyField(new Rect(position.x, y, w, line), category);
        y += line + spacing;

        AudioCommand cmd = (AudioCommand)command.enumValueIndex;
        AudioCategory cat = (AudioCategory)category.enumValueIndex;

        // Clip name dropdown — sourced from AudioManager lists in scene
        if (cmd == AudioCommand.Play)
        {
            if (audio == null)
            {
                EditorGUI.LabelField(new Rect(position.x, y, w, line), "Clip", "AudioManager not found in scene");
            }
            else
            {
                string[] names = GetNamesForCategory(audio, cat);

                if (names.Length == 0)
                {
                    EditorGUI.LabelField(new Rect(position.x, y, w, line), "Clip", "No clips in this category");
                }
                else
                {
                    int currentIndex = System.Array.IndexOf(names, clipName.stringValue);
                    if (currentIndex < 0) currentIndex = 0;

                    int selected = EditorGUI.Popup(new Rect(position.x, y, w, line), "Clip", currentIndex, names);
                    clipName.stringValue = names[selected];
                }
            }
            y += line + spacing;
        }

        // Fade out duration — only relevant for Stop BGM
        if (cmd == AudioCommand.Stop && cat == AudioCategory.BGM)
        {
            EditorGUI.PropertyField(new Rect(position.x, y, w, line), fadeOut, new GUIContent("Fade Out Duration"));
            y += line + spacing;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float s = spacing;

        SerializedProperty command = property.FindPropertyRelative("command");
        SerializedProperty category = property.FindPropertyRelative("category");

        AudioCommand cmd = (AudioCommand)command.enumValueIndex;
        AudioCategory cat = (AudioCategory)category.enumValueIndex;

        float height = (line + s) * 2; // command + category always shown

        if (cmd == AudioCommand.Play)
            height += line + s; // clip dropdown

        if (cmd == AudioCommand.Stop && cat == AudioCategory.BGM)
            height += line + s; // fade duration

        return height;
    }

    private string[] GetNamesForCategory(AudioManager audio, AudioCategory cat)
    {
        return cat switch
        {
            AudioCategory.BGM => audio.BGMList.Select(a => a.audioName).ToArray(),
            AudioCategory.SFX => audio.SFXList.Select(a => a.audioName).ToArray(),
            AudioCategory.Character => audio.CharacterVoiceList.Select(a => a.audioName).ToArray(),
            _ => new string[0]
        };
    }
}