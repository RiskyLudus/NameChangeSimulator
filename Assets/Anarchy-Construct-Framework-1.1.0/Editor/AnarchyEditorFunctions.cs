using System.Collections;
using System.Collections.Generic;
using AnarchyConstructFramework.Core.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace AnarchyConstructFramework.Editor
{
    public static class AnarchyConstructFrameworkEditorFunctions
    {
        public static AnarchySettings GetSettings()
        {
            // Find AnarchySettings asset in the project
            string[] guids = AssetDatabase.FindAssets("t:AnarchySettings"); // Search for assets of type AnarchySettings
            string settingsPath = null;

            // Check if AnarchySettings is found
            if (guids.Length > 0)
            {
                settingsPath = AssetDatabase.GUIDToAssetPath(guids[0]); // Get the path of the first found instance
            }
            else
            {
                Debug.LogError("AnarchySettings not found. Please create AnarchySettings asset.");
                return null;
            }

            // Access AnarchySettings asset
            AnarchySettings settings = AssetDatabase.LoadAssetAtPath<AnarchySettings>(settingsPath);

            // Check if AnarchySettings asset is loaded successfully
            if (settings == null)
            {
                Debug.LogError("AnarchySettings asset found, but failed to load.");
                return null;
            }

            return settings;
        }
    }
}
