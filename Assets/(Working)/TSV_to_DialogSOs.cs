using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

using UnityEngine;

public static class DialogSOGenerator {
	[MenuItem("Assets/Generate DialogSOs from TSV")]
	private static void GenerateFromTSV() {
		// Get selected file
		var obj = Selection.activeObject;
		var path = AssetDatabase.GetAssetPath(obj);

		if (!path.EndsWith(".tsv")) {
			Debug.LogError("Please select a .tsv file.");
			return;
		}

		// Read all lines
		var lines = File.ReadAllLines(path);

		if (lines.Length < 2) {
			Debug.LogError("TSV has no data.");
			return;
		}

		// Split header
		var headers = lines[0].Split('\t');
		int colIndex = System.Array.IndexOf(headers, "Suggested Field Name");

		if (colIndex == -1) {
			Debug.LogError("Could not find 'Suggested Field Name' column.");
			return;
		}

		// Collect unique values
		HashSet<string> uniqueNames = new HashSet<string>();
		foreach (var line in lines.Skip(1)) {
			var cols = line.Split('\t');
			if (cols.Length > colIndex) {
				var val = cols[colIndex].Trim();
				if (!string.IsNullOrEmpty(val))
					uniqueNames.Add(val);
			}
		}

		// Ensure folder exists
		string folder = "Assets/Dialog";
		if (!AssetDatabase.IsValidFolder(folder)) {
			AssetDatabase.CreateFolder("Assets", "Dialog");
		}

		// Create ScriptableObjects
		foreach (var name in uniqueNames) {
			string safeName = MakeSafeFileName(name);
			string assetPath = $"{folder}/{safeName}.asset";

			if (AssetDatabase.LoadAssetAtPath<DialogSO>(assetPath) != null)
				continue; // Skip if already exists

			var dialog = ScriptableObject.CreateInstance<DialogSO>();
			dialog.Keyword = name;
			dialog.Dialog = "";

			AssetDatabase.CreateAsset(dialog, assetPath);
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log($"Generated {uniqueNames.Count} DialogSOs in {folder}");
	}

	private static string MakeSafeFileName(string input) {
		var invalidChars = Path.GetInvalidFileNameChars();
		foreach (var c in invalidChars)
			input = input.Replace(c.ToString(), "_");
		return input;
	}

	[MenuItem("Assets/Generate DialogSOs from TSV", true)]
	private static bool ValidateGenerateFromTSV() {
		var obj = Selection.activeObject;
		if (obj == null)
			return false;
		var path = AssetDatabase.GetAssetPath(obj);
		return path.EndsWith(".tsv");
	}
}
