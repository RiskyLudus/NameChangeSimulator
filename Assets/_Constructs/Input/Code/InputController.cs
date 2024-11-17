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
        
        private string _keyword = string.Empty;

        private void OnEnable()
        {
            ConstructBindings.Send_InputData_ShowInputWindow?.AddListener(OnShowInputWindow);
            ConstructBindings.Send_InputData_CloseInputWindow?.AddListener(OnCloseInputWindow);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_InputData_ShowInputWindow?.RemoveListener(OnShowInputWindow);
            ConstructBindings.Send_InputData_CloseInputWindow?.RemoveListener(OnCloseInputWindow);
        }

        private void OnShowInputWindow(string keyword, string promptString, string placeholderString)
        {
            _keyword = keyword;
            placeholderText.text = placeholderString;
            promptText.text = promptString;
            container.SetActive(true);
        }
        
        private void OnCloseInputWindow()
        {
            _keyword = string.Empty;
            placeholderText.text = string.Empty;
            promptText.text = string.Empty;
            container.SetActive(false);
        }
        
        public void SubmitInput()
        {
            ConstructBindings.Send_InputData_SubmitInput?.Invoke(_keyword, inputField.text);
        }
    }
}
