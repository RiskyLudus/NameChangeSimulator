using System.Collections.Generic;
using System.Linq;
using NameChangeSimulator.Shared;
using UnityEngine;
using XNode;
using Node = XNode.Node;

[NodeWidth(400)]
public class ChoiceNode : Node
{
    public CharacterSpriteType SpriteType = CharacterSpriteType.None;
    [TextArea] public string QuestionText;
    public string Keyword;

    // Dynamically add output ports based on the number of choices
    [Input(backingValue = ShowBackingValue.Never)] public DialogueNode Input;
    [Output(dynamicPortList = true, backingValue = ShowBackingValue.Never)] public List<DialogueNode> Outputs;

    public List<Choice> Choices = new List<Choice>();

    public override object GetValue(NodePort port)
    {
        return null; // No value to return, used only for connections
    }

    public void UpdateDynamicPorts()
    {
        // Add missing ports
        for (int i = 0; i < Choices.Count; i++)
        {
            string portName = $"Output_{i}";
            if (GetOutputPort(portName) == null)
            {
                Debug.Log($"Creating port: {portName}");
                AddDynamicOutput(typeof(DialogueNode), Node.ConnectionType.Override, TypeConstraint.None, portName);
            }
        }

        // Remove extra ports
        var existingPorts = DynamicOutputs.ToList();
        for (int i = Choices.Count; i < existingPorts.Count; i++)
        {
            string portName = $"Output_{i}";
            Debug.Log($"Removing port: {portName}");
            RemoveDynamicPort(GetOutputPort(portName));
        }
    }

    // Called when the node is initialized or changes are made in the editor
    public ChoiceNode()
    {
        UpdateDynamicPorts();
    }
}

[System.Serializable]
public class Choice
{
    public string Prompt;
}