using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class RecordFieldsEditor : EditorWindow
{
    private GameObject prefab; // To hold the user-provided prefab
    private Sprite form;
    private string assetName = "StateData"; // Default asset name

    [MenuItem("Tools/Record Fields Generator")]
    public static void ShowWindow()
    {
        GetWindow<RecordFieldsEditor>("Record Fields Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Record Fields Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // Field to assign the form image
        form = (Sprite)EditorGUILayout.ObjectField(form, typeof(Sprite), false);

        // Field to assign the prefab
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

        // Field to customize the asset name
        assetName = EditorGUILayout.TextField("Asset Name", assetName);

        // Generate button
        if (GUILayout.Button("Generate StateData"))
        {
            if (prefab == null || form == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign all fields.", "OK");
            }
            else
            {
                GenerateStateData(prefab, assetName);
            }
        }
    }

    private void GenerateStateData(GameObject prefab, string assetName)
    {
        // Ensure the provided object is a prefab
        if (!PrefabUtility.IsPartOfPrefabAsset(prefab))
        {
            EditorUtility.DisplayDialog("Error", "The provided GameObject is not a prefab.", "OK");
            return;
        }

        // Create a new list to store fields
        var fields = new List<Field>();

        // Instantiate a temporary copy of the prefab to inspect its children
        GameObject tempInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        foreach (Transform child in tempInstance.transform)
        {
            Field newField = new Field
            {
                Name = child.name,
                IsText = child.TryGetComponent(out TMP_Text _),
                IsCheck = child.TryGetComponent(out Image _)
            };

            fields.Add(newField);
        }

        // Destroy the temporary instance
        DestroyImmediate(tempInstance);

        // Create and populate the ScriptableObject
        var instance = ScriptableObject.CreateInstance<StateData>();
        instance.name = prefab.name;
        instance.formSprite = form;
        instance.formFieldObject = prefab;
        instance.fields = fields.ToArray();

        // Save the asset
        string assetPath = $"Assets/{assetName}.asset";
        AssetDatabase.CreateAsset(instance, assetPath);
        AssetDatabase.SaveAssets();

        // Notify the user
        EditorUtility.DisplayDialog("Success", $"StateData asset created at:\n{assetPath}", "OK");
        Debug.Log($"StateData asset saved at: {assetPath}");
    }
}
