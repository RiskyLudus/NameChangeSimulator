using NameChangeSimulator.Shared;
using UnityEngine;
using XNode;
using Node = XNode.Node;

[NodeWidth(350)]
public class InputNode : DialogueNode
{
    public bool CanLeaveBlank = false;
    
    public override object GetValue(NodePort port)
    {
        return null; // No value to return, used only for connections
    }
}