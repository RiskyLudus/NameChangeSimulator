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
        [SerializeField] private GameObject nextButton;

        private string _nodeFieldNameToGoTo = String.Empty;

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

        private void OnDisplayConversation(string nameString, string conversationPromptString, string nodeFieldName, bool doNotShowNextButton = true)
        {
            nameText.text = nameString;
            conversationPromptText.text = conversationPromptString;
            _nodeFieldNameToGoTo = nodeFieldName;
            container.gameObject.SetActive(true);
            nextButton.SetActive(doNotShowNextButton);
        }
        
        private void OnClearConversation(bool windowState)
        {
            nameText.text = string.Empty;
            conversationPromptText.text = string.Empty;
            _nodeFieldNameToGoTo = string.Empty;
            if (!windowState)
            {
                container.gameObject.SetActive(false);
            }
        }

        public void Submit()
        {
            ConstructBindings.Send_ConversationData_SubmitNode?.Invoke(_nodeFieldNameToGoTo);
        }
    }
}
