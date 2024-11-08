using System.Linq;
using AnarchyConstructFramework.Core.ScriptableObjects;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace AnarchyConstructFramework.Editor
{
    public class AddressablesConstructBuilder
    {
        [MenuItem("Anarchy/Build Constructs Addressables Group")]
        static void BuildConstructsGroup()
        {
            AnarchySettings settings = AnarchyConstructFrameworkEditorFunctions.GetSettings();

            // Create a new Addressables group named "Constructs"
            CreateAddressablesGroup("Constructs");

            // Get all assets within the construct folder
            string[] constructAssets = AssetDatabase.FindAssets("", new[] { settings.PathToConstructs });

            // Filter out empty folders
            constructAssets = FilterEmptyFolders(constructAssets);

            // Add each asset to the "Constructs" group
            foreach (var assetGUID in constructAssets)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                AddAssetToAddressablesGroup("Constructs", assetPath);
            }

            // Build Addressables to apply the changes
            BuildAddressables();
        }

        static void CreateAddressablesGroup(string groupName)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);

            // Check if the group already exists
            AddressableAssetGroup group = settings.FindGroup(groupName);
            if (group == null)
            {
                // Create a new group
                group = settings.CreateGroup(groupName, false, false, false, null, typeof(BundledAssetGroupSchema));
            }
        }

        static void AddAssetToAddressablesGroup(string groupName, string assetPath)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);

            // Get the group by name
            AddressableAssetGroup group = settings.FindGroup(groupName);
            if (group != null)
            {
                settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), group);
            }
        }

        static void BuildAddressables()
        {
            AddressableAssetSettings.BuildPlayerContent();
        }

        static string[] FilterEmptyFolders(string[] assetGuids)
        {
            // Filter out folders that do not contain any assets
            return assetGuids.Where(guid => !AssetDatabase.IsValidFolder(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
        }
    }
}
