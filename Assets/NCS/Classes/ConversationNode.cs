using System;

namespace NCS.Classes
{
    [Serializable]
    public class ConversationNode
    {
        public string id;
        public string type;  // No renaming required with Newtonsoft
        public string character;
        public string sprite;
        public string text;
        public ChoicesData choicesData;
        public InputData inputData;
        public string next;
    }
}