using UnityEngine;

namespace NameChangeSimulator.Shared.Node
{
    [NodeWidth(350)]
    public class DropdownNode : XNode.Node
    {
        [TextArea] public string DialogueText;
        public string Keyword;
        public string[] Options;
        
        [Input(backingValue = ShowBackingValue.Never)] public DialogueNode Input;
        [Output(backingValue = ShowBackingValue.Never)] public DialogueNode Output;
    }
}
