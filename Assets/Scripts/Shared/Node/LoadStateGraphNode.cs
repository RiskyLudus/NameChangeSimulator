using UnityEngine;
using XNode;

[Node.NodeWidth(150)]
public class LoadStateGraphNode : Node
{
    // Input and Output ports for connecting to other nodes
    [Input(backingValue = ShowBackingValue.Never)] public DialogueNode Input;
        
    public override object GetValue(NodePort port)
    {
        return null; // No value to return, used only for connections
    }
}
