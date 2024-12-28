using TMPro;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Dialogue.InputBox
{
    public class NewNameInputBoxController : MonoBehaviour
    {
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_InputField firstNameInputField, middleNameInputField, lastNameInputField;
        
        public void DisplayNewNameInputWindow()
        {
            Debug.Log("Showing input window");
            firstNameInputField.text = string.Empty;
            middleNameInputField.text = string.Empty;
            lastNameInputField.text = string.Empty;
            container.SetActive(true);
        }
        
        public void SubmitInput()
        {
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