using NameChangeSimulator.Shared;
using TMPro;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Dialogue.StatePickerBox
{
    public class StatePickerBoxController : MonoBehaviour
    {
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private SupportedStateData supportedStateData;
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private TMP_Text dropdownText; // We're being hella lazy about this lol

        private void Start()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(supportedStateData.supportedStates);
        }
        
        public void DisplayStatePicker()
        {
            Debug.Log($"Showing StatePicker node");
            container.SetActive(true);
        }

        public void SubmitStatePick()
        {
            dialogueController.StateToLoad = dropdownText.text;
            dialogueController.GoToNext();
            container.SetActive(false);
        }
    }
}