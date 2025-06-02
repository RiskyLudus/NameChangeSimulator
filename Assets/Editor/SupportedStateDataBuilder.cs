#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NameChangeSimulator.Shared
{
    public class SupportedStateDataBuilder
    {
        [MenuItem("Tools/Update Supported State Data")]
        public static void UpdateSupportedStateData()
        {
            // Path to the Resources/States directory
            string statesFolderPath = Path.Combine(Application.dataPath, "Resources", "States");

            if (!Directory.Exists(statesFolderPath))
            {
                Debug.LogError("Resources/States directory does not exist.");
                return;
            }

            // Get all subdirectories in Resources/States
            string[] subdirectories = Directory.GetDirectories(statesFolderPath);

            // Load or create the ScriptableObject
            string assetPath = "Assets/Resources/States/SupportedStateData.asset";
            SupportedStateData supportedStateData = AssetDatabase.LoadAssetAtPath<SupportedStateData>(assetPath);

            if (supportedStateData == null)
            {
                supportedStateData = ScriptableObject.CreateInstance<SupportedStateData>();
                AssetDatabase.CreateAsset(supportedStateData, assetPath);
            }

            // Clear existing lists
            supportedStateData.supportedStates.Clear();
            supportedStateData.nonSupportedStates.Clear();

            // Populate lists based on folder content
            foreach (string subdirectory in subdirectories)
            {
                string folderName = Path.GetFileName(subdirectory);

                // Check if the folder contains any files
                bool hasContent = Directory.GetFiles(subdirectory).Length > 0;

                if (hasContent && folderName != "Introduction" && folderName != "Ending")
                {
                    supportedStateData.supportedStates.Add(folderName);
                }
                else
                {
                    supportedStateData.nonSupportedStates.Add(folderName);
                }
            }

            // Save changes to the ScriptableObject
            EditorUtility.SetDirty(supportedStateData);
            AssetDatabase.SaveAssets();

            Debug.Log("SupportedStateData updated successfully!");
        }
    }
}
#endif