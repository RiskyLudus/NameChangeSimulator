using System;
using System.IO;
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

        private void OnLoadFormFiller(string formDataName)
        {
            StateData data = Resources.Load<StateData>(Path.Combine("StateData", formDataName));
            stateData = data;
        }

        private void OnSubmitInput(string keyword, string inputValue)
        {
            foreach (var field in stateData.fields)
            {
                if (field.Name == keyword)
                {
                    field.Value = inputValue;
                }
                break;
            }
        }
        
        private void OnSubmitChoice(string keyword, bool toggle)
        {
            foreach (var field in stateData.fields)
            {
                if (field.Name == keyword)
                {
                    field.Value =  toggle ? "True" : "False";
                }
                break;
            }
        }
    }
}
