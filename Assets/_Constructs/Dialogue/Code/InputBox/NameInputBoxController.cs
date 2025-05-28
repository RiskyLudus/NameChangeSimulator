using NameChangeSimulator.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private Sprite textureSwapImage;

        private bool _goToNextRunning = false; // I hate doing stuff like this for animation control but ah well -Risky
        
        public void DisplayNameInputWindow()
        {
            Debug.Log("<color=lightblue>[INPUT]</color>Showing name input window");
            ClearTextFields();
            nameInputAnimator.SetTrigger(OpenTrigger);
        }
        
        public void SubmitInput()
        {
            Debug.Log("<color=lightblue>[SUBMIT]</color> name input");
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

        public void ClearTextFields()
        {
            firstNameInputField.text = string.Empty;
            middleNameInputField.text = string.Empty;
            lastNameInputField.text = string.Empty;
        }

        public void TurnOffInputFields()
        {
            firstNameInputField.gameObject.SetActive(false);
            middleNameInputField.gameObject.SetActive(false);
            lastNameInputField.gameObject.SetActive(false);
        }

        public void ChangeTexture()
        {
            firstNameInputField.GetComponent<TMP_InputField>().enabled = false;
            firstNameInputField.GetComponent<Image>().sprite = textureSwapImage;
            
            middleNameInputField.GetComponent<TMP_InputField>().enabled = false;
            middleNameInputField.GetComponent<Image>().sprite = textureSwapImage;
            
            lastNameInputField.GetComponent<TMP_InputField>().enabled = false;
            lastNameInputField.GetComponent<Image>().sprite = textureSwapImage;
        }
    }
}
