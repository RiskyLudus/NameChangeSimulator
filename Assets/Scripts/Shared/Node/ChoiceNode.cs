using System.Collections.Generic;
using System.Linq;
using NameChangeSimulator.Shared;
using UnityEngine;
using XNode;
using Node = XNode.Node;

[NodeWidth(400)]
public class ChoiceNode : DialogueNode
{
    public string[] Options;

    public override object GetValue(NodePort port)
    {
        return null; // No value to return, used only for connections
    }
}