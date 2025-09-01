#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;

using XNode;

public class DialogSO_BulkFromDialogueGraphWindow : EditorWindow {
	[Serializable]
	private class ResultLine { public string node; public string action; public string path; }

	private NodeGraph _graph;
	private DefaultAsset _outputFolder;
	private bool _splitAmpersandKeywords = true;
	private bool _sanitizeFilenames = true;
	private bool _dryRun = false;
	private Vector2 _logScroll;
	private readonly List<ResultLine> _log = new();

	[MenuItem("Tools/NCS/Generate DialogSOs From DialogueGraph…")]
	private static void Open() {
		var w = GetWindow<DialogSO_BulkFromDialogueGraphWindow>("DialogSO Generator");
		w.minSize = new Vector2(520, 420);
		w.Show();
	}

	private void OnGUI() {
		EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);
		_graph = (NodeGraph)EditorGUILayout.ObjectField("DialogueGraph", _graph, typeof(NodeGraph), false);

		EditorGUILayout.Space(6);
		EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
		_outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Folder (Project)", _outputFolder, typeof(DefaultAsset), false);

		using (new EditorGUILayout.HorizontalScope()) {
			if (GUILayout.Button("Use Assets/Resources/Dialogs", GUILayout.Height(22))) {
				EnsureDefaultFolder("Assets/Resources/Dialogs");
			}
			if (GUILayout.Button("Pick Folder…", GUILayout.Height(22))) {
				PickFolderWithOSDialog();
			}
			if (GUILayout.Button("Reveal Folder", GUILayout.Height(22))) {
				var p = GetFolderPath();
				if (Directory.Exists(p))
					EditorUtility.RevealInFinder(p);
			}
		}

		var relOut = _outputFolder ? AssetDatabase.GetAssetPath(_outputFolder) : "Assets/Resources/Dialogs";
		EditorGUILayout.LabelField("Selected:", relOut);

		EditorGUILayout.Space(6);
		EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
		_splitAmpersandKeywords = EditorGUILayout.ToggleLeft("Split keywords on '&'", _splitAmpersandKeywords);
		_sanitizeFilenames = EditorGUILayout.ToggleLeft("Sanitize asset file names", _sanitizeFilenames);
		_dryRun = EditorGUILayout.ToggleLeft("Dry run (don’t write files)", _dryRun);

		EditorGUILayout.Space(8);
		using (new EditorGUI.DisabledScope(_graph == null)) {
			if (GUILayout.Button(_dryRun ? "Preview" : "Generate / Update", GUILayout.Height(28))) {
				Run();
			}
		}

