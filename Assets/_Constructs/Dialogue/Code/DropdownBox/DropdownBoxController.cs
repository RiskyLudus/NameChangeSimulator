using NameChangeSimulator.Shared;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NameChangeSimulator.Constructs.Dialogue.DropdownBox
{
    public class DropdownBoxController : MonoBehaviour
    {
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private TMP_Text dropdownText; // We're being hella lazy about this lol
        
        public void DisplayDropdownWindow(string[] options)
        {
            Debug.Log($"Showing dropdown window");
            dropdown.ClearOptions();
            dropdown.AddOptions(options.ToList());
            container.SetActive(true);
        }

        public void SubmitOption()
        {
            AudioManager.Instance.PlayUIConfirm_SFX();
            dialogueController.GoToNext(dropdownText.text);
            container.SetActive(false);
        }

        public void Close()
        {
            container.SetActive(false);
        }
    }
}
