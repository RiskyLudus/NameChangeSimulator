using System.Collections.Generic;
using NCS.Classes;
using NCS.Core;
using TMPro;
using UnityEngine;

namespace AnarchyConstructFramework.Constructs.Choices
{
    public class ChoiceDialogueController : NCSBehaviour
    {
        [SerializeField] private GameObject choiceContainer;
        [SerializeField] private GameObject choicePrefab;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Transform choiceLayoutGroup;
        
        private HashSet<string> _choices = new();

        private void OnEnable()
        {
            NCSEvents.DisplayChoiceDialogue?.AddListener(DisplayChoiceDialogue);
            NCSEvents.CloseChoiceDialogue?.AddListener(CloseChoiceDialogue);
            NCSEvents.SubmitChoices.AddListener(SubmitChoices);
            NCSEvents.ChoiceMade.AddListener(ModifyChoices);
            CloseChoiceDialogue();
        }

        void OnDisable()
        {
            NCSEvents.DisplayChoiceDialogue?.RemoveListener(DisplayChoiceDialogue);
            NCSEvents.CloseChoiceDialogue?.RemoveListener(CloseChoiceDialogue);
            NCSEvents.SubmitChoices.RemoveListener(SubmitChoices);
            NCSEvents.ChoiceMade.RemoveListener(ModifyChoices);
        }

        private void DisplayChoiceDialogue(ChoicesData data)
        {
            dialogueText.text = data.text;
            foreach (var choice in data.choices)
            {
                var choiceToggle = Instantiate(choicePrefab, choiceLayoutGroup);
                choiceToggle.name = choice;
                choiceToggle.transform.GetChild(1).GetComponent<TMP_Text>().text = choice;
            }
            choiceContainer.SetActive(true);
        }
        
        private void CloseChoiceDialogue()
        {
            dialogueText.text = string.Empty;
            for (int i = 0; i < choiceLayoutGroup.childCount; i++)
            {
                Destroy(choiceLayoutGroup.GetChild(i).gameObject);
            }
            choiceContainer.SetActive(false);
        }
        
        private void ModifyChoices(string choiceName, bool choiceToggled)
        {
            if (choiceToggled)
            {
                _choices.Add(choiceName); // Add choice if toggled on
            }
            else
            {
                _choices.Remove(choiceName); // Remove choice if toggled off
            }
        }
        
        private void SubmitChoices()
        {
            
        }
    }
}
