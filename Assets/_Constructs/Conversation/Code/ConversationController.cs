using System;
using Anarchy.Shared;
using TMPro;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Conversation
{
    public class ConversationController : MonoBehaviour
    {
        [SerializeField] private ConversationData conversationData;
        
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_Text conversationPromptText;
        [SerializeField] private GameObject backButton;
        [SerializeField] private GameObject nextButton;

        private string _nodeFieldNameToGoBackTo = String.Empty;
        private string _nodeFieldNameToGoNextTo = String.Empty;

        private void OnEnable()
        {
            ConstructBindings.Send_ConversationData_DisplayConversation?.AddListener(OnDisplayConversation);
            ConstructBindings.Send_ConversationData_ClearConversation?.AddListener(OnClearConversation);
            ConstructBindings.Send_ConversationData_ToggleButtons?.AddListener(OnToggleButtons);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_ConversationData_DisplayConversation?.RemoveListener(OnDisplayConversation);
            ConstructBindings.Send_ConversationData_ClearConversation?.RemoveListener(OnClearConversation);
            ConstructBindings.Send_ConversationData_ToggleButtons?.RemoveListener(OnToggleButtons);
        }

        private void OnDisplayConversation(string nameString, string conversationPromptString, string previousNodeFieldName, string nextNodeFieldName)
        {
            conversationPromptText.text = conversationPromptString;
            _nodeFieldNameToGoBackTo = previousNodeFieldName;
            _nodeFieldNameToGoNextTo = nextNodeFieldName;
            container.gameObject.SetActive(true);
        }

        private void OnToggleButtons(bool showBackButton, bool showNextButton)
        {
            backButton.SetActive(showBackButton);
            nextButton.SetActive(showNextButton);
        }
        
        private void OnClearConversation(bool windowState)
        {
            conversationPromptText.text = string.Empty;
            _nodeFieldNameToGoBackTo = string.Empty;
            _nodeFieldNameToGoNextTo = string.Empty;
            if (!windowState)
            {
                container.gameObject.SetActive(false);
            }
        }
        
        public void SubmitBack()
        {
            ConstructBindings.Send_ConversationData_SubmitPrevNode?.Invoke(_nodeFieldNameToGoBackTo);
        }

        public void SubmitNext()
        {
            ConstructBindings.Send_ConversationData_SubmitNextNode?.Invoke(_nodeFieldNameToGoNextTo);
        }
    }
}
