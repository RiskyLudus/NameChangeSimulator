using NameChangeSimulator.Shared;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

[CustomNodeEditor(typeof(InputNode))]
public class InputNodeEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        serializedObject.Update();

        InputNode node = target as InputNode;
        NodeEditorGUILayout.PortField(new GUIContent("OverrideInput"), target.GetInputPort("OverrideInput"), GUILayout.MinWidth(0));
        // Display input port
        NodeEditorGUILayout.PortField(new GUIContent("Input"), target.GetInputPort("Input"), GUILayout.MinWidth(0));
        
        GUILayout.Space(10);
        
        // Display Character Sprite Type
        node.SpriteType = (CharacterSpriteType)EditorGUILayout.EnumPopup("Sprite:", node.SpriteType);
        
        GUILayout.Space(10);
        
        // Display Voice Line Type
        node.VoiceLine = (VoiceLineType)EditorGUILayout.EnumPopup("Voice Line:", node.VoiceLine);
        
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.wordWrap = true;
        style.richText = true;
        
        // Display Dialogue Text
        EditorGUILayout.LabelField("Dialogue Text", EditorStyles.boldLabel);
        node.DialogueText = EditorGUILayout.TextArea(node.DialogueText, style, GUILayout.Height(100));
        
        // Display prompt
        EditorGUILayout.LabelField("Keyword", EditorStyles.boldLabel);
        node.Keyword = EditorGUILayout.TextField(node.Keyword);
        
        GUILayout.Space(10);

        // Display output port
        NodeEditorGUILayout.PortField(new GUIContent("Output"), target.GetOutputPort("Output"), GUILayout.MinWidth(0));

        serializedObject.ApplyModifiedProperties();
    }
}