using NameChangeSimulator.Shared;
using TMPro;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Dialogue.InputBox
{
    public class NameInputBoxController : MonoBehaviour
    {
        private static readonly int OpenTrigger = Animator.StringToHash("Open");
        private static readonly int CloseTrigger = Animator.StringToHash("Close");
        
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_InputField firstNameInputField, middleNameInputField, lastNameInputField;
        [SerializeField] private Animator nameInputAnimator;
        [SerializeField] private Animator[] animators;
        [SerializeField] private string triggerName;

        private bool _goToNextRunning = false; // I hate doing stuff like this for animation control but ah well -Risky
        
        public void DisplayNameInputWindow()
        {
            Debug.Log("Showing name input window");
            firstNameInputField.text = string.Empty;
            middleNameInputField.text = string.Empty;
            lastNameInputField.text = string.Empty;
            nameInputAnimator.SetTrigger(OpenTrigger);
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
            dialogueController.GoToNext($"{firstNameInputField.text}~{middleNameInputField.text}~{lastNameInputField.text}");
            nameInputAnimator.SetTrigger(CloseTrigger);
        }

        public void Close()
        {
            
        }
    }
}
