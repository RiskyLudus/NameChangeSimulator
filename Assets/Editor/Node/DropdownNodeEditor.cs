using NameChangeSimulator.Shared;
using NameChangeSimulator.Shared.Node;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

[CustomNodeEditor(typeof(DropdownNode))]
public class DropdownNodeEditor : NodeEditor
{
    private bool showOptions = false; // Tracks foldout state

    public override void OnBodyGUI()
    {
        serializedObject.Update();

        DropdownNode node = target as DropdownNode;
        NodeEditorGUILayout.PortField(new GUIContent("OverrideInput"), target.GetInputPort("OverrideInput"), GUILayout.MinWidth(0));
        // Explicitly draw input port from base class
        var inputPort = target.GetInputPort("Input");
        if (inputPort != null)
            NodeEditorGUILayout.PortField(new GUIContent("Input"), inputPort, GUILayout.MinWidth(0));
        else
            EditorGUILayout.LabelField("Input Port not found!");

        GUILayout.Space(10);

        // Display Character Sprite Type
        node.SpriteType = (CharacterSpriteType)EditorGUILayout.EnumPopup("Sprite:", node.SpriteType);
        
        GUILayout.Space(10);
        
        // Display Voice Line Type
        node.VoiceLine = (VoiceLineType)EditorGUILayout.EnumPopup("Voice Line:", node.VoiceLine);

        GUIStyle style = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true,
            richText = true
        };

        // Display Dialogue Text
        EditorGUILayout.LabelField("Dialogue Text", EditorStyles.boldLabel);
        node.DialogueText = EditorGUILayout.TextArea(node.DialogueText, style, GUILayout.Height(100));

        // Display Keyword
        EditorGUILayout.LabelField("Keyword", EditorStyles.boldLabel);
        node.Keyword = EditorGUILayout.TextField(node.Keyword);

        GUILayout.Space(10);

        // Foldout for Options
        showOptions = EditorGUILayout.Foldout(showOptions, "Options", true, EditorStyles.foldoutHeader);
        if (showOptions && node.Options != null)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < node.Options.Length; i++)
            {
                EditorGUILayout.LabelField($"Option {i + 1}: {node.Options[i]}");
            }
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(10);

        // Explicitly draw output port from base class
        var outputPort = target.GetOutputPort("Output");
        if (outputPort != null)
            NodeEditorGUILayout.PortField(new GUIContent("Output"), outputPort, GUILayout.MinWidth(0));
        else
            EditorGUILayout.LabelField("Output Port not found!");

        serializedObject.ApplyModifiedProperties();
    }
}
