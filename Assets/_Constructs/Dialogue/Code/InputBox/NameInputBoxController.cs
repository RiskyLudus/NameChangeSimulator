using NameChangeSimulator.Shared;
using TMPro;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Dialogue.InputBox
{
    public class NameInputBoxController : MonoBehaviour
    {
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_InputField firstNameInputField, middleNameInputField, lastNameInputField;
        [SerializeField] private Animator[] animators;
        [SerializeField] private string triggerName;

        private bool _goToNextRunning = false; // I hate doing stuff like this for animation control but ah well -Risky
        
        public void DisplayNameInputWindow()
        {
            Debug.Log("Showing name input window");
            firstNameInputField.text = string.Empty;
            middleNameInputField.text = string.Empty;
            lastNameInputField.text = string.Empty;
            container.SetActive(true);
        }
        
        public void SubmitInput()
        {
            Debug.Log("Submit name input");
            AudioManager.Instance.PlayUIConfirm_SFX();
            foreach (var animator in animators)
            {
                animator.SetTrigger(triggerName);
            }
        }

        public void GoToNext()
        {
            if (_goToNextRunning) return;
            
            _goToNextRunning = true;
            container.SetActive(false);
            dialogueController.GoToNext($"{firstNameInputField.text}~{middleNameInputField.text}~{lastNameInputField.text}");
        }

        public void Close()
        {
            container.SetActive(false);
        }
    }
}
