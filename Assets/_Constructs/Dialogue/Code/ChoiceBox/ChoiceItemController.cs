using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.Dialogue.ChoiceBox
{
    public class ChoiceItemController : MonoBehaviour
    {
        [SerializeField] private TMP_Text promptText;
        
        private ChoiceBoxController _choicesController = null;
        
        public void Initialize(string optionText,ChoiceBoxController controller)
        {
            promptText.text = optionText;
            _choicesController = controller;
        }

        public void SubmitChoice()
        {
            _choicesController.Submit(promptText.text);
        }
    }
}
