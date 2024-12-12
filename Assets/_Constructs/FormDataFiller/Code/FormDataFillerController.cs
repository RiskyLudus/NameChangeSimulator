using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Anarchy.Shared;
using NameChangeSimulator.Shared;
using UnityEngine;

namespace NameChangeSimulator.Constructs.FormDataFiller
{
    public class FormDataFillerController : MonoBehaviour
    {
        [SerializeField] private IntroductionStateData introductionStateData;
        [SerializeField] private FormDataFillerData formDataFillerData;
        [SerializeField] private StateData[] stateDatas;

        private const string currentDateKeywordString = "CurrentDate";
        private const string currentDayKeywordString = "CurrentDay";
        private const string currentMonthKeywordString = "CurrentMonth";
        private const string currentMonthNameKeywordString = "CurrentMonthName";
        private const string currentYearKeywordString = "CurrentYear";
        private const string currentYearEndKeywordString = "CurrentYearEnd";

        private void OnEnable()
        {
            ConstructBindings.Send_FormDataFillerData_LoadFormFiller?.AddListener(OnLoadFormFiller);
            ConstructBindings.Send_InputData_SubmitInput?.AddListener(OnSubmitInput);
            ConstructBindings.Send_ChoicesData_SubmitChoice?.AddListener(OnSubmitChoice);
            ConstructBindings.Send_MultiInputData_SubmitMultiInput?.AddListener(OnSubmitMultiInput);
            ConstructBindings.Send_StatePickerData_SendStateString?.AddListener(OnSendStateString);
        }

        private void OnLoadFormFiller(string stateName)
        {
            // Load relevant state datas
            stateDatas = Resources.LoadAll<StateData>($"States/{stateName}");
            
            // Reset all fields for fresh experience
            foreach (StateData state in stateDatas)
            {
                foreach (Field field in state.fields)
                {
                    field.Value = string.Empty;
                }
            }

            // Set the progress bar
            var maxProgressToSet = new HashSet<Field>();
            
            foreach (StateData state in stateDatas)
            {
                foreach (Field field in state.fields)
                {
                    maxProgressToSet.Add(field);
                }
            }

            SetNonUserFields();
            PrefillFieldsFromIntroductionData();
            
            ConstructBindings.Send_ProgressBarData_ShowProgressBar?.Invoke(GetCompletedFieldsOnDatas(), maxProgressToSet.Count());
        }

        // Here we are setting fields that do not require user input
        private void SetNonUserFields()
        {
            // Set Current Date to relevant fields in forms
            foreach (var stateData in stateDatas)
            {
                foreach (var field in stateData.fields)
                {
                    field.Value = field.Name switch
                    {
                        currentDateKeywordString => DateTime.Today.ToString("MM/dd/yyyy"),
                        currentDayKeywordString => DateTime.Today.ToString("dd"),
                        currentMonthKeywordString => DateTime.Today.ToString("MM"),
                        currentMonthNameKeywordString => DateTime.Today.ToString("MMMM"),
                        currentYearKeywordString => DateTime.Today.Year.ToString(),
                        currentYearEndKeywordString => DateTime.Today.Year.ToString("yy"),
                        _ => string.Empty
                    };
                    if (field.Name == currentDateKeywordString)
                    {
                        field.Value = DateTime.Today.ToString("MM/dd/yyyy");
                    }
                }
            }
            
            // Set Name Change Check to relevant fields in forms
            foreach (var stateData in stateDatas)
            {
                foreach (var field in stateData.fields)
                {
                    if (field.Name == "ChangeNameCheck")
                    {
                        field.Value = "True";
                    }
                }
            }
        }

        private void PrefillFieldsFromIntroductionData()
        {
            introductionStateData.AddCityStateZip();
            introductionStateData.AddNewFullName();
            foreach (var stateData in stateDatas)
            {
                foreach (var field in stateData.fields)
                {
                    foreach (var introductionField in introductionStateData.fields)
                    {
                        if (introductionField.Name == field.Name)
                        {
                            field.Value = introductionField.Value;
                        }
                    }
                }
            }
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
            
            ConstructBindings.Send_ProgressBarData_UpdateProgress?.Invoke(GetCompletedFieldsOnDatas());
            ConstructBindings.Send_ConversationData_SubmitNextNode?.Invoke(nodeFieldName);
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
            
            ConstructBindings.Send_ProgressBarData_UpdateProgress?.Invoke(GetCompletedFieldsOnDatas());
            ConstructBindings.Send_ConversationData_SubmitNextNode?.Invoke(nodeFieldName);
        }

        private void OnSubmitMultiInput(string keyword, string inputText, string delimiter, string nodeFieldName)
        {
            // Split the input text and remove any empty entries from the list
            var inputs = inputText.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (StateData stateData in stateDatas)
            {
                // Loop through inputs to set the values in fields
                for (int i = 0; i < inputs.Count; i++)
                {
                    Debug.Log(inputs[i]);
                    foreach (var field in stateData.fields)
                    {
                        if (field.Name == (keyword + i.ToString()))
                        {
                            field.Value = inputs[i];
                            break; // Exit the inner loop once a match is found and value is assigned
                        }
                    }
                }
            }

            ConstructBindings.Send_ProgressBarData_UpdateProgress?.Invoke(GetCompletedFieldsOnDatas());
            ConstructBindings.Send_ConversationData_SubmitNextNode?.Invoke(nodeFieldName);
        }
        
        private void OnSendStateString(string stateString, string nodeFieldName)
        {
            foreach (var stateData in stateDatas)
            {
                foreach (var field in stateData.fields)
                {
                    if (field.Name == "State")
                    {
                        field.Value = stateString;
                    }
                }
            }
            ConstructBindings.Send_ProgressBarData_UpdateProgress?.Invoke(GetCompletedFieldsOnDatas());
            ConstructBindings.Send_ConversationData_SubmitNextNode?.Invoke(nodeFieldName);
        }

        private int GetCompletedFieldsOnDatas()
        {
            int completed = 0;
            foreach (var stateData in stateDatas)
            {
                completed += stateData.GetCompletedFields();
            }
            return completed;
        }
    }
}
