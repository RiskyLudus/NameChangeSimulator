using NameChangeSimulator.Shared;
using UnityEngine;
using XNode;
using Node = XNode.Node;

[NodeWidth(350)]
public class InputNode : Node
{
    public CharacterSpriteType SpriteType = CharacterSpriteType.None;
    [TextArea] public string QuestionText;
    public string Keyword;

    // Input and Output ports for connecting to other nodes
    [Input(backingValue = ShowBackingValue.Never)] public DialogueNode Input;
    [Output(backingValue = ShowBackingValue.Never)] public DialogueNode Output;

    public override object GetValue(NodePort port)
    {
        return null; // No value to return, used only for connections
    }
}