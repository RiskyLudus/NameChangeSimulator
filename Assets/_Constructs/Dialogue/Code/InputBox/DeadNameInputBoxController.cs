using System.Collections;
using NameChangeSimulator.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NameChangeSimulator.Constructs.Dialogue.InputBox
{
    public class DeadNameInputBoxController : MonoBehaviour
    {
        private static readonly int Burn = Animator.StringToHash("Burn");
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_InputField firstNameInputField, middleNameInputField, lastNameInputField;
        [SerializeField] private Animator burnAnimator;
        private bool isPressed = false;
        
        public void DisplayDeadNameInputWindow()
        {
            Debug.Log("Showing input window");
            // I hate doing individual char limits, but if you have a better idea, I'm all ears. -Ai
            firstNameInputField.text = string.Empty;
                firstNameInputField.characterLimit = 16; 
            middleNameInputField.text = string.Empty;
                middleNameInputField.characterLimit = 16;
            lastNameInputField.text = string.Empty;
                lastNameInputField.characterLimit = 16;
            container.SetActive(true);
        }
        
        public void SubmitInput()
        {
            if (isPressed)
                return;
            
            isPressed = true;
            var fullDeadName = $"{firstNameInputField.text}~{middleNameInputField.text}~{lastNameInputField.text}";
            
            // Play the "Burn" animation before moving on
            burnAnimator.SetTrigger(Burn);
            AudioManager.Instance.PlayBurn_SFX();
            StartCoroutine(WaitForBurn_Co(fullDeadName));
        }

        private IEnumerator WaitForBurn_Co(string fullDeadName)
        {
            yield return new WaitForSeconds(5f);
            container.SetActive(false);
            dialogueController.GoToNext(fullDeadName);
        }

        public void Close()
        {
            container.SetActive(false);
        }
    }
}
