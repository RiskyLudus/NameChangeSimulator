using System;
using System.Collections.Generic;
using UnityEngine;

namespace NameChangeSimulator.Shared
{
    [Serializable]
    public class Node
    {
        public int Id;
        public string CharacterDataName;
        public string CharacterSpriteType;
        public string ConversationText;
        public string Keyword;
        public Input Input;
        public Choice Choice;
    }

    [Serializable]
    public class Input
    {
        public string Prompt;
        public string Placeholder;
        public int NodeConnectionId;
    }

    [Serializable]
    public class Choice
    {
        public string Prompt;
        public bool Value;
        public int NodeConnectionId;
    }
}