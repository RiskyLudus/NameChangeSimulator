using System;
using NCS.Classes;
using NCS.Core;
using TMPro;
using UnityEngine;

namespace AnarchyConstructFramework.Constructs.Conversation
{
    public class ConversationDialogueController : NCSBehaviour
    {
        [SerializeField] private TMP_Text characterNameText;
        [SerializeField] private TMP_Text conversationText;
        
        private void OnEnable()
        {
            NCSEvents.DisplayConversationDialogue?.AddListener(DisplayConversationDialogue);
            NCSEvents.ClearConversationDialogue?.AddListener(ClearConversationDialogue);
        }
        
        private void OnDisable()
        {
            NCSEvents.DisplayConversationDialogue?.RemoveListener(DisplayConversationDialogue);
            NCSEvents.ClearConversationDialogue?.RemoveListener(ClearConversationDialogue);
        }
        
        private void DisplayConversationDialogue(ConversationNode data)
        {
            ClearConversationDialogue();
            characterNameText.text = data.character;
            conversationText.text = data.text;
        }
        
        private void ClearConversationDialogue()
        {
            characterNameText.text = string.Empty;
            conversationText.text = string.Empty;
        }
    }
}
