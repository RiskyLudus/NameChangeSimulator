using Anarchy.Shared;
using TMPro;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Dialogue.InputBox
{
    public class InputBoxController : MonoBehaviour
    {
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_InputField inputField;

        public void DisplayInputWindow()
        {
            Debug.Log("Showing input window");
            inputField.text = string.Empty;
            container.SetActive(true);
        }
        
        public void SubmitInput()
        {
            container.SetActive(false);
            dialogueController.GoToNext(inputField.text);
        }
    }
}
