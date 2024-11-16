using System;
using Anarchy.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.Conversation
{
    public class ConversationController : MonoBehaviour
    {
        [SerializeField] private ConversationData conversationData;
        
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_Text conversationPromptText;
        [SerializeField] private TMP_Text nameText;

        private void OnEnable()
        {
            ConstructBindings.Send_ConversationData_DisplayConversation?.AddListener(OnDisplayConversation);
            ConstructBindings.Send_ConversationData_ClearConversation?.AddListener(OnClearConversation);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_ConversationData_DisplayConversation?.RemoveListener(OnDisplayConversation);
            ConstructBindings.Send_ConversationData_ClearConversation?.RemoveListener(OnClearConversation);
        }

        private void OnDisplayConversation(string nameString, string conversationPromptString, int nodeID)
        {
            nameText.text = nameString;
            conversationPromptText.text = conversationPromptString;
            conversationData.node = nodeID;
            container.gameObject.SetActive(true);
        }
        
        private void OnClearConversation(bool windowState)
        {
            nameText.text = string.Empty;
            conversationPromptText.text = string.Empty;
            conversationData.node = 0;
            if (!windowState)
            {
                container.gameObject.SetActive(false);
            }
        }

        public void Submit()
        {
            ConstructBindings.Send_ConversationData_SubmitNode?.Invoke(conversationData.node);
        }
    }
}
