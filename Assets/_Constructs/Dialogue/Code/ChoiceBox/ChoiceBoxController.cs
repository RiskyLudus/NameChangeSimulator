using NameChangeSimulator.Shared;
using System;
using Anarchy.Shared;
using NameChangeSimulator.Constructs.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NameChangeSimulator.Constructs.Dialogue.ChoiceBox
{
    public class ChoiceBoxController : MonoBehaviour
    {
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private GameObject container;
        [SerializeField] private GameObject choicePrefab;
        [SerializeField] private Transform choiceLayout;
        [SerializeField] private GameObject defaultChoiceObject;
        [SerializeField] private GameObject yesNoChoiceObject;

        public void DisplayChoicesWindow(string[] options)
        {
            defaultChoiceObject.SetActive(false);
            yesNoChoiceObject.SetActive(false);
            
            if (options.Length == 2 && options[0] == "Yes" && options[1] == "No")
            {
                yesNoChoiceObject.SetActive(true);
            }
            else
            {
                defaultChoiceObject.SetActive(true);
                for (int i = 0; i < choiceLayout.transform.childCount; i++)
                {
                    Destroy(choiceLayout.transform.GetChild(i).gameObject);
                }
            }

            foreach (var option in options)
            {
                var choice = Instantiate(choicePrefab, choiceLayout);
                choice.GetComponent<ChoiceItemController>().Initialize(option, this);
            }
            
            container.SetActive(true);
        }
        public void Submit(string optionSelected)
        {
            container.SetActive(false);
            dialogueController.GoToNext(optionSelected);
        }

        public void Close()
        {
            container.SetActive(false);
        }
    }
}
