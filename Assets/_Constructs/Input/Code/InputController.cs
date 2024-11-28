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
        
        private string _keyword = string.Empty;
        private string _nodeFieldName = string.Empty;

        private void OnEnable()
        {
            ConstructBindings.Send_InputData_ShowInputWindow?.AddListener(OnShowInputWindow);
            ConstructBindings.Send_InputData_CloseInputWindow?.AddListener(OnCloseInputWindow);
            ConstructBindings.Send_NodeLoaderData_CloseAllWindows?.AddListener(OnCloseInputWindow);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_InputData_ShowInputWindow?.RemoveListener(OnShowInputWindow);
            ConstructBindings.Send_InputData_CloseInputWindow?.RemoveListener(OnCloseInputWindow);
            ConstructBindings.Send_NodeLoaderData_CloseAllWindows?.RemoveListener(OnCloseInputWindow);
        }

        private void OnShowInputWindow(string keyword, string nodeFieldName)
        {
            _keyword = keyword;
            _nodeFieldName = nodeFieldName;
            inputField.text = string.Empty;
            container.SetActive(true);
        }
        
        private void OnCloseInputWindow()
        {
            container.SetActive(false);
        }
        
        public void SubmitInput()
        {
            OnCloseInputWindow();
            ConstructBindings.Send_InputData_SubmitInput?.Invoke(_keyword, inputField.text, _nodeFieldName);
        }
    }
}
