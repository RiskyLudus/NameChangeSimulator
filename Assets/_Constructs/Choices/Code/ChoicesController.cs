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
        
        private string _keyword = String.Empty;

        private void OnEnable()
        {
            ConstructBindings.Send_ChoicesData_ShowChoicesWindow?.AddListener(OnShowChoicesWindow);
            ConstructBindings.Send_ChoicesData_AddChoice?.AddListener(OnAddChoice);
            ConstructBindings.Send_ChoicesData_ClearChoices?.AddListener(OnClearChoices);
            ConstructBindings.Send_ChoicesData_CloseChoicesWindow?.AddListener(OnCloseChoicesWindow);
            ConstructBindings.Send_NodeLoaderData_CloseAllWindows?.AddListener(OnCloseChoicesWindow);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_ChoicesData_ShowChoicesWindow?.RemoveListener(OnShowChoicesWindow);
            ConstructBindings.Send_ChoicesData_AddChoice?.RemoveListener(OnAddChoice);
            ConstructBindings.Send_ChoicesData_ClearChoices?.RemoveListener(OnClearChoices);
            ConstructBindings.Send_ChoicesData_CloseChoicesWindow?.RemoveListener(OnCloseChoicesWindow);
            ConstructBindings.Send_NodeLoaderData_CloseAllWindows?.RemoveListener(OnCloseChoicesWindow);
        }

        private void OnShowChoicesWindow(string keyword)
        {
            for (int i = 0; i < choiceLayout.transform.childCount; i++)
            {
                Destroy(choiceLayout.transform.GetChild(i).gameObject);
            }
            choicesData.choices.Clear();
            _keyword = keyword;
            container.SetActive(true);
        }
        
        private void OnCloseChoicesWindow()
        {
            ConstructBindings.Send_ChoicesData_ClearChoices?.Invoke();
        }
        
        private void OnAddChoice(string choicePromptString, bool choiceValue, string nodeFieldName)
        {
            var choice = Instantiate(choicePrefab, choiceLayout);
            choice.GetComponent<ChoiceItemController>().Initialize(choicePromptString, choiceValue, nodeFieldName, this);
            choicesData.choices.Add(choice);
        }
        
        private void OnClearChoices()
        {
            container.SetActive(false);
        }

        public void Submit(bool choiceMade, string nodeFieldName)
        {
            OnClearChoices();
            ConstructBindings.Send_ChoicesData_SubmitChoice?.Invoke(_keyword, choiceMade, nodeFieldName);
        }
    }
}
