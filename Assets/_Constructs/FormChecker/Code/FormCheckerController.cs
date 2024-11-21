using System;
using System.IO;
using System.Linq;
using Anarchy.Shared;
using Mono.Cecil;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.FormChecker
{
    public class FormCheckerController : MonoBehaviour
    {
        [SerializeField] private FormCheckerData formCheckerData;
        [SerializeField] private GameObject container;
        [SerializeField] private Image formImage;

        [Header("DEBUG")] 
        [SerializeField] private string checkFormStateData = "Oregon_Form_1_Data";
        [SerializeField] private bool checkFormLoading = false;

        private GameObject _form = null;

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
            StateData data = Resources.LoadAll<StateData>($"States/{stateName}/").First();
            formImage.sprite = data.formSprite;
            _form = Instantiate(data.formFieldObject, formImage.transform);
            formImage.enabled = true;
            container.SetActive(true);
            SetFieldsOnForm(data);
        }

        private void OnCloseForm()
        {
            container.SetActive(false);
            Destroy(_form);
            _form = null;
            formImage.sprite = null;
            formImage.enabled = false;
        }

        private void SetFieldsOnForm(StateData data)
        {
            for (int i = 0; i < _form.transform.childCount; i++)
            {
                var matchingField = data.fields.First(t => t.Name == _form.transform.GetChild(i).name);
                if (matchingField.IsText)
                {
                    _form.transform.GetChild(i).GetComponent<TMP_Text>().text = matchingField.Value;
                }
                else
                {
                    _form.transform.GetChild(i).GetComponent<Image>().enabled = matchingField.Value == "True";
                }
            }
        }
    }
}
