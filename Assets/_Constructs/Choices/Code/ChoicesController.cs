using System;
using Anarchy.Shared;
using TMPro;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Choices
{
    public class ChoicesController : MonoBehaviour
    {
        [SerializeField] private ChoicesData choicesData;
        [SerializeField] private GameObject choicePrefab;
        [SerializeField] private GameObject container;
        [SerializeField] private Transform choiceLayout;
        [SerializeField] private TMP_Text choicePromptText;

        private void OnEnable()
        {
            ConstructBindings.Send_ChoicesData_ShowChoicesWindow?.AddListener(OnShowChoicesWindow);
            ConstructBindings.Send_ChoicesData_AddChoice?.AddListener(OnAddChoice);
            ConstructBindings.Send_ChoicesData_ClearChoices?.AddListener(OnClearChoices);
            ConstructBindings.Send_ChoicesData_CloseChoicesWindow?.AddListener(OnCloseChoicesWindow);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_ChoicesData_ShowChoicesWindow?.RemoveListener(OnShowChoicesWindow);
            ConstructBindings.Send_ChoicesData_AddChoice?.RemoveListener(OnAddChoice);
            ConstructBindings.Send_ChoicesData_ClearChoices?.RemoveListener(OnClearChoices);
            ConstructBindings.Send_ChoicesData_CloseChoicesWindow?.RemoveListener(OnCloseChoicesWindow);
        }

        private void OnShowChoicesWindow(string choicesPromptString)
        {
            choicePromptText.text = choicesPromptString;
            container.SetActive(true);
        }
        
        private void OnCloseChoicesWindow()
        {
            ConstructBindings.Send_ChoicesData_ClearChoices?.Invoke();
        }
        
        private void OnAddChoice(string choicePromptString, int choiceIndex)
        {
            var choice = Instantiate(choicePrefab, choiceLayout);
            choice.GetComponent<ChoiceItemController>().Initialize(choicePromptString, choiceIndex, this);
            choicesData.choices.Add(choice);
        }
        
        private void OnClearChoices()
        {
            for (int i = 0; i < choiceLayout.transform.childCount; i++)
            {
                Destroy(choiceLayout.transform.GetChild(i).gameObject);
            }
            
            choicesData.choices.Clear();
            container.SetActive(false);
        }

        public void Submit(int choiceIndex)
        {
            ConstructBindings.Send_ChoicesData_SubmitChoice?.Invoke(choiceIndex);
        }
    }
}
