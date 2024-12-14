using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using iTextSharp.text.pdf;
using NameChangeSimulator.Shared.Shared.Classes;
using NameChangeSimulator.Shared.Shared.ScriptableObjects;

public class PDFFieldReaderEditor : EditorWindow
{
    private Object pdfFile; // Generic Unity object for selecting the file
    private Vector2 scrollPosition; // Scroll position for the output section
    private string outputText = "";
    List<PDFField> fieldsList = new List<PDFField>();
    private bool fileRead = false;

    [MenuItem("Tools/PDF Field Reader")]
    public static void ShowWindow()
    {
        GetWindow<PDFFieldReaderEditor>("PDF Field Reader");
    }

    void OnGUI()
    {
        GUILayout.Label("PDF Field Reader", EditorStyles.boldLabel);
        
        GUILayout.Space(10);

        // File selector
        pdfFile = EditorGUILayout.ObjectField("PDF File", pdfFile, typeof(Object), false);

        // Button to read PDF fields
        GUILayout.Label(
            pdfFile != null ? "Oh yeah, lemme read those hot steamy fields, ma'am :3" : "No pdf for good girl? :(",
            EditorStyles.boldLabel);

        GUILayout.Space(10);
        
        if (GUILayout.Button("Read PDF Fields"))
        {
            if (pdfFile != null)
            {
                string path = AssetDatabase.GetAssetPath(pdfFile);
                if (File.Exists(path))
                {
                    ReadPDFFillableFields(File.ReadAllBytes(path));
                }
                else
                {
                    outputText = "Invalid file path.";
                }
            }
            else
            {
                outputText = "No PDF file assigned.";
            }
        }
        
        GUILayout.Space(10);
        
        if (fileRead)
        {
            // Button to read PDF fields
            if (GUILayout.Button("Save Me!"))
            {
                // Create ScriptableObject
                CreatePDFFieldScriptableObject(fieldsList);
            }
        }
        
        GUILayout.Space(10);
        
        // Output section with scroll view
        GUILayout.Label("Output:", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(800));
        EditorGUILayout.TextArea(outputText, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    void ReadPDFFillableFields(byte[] pdfBytes)
    {
        try
        {
            using (MemoryStream stream = new MemoryStream(pdfBytes))
            {
                // Create a PdfReader for the PDF file
                PdfReader reader = new PdfReader(stream);
                
                string result = "Form Fields:\n";

                // Get the AcroFields from the PDF
                var acroFields = reader.AcroFields;
                foreach (var field in acroFields.Fields)
                {
                    string fieldName = field.Key; // Field name
                    string fieldValue = acroFields.GetField(fieldName); // Field value (empty if not filled)
                    string fieldType = GetFieldType(acroFields.GetFieldType(fieldName)); // Field type (e.g., Text, Checkbox)

                    var fieldData = new PDFField
                    {
                        fieldName = fieldName,
                        fieldValue = fieldValue,
                        fieldType = fieldType,
                        options = null
                    };

                    result += $"- Name: {fieldName}\n  Value: {fieldValue}\n  Type: {fieldType}\n";

                    // Check for dropdown options (combobox)
                    if (acroFields.GetFieldType(fieldName) == AcroFields.FIELD_TYPE_COMBO)
                    {
                        var options = acroFields.GetListOptionExport(fieldName); // Export values for dropdown
                        if (options != null && options.Length > 0)
                        {
                            fieldData.options = options;
                            result += "  Dropdown Options:\n";
                            result = options.Where(option => !string.IsNullOrWhiteSpace(option)).Aggregate(result, (current, option) => current + $"    - {option}\n");
                        }
                    }

                    // Check for radio button options
                    if (acroFields.GetFieldType(fieldName) == AcroFields.FIELD_TYPE_RADIOBUTTON)
                    {
                        var appearances = acroFields.GetAppearanceStates(fieldName); // Radio button options
                        if (appearances != null && appearances.Length > 0)
                        {
                            fieldData.options = appearances;
                            result += "  Radio Button Options:\n";
                            result = appearances.Where(option => option != "Off").Aggregate(result, (current, option) => current + $"    - {option}\n");
                        }
                    }

                    result += "\n";
                    fieldsList.Add(fieldData);
                }

                if (acroFields.Fields.Count == 0)
                {
                    result += "No form fields found in the PDF.\n";
                }

                outputText = result;
                
                fileRead = true;

                // Close the PdfReader
                reader.Close();
            }
        }
        catch (IOException e)
        {
            outputText = $"Error reading PDF: {e.Message}";
        }
    }

    void CreatePDFFieldScriptableObject(List<PDFField> fieldsList)
    {
        PDFFieldData scriptableObject = CreateInstance<PDFFieldData>();
        scriptableObject.Fields = fieldsList.ToArray();

        string path = EditorUtility.SaveFilePanelInProject("Save PDF Fields", "PDFFields", "asset", "Save the fields as a ScriptableObject asset.", "Assets/Resources/States");

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(scriptableObject, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    // Helper function to interpret field type
    string GetFieldType(int fieldType)
    {
        switch (fieldType)
        {
            case AcroFields.FIELD_TYPE_CHECKBOX: return "Checkbox";
            case AcroFields.FIELD_TYPE_COMBO: return "Dropdown";
            case AcroFields.FIELD_TYPE_LIST: return "List";
            case AcroFields.FIELD_TYPE_NONE: return "None";
            case AcroFields.FIELD_TYPE_PUSHBUTTON: return "Button";
            case AcroFields.FIELD_TYPE_RADIOBUTTON: return "Radio Button";
            case AcroFields.FIELD_TYPE_SIGNATURE: return "Signature";
            case AcroFields.FIELD_TYPE_TEXT: return "Text Field";
            default: return "Unknown";
        }
    }
}
