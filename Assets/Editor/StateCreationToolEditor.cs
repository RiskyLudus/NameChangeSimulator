using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text.pdf;
using NameChangeSimulator.Shared.Node;
using NameChangeSimulator.Shared.Shared.Classes;
using NameChangeSimulator.Shared.Shared.ScriptableObjects;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NameChangeSimulator.Editor
{
    public class StateCreationToolEditor : EditorWindow
    {
        private string stateName;
        private Object pdfFile;
        private Vector2 scrollPosition; // Scroll position for the output section
        private string outputText = "";
        List<PDFField> fieldsList = new List<PDFField>();
        private int stepNumber = 1;
        private bool readyForNextStep = false;
        private string[] folderNames;
        private int selectedFolderIndex = 0;
        private string pdfFilePath;
        private string pdfFileName;
        private Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>(); // Tracks foldout states for fields
        
        [MenuItem("Tools/State Creation Tool")]
        public static void ShowWindow()
        {
            GetWindow<StateCreationToolEditor>("State Creation Tool");
        }

        void OnGUI()
        {
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("State Creation Tool", EditorStyles.boldLabel);
            
            GUILayout.Space(15);

            switch (stepNumber)
            {
                case 1:
                {
                    ShowStep1();
                    break;
                }
                case 2:
                {
                    ShowStep2();
                    break;
                }
                case 3:
                {
                    ShowStep3();
                    break;
                }
                case 4:
                {
                    ShowStep4();
                    break;
                }
                case 5:
                {
                    ShowStep5();
                    break;
                }
            }
            
            GUILayout.Space(25);
            EditorGUILayout.LabelField("Step: " + stepNumber.ToString());
            GUILayout.Space(15);
        }

        private void ShowStep1()
        {
            LoadFolderNames();
            EditorGUILayout.LabelField("Hewwo~! Welcome to the State Creation tool! :3");
            GUILayout.Space(5);

            // Dropdown for folder selection
            EditorGUILayout.LabelField("Choose a State Folder:");
            if (folderNames != null && folderNames.Length > 0)
            {
                selectedFolderIndex = EditorGUILayout.Popup(selectedFolderIndex, folderNames);
                stateName = folderNames[selectedFolderIndex]; // Update stateName based on selection
            }
            else
            {
                EditorGUILayout.LabelField("No valid folders found.");
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Submit... UwU"))
            {
                stepNumber++;
                Debug.Log($"Selected State: {stateName}");
            }
        }

        private void LoadFolderNames()
        {
            string path = "Assets/Resources/States/";
            if (Directory.Exists(path))
            {
                // Get folders and filter out "Introduction"
                folderNames = Directory.GetDirectories(path)
                    .Select(folder => Path.GetFileName(folder)) // Get folder names only
                    .Where(name => name != "Introduction" || name != "Ending")     // Filter out the Introduction folder
                    .ToArray();
            }
            else
            {
                folderNames = new string[0]; // Empty array if directory doesn't exist
            }
        }
        
        private void ShowStep2()
        {
            EditorGUILayout.LabelField($"Oh wowie zowie! Making {stateName} are we OwO? Can you pwease give me the PDF?");
            GUILayout.Space(5);
            pdfFile = EditorGUILayout.ObjectField("PDF File", pdfFile, typeof(Object), false);
            if (GUILayout.Button("Submit... UwU"))
            {
                stepNumber++;
            }
        }
        
        private void ShowStep3()
        {
            EditorGUILayout.LabelField($"Okie dokie, time to get my reading paws on! (I hate that I'm doing this lol)");
            GUILayout.Space(5);

            // Button to read PDF fields
            if (GUILayout.Button("Lemme see what we got..."))
            {
                if (pdfFile != null)
                {
                    pdfFileName = pdfFile.name;
                    pdfFilePath = AssetDatabase.GetAssetPath(pdfFile);
                    if (File.Exists(pdfFilePath))
                    {
                        ReadPDFFillableFields(File.ReadAllBytes(pdfFilePath));
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

            // Scrollable area for displaying parsed fields
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(800));
            if (fieldsList.Count > 0)
            {
                foreach (var field in fieldsList)
                {
                    // Initialize foldout state for each field
                    if (!foldoutStates.ContainsKey(field.fieldName))
                    {
                        foldoutStates[field.fieldName] = false;
                    }

                    // Foldout for field details
                    foldoutStates[field.fieldName] = EditorGUILayout.Foldout(foldoutStates[field.fieldName], $"Field: {field.fieldName}", true);

                    if (foldoutStates[field.fieldName])
                    {
                        EditorGUI.indentLevel++;

                        // Display field details as read-only
                        EditorGUILayout.LabelField("Field Name:", field.fieldName);
                        EditorGUILayout.LabelField("Field Type:", field.fieldType ?? "Unknown");

                        // Display dropdown/radio options if available
                        if (field.options != null && field.options.Length > 0)
                        {
                            EditorGUILayout.LabelField("Options:");
                            foreach (var option in field.options)
                            {
                                EditorGUILayout.LabelField($"- {option}");
                            }
                        }

                        // Editable override key
                        field.overrideKey = EditorGUILayout.TextField("Override Key:", field.overrideKey);

                        EditorGUI.indentLevel--;
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("No fields have been loaded yet.");
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(5);

            // Move to the next step when ready
            if (readyForNextStep && GUILayout.Button("Submit... UwU"))
            {
                stepNumber++;
                readyForNextStep = false;
            }
        }
        
        private void ShowStep4()
        {
            EditorGUILayout.LabelField($"Awright, let's make you an asset that we can use to fill fields into!");
            GUILayout.Space(5);
            // Button to read PDF fields
            if (readyForNextStep)
            {
                if (GUILayout.Button("Submit... UwU"))
                {
                    // Create ScriptableObject
                    stepNumber++;
                    readyForNextStep = false;
                }
            }
            else
            {
                if (GUILayout.Button("Save Me!"))
                {
                    // Create ScriptableObject
                    CreatePDFFieldScriptableObject(fieldsList);
                }
            }
        }
        
        private void ShowStep5()
        {
            EditorGUILayout.LabelField($"Now, let's see how well we can make our nodes for Default-Chan!");
            GUILayout.Space(5);
            if (readyForNextStep)
            {
                if (GUILayout.Button("Submit... UwU"))
                {
                    // Create ScriptableObject
                    stepNumber++;
                    readyForNextStep = false;
                }
            }
            else
            {
                if (GUILayout.Button("Make Lines for Default-Chan"))
                {
                    AutoGenerateNodes();
                }
            }
        }

        private void ReadPDFFillableFields(byte[] pdfBytes)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(pdfBytes))
                {
                    PdfReader reader = new PdfReader(stream);
                    string result = "Form Fields:\n";
                    var acroFields = reader.AcroFields;

                    foreach (var field in acroFields.Fields)
                    {
                        string fieldName = field.Key;
                        string fieldValue = acroFields.GetField(fieldName);
                        string fieldType = GetFieldType(acroFields.GetFieldType(fieldName));

                        var fieldData = new PDFField
                        {
                            fieldName = fieldName,
                            fieldValue = fieldValue,
                            fieldType = fieldType,
                            options = null,
                            overrideKey = null // Default to null
                        };

                        result += $"- Name: {fieldName}\n  Value: {fieldValue}\n  Type: {fieldType}\n";

                        // Check for dropdown options (combobox)
                        if (acroFields.GetFieldType(fieldName) == AcroFields.FIELD_TYPE_COMBO)
                        {
                            var options = acroFields.GetListOptionExport(fieldName);
                            if (options != null && options.Length > 0)
                            {
                                fieldData.options = options;
                                result += "  Dropdown Options:\n";
                                foreach (var option in options)
                                {
                                    if (!string.IsNullOrWhiteSpace(option))
                                    {
                                        result += $"    - {option}\n";
                                    }
                                }
                            }
                        }

                        // Check for radio button options
                        if (acroFields.GetFieldType(fieldName) == AcroFields.FIELD_TYPE_RADIOBUTTON)
                        {
                            var appearances = acroFields.GetAppearanceStates(fieldName);
                            if (appearances != null && appearances.Length > 0)
                            {
                                fieldData.options = appearances;
                                result += "  Radio Button Options:\n";
                                foreach (var option in appearances)
                                {
                                    if (option != "Off")
                                    {
                                        result += $"    - {option}\n";
                                    }
                                }
                            }
                        }

                        // Assign an override keyword manually for certain fields (e.g., based on field name)
                        if (fieldName.Contains("Override", StringComparison.OrdinalIgnoreCase))
                        {
                            fieldData.overrideKey = "CustomOverride";
                            result += $"  Override Key: {fieldData.overrideKey}\n";
                        }

                        result += "\n";
                        fieldsList.Add(fieldData);
                    }

                    if (acroFields.Fields.Count == 0)
                    {
                        result += "No form fields found in the PDF.\n";
                    }

                    outputText = result;
                    readyForNextStep = true;
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
            scriptableObject.PdfFileName = pdfFileName;

            string path = EditorUtility.SaveFilePanelInProject("Save PDF Fields", $"{stateName}PDFFields", "asset", "Save the fields as a ScriptableObject asset.", $"Assets/Resources/States/{stateName}");

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(scriptableObject, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                readyForNextStep = true;
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
        
        private void AutoGenerateNodes()
        {
            DialogueGraph dialogueGraph = ScriptableObject.CreateInstance<DialogueGraph>();
            string path = EditorUtility.SaveFilePanelInProject("Save Dialogue", $"{stateName}Dialogue", "asset", "Save the dialogue as a ScriptableObject asset.", $"Assets/Resources/States/{stateName}");
            
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(dialogueGraph, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            var startNode = ScriptableObject.CreateInstance<StartNode>();
            startNode.name = "StartNode";
            startNode.graph = dialogueGraph;
            AssetDatabase.AddObjectToAsset(startNode, dialogueGraph);
            dialogueGraph.nodes.Add(startNode);

            foreach (var field in fieldsList)
            {
                switch (field.fieldType)
                {
                    case "Checkbox":
                    {
                        var node = ScriptableObject.CreateInstance<ChoiceNode>();
                        node.name = field.fieldName;
                        node.DialogueText = field.fieldName;
                        node.graph = dialogueGraph;
                        node.Options = field.options;
                        node.Keyword = field.overrideKey;
                        AssetDatabase.AddObjectToAsset(node, dialogueGraph);
                        dialogueGraph.nodes.Add(node);
                    }
                        break;
                    case "Dropdown":
                    {
                        var node = ScriptableObject.CreateInstance<DropdownNode>();
                        node.name = field.fieldName;
                        node.DialogueText = field.fieldName;
                        node.Options = field.options;
                        node.graph = dialogueGraph;
                        node.Keyword = field.overrideKey;
                        AssetDatabase.AddObjectToAsset(node, dialogueGraph);
                        dialogueGraph.nodes.Add(node);
                    }
                        break;
                    case "List":
                    {
                        var node = ScriptableObject.CreateInstance<ShowStatePickerNode>();
                        node.graph = dialogueGraph;
                        node.DialogueText = field.fieldName;
                        node.Keyword = field.overrideKey;
                        AssetDatabase.AddObjectToAsset(node, dialogueGraph);
                        dialogueGraph.nodes.Add(node);
                    }
                        break;
                    case "None":
                        break;
                    case "Button":
                    {
                        var node = ScriptableObject.CreateInstance<ChoiceNode>();
                        node.name = field.fieldName;
                        node.DialogueText = field.fieldName;
                        node.graph = dialogueGraph;
                        node.Options = field.options;
                        node.Keyword = field.overrideKey;
                        AssetDatabase.AddObjectToAsset(node, dialogueGraph);
                        dialogueGraph.nodes.Add(node);
                    }
                        break;
                    case "Radio Button":
                    {
                        var node = ScriptableObject.CreateInstance<ChoiceNode>();
                        node.name = field.fieldName;
                        node.DialogueText = field.fieldName;
                        node.graph = dialogueGraph;
                        node.Options = field.options;
                        node.Keyword = field.overrideKey;
                        AssetDatabase.AddObjectToAsset(node, dialogueGraph);
                        dialogueGraph.nodes.Add(node);
                    }
                        break;
                    case "Signature":
                    {
                        var node = ScriptableObject.CreateInstance<InputNode>();
                        node.name = field.fieldName;
                        node.DialogueText = field.fieldName;
                        node.graph = dialogueGraph;
                        node.Keyword = field.overrideKey;
                        AssetDatabase.AddObjectToAsset(node, dialogueGraph);
                        dialogueGraph.nodes.Add(node);
                    }
                        break;
                    case "Text Field":
                    {
                        var node = ScriptableObject.CreateInstance<InputNode>();
                        node.name = field.fieldName;
                        node.DialogueText = field.fieldName;
                        node.graph = dialogueGraph;
                        node.Keyword = field.overrideKey;
                        AssetDatabase.AddObjectToAsset(node, dialogueGraph);
                        dialogueGraph.nodes.Add(node);
                    }
                        break;
                    default:
                        break;
                }
            }
            
            var endNode = ScriptableObject.CreateInstance<EndNode>();
            endNode.name = "EndNode";
            endNode.graph = dialogueGraph;
            AssetDatabase.AddObjectToAsset(endNode, dialogueGraph);
            dialogueGraph.nodes.Add(endNode);
            
            Vector2 pos = Vector2.zero;
            
            // Daisy chain the nodes
            for (var i = 0; i < dialogueGraph.nodes.Count; i++)
            {
                var node = dialogueGraph.nodes[i];
                
                pos = new Vector2(pos.x + 500, pos.y);
                if (i % 4 == 0)
                {
                    pos = new Vector2(pos.x - 2000, pos.y + 600);
                }
                node.position = pos;

                if (node.GetType() != typeof(EndNode))
                {
                    try
                    {
                        var next = dialogueGraph.nodes[i + 1];
                        node.GetOutputPort("Output").Connect(next.GetInputPort("Input"));
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            readyForNextStep = true;
        }
    }
    
}
