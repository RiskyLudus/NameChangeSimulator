using NameChangeSimulator.Shared;
using Anarchy.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NameChangeSimulator.Constructs.Dialogue.InputBox
{
    public class InputBoxController : MonoBehaviour
    {
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_InputField inputField;

        private bool _canLeaveBlank = false;

        public void DisplayInputWindow(bool canLeaveBlank)
        {
            Debug.Log("Showing input window");
            inputField.text = string.Empty;
            inputField.characterLimit = 64;
            container.SetActive(true);
            _canLeaveBlank = canLeaveBlank;
        }
        
        public void SubmitInput()
        {
            if (!string.IsNullOrEmpty(inputField.text) || _canLeaveBlank)
            {
                container.SetActive(false);
                AudioManager.Instance.PlayUIConfirm_SFX();
                _canLeaveBlank = false;
                dialogueController.GoToNext(inputField.text);
            }
        }

        public void Close()
        {
            container.SetActive(false);
        }
    }
}
