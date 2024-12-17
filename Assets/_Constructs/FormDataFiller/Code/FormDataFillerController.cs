using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Anarchy.Shared;
using iTextSharp.text.pdf;
using NameChangeSimulator.Shared;
using NameChangeSimulator.Shared.Shared.Classes;
using NameChangeSimulator.Shared.Shared.ScriptableObjects;
using UnityEngine;

namespace NameChangeSimulator.Constructs.FormDataFiller
{
    public class FormDataFillerController : MonoBehaviour
    {
        private PDFFieldData _fieldData;
        
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
            Debug.Log($"Loading Form Data: {formDataToLoad}");
            
            // Load the Form Data
            _fieldData = Resources.LoadAll<PDFFieldData>("States/" + formDataToLoad).First();
            if (_fieldData == null)
            {
                Debug.LogError($"Failed to load form data {formDataToLoad}");
            }
        }
        
        private void OnSubmit(string keyword, string value)
        {
            _fieldData.SetValue(keyword, value);
        }
        
        private void OnApplyToPDF()
        {
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
                SetPDFFields(pdfBytes, fieldData.Fields, pdfFilePath);
            }
        }

        private void SetPDFFields(byte[] pdfBytes, PDFField[] pdfFields, string originalFilePath)
        {
            try
            {
                // Define output file path
                string outputFilePath = Path.Combine(Path.GetDirectoryName(originalFilePath), "Updated_" + Path.GetFileName(originalFilePath));

                // Load the PDF into a reader
                using MemoryStream inputStream = new MemoryStream(pdfBytes);
                using PdfReader reader = new PdfReader(inputStream);
                using FileStream outputStream = new FileStream(outputFilePath, FileMode.Create);
                using PdfStamper stamper = new PdfStamper(reader, outputStream);

                // Get the AcroFields from the PDF
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

                // Finalize changes (flatten the form if needed, optional)
                stamper.FormFlattening = true;

                Debug.Log("PDF fields updated successfully!");
                Debug.Log($"Updated PDF saved to: {outputFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating PDF fields: {e.Message}");
            }
        }
    }
}
