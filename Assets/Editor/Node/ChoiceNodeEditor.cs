using NameChangeSimulator.Shared;
using NameChangeSimulator.Shared.Node;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(ChoiceNode))]
public class ChoiceNodeEditor : NodeEditor
{
    private bool showOptions = false; // Tracks foldout state

    public override void OnBodyGUI()
    {
        serializedObject.Update();

        ChoiceNode node = target as ChoiceNode;
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
        
        // Draw GUI
        NodeEditorGUILayout.DynamicPortList(
            "Options", // field name
            typeof(string), // field type
            serializedObject, // serializable object
            NodePort.IO.Output, // new port i/o
            Node.ConnectionType.Override, // new port connection type
            Node.TypeConstraint.None); // onCreate override. This is where the magic happens.

        GUILayout.Space(10);

        serializedObject.ApplyModifiedProperties();
    }
}
