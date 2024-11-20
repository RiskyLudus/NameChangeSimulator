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
        [SerializeField] private StateData stateData;

        private void OnEnable()
        {
            ConstructBindings.Send_FormDataFillerData_LoadFormFiller?.AddListener(OnLoadFormFiller);
            ConstructBindings.Send_InputData_SubmitInput?.AddListener(OnSubmitInput);
            ConstructBindings.Send_ChoicesData_SubmitChoice?.AddListener(OnSubmitChoice);
        }

        private void OnLoadFormFiller(string stateName)
        {
            StateData data = Resources.LoadAll<StateData>($"States/{stateName}/").First();
            stateData = data;
        }

        private void OnSubmitInput(string keyword, string inputValue, string nodeFieldName)
        {
            foreach (var field in stateData.fields)
            {
                if (field.Name == keyword)
                {
                    field.Value = inputValue;
                }
                break;
            }
            
            ConstructBindings.Send_ConversationData_SubmitNode?.Invoke(nodeFieldName);
        }
        
        private void OnSubmitChoice(string keyword, bool toggle, string nodeFieldName)
        {
            foreach (var field in stateData.fields)
            {
                if (field.Name == keyword)
                {
                    field.Value =  toggle ? "True" : "False";
                }
                break;
            }
            
            ConstructBindings.Send_ConversationData_SubmitNode?.Invoke(nodeFieldName);
        }
    }
}
