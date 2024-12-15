using System.Collections.Generic;
using System.Linq;
using NameChangeSimulator.Shared;
using UnityEngine;
using XNode;
using Node = XNode.Node;

[NodeWidth(400)]
public class MultiInputNode : Node
{
    public CharacterSpriteType SpriteType = CharacterSpriteType.None;
    [TextArea] public string QuestionText;
    public string Keyword;

    // Dynamically add output ports based on the number of choices
    [Input(backingValue = ShowBackingValue.Never)] public DialogueNode Input;
    [Output(backingValue = ShowBackingValue.Never)] public DialogueNode Output;

    public List<string> Inputs = new List<string>();

    public override object GetValue(NodePort port)
    {
        return null; // No value to return, used only for connections
    }
}