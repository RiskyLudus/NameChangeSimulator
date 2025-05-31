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

        public void DisplayInputWindow()
        {
            Debug.Log("Showing input window");
            inputField.text = string.Empty;
                inputField.characterLimit = 64;
            container.SetActive(true);
        }
        
        public void SubmitInput()
        {
            container.SetActive(false);
            AudioManager.Instance.PlayUIConfirm_SFX();
            dialogueController.GoToNext(inputField.text);
        }

        public void Close()
        {
            container.SetActive(false);
        }
    }
}
