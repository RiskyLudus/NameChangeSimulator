using NameChangeSimulator.Shared;
using UnityEngine;
using XNode;
using Node = XNode.Node;

[NodeWidth(100)]
public class StartNode : Node
{
    // Define input and output ports
    [Output(backingValue = ShowBackingValue.Never)] public DialogueNode Output;

    public override object GetValue(NodePort port)
    {
        return null; // No value to return, used only for connections
    }
}