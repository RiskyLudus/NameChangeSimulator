using NameChangeSimulator.Shared;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

[CustomNodeEditor(typeof(DialogueNode))]
public class DialogueNodeEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        serializedObject.Update();

        DialogueNode node = target as DialogueNode;

        // Display input port
        NodeEditorGUILayout.PortField(new GUIContent("Input"), target.GetInputPort("Input"), GUILayout.MinWidth(0));
        NodeEditorGUILayout.PortField(new GUIContent("OverrideInput"), target.GetInputPort("OverrideInput"), GUILayout.MinWidth(0));
        
        GUILayout.Space(10);

        // Display Character Sprite Type
        node.SpriteType = (CharacterSpriteType)EditorGUILayout.EnumPopup("Sprite:", node.SpriteType);
        
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.wordWrap = true;
        style.richText = true;
        
        // Display Dialogue Text
        EditorGUILayout.LabelField("Dialogue Text", EditorStyles.boldLabel);
        node.DialogueText = EditorGUILayout.TextArea(node.DialogueText, style, GUILayout.Height(100));

        GUILayout.Space(10);
        
        // Display output port
        NodeEditorGUILayout.PortField(new GUIContent("Output"), target.GetOutputPort("Output"), GUILayout.MinWidth(0));

        serializedObject.ApplyModifiedProperties();
    }
}