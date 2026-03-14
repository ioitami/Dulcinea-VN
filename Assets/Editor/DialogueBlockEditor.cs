using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(DialogueBlock))]
public class DialogueBlockEditor : Editor
{
    SerializedProperty nodes;
    SerializedProperty id;

    bool[] foldouts;

    void OnEnable()
    {
        nodes = serializedObject.FindProperty("nodes");
        id = serializedObject.FindProperty("ID");

        EnsureFoldoutArray();
    }

    void EnsureFoldoutArray()
    {
        if (foldouts == null || foldouts.Length != nodes.arraySize)
        {
            foldouts = new bool[nodes.arraySize];
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(id);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dialogue Nodes", EditorStyles.boldLabel);

        EnsureFoldoutArray();

        for (int i = 0; i < nodes.arraySize; i++)
        {
            SerializedProperty node = nodes.GetArrayElementAtIndex(i);

            if (node.managedReferenceValue == null)
                continue;

            DrawNode(node, i);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("+ Add Node", GUILayout.Height(30)))
        {
            ShowAddMenu();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawNode(SerializedProperty node, int index)
    {
        var parts = node.managedReferenceFullTypename.Split('.');
        string typeName = parts[parts.Length - 1];


        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        foldouts[index] = EditorGUILayout.Foldout(
            foldouts[index],
            $"{index}. {typeName}",
            true,
            EditorStyles.foldoutHeader
        );

        GUILayout.FlexibleSpace();

        GUI.enabled = index > 0;
        if (GUILayout.Button("Å™", GUILayout.Width(25)))
        {
            nodes.MoveArrayElement(index, index - 1);
        }

        GUI.enabled = index < nodes.arraySize - 1;
        if (GUILayout.Button("Å´", GUILayout.Width(25)))
        {
            nodes.MoveArrayElement(index, index + 1);
        }

        GUI.enabled = true;

        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            nodes.DeleteArrayElementAtIndex(index);
            EnsureFoldoutArray();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.EndHorizontal();

        if (foldouts[index])
        {
            EditorGUI.indentLevel++;

            SerializedProperty iterator = node.Copy();
            SerializedProperty end = iterator.GetEndProperty();

            iterator.NextVisible(true);

            while (!SerializedProperty.EqualContents(iterator, end))
            {
                EditorGUILayout.PropertyField(iterator, true);
                if (!iterator.NextVisible(false))
                    break;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    void ShowAddMenu()
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Text Node"), false, () => AddNode(typeof(DialogueTextNode)));
        menu.AddItem(new GUIContent("Pause Node"), false, () => AddNode(typeof(DialoguePauseNode)));
        menu.AddItem(new GUIContent("Choice Node"), false, () => AddNode(typeof(DialogueChoiceNode)));
        menu.AddItem(new GUIContent("Script Node"), false, () => AddNode(typeof(DialogueScriptNode)));

        menu.ShowAsContext();
    }

    void AddNode(Type type)
    {
        serializedObject.Update();

        int index = nodes.arraySize;

        nodes.InsertArrayElementAtIndex(index);

        SerializedProperty newNode = nodes.GetArrayElementAtIndex(index);
        newNode.managedReferenceValue = Activator.CreateInstance(type);

        EnsureFoldoutArray();

        serializedObject.ApplyModifiedProperties();
    }
}
