using System;
using Anarchy.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.MultiInput
{
    public class MultiInputController : MonoBehaviour
    {
        [SerializeField] private GameObject container;
        [SerializeField] private Transform inputLayoutGroup;
        [SerializeField] private GameObject inputFieldPrefab;
        
        private string _keyword;
        private string _nodeFieldName;

        private void OnEnable()
        {
            ConstructBindings.Send_MultiInputData_ShowMultiInputWindow?.AddListener(OnShowMultiInputWindow);
            ConstructBindings.Send_MultiInputData_CloseMultiInputWindow?.AddListener(OnCloseMultiInputWindow);
            ConstructBindings.Send_NodeLoaderData_CloseAllWindows?.AddListener(OnCloseMultiInputWindow);
        }

        void OnDisable()
        {
            ConstructBindings.Send_MultiInputData_ShowMultiInputWindow?.RemoveListener(OnShowMultiInputWindow);
            ConstructBindings.Send_MultiInputData_CloseMultiInputWindow?.RemoveListener(OnCloseMultiInputWindow);
            ConstructBindings.Send_NodeLoaderData_CloseAllWindows?.RemoveListener(OnCloseMultiInputWindow);
        }

        private void OnShowMultiInputWindow(string keyword, int numberOfFields, string nodeFieldName)
        {
            _keyword = keyword;
            _nodeFieldName = nodeFieldName;

            if (inputLayoutGroup.childCount > 0)
            {
                for (int i = 0; i < inputLayoutGroup.childCount; i++)
                {
                    Destroy(inputLayoutGroup.GetChild(i).gameObject);
                }
            }

            for (int i = 0; i < numberOfFields; i++)
            {
                Instantiate(inputFieldPrefab, inputLayoutGroup);
            }
            
            container.gameObject.SetActive(true);
        }
        
        private void OnCloseMultiInputWindow()
        {
            container.gameObject.SetActive(false);
        }
        
        public void Submit()
        {
            string delimiter = "~";
            string inputText = inputLayoutGroup.GetChild(0).GetComponent<TMP_InputField>().text;
            for (int i = 1; i < inputLayoutGroup.childCount; i++)
            {
                inputText += delimiter + inputLayoutGroup.GetChild(i).GetComponent<TMP_InputField>().text;
            }
            OnCloseMultiInputWindow();
            ConstructBindings.Send_MultiInputData_SubmitMultiInput?.Invoke(_keyword,  inputText, delimiter, _nodeFieldName);
        }
    }
}
