using NameChangeSimulator.Shared;
using UnityEngine;
using XNode;
using Node = XNode.Node;

[Node.NodeWidth(350)]
public class DialogueNode : Node
{
    public CharacterSpriteType SpriteType = CharacterSpriteType.Idle;
    [TextArea] public string DialogueText;
    public string Keyword;

    // Define input and output ports
    [Input(backingValue = ShowBackingValue.Never)] public DialogueNode Input;
    [Output(backingValue = ShowBackingValue.Never)] public DialogueNode Output;

    public override object GetValue(NodePort port)
    {
        return DialogueText;
    }

    public void SetValue(string value)
    {
        
    }
}