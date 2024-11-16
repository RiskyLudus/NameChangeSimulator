using System;
using Anarchy.Shared;
using TMPro;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Input
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private InputData inputData;

        [SerializeField] private GameObject container;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text placeholderText;
        [SerializeField] private TMP_Text promptText;

        private void OnEnable()
        {
            ConstructBindings.Send_InputData_ShowInputWindow?.AddListener(OnToggleInputWindow);
            ConstructBindings.Send_InputData_CloseInputWindow?.AddListener(OnCloseInputWindow);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_InputData_ShowInputWindow?.RemoveListener(OnToggleInputWindow);
            ConstructBindings.Send_InputData_CloseInputWindow?.RemoveListener(OnCloseInputWindow);
        }

        private void OnToggleInputWindow(string promptString, string placeholderString)
        {
            placeholderText.text = placeholderString;
            promptText.text = promptString;
            container.SetActive(true);
        }
        
        private void OnCloseInputWindow()
        {
            placeholderText.text = string.Empty;
            promptText.text = string.Empty;
            container.SetActive(false);
        }
        
        public void SubmitInput()
        {
            ConstructBindings.Send_InputData_SubmitInput?.Invoke(inputField.text);
        }
    }
}
