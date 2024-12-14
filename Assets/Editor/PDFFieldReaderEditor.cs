using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using iTextSharp.text.pdf;

[System.Serializable]
public class PDFFieldData
{
    public string fieldName;
    public string fieldValue;
    public string fieldType;
    public string[] options; // Dropdown or radio button options
}

[CreateAssetMenu(fileName = "PDFFields", menuName = "ScriptableObjects/PDFFields", order = 1)]
public class PDFFieldScriptableObject : ScriptableObject
{
    public PDFFieldData[] fields;
}

public class PDFFieldReaderEditor : EditorWindow
{
    private Object pdfFile; // Generic Unity object for selecting the file
    private Vector2 scrollPosition; // Scroll position for the output section
    private string outputText = "";
    List<PDFFieldData> fieldsList = new List<PDFFieldData>();
    private bool fileRead = false;

    [MenuItem("Tools/PDF Field Reader")]
    public static void ShowWindow()
    {
        GetWindow<PDFFieldReaderEditor>("PDF Field Reader");
    }

    void OnGUI()
    {
        GUILayout.Label("PDF Field Reader", EditorStyles.boldLabel);

        // File selector
        pdfFile = EditorGUILayout.ObjectField("PDF File", pdfFile, typeof(Object), false);

        // Button to read PDF fields
        if (pdfFile != null)
        {
            GUILayout.Label("Oh yeah, lemme read those hot steamy fields, ma'am :3", EditorStyles.boldLabel);
        }
        else
        {
            GUILayout.Label("No pdf for good girl? :(", EditorStyles.boldLabel);
        }
        
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

        if (fileRead)
        {
            // Button to read PDF fields
            if (GUILayout.Button("Save Me!"))
            {
                // Create ScriptableObject
                CreatePDFFieldScriptableObject(fieldsList);
            }
        }

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

                    var fieldData = new PDFFieldData
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
                            foreach (var option in options)
                            {
                                result += $"    - {option}\n";
                            }
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
                            foreach (var option in appearances)
                            {
                                result += $"    - {option}\n";
                            }
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

    void CreatePDFFieldScriptableObject(System.Collections.Generic.List<PDFFieldData> fieldsList)
    {
        PDFFieldScriptableObject scriptableObject = CreateInstance<PDFFieldScriptableObject>();
        scriptableObject.fields = fieldsList.ToArray();

        string path = EditorUtility.SaveFilePanelInProject("Save PDF Fields", "PDFFields", "asset", "Save the fields as a ScriptableObject asset.");

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