		EditorGUILayout.Space(8);
		EditorGUILayout.LabelField("Log", EditorStyles.boldLabel);
		using (var sv = new EditorGUILayout.ScrollViewScope(_logScroll, GUILayout.ExpandHeight(true))) {
			_logScroll = sv.scrollPosition;
			foreach (var line in _log) {
				GUILayout.Label($"[{line.action}] {line.node}  =>  {line.path}");
			}
		}
	}

	private void Run() {
		_log.Clear();

		if (_graph == null) {
			Log("—", "error", "No DialogueGraph selected.");
			return;
		}

		string folder = GetFolderPath();
		if (!folder.StartsWith(Application.dataPath.Replace('\\', '/'))) {
			Log("—", "error", "Folder must be inside Assets/. Choose Assets/Resources/Dialogs for runtime loading.");
			return;
		}

		if (!_dryRun)
			Directory.CreateDirectory(folder);

		int created = 0, updated = 0, skipped = 0;

		foreach (var node in _graph.nodes) {
			if (node == null) { skipped++; Log("(null)", "skip", "Node is null"); continue; }

			string dialogText = GetStringField(node, "DialogueText");
			if (string.IsNullOrWhiteSpace(dialogText)) {
				skipped++;
				Log(node.name, "skip", "No DialogueText");
				continue;
			}

			string keyword = GetStringField(node, "Keyword");
			if (string.IsNullOrWhiteSpace(keyword))
				keyword = node.name;

			var keywords = _splitAmpersandKeywords && keyword.Contains("&")
				? keyword.Split('&').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray()
				: new[] { keyword };

			foreach (var kw in keywords) {
				string safeName = ToSafeFileName(_sanitizeFilenames ? kw : kw.Replace(' ', '_'));
				string assetRel = ToProjectRelativePath(Path.Combine(folder, safeName + ".asset"));

				var existing = AssetDatabase.LoadAssetAtPath<DialogSO>(assetRel);
				if (existing == null) {
					if (_dryRun) {
						Log(node.name, "create*", assetRel);
						created++;
						continue;
					}
					var so = ScriptableObject.CreateInstance<DialogSO>();
					so.Keyword = kw;
					so.Dialog = dialogText;
					AssetDatabase.CreateAsset(so, assetRel);
					AssetDatabase.ImportAsset(assetRel);
					Log(node.name, "create", assetRel);
					created++;
				} else {
					if (_dryRun) {
						Log(node.name, "update*", assetRel);
						updated++;
						continue;
					}
					Undo.RecordObject(existing, "Update DialogSO");
					existing.Keyword = kw;
					existing.Dialog = dialogText;
					EditorUtility.SetDirty(existing);
					Log(node.name, "update", assetRel);
					updated++;
				}
			}
		}

		if (!_dryRun) {
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		Debug.Log($"[DialogSO Generator] Created: {created}, Updated: {updated}, Skipped: {skipped}.");
		_log.Add(new ResultLine { node = "—", action = "summary", path = $"Created: {created}, Updated: {updated}, Skipped: {skipped}" });
	}

	private static string GetStringField(Node node, string fieldName) {
		if (node == null)
			return null;

		var f = node.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if (f != null && f.FieldType == typeof(string))
			return (string)f.GetValue(node);

		var p = node.GetType().GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if (p != null && p.PropertyType == typeof(string) && p.CanRead)
			return (string)p.GetValue(node);

		return null;
	}

	private static string ToSafeFileName(string name) {
		if (string.IsNullOrWhiteSpace(name))
			return "Dialog";

		var invalid = Path.GetInvalidFileNameChars();
		var cleaned = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
		cleaned = cleaned.Replace(' ', '_');

		while (cleaned.Contains("__"))
			cleaned = cleaned.Replace("__", "_");
		return cleaned.Trim('_');
	}

	private void EnsureDefaultFolder(string rel) {
		if (!rel.StartsWith("Assets"))
			rel = "Assets/Resources/Dialogs";
		var parts = rel.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
		string acc = parts[0];
		for (int i = 1; i < parts.Length; i++) {
			string next = acc + "/" + parts[i];
			if (!AssetDatabase.IsValidFolder(next)) {
				AssetDatabase.CreateFolder(acc, parts[i]);
			}
			acc = next;
		}
		_outputFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(acc);
	}

	private string GetFolderPath() {
		if (_outputFolder == null)
			return Path.Combine(Application.dataPath, "Resources/Dialogs").Replace('\\', '/');
		var rel = AssetDatabase.GetAssetPath(_outputFolder);
		if (string.IsNullOrEmpty(rel))
			return Path.Combine(Application.dataPath, "Resources/Dialogs").Replace('\\', '/');
		return ToAbsoluteProjectPath(rel);
	}

	private static string ToAbsoluteProjectPath(string rel) {
		rel = rel.Replace('\\', '/').TrimStart('/');
		string proj = Application.dataPath.Replace("Assets", "");
		return Path.Combine(proj, rel).Replace('\\', '/');
	}

	private static string ToProjectRelativePath(string abs) {
		abs = abs.Replace('\\', '/');
		string proj = Application.dataPath.Replace("Assets", "");
		if (!abs.StartsWith(proj))
			return string.Empty;
		return abs.Substring(proj.Length);
	}

	private void Log(string node, string action, string messageOrPath) {
		_log.Add(new ResultLine { node = node, action = action, path = messageOrPath });
		Debug.Log($"[DialogSO Generator] [{action}] {node} -> {messageOrPath}");
	}

	private void PickFolderWithOSDialog() {
		var start = _outputFolder ? AssetDatabase.GetAssetPath(_outputFolder) : "Assets/Resources/Dialogs";
		if (string.IsNullOrEmpty(start))
			start = "Assets/Resources/Dialogs";
		var startAbs = ToAbsoluteProjectPath(start);
		var selectedAbs = EditorUtility.OpenFolderPanel("Choose output folder (must be inside Assets)", startAbs, "");
		if (string.IsNullOrEmpty(selectedAbs))
			return;
		selectedAbs = selectedAbs.Replace('\\', '/');
		var rel = ToProjectRelativePath(selectedAbs);
		if (string.IsNullOrEmpty(rel) || !rel.StartsWith("Assets/")) {
			Log("—", "error", "Selected path must be inside the project Assets folder.");
			return;
		}
		_outputFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(rel);
	}
}
#endif
