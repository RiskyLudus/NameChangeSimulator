using NameChangeSimulator.Shared;
using UnityEngine;
using XNode;
using Node = XNode.Node;

[NodeWidth(200)]
public class ShowStatePickerNode : DialogueNode
{
    public override object GetValue(NodePort port)
    {
        return null; // No value to return, used only for connections
    }
}