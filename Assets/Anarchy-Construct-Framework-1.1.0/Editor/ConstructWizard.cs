using UnityEngine;
using UnityEditor;
using System.IO;

namespace AnarchyConstructFramework.Editor
{
    public class ConstructWizard : EditorWindow
    {
        private string constructName;

        // Define a menu item to invoke this window
        [MenuItem("Anarchy/Create Construct")]
        static void Init()
        {
            // Create an instance of ConstructWizard and show it
            ConstructWizard window = (ConstructWizard)EditorWindow.GetWindow(typeof(ConstructWizard));
            window.Show();
        }

        // GUI content of the window
        void OnGUI()
        {
            GUILayout.Label("Create Construct", EditorStyles.boldLabel);

            GUILayout.Space(10);

            GUILayout.Label("Enter Construct Name:");
            constructName = EditorGUILayout.TextField(constructName);

            GUILayout.Space(10);

            // Button to create the construct
            if (GUILayout.Button("Create"))
            {
                // Check if the construct name is not empty
                if (!string.IsNullOrEmpty(constructName))
                {
                    // Create the construct folder structure
                    CreateConstructFolderStructure();
                    // Close the window
                    Close();
                }
                else
                {
                    Debug.LogError("Please enter a valid construct name.");
                }
            }
        }

        // Method to create the folder structure for the construct
        private void CreateConstructFolderStructure()
        {
            var settings = AnarchyConstructFrameworkEditorFunctions.GetSettings();

            // Construct the path using AnarchySettings properties
            string constructFolderPath = Path.Combine(settings.PathToConstructs, constructName);

            // Create the main construct folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder(constructFolderPath))
            {
                try
                {
                    AssetDatabase.CreateFolder(settings.PathToConstructs, constructName);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error creating folder: " + ex.Message);
                }
            }

            // Refresh the AssetDatabase to reflect changes
            AssetDatabase.Refresh();

            // Create subfolders
            CreateSubfolder(constructFolderPath, "Code");
            CreateSubfolder(constructFolderPath, "Models");
            CreateSubfolder(constructFolderPath, "Materials");
            CreateSubfolder(constructFolderPath, "Shaders");
            CreateSubfolder(constructFolderPath, "Prefabs");
            CreateSubfolder(constructFolderPath, "Textures");
            CreateSubfolder(constructFolderPath, "Animations");
            CreateSubfolder(constructFolderPath, "Settings");
            CreateSubfolder(constructFolderPath, "Scenes");
            CreateSubfolder(constructFolderPath, "Tests");

            // Create Assembly Definition file
            CreateAssemblyDefinitionFile(constructFolderPath, settings.RootNamespace);

            // Refresh the AssetDatabase to reflect changes
            AssetDatabase.Refresh();
        }

        // Method to create a subfolder inside a parent folder
        private void CreateSubfolder(string parentFolder, string folderName)
        {
            string subfolderPath = Path.Combine(parentFolder, folderName);
            if (!AssetDatabase.IsValidFolder(subfolderPath))
            {
                try
                {
                    AssetDatabase.CreateFolder(parentFolder, folderName);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error creating folder: " + ex.Message);
                }
            }
        }

        // Method to create the Assembly Definition file for the construct
        private void CreateAssemblyDefinitionFile(string folderPath, string rootNamespace)
        {
            string asmdefFileName = $"{constructName}.asmdef";
            string asmdefFilePath = Path.Combine(folderPath, asmdefFileName);

            // Check if the Assembly Definition file doesn't already exist
            if (!File.Exists(asmdefFilePath))
            {
                // Create the Assembly Definition file and write its content
                using (StreamWriter writer = new StreamWriter(asmdefFilePath))
                {
                    writer.WriteLine("{");
                    writer.WriteLine("  \"name\": \"" + constructName + "\",");
                    writer.WriteLine("  \"rootNamespace\": \""+ rootNamespace + ".Constructs." + constructName + "\",");
                    writer.WriteLine("  \"references\": [],");
                    writer.WriteLine("  \"includePlatforms\": [],");
                    writer.WriteLine("  \"excludePlatforms\": [],");
                    writer.WriteLine("  \"allowUnsafeCode\": false,");
                    writer.WriteLine("  \"overrideReferences\": false,");
                    writer.WriteLine("  \"precompiledReferences\": [],");
                    writer.WriteLine("  \"autoReferenced\": true,");
                    writer.WriteLine("  \"defineConstraints\": [],");
                    writer.WriteLine("  \"versionDefines\": []");
                    writer.WriteLine("}");
                }
            }
        }
    }
}
