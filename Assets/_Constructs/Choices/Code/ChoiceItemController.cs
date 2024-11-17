using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.Choices
{
    public class ChoiceItemController : MonoBehaviour
    {
        [SerializeField] private TMP_Text promptText;
        
        private ChoicesController _choicesController = null;
        private bool _choice = false;
        
        public void Initialize(string prompt, bool choice, ChoicesController controller)
        {
            promptText.text = prompt;
            _choicesController = controller;
            _choice = choice;
        }

        public void SubmitChoice()
        {
            _choicesController.Submit(_choice);
        }
    }
}
