using NameChangeSimulator.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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
            AudioManager.Instance.PlayShowWindow_SFX();
            container.SetActive(true);
        }

         private void OnMouseEnter()
        {
            AudioManager.Instance.PlayUIHover_SFX();
        }

        private void OnMouseExit()
        {
            AudioManager.Instance.PlayUIHoverExit_SFX();
        }

        public void SubmitStatePick()
        {
            AudioManager.Instance.PlayUIConfirm_SFX();
            AudioManager.Instance.PlayCloseWindow_SFX();
            dialogueController.StateToLoad = dropdownText.text;
            dialogueController.GoToNext();
            container.SetActive(false);
        }

        public void Close()
        {
            container.SetActive(false);
        }
    }
}
