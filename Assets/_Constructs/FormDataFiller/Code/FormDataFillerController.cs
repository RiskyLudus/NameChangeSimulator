using System;
using System.IO;
using System.Linq;
using Anarchy.Shared;
using UnityEngine;

namespace NameChangeSimulator.Constructs.FormDataFiller
{
    public class FormDataFillerController : MonoBehaviour
    {
        [SerializeField] private FormDataFillerData formDataFillerData;
        [SerializeField] private StateData[] stateDatas;

        private void OnEnable()
        {
            ConstructBindings.Send_FormDataFillerData_LoadFormFiller?.AddListener(OnLoadFormFiller);
            ConstructBindings.Send_InputData_SubmitInput?.AddListener(OnSubmitInput);
            ConstructBindings.Send_ChoicesData_SubmitChoice?.AddListener(OnSubmitChoice);
            ConstructBindings.Send_MultiInputData_SubmitMultiInput?.AddListener(OnSubmitMultiInput);
        }

        private void OnLoadFormFiller(string stateName)
        {
            stateDatas = Resources.LoadAll<StateData>("States/Oregon");
        }

        private void OnSubmitInput(string keyword, string inputValue, string nodeFieldName)
        {
            foreach (var stateData in stateDatas)
            {
                foreach (var field in stateData.fields)
                {
                    if (field.Name == keyword)
                    {
                        field.Value = inputValue;
                    }
                }
            }
            
            ConstructBindings.Send_ConversationData_SubmitNode?.Invoke(nodeFieldName);
        }
        
        private void OnSubmitChoice(string keyword, bool toggle, string nodeFieldName)
        {
            foreach (var stateData in stateDatas)
            {
                foreach (var field in stateData.fields)
                {
                    if (field.Name == keyword)
                    {
                        field.Value =  toggle ? "True" : "False";
                    }
                }
            }
            
            ConstructBindings.Send_ConversationData_SubmitNode?.Invoke(nodeFieldName);
        }

        private void OnSubmitMultiInput(string keyword, string inputText, string delimiter, string nodeFieldName)
        {
            var inputs = inputText.Split(delimiter);

            foreach (StateData stateData in stateDatas)
            {
                for (int i = 0; i < inputs.Length; i++)
                {
                    Debug.Log(inputs[i]);
                    foreach (var field in stateData.fields)
                    {
                        if (field.Name == (keyword + i.ToString()))
                        {
                            field.Value = inputs[i];
                        }
                    }
                }
            }
            
            ConstructBindings.Send_ConversationData_SubmitNode?.Invoke(nodeFieldName);
        }
    }
}
