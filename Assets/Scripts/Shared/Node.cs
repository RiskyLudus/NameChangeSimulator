using System;
using System.Collections.Generic;
using UnityEngine;

namespace NameChangeSimulator.Shared
{
    [Serializable]
    public class Node
    {
        public int Id;
        public string ConversationText;
        public string CharacterName;
        public string CharacterEmotion;
        public string Keyword;
        public bool IsInput; // True if using input window, false if choice window
        /// <summary>
        /// For choices, this is prompt text and whether the choice is "True" or "False"
        /// For input, this prompt and the placeholder text
        /// </summary>
        public List<string[]> DataToInject = new List<string[]>();
    }
}
