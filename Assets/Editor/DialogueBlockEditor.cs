using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(DialogueBlock))]
public class DialogueBlockEditor : Editor
{
    SerializedProperty nodes;
    SerializedProperty id;
    SerializedProperty textBox;

    bool[] foldouts;

    void OnEnable()
    {
        nodes = serializedObject.FindProperty("nodes");
        id = serializedObject.FindProperty("ID");
        textBox = serializedObject.FindProperty("textBox");

        SerializedProperty nodesProp = serializedObject.FindProperty("nodes"); // your nodes array
        foldouts = new bool[nodesProp.arraySize];

        for (int i = 0; i < foldouts.Length; i++)
            foldouts[i] = true; // open by default

        EnsureFoldoutArray();
    }

    void EnsureFoldoutArray()
    {
        if (foldouts == null)
        {
            foldouts = new bool[nodes.arraySize];
            return;
        }

        if (foldouts.Length != nodes.arraySize)
        {
            bool[] newFoldouts = new bool[nodes.arraySize];

            for (int i = 0; i < Mathf.Min(foldouts.Length, newFoldouts.Length); i++)
            {
                newFoldouts[i] = foldouts[i];
            }

            foldouts = newFoldouts;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(id);
        EditorGUILayout.PropertyField(textBox);

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
        EditorGUILayout.Space();

        if (GUILayout.Button("+ Add Node", GUILayout.Height(30)))
        {
            ShowAddMenu();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawNode(SerializedProperty node, int index)
    {
        string fullType = node.managedReferenceFullTypename;
        string typeName = fullType.Split(' ')[1]
                                  .Replace("Dialogue", "")
                                  .Replace("Node", "");


        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        foldouts[index] = EditorGUILayout.Foldout(
            foldouts[index],
            typeName,
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

            // Get correct height from the drawer
            float height = EditorGUI.GetPropertyHeight(node, true);

            // Add padding buffer to prevent clipping (VERY important)
            height += 10f;

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height));

            // IMPORTANT: Temporarily disable foldout drawing
            bool oldState = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = true;

            EditorGUI.BeginProperty(rect, GUIContent.none, node);
            EditorGUI.PropertyField(rect, node, GUIContent.none, true);
            EditorGUI.EndProperty();

            EditorGUIUtility.hierarchyMode = oldState;

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
        menu.AddItem(new GUIContent("Change Font Node"), false, () => AddNode(typeof(DialogueChangeFontNode)));
        menu.AddItem(new GUIContent("Show Character Node"), false, () => AddNode(typeof(DialogueShowCharacterNode)));
        menu.AddItem(new GUIContent("Hide Character Node"), false, () => AddNode(typeof(DialogueHideCharacterNode)));
        menu.AddItem(new GUIContent("Play Animation Node"), false, () => AddNode(typeof(DialoguePlayAnimationNode)));
        menu.AddItem(new GUIContent("Req Player Click Continue Node"), false, () => AddNode(typeof(DialogueRequirePlayerClickContinueNode)));
        menu.AddItem(new GUIContent("Play Sound Node"), false, () => AddNode(typeof(DialoguePlaySoundNode)));
        menu.AddItem(new GUIContent("Play Group Node"), false, () => AddNode(typeof(DialoguePlayGroupNode)));
        menu.AddItem(new GUIContent("Set Dialogue Click Rights Node"), false, () => AddNode(typeof(DialogueSetDialogueClickRightsNode)));

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

        // Expand the newly created node
        foldouts[index] = true;

        SerializedProperty nodesProp = serializedObject.FindProperty("nodes");
        nodesProp.arraySize++;
        serializedObject.ApplyModifiedProperties();

        // Resize foldouts array
        bool[] newFoldouts = new bool[nodesProp.arraySize];
        if (foldouts != null)
            foldouts.CopyTo(newFoldouts, 0);

        newFoldouts[newFoldouts.Length - 1] = true; // new node open by default
        foldouts = newFoldouts;

        serializedObject.ApplyModifiedProperties();
    }
}
