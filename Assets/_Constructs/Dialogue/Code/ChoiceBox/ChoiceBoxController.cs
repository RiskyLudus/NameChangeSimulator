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

        public void DisplayChoicesWindow(string[] options)
        {
            for (int i = 0; i < choiceLayout.transform.childCount; i++)
            {
                Destroy(choiceLayout.transform.GetChild(i).gameObject);
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
            AudioManager.Instance.PlayUIConfirm_SFX();
            container.SetActive(false);
            dialogueController.GoToNext(optionSelected);
        }

        public void Close()
        {
            container.SetActive(false);
        }
    }
}
