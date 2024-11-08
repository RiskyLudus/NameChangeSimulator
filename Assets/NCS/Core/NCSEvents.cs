using NCS.Classes;
using NCS.Data;
using UnityEngine;
using UnityEngine.Events;

namespace NCS.Core
{
    public static class NCSEvents
    {
        //Flow Functions
        public static UnityEvent LoadFlow = new UnityEvent();
        public static UnityEvent UnloadFlow = new UnityEvent();
        
        // Choice Dialogue Functions
        public static UnityEvent<ChoicesData> DisplayChoiceDialogue = new UnityEvent<ChoicesData>();
        public static UnityEvent CloseChoiceDialogue = new UnityEvent();
        public static UnityEvent<string, bool> ChoiceMade = new UnityEvent<string, bool>();
        public static UnityEvent SubmitChoices = new UnityEvent();
        
        // Input Dialogue Functions
        public static UnityEvent<InputData> DisplayInputDialogue = new UnityEvent<InputData>();
        public static UnityEvent CloseInputDialogue = new UnityEvent();
        public static UnityEvent SubmitInput = new UnityEvent();
        
        // Conversation Dialogue Functions
        public static UnityEvent<ConversationNode> DisplayConversationDialogue = new UnityEvent<ConversationNode>();
        public static UnityEvent ClearConversationDialogue = new UnityEvent();
        public static UnityEvent NextButtonPressed = new UnityEvent();
        
        // Character Functions
        public static UnityEvent<Sprite> DisplayCharacterSprite = new UnityEvent<Sprite>();
        public static UnityEvent ClearCharacterSprite = new UnityEvent();
        
        // Background Functions
        public static UnityEvent<Sprite> DisplayBackgroundSprite = new UnityEvent<Sprite>();
        public static UnityEvent ClearBackgroundSprite = new UnityEvent();
    }
}
