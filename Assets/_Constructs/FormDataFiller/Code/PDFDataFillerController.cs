using System;
using System.IO;
using UnityEngine;
using iTextSharp.text.pdf;
using NameChangeSimulator.Shared.Shared.Classes;
using NameChangeSimulator.Shared.Shared.ScriptableObjects;

namespace NameChangeSimulator.Constructs.FormDataFiller
{
    public class PDFDataFillerController : MonoBehaviour
    {
        [SerializeField] private bool _testFormFilling = false;
        [SerializeField] private string _pdfFileName; // Name of the PDF file in StreamingAssets
        [SerializeField] private PDFFieldData _testFormFillingFieldData;
        [SerializeField] private string _dummyText;

        private void Update()
        {
            if (!_testFormFilling) return;
            _testFormFilling = false;

            string pdfFilePath = Path.Combine(Application.streamingAssetsPath, _pdfFileName + ".pdf");
            RunDataFiller(pdfFilePath, _testFormFillingFieldData);
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
                        form.SetField(field.fieldName, _dummyText);
                        Debug.Log($"Field '{field.fieldName}' set to '{_dummyText}'.");
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
