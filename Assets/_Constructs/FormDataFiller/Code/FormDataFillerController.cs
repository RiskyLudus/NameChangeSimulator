using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Anarchy.Shared;
using iTextSharp.text.pdf;
using NameChangeSimulator.Shared.Shared.Classes;
using NameChangeSimulator.Shared.Shared.ScriptableObjects;
using TMPro;
using UnityEngine;

namespace NameChangeSimulator.Constructs.FormDataFiller
{
    public class FormDataFillerController : MonoBehaviour
    {
        private PDFFieldData _fieldData;
        private string _fullDeadName => $"{_deadFirstName} {_deadMiddleName} {_deadLastName}";
        private string _deadFirstName, _deadMiddleName, _deadLastName;
        
        
        private string _fullNewName => $"{_newFirstName} {_newMiddleName} {_newLastName}";
        private string _newFirstName, _newMiddleName, _newLastName;
        
        private void OnEnable()
        {
            ConstructBindings.Send_FormDataFillerData_Load?.AddListener(OnLoad);
            ConstructBindings.Send_FormDataFillerData_Submit?.AddListener(OnSubmit);
            ConstructBindings.Send_FormDataFillerData_ApplyToPDF?.AddListener(OnApplyToPDF);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_FormDataFillerData_Load?.RemoveListener(OnLoad);
            ConstructBindings.Send_FormDataFillerData_Submit?.RemoveListener(OnSubmit);
            ConstructBindings.Send_FormDataFillerData_ApplyToPDF?.RemoveListener(OnApplyToPDF);
        }

        private void OnLoad(string formDataToLoad)
        {
            Debug.Log($"<color=silver>[LOAD]</color> Form Data: {formDataToLoad}");
            
            // Load the Form Data
            _fieldData = Resources.LoadAll<PDFFieldData>("States/" + formDataToLoad).First();
            if (_fieldData == null)
            {
                Debug.LogError($"Failed to load form data {formDataToLoad}");
            }
            
            _fieldData.ClearValues();
            
            // Set Dead Name Fields
            _fieldData.SetOverrideValue("FullDeadName", _fullDeadName);
            _fieldData.SetOverrideValue("DeadFirstName", _deadFirstName);
            _fieldData.SetOverrideValue("DeadMiddleName", _deadMiddleName);
            _fieldData.SetOverrideValue("DeadLastName", _deadLastName);
            
            // Set New Name Fields
            _fieldData.SetOverrideValue("FullNewName", _fullNewName);
            _fieldData.SetOverrideValue("NewFirstName", _newFirstName);
            _fieldData.SetOverrideValue("NewMiddleName", _newMiddleName);
            _fieldData.SetOverrideValue("NewLastName", _newLastName);
            
            _fieldData.SetOverrideValue("IsAdult", "Yes");
        }
        
        private void OnSubmit(string keyword, string value)
        {
            if (keyword.Contains("&"))
            {
                foreach (var keywordPart in keyword.Split('&'))
                {
                    OnSubmit(keywordPart, value);
                }
            }
            else
            {
                switch (keyword)
                {
                    case "Dead Name Input":
                    {
                        string[] parsedValues = value.Split('~');
                        _deadFirstName = parsedValues[0];
                        _deadMiddleName = parsedValues[1];
                        _deadLastName = parsedValues[2];
                        break;
                    }
                    case "New Name Input":
                    {
                        string[] parsedValues = value.Split('~');
                        _newFirstName = parsedValues[0];
                        PlayerPrefs.SetString("NewFirstName", _newFirstName);
                        _newMiddleName = parsedValues[1];
                        _newLastName = parsedValues[2];
                        break;
                    }
                    default:
                        Debug.Log($"Keyword {keyword} with Value {value}");
                        _fieldData.SetValue(keyword, value);
                        break;
                }
            }
        }
        
        private void OnApplyToPDF()
        {
            ConstructBindings.Send_ProgressBarData_CloseProgressBar?.Invoke();
            RunDataFiller(Path.Combine(Application.streamingAssetsPath, _fieldData.PdfFileName + ".pdf"), _fieldData);
        }
        
        public void RunDataFiller(string pdfFilePath, PDFFieldData fieldData)
        {
            if (string.IsNullOrEmpty(pdfFilePath))
            {
                Debug.LogError("PDF file path is not assigned.");
                return;
            }

            if (!File.Exists(pdfFilePath))
            {
                Debug.LogError($"PDF file not found at path: {pdfFilePath}");
                return;
            }

            // Read the PDF file into a byte array
            byte[] pdfBytes = File.ReadAllBytes(pdfFilePath);

            if (pdfBytes != null)
            {
                StartCoroutine(SetPDFFields(pdfBytes, fieldData.Fields, pdfFilePath));
            }
        }

        private IEnumerator SetPDFFields(byte[] pdfBytes, PDFField[] pdfFields, string originalFilePath)
        {
            string outputFilePath = Path.Combine(Path.GetDirectoryName(originalFilePath), "Updated_" + Path.GetFileName(originalFilePath));
            byte[] updatedPdfBytes;

            using (MemoryStream inputStream = new MemoryStream(pdfBytes))
            using (PdfReader reader = new PdfReader(inputStream))
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (PdfStamper stamper = new PdfStamper(reader, outputStream))
                {
                    AcroFields form = stamper.AcroFields;

                    foreach (var field in pdfFields)
                    {
                        if (form.Fields.ContainsKey(field.fieldName))
                        {
                            form.SetField(field.fieldName, field.fieldValue);
                            Debug.Log($"Field '{field.fieldName}' set to '{field.fieldValue}'.");
                        }
                        else
                        {
                            Debug.LogWarning($"Field '{field.fieldName}' not found in the PDF.");
                        }
                    }

                    stamper.FormFlattening = true; // Flatten form (optional)
                } // Ensure PdfStamper is closed
                updatedPdfBytes = outputStream.ToArray();
            }

            File.WriteAllBytes(outputFilePath, updatedPdfBytes); // Save updated PDF
            Debug.Log($"Updated PDF saved to: {outputFilePath}");

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
            // Load the updated PDF for viewing
            ConstructBindings.Send_PDFViewerData_Load?.Invoke(updatedPdfBytes);

            yield return null;
        }
    }
}
