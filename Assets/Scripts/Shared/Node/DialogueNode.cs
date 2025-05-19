using NameChangeSimulator.Shared;
using UnityEngine;
using XNode;
using Node = XNode.Node;

[Node.NodeWidth(350)]
public class DialogueNode : Node
{
    public CharacterSpriteType SpriteType = CharacterSpriteType.Idle;
    public VoiceLineType VoiceLine = VoiceLineType.None;
    [TextArea] public string DialogueText;
    public string Keyword;

    // Define input and output ports
    // Order of operations matters here. Input needs to come before Overrideinput, otherwise new nodes will default to Overrideinputs. -Ai
    [Input(backingValue = ShowBackingValue.Never)] public DialogueNode Input;
    [Input(backingValue = ShowBackingValue.Never)] public DialogueNode OverrideInput;
    [Output(backingValue = ShowBackingValue.Never)] public DialogueNode Output;

    public override object GetValue(NodePort port)
    {
        return DialogueText;
    }

    public void SetValue(string value)
    {
        
    }
}