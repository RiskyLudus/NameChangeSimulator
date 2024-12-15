using NameChangeSimulator.Shared;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(ChoiceNode))]
public class ChoiceNodeEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        serializedObject.Update();

        ChoiceNode node = target as ChoiceNode;

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
        EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);
        int removeIndex = -1; // Track the index to remove after the loop
        for (int i = 0; i < node.Choices.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // Remove choice button
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                removeIndex = i; // Mark the index to remove later
            }

            // Display the prompt field for the choice
            node.Choices[i].Prompt = EditorGUILayout.TextField("", node.Choices[i].Prompt, GUILayout.Width(250));
            node.Choices[i].Value = EditorGUILayout.Toggle("", node.Choices[i].Value, GUILayout.Width(25));
            node.Choices[i].PortFieldName = $"Output_{i}";

            // Render the dynamic output port for each choice
            NodeEditorGUILayout.PortField(new GUIContent($"Output {i + 1}"), node.GetOutputPort($"Output_{i}"), GUILayout.MinWidth(0));

            EditorGUILayout.EndHorizontal();
        }

        // Remove the choice outside the loop to avoid GUILayout state issues
        if (removeIndex >= 0)
        {
            node.Choices.RemoveAt(removeIndex);
            node.UpdateDynamicPorts(); // Synchronize ports
        }

        // Add choice button
        if (GUILayout.Button("Add Choice"))
        {
            // Add a new choice and update ports
            node.Choices.Add(new Choice ("", ""));
            node.UpdateDynamicPorts(); // Synchronize ports
        }

        serializedObject.ApplyModifiedProperties();
    }
}
