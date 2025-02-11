using NameChangeSimulator.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.Dialogue.InputBox
{
    public class NewNameInputBoxController : MonoBehaviour
    {
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_InputField firstNameInputField, middleNameInputField, lastNameInputField;
        
        public void DisplayNewNameInputWindow()
        {
            // I hate doing individual char limits, but if you have a better idea, I'm all ears. -Ai
            Debug.Log("Showing input window");
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
            AudioManager.Instance.PlayUIConfirm_SFX();
            container.SetActive(false);
            var fullDeadName = $"{firstNameInputField.text}~{middleNameInputField.text}~{lastNameInputField.text}";
            dialogueController.GoToNext(fullDeadName);
        }

        public void Close()
        {
            container.SetActive(false);
        }
    }
}