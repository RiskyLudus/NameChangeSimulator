using System.Linq;
using System.Collections.Generic;
using Anarchy.Shared;
using NameChangeSimulator.Shared;
using TMPro;
using UnityEngine;

namespace NameChangeSimulator.Constructs.StatePicker
{
    public class StatePickerController : MonoBehaviour
    {
        [SerializeField] private SupportedStateData supportedStateData;
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private TMP_Text dropdownText; // We're being hella lazy about this lol
        
        private string _nextNodeFieldName = string.Empty;

        private void OnEnable()
        {
            ConstructBindings.Send_StatePickerData_ShowStatePickerWindow?.AddListener(OnShowStatePickerWindow);
            ConstructBindings.Send_StatePickerData_CloseStatePickerWindow?.AddListener(OnCloseStatePickerWindow);
            ConstructBindings.Send_NodeLoaderData_CloseAllWindows?.AddListener(OnCloseStatePickerWindow);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_StatePickerData_ShowStatePickerWindow?.RemoveListener(OnShowStatePickerWindow);
            ConstructBindings.Send_StatePickerData_CloseStatePickerWindow?.RemoveListener(OnCloseStatePickerWindow);
            ConstructBindings.Send_NodeLoaderData_CloseAllWindows?.RemoveListener(OnCloseStatePickerWindow);
        }

        private void Start()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(supportedStateData.supportedStates);
        }
        
        private void OnShowStatePickerWindow(string nextNodeString)
        {
            container.SetActive(true);
            _nextNodeFieldName = nextNodeString;
        }

        private void OnCloseStatePickerWindow()
        {
            container.SetActive(false);
        }

        public void SubmitStatePick()
        {
            ConstructBindings.Send_StatePickerData_SendStateString?.Invoke(dropdownText.text, _nextNodeFieldName);
            ConstructBindings.Send_StatePickerData_CloseStatePickerWindow?.Invoke();
        }
    }
}
