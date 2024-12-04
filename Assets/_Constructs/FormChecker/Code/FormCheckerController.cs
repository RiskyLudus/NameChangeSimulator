using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Anarchy.Shared;
using Mono.Cecil;
using NameChangeSimulator.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.FormChecker
{
    public class FormCheckerController : MonoBehaviour
    {
        [SerializeField] private FormCheckerData formCheckerData;
        [SerializeField] private GameObject container;
        [SerializeField] private GameObject formImageTemplate;
        [SerializeField] private Transform formImageLayout;

        [Header("DEBUG")] 
        [SerializeField] private string checkFormStateData = "Oregon";
        [SerializeField] private bool checkFormLoading = false;

        private List<GameObject> _forms = new List<GameObject>();

        private void OnEnable()
        {
            ConstructBindings.Send_FormCheckerData_ShowForm?.AddListener(OnShowForm);
            ConstructBindings.Send_FormCheckerData_CloseForm.AddListener(OnCloseForm);
        }
        
        private void OnDisable()
        {
            ConstructBindings.Send_FormCheckerData_ShowForm?.RemoveListener(OnShowForm);
            ConstructBindings.Send_FormCheckerData_CloseForm.RemoveListener(OnCloseForm);
        }

        private void Update()
        {
            if (checkFormLoading)
            {
                checkFormLoading = false;
                ConstructBindings.Send_FormCheckerData_ShowForm?.Invoke(checkFormStateData);
            }
        }

        private void OnShowForm(string stateName)
        {
            StateData[] data = Resources.LoadAll<StateData>($"States/{stateName}/");
            foreach (var stateData in data)
            {
                var form = Instantiate(formImageTemplate, formImageLayout);
                form.gameObject.SetActive(true);
                form.GetComponent<Image>().sprite = stateData.formSprite;
                var formFields = Instantiate(stateData.formFieldObject, form.transform);
                _forms.Add(formFields);
                SetFieldsOnForm(formFields, stateData);
            }
            container.SetActive(true);
        }

        private void OnCloseForm()
        {
            container.SetActive(false);
            foreach (var form in _forms)
            {
                Destroy(form);
            }
            _forms.Clear();
        }

        // We are checking the fields in StateData and marking the filling in the relevant fields on the form.
        private void SetFieldsOnForm(GameObject form, StateData data)
        {
            for (var i = 0; i < data.fields.Length; i++)
            {
                var field = data.fields[i];
                var fieldName = field.Name;
                var fieldValue = field.Value;
                
                for (var j = 0; j < form.transform.childCount; j++)
                {
                    var child = form.transform.GetChild(j);
                    if (child.name == fieldName)
                    {
                        if (child.TryGetComponent<TMP_Text>(out var textField))
                        {
                            textField.text = fieldValue;
                        }
                        else
                        {
                            if (child.TryGetComponent<Image>(out var check))
                            {
                                check.enabled = fieldValue == "True";
                            }
                        }
                    }
                }
            }
        }
    }
}
