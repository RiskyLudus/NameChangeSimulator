using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.Choices
{
    public class ChoiceItemController : MonoBehaviour
    {
        [SerializeField] private TMP_Text promptText;
        
        private ChoicesController _choicesController = null;
        private int _choiceIndex = 0;
        
        public void Initialize(string prompt, int choiceIndex, ChoicesController controller)
        {
            promptText.text = prompt;
            _choicesController = controller;
            _choiceIndex = choiceIndex;
        }

        public void SubmitChoice()
        {
            _choicesController.Submit(_choiceIndex);
        }
    }
}
