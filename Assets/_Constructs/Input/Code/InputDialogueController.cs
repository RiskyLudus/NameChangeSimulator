using System;
using NCS.Classes;
using NCS.Core;
using NCS.Data;
using TMPro;
using UnityEngine;

namespace AnarchyConstructFramework.Constructs.Input
{
    public class InputDialogueController : NCSBehaviour
    {
        [SerializeField] private GameObject inputContainer;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private TMP_Text placeholderText;
        
        private void OnEnable()
        {
            NCSEvents.DisplayInputDialogue?.AddListener(DisplayInputDialogue);
            NCSEvents.CloseInputDialogue?.AddListener(CloseInputDialogue);
            NCSEvents.SubmitInput?.AddListener(SubmitInput);
            CloseInputDialogue();
        }

        private void OnDisable()
        {
            NCSEvents.DisplayInputDialogue?.RemoveListener(DisplayInputDialogue);
            NCSEvents.CloseInputDialogue?.RemoveListener(CloseInputDialogue);
            NCSEvents.SubmitInput?.RemoveListener(SubmitInput);
        }

        private void DisplayInputDialogue(InputData data)
        {
            dialogueText.text = data.text;
            placeholderText.text = data.placeholderText;
            inputContainer.SetActive(true);
        }

        private void CloseInputDialogue()
        {
            dialogueText.text = string.Empty;
            placeholderText.text = string.Empty;
            inputField.text = string.Empty;
            inputContainer.SetActive(false);
        }
        
        private void SubmitInput()
        {
            
        }
    }
}
