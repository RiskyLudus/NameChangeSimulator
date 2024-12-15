using NameChangeSimulator.Shared;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(MultiInputNode))]
public class MultiInputNodeEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        serializedObject.Update();

        MultiInputNode node = target as MultiInputNode;

        // Display input port
        NodeEditorGUILayout.PortField(new GUIContent("Input"), target.GetInputPort("Input"), GUILayout.MinWidth(0));

        GUILayout.Space(10);
        
        // Display Character Sprite Type
        node.SpriteType = (CharacterSpriteType)EditorGUILayout.EnumPopup("Sprite:", node.SpriteType);
        
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.wordWrap = true;
        style.richText = true;
        
        // Display Dialogue Text
        EditorGUILayout.LabelField("Dialogue Text", EditorStyles.boldLabel);
        node.QuestionText = EditorGUILayout.TextArea(node.QuestionText, style, GUILayout.Height(100));
        
        // Display prompt
        EditorGUILayout.LabelField("Keyword", EditorStyles.boldLabel);
        node.Keyword = EditorGUILayout.TextField(node.Keyword);
        
        GUILayout.Space(10);

        // Display dynamic choices
        EditorGUILayout.LabelField("Inputs", EditorStyles.boldLabel);
        int removeIndex = -1; // Track the index to remove after the loop
        for (int i = 0; i < node.Inputs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // Remove choice button
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                removeIndex = i; // Mark the index to remove later
            }

            // Display the prompt field for the choice
            node.Inputs[i] = EditorGUILayout.TextField("", node.Inputs[i], GUILayout.Width(250));

            EditorGUILayout.EndHorizontal();
        }

        // Remove the choice outside the loop to avoid GUILayout state issues
        if (removeIndex >= 0)
        {
            node.Inputs.RemoveAt(removeIndex);
        }

        // Add choice button
        if (GUILayout.Button("Add Input"))
        {
            // Add a new choice and update ports
            node.Inputs.Add(new string(""));
        }
        
        GUILayout.Space(10);
        
        // Display output port
        NodeEditorGUILayout.PortField(new GUIContent("Output"), target.GetOutputPort("Output"), GUILayout.MinWidth(0));

        serializedObject.ApplyModifiedProperties();
    }
}
