
/*
#if UNITY_EDITOR
namespace NameChangeSimulator.Editor {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using UnityEditor;
	using UnityEngine;
	using XNode;
	using DW.Tools;


	public class StateCreationToolEditor : EditorWindow {
		private string stateName;
		private ImageFieldingAsset selectedLayout;

		private readonly List<ImageFieldingAsset> resolvedChain = new List<ImageFieldingAsset>();
		private readonly List<Field> fieldsList = new List<Field>();
		private readonly HashSet<string> fieldNamesSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private Vector2 scrollPosition;
		private int stepNumber = 1;
		private string[] folderNames;
		private int selectedFolderIndex = 0;

		private bool step3Scanned = false;

		// Config per row
		private readonly Dictionary<string, FieldConfig> fieldConfigs = new Dictionary<string, FieldConfig>(StringComparer.OrdinalIgnoreCase);
		private readonly List<string> orderedKeys = new List<string>();
		private int newRowCounter = 0;

		private enum PortSel { Input, OverrideInput }

		private static readonly string[] NodeTypeOptions = new[] {
			"DialogueNode",
			"DeadNameInputNode",
			"NewNameInputNode",
			"ShowStatePickerNode",
			"LoadStateGraphNode",
			"InputNode",
			"DropdownNode",
			"ChoiceNode",
			"EndNode",
			"StartNode",
			"QuitNode"
		};

		private class FieldConfig {
			// Row 1
			public string NodeName;
			public PortSel NextRowPort = PortSel.Input;
			public string InputOverride;

			// Row 2
			public string Dialog;
			public string Keyword;
			public int TypeIndex;

			// Row 2.5 (DialogueNode only)
			public int PoseIndex;
			public int VoiceIndex;

			// Row 3 (options-only)
			public string Opt1Label;
			public PortSel Opt1TargetPort = PortSel.Input;
			public string Opt1TargetName;

			public string Opt2Label;
			public PortSel Opt2TargetPort = PortSel.Input;
			public string Opt2TargetName;
		}

		private const float kViewHeight = 520f;
		private float _avgRowH = 64f;

		// Cached dropdown choices
		private string[] _nodeNameChoices = Array.Empty<string>();
		private bool _nodeNameChoicesDirty = true;

		private string[] _poseChoices = Array.Empty<string>();
		private string[] _voiceChoices = Array.Empty<string>();

		// Width caching
		private float _lastW = -1f;
		private struct Widths {
			public float controlsW, nameW, portLabelW, connectW, overrideW;
			public float dialogW, keywordW, typeLabelW, typeW;
			public float poseLabelW, poseW, voiceLabelW, voiceW;
			public float optLabelW, optPortW, optTargetW;
		}
		private Widths _w;

		// Reused GUI artifacts
		private static readonly GUIContent GOutputArrow = new GUIContent("output ->");
		private static readonly GUIContent GTypeEquals = new GUIContent("type=");
		private static readonly GUIContent GPoseEquals = new GUIContent("pose=");
		private static readonly GUIContent GVoiceEquals = new GUIContent("voice=");

		private static GUIStyle _box;
		private static GUIStyle Box => _box ??= new GUIStyle("box") {
			margin = new RectOffset(4, 4, 2, 2),
			padding = new RectOffset(6, 6, 6, 6)
		};

		private enum PendingOp { None, InsertAfter, RemoveAt, MoveUp, MoveDown }
		private PendingOp _pendingOp = PendingOp.None;
		private int _pendingIndex = -1;

		[MenuItem("Tools/State Creation Tool")]
		public static void ShowWindow() => GetWindow<StateCreationToolEditor>("State Creation Tool");

		private void OnGUI() {
			GUILayout.Space(10);
			EditorGUILayout.LabelField("State Creation Tool", EditorStyles.boldLabel);
			GUILayout.Space(15);

			switch (stepNumber) {
			case 1:
				ShowStep1();
				break;
			case 2:
				ShowStep2();
				break;
			case 3:
				ShowStep3();
				break;
			case 4:
				ShowStep4();
				break;
			}

			GUILayout.Space(25);
			EditorGUILayout.LabelField("Step: " + stepNumber);
			GUILayout.Space(15);
		}

		private void ShowStep1() {
			LoadFolderNames();
			EditorGUILayout.LabelField("Choose a State Folder:");
			if (folderNames != null && folderNames.Length > 0) {
				selectedFolderIndex = EditorGUILayout.Popup(selectedFolderIndex, folderNames);
				stateName = folderNames[selectedFolderIndex];
			} else {
				EditorGUILayout.LabelField("No valid folders found in Assets/Resources/States/");
			}

			GUILayout.Space(10);
			if (GUILayout.Button("Next")) {
				if (string.IsNullOrEmpty(stateName)) {
					EditorUtility.DisplayDialog("State", "Please pick a state folder.", "OK");
					return;
				}
				stepNumber = 2;
			}
		}

		private void ShowStep2() {
			EditorGUILayout.LabelField($"Select a LayoutAsset for {stateName}:");
			selectedLayout = (ImageFieldingAsset)EditorGUILayout.ObjectField("Layout Asset", selectedLayout, typeof(ImageFieldingAsset), false);

			GUILayout.Space(6);
			using (new EditorGUI.DisabledScope(selectedLayout == null)) {
				if (GUILayout.Button("Next")) {
					stepNumber = 3;
					step3Scanned = false;

					fieldsList.Clear();
					fieldNamesSeen.Clear();
					resolvedChain.Clear();

					fieldConfigs.Clear();
					orderedKeys.Clear();
					newRowCounter = 0;

					_nodeNameChoicesDirty = true;
					_poseChoices = Array.Empty<string>();
					_voiceChoices = Array.Empty<string>();
				}
			}
		}

		private void ApplyPendingOpIfAny() {
			if (_pendingOp == PendingOp.None)
				return;

			switch (_pendingOp) {
			case PendingOp.InsertAfter:
				InsertRowAfter(Mathf.Clamp(_pendingIndex, -1, orderedKeys.Count - 1));
				break;
			case PendingOp.RemoveAt:
				if (orderedKeys.Count > 0)
					RemoveRowAt(Mathf.Clamp(_pendingIndex, 0, orderedKeys.Count - 1));
				break;
			case PendingOp.MoveUp:
				MoveRow(Mathf.Clamp(_pendingIndex, 0, orderedKeys.Count - 1), -1);
				break;
			case PendingOp.MoveDown:
				MoveRow(Mathf.Clamp(_pendingIndex, 0, orderedKeys.Count - 1), +1);
				break;
			}

			_pendingOp = PendingOp.None;
			_pendingIndex = -1;
			_nodeNameChoicesDirty = true;
			Repaint();
		}

		private void ShowStep3() {
			// Apply any deferred structural changes from the last click before drawing
			ApplyPendingOpIfAny();

			if (!step3Scanned) {
				CollectFieldsFromLayoutChainOnce();
				InitializeFieldRows();
				_nodeNameChoicesDirty = true;

				if (_poseChoices.Length == 0)
					_poseChoices = GetEnumChoices("CharacterSpriteType", out _);
				if (_voiceChoices.Length == 0)
					_voiceChoices = GetEnumChoices("VoiceLineType", out _);

				step3Scanned = true;
			}

			EditorGUILayout.HelpBox("InputOveride = skip on back button", MessageType.Info);
			EnsureWidths();

			// Cached dropdowns for this repaint
			var nodeNameChoices = GetNodeNameChoicesCached();
			var poseChoices = _poseChoices;
			var voiceChoices = _voiceChoices;

			// Take a stable snapshot for this GUI frame to avoid index drift while drawing
			var keySnapshot = orderedKeys.ToArray();
			int count = keySnapshot.Length;

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(kViewHeight));

			// Approximate visible slice
			float approxRow = Mathf.Max(24f, _avgRowH);
			int approxPerScreen = Mathf.Max(1, Mathf.CeilToInt(kViewHeight / approxRow));
			int start = Mathf.Clamp(Mathf.FloorToInt(scrollPosition.y / approxRow) - 3, 0, Mathf.Max(0, count - 1));
			int end = Mathf.Min(count, start + approxPerScreen + 6);

			// top spacer
			float topPad = 0f;
			for (int i = 0; i < start; i++) {
				if (!fieldConfigs.TryGetValue(keySnapshot[i], out var cfg0))
					continue;
				topPad += ApproxRowHeight(cfg0);
			}
			GUILayout.Space(topPad);

			float drawnH = 0f;

			for (int i = start; i < end; i++) {
				var key = keySnapshot[i];
				if (!fieldConfigs.TryGetValue(key, out var cfg))
					continue;

				bool isOptions = IsOptionsType(cfg.TypeIndex);
				bool isDialogue = IsDialogueType(cfg.TypeIndex);

				EditorGUILayout.BeginVertical(Box);

				// Row 1: + - ^ v | NodeName | "output ->" | port | InputOverride
				using (new EditorGUILayout.HorizontalScope()) {
					// controls
					using (new EditorGUILayout.HorizontalScope(GUILayout.Width(_w.controlsW))) {
						if (GUILayout.Button("+", GUILayout.Width(_w.controlsW * 0.25f - 2))) {
							GUI.FocusControl(null);
							_pendingOp = PendingOp.InsertAfter;
							_pendingIndex = i;
							GUIUtility.ExitGUI();
						}
						if (GUILayout.Button("-", GUILayout.Width(_w.controlsW * 0.25f - 2))) {
							GUI.FocusControl(null);
							_pendingOp = PendingOp.RemoveAt;
							_pendingIndex = i;
							GUIUtility.ExitGUI();
						}
						using (new EditorGUI.DisabledScope(i <= 0)) {
							if (GUILayout.Button("^", GUILayout.Width(_w.controlsW * 0.25f - 2))) {
								GUI.FocusControl(null);
								_pendingOp = PendingOp.MoveUp;
								_pendingIndex = i;
								GUIUtility.ExitGUI();
							}
						}
						using (new EditorGUI.DisabledScope(i >= count - 1)) {
							if (GUILayout.Button("v", GUILayout.Width(_w.controlsW * 0.25f - 2))) {
								GUI.FocusControl(null);
								_pendingOp = PendingOp.MoveDown;
								_pendingIndex = i;
								GUIUtility.ExitGUI();
							}
						}
					}

					// Node Name (marks name list cache dirty if changed)
					string oldName = cfg.NodeName;
					cfg.NodeName = PlaceholderTextField(cfg.NodeName ?? key, "Node Name", _w.nameW);
					if (!string.Equals(oldName, cfg.NodeName, StringComparison.Ordinal))
						_nodeNameChoicesDirty = true;

					// "output ->" + port dropdown
					GUILayout.Label(GOutputArrow, GUILayout.Width(_w.portLabelW));
					cfg.NextRowPort = PortSelPopup(cfg.NextRowPort, _w.connectW, false);

					// InputOverride (dropdown of node names; first is "(none)")
					int ovIdx = IndexOfChoice(nodeNameChoices, cfg.InputOverride);
					ovIdx = EditorGUILayout.Popup(ovIdx, nodeNameChoices, GUILayout.Width(_w.overrideW));
					cfg.InputOverride = (ovIdx <= 0) ? string.Empty : nodeNameChoices[ovIdx];
				}

				// Row 2: Dialog | Keyword | "type=" | node type
				using (new EditorGUILayout.HorizontalScope()) {
					// Heaviest field uses Delayed to avoid churn per keystroke
					cfg.Dialog = DelayedTextWithPlaceholder(cfg.Dialog, "Dialog", _w.dialogW);
					cfg.Keyword = PlaceholderTextField(cfg.Keyword, "Keyword", _w.keywordW);
					GUILayout.Label(GTypeEquals, GUILayout.Width(_w.typeLabelW));
					cfg.TypeIndex = EditorGUILayout.Popup(cfg.TypeIndex, NodeTypeOptions, GUILayout.Width(_w.typeW));
				}

				// Row 2.5: Dialogue-only (pose + voice)
				if (isDialogue) {
					using (new EditorGUILayout.HorizontalScope()) {
						// pose=
						GUILayout.Label(GPoseEquals, GUILayout.Width(_w.poseLabelW));
						cfg.PoseIndex = EditorGUILayout.Popup(
							Mathf.Clamp(cfg.PoseIndex, 0, Mathf.Max(0, poseChoices.Length - 1)),
							poseChoices.Length > 0 ? poseChoices : new[] { "(no CharacterSpriteType enum found)" },
							GUILayout.Width(_w.poseW)
						);

						// voice=
						GUILayout.Label(GVoiceEquals, GUILayout.Width(_w.voiceLabelW));
						cfg.VoiceIndex = EditorGUILayout.Popup(
							Mathf.Clamp(cfg.VoiceIndex, 0, Mathf.Max(0, voiceChoices.Length - 1)),
							voiceChoices.Length > 0 ? voiceChoices : new[] { "(no VoiceLineType enum found)" },
							GUILayout.Width(_w.voiceW)
						);
					}
				}

				// Row 3 (only for Choice/Dropdown): Option 1 & 2
				if (isOptions) {
					using (new EditorGUILayout.HorizontalScope()) {
						// Option 1: label | target dropdown | port dropdown
						cfg.Opt1Label = PlaceholderTextField(cfg.Opt1Label, "Option 1 label", _w.optLabelW);

						int t1 = IndexOfChoice(nodeNameChoices, cfg.Opt1TargetName);
						t1 = EditorGUILayout.Popup(t1, nodeNameChoices, GUILayout.Width(_w.optTargetW));
						cfg.Opt1TargetName = (t1 <= 0) ? string.Empty : nodeNameChoices[t1];

						cfg.Opt1TargetPort = PortSelPopup(cfg.Opt1TargetPort, _w.optPortW, true);
					}
					using (new EditorGUILayout.HorizontalScope()) {
						// Option 2: label | target dropdown | port dropdown
						cfg.Opt2Label = PlaceholderTextField(cfg.Opt2Label, "Option 2 label", _w.optLabelW);

						int t2 = IndexOfChoice(nodeNameChoices, cfg.Opt2TargetName);
						t2 = EditorGUILayout.Popup(t2, nodeNameChoices, GUILayout.Width(_w.optTargetW));
						cfg.Opt2TargetName = (t2 <= 0) ? string.Empty : nodeNameChoices[t2];

						cfg.Opt2TargetPort = PortSelPopup(cfg.Opt2TargetPort, _w.optPortW, true);
					}
				}

				EditorGUILayout.EndVertical();
				GUILayout.Space(6);

				drawnH += ApproxRowHeight(cfg);
			}

			// bottom spacer
			float bottomPad = 0f;
			for (int i = end; i < count; i++) {
				if (!fieldConfigs.TryGetValue(keySnapshot[i], out var cfg2))
					continue;
				bottomPad += ApproxRowHeight(cfg2);
			}
			GUILayout.Space(bottomPad);

			// rolling average for virtualization
			if (end > start)
				_avgRowH = Mathf.Lerp(_avgRowH, drawnH / Mathf.Max(1, end - start), 0.15f);

			EditorGUILayout.EndScrollView();

			GUILayout.Space(10);
			using (new EditorGUI.DisabledScope(orderedKeys.Count == 0)) {
				if (GUILayout.Button("Next"))
					stepNumber = 4;
			}
		}

		private void ShowStep4() {
			EditorGUILayout.LabelField("Create Dialogue Graph from rows.");
			if (GUILayout.Button("Create Dialogue Graph"))
				CreateDialogueFromRows();
		}

		private void EnsureWidths() {
			float w = position.width;
			if (!Mathf.Approximately(w, _lastW)) {
				ComputeWidths(
					out _w.controlsW, out _w.nameW, out _w.portLabelW, out _w.connectW, out _w.overrideW,
					out _w.dialogW, out _w.keywordW, out _w.typeLabelW, out _w.typeW,
					out _w.poseLabelW, out _w.poseW, out _w.voiceLabelW, out _w.voiceW,
					out _w.optLabelW, out _w.optPortW, out _w.optTargetW
				);
				_lastW = w;
			}
		}

		private void ComputeWidths(
			out float controlsW, out float nameW, out float portLabelW, out float connectW, out float overrideW,
			out float dialogW, out float keywordW, out float typeLabelW, out float typeW,
			out float poseLabelW, out float poseW, out float voiceLabelW, out float voiceW,
			out float optLabelW, out float optPortW, out float optTargetW
		) {
			float total = Mathf.Max(560f, position.width - 36f);

			controlsW = 120f;
			portLabelW = 80f;
			connectW = 120f;

			typeLabelW = 44f;
			typeW = 180f;

			poseLabelW = 48f;
			poseW = 180f;
			voiceLabelW = 56f;
			voiceW = 180f;

			// Row 1: controls + Name + "output ->" + Port + Override
			float fixedRow1 = controlsW + portLabelW + connectW;
			float remRow1 = Mathf.Max(280f, total - fixedRow1);
			nameW = Mathf.Max(160f, remRow1 * 0.50f);
			overrideW = Mathf.Max(220f, remRow1 - nameW);

			// Row 2: Dialog + Keyword + "type=" + Type
			float fixedRow2 = typeLabelW + typeW;
			float remRow2 = Mathf.Max(260f, total - fixedRow2);
			dialogW = Mathf.Max(220f, remRow2 * 0.58f);
			keywordW = Mathf.Max(120f, remRow2 - dialogW);

			// Row 3: options
			optPortW = 150f;
			optLabelW = Mathf.Max(200f, (total - optPortW) * 0.40f);
			optTargetW = Mathf.Max(200f, total - optLabelW - optPortW);
		}

		private float ApproxRowHeight(FieldConfig cfg) {
			float h = EditorGUIUtility.singleLineHeight + 6f;
			h += EditorGUIUtility.singleLineHeight + 6f;
			if (IsDialogueType(cfg.TypeIndex))
				h += EditorGUIUtility.singleLineHeight + 6f;
			if (IsOptionsType(cfg.TypeIndex)) {
				h += (EditorGUIUtility.singleLineHeight + 6f) * 2f;
			}
			h += 6f;
			return h;
		}

		private string PlaceholderTextField(string value, string placeholder, float width) {
			var rect = GUILayoutUtility.GetRect(width, EditorGUIUtility.singleLineHeight, EditorStyles.textField);
			string newVal = EditorGUI.TextField(rect, value ?? string.Empty);
			if (string.IsNullOrEmpty(newVal) && Event.current.type == EventType.Repaint) {
				var r = new Rect(rect.x + 4, rect.y + 1, rect.width - 8, rect.height);
				var old = GUI.color;
				GUI.color = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.35f) : new Color(0, 0, 0, 0.45f);
				GUI.Label(r, placeholder, EditorStyles.miniLabel);
				GUI.color = old;
			}
			return newVal;
		}

		private string DelayedTextWithPlaceholder(string value, string placeholder, float width) {
			var rect = GUILayoutUtility.GetRect(width, EditorGUIUtility.singleLineHeight, EditorStyles.textField);
			string newVal = EditorGUI.DelayedTextField(rect, value ?? string.Empty);
			if (string.IsNullOrEmpty(newVal) && Event.current.type == EventType.Repaint) {
				var r = new Rect(rect.x + 4, rect.y + 1, rect.width - 8, rect.height);
				var old = GUI.color;
				GUI.color = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.35f) : new Color(0, 0, 0, 0.45f);
				GUI.Label(r, placeholder, EditorStyles.miniLabel);
				GUI.color = old;
			}
			return newVal;
		}

		private PortSel PortSelPopup(PortSel value, float width, bool isOptionRow) {
			int idx = value == PortSel.OverrideInput ? 1 : 0;
			string[] labels = isOptionRow
				? new[] { "output -> Input", "output -> OverrideInput" }
				: new[] { "Input", "OverrideInput" };
			idx = EditorGUILayout.Popup(idx, labels, GUILayout.Width(width));
			return (idx == 1) ? PortSel.OverrideInput : PortSel.Input;
		}

		private string[] GetNodeNameChoicesCached() {
			if (_nodeNameChoicesDirty) {
				var names = new List<string> { "(none)" };
				for (int i = 0; i < orderedKeys.Count; i++) {
					if (!fieldConfigs.TryGetValue(orderedKeys[i], out var cfg))
						continue;
					var nm = string.IsNullOrWhiteSpace(cfg.NodeName) ? orderedKeys[i] : cfg.NodeName;
					if (!string.IsNullOrWhiteSpace(nm))
						names.Add(nm);
				}
				_nodeNameChoices = names.ToArray();
				_nodeNameChoicesDirty = false;
			}
			return _nodeNameChoices;
		}
		private int IndexOfChoice(string[] choices, string value) {
			if (string.IsNullOrWhiteSpace(value))
				return 0;
			for (int i = 1; i < choices.Length; i++)
				if (string.Equals(choices[i], value, StringComparison.OrdinalIgnoreCase))
					return i;
			return 0;
		}

		private string[] GetEnumChoices(string enumSimpleName, out Type enumType) {
			enumType = ResolveTypeBySimpleName(enumSimpleName);
			if (enumType != null && enumType.IsEnum) {
				try { return Enum.GetNames(enumType); }
				catch { }
			}
			enumType = null;
			return Array.Empty<string>();
		}

		private void LoadFolderNames() {
			string path = "Assets/Resources/States/";
			if (Directory.Exists(path)) {
				folderNames = Directory.GetDirectories(path)
					.Select(Path.GetFileName)
					.Where(name => !string.Equals(name, "Introduction", StringComparison.OrdinalIgnoreCase) &&
								   !string.Equals(name, "Ending", StringComparison.OrdinalIgnoreCase))
					.OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
					.ToArray();
			} else {
				folderNames = Array.Empty<string>();
			}
		}

		private void CollectFieldsFromLayoutChainOnce() {
			fieldsList.Clear();
			fieldNamesSeen.Clear();
			resolvedChain.Clear();

			if (!selectedLayout) {
				Debug.LogWarning("[StateCreationTool] No layout selected.");
				return;
			}

			var layouts = ResolveChain(selectedLayout);
			resolvedChain.AddRange(layouts.Where(l => l));

			int added = 0;
			for (int li = 0; li < resolvedChain.Count; li++) {
				var la = resolvedChain[li];
				if (la == null || la.fields == null)
					continue;
				for (int i = 0; i < la.fields.Count; i++) {
					var f = la.fields[i];
					if (f.fieldType != ImageFieldingTypes.String)
						continue;
					if (string.IsNullOrEmpty(f.ID))
						continue;
					if (fieldNamesSeen.Add(f.ID)) { fieldsList.Add(f); added++; }
				}
			}

			Debug.Log($"[StateCreationTool] Chain layouts: {resolvedChain.Count}, collected string fields: {fieldsList.Count} (added {added}).");
		}

		private void InitializeFieldRows() {
			orderedKeys.Clear();
			for (int i = 0; i < fieldsList.Count; i++) {
				var f = fieldsList[i];
				if (!fieldConfigs.TryGetValue(f.ID, out var cfg)) {
					cfg = new FieldConfig {
						NodeName = f.ID,
						Dialog = string.Empty,
						Keyword = string.Empty,
						InputOverride = string.Empty,
						TypeIndex = Array.IndexOf(NodeTypeOptions, "InputNode"),
						NextRowPort = PortSel.Input,
						PoseIndex = 0,
						VoiceIndex = 0,
						Opt1Label = string.Empty,
						Opt1TargetName = string.Empty,
						Opt2Label = string.Empty,
						Opt2TargetName = string.Empty
					};
					if (cfg.TypeIndex < 0)
						cfg.TypeIndex = 0;
					fieldConfigs[f.ID] = cfg;
				} else if (string.IsNullOrEmpty(cfg.NodeName)) {
					cfg.NodeName = f.ID;
				}
				orderedKeys.Add(f.ID);
			}
			if (orderedKeys.Count == 0) {
				var k = NewKey();
				fieldConfigs[k] = new FieldConfig {
					NodeName = "New Node",
					Dialog = string.Empty,
					Keyword = string.Empty,
					InputOverride = string.Empty,
					TypeIndex = Array.IndexOf(NodeTypeOptions, "InputNode"),
					PoseIndex = 0,
					VoiceIndex = 0
				};
				if (fieldConfigs[k].TypeIndex < 0)
					fieldConfigs[k].TypeIndex = 0;
				orderedKeys.Add(k);
			}
		}

		private void InsertRowAfter(int index) {
			var k = NewKey();
			fieldConfigs[k] = new FieldConfig {
				NodeName = "New Node",
				Dialog = string.Empty,
				Keyword = string.Empty,
				InputOverride = string.Empty,
				TypeIndex = Array.IndexOf(NodeTypeOptions, "InputNode"),
				PoseIndex = 0,
				VoiceIndex = 0
			};
			if (fieldConfigs[k].TypeIndex < 0)
				fieldConfigs[k].TypeIndex = 0;
			orderedKeys.Insert(Mathf.Clamp(index + 1, 0, orderedKeys.Count), k);
			_nodeNameChoicesDirty = true;
		}
		private void RemoveRowAt(int index) {
			if (orderedKeys.Count <= 1)
				return;
			index = Mathf.Clamp(index, 0, orderedKeys.Count - 1);
			orderedKeys.RemoveAt(index);
			_nodeNameChoicesDirty = true;
		}
		private void MoveRow(int index, int delta) {
			if (index < 0 || index >= orderedKeys.Count)
				return;
			int newIndex = Mathf.Clamp(index + delta, 0, orderedKeys.Count - 1);
			if (newIndex == index)
				return;
			var key = orderedKeys[index];
			orderedKeys.RemoveAt(index);
			orderedKeys.Insert(newIndex, key);
			_nodeNameChoicesDirty = true;
		}

		private string NewKey() { newRowCounter++; return $"__new_{newRowCounter}"; }

		private bool IsOptionsType(int typeIndex) {
			var name = NodeTypeOptions[Mathf.Clamp(typeIndex, 0, NodeTypeOptions.Length - 1)];
			return string.Equals(name, "ChoiceNode", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(name, "DropdownNode", StringComparison.OrdinalIgnoreCase);
		}
		private bool IsDialogueType(int typeIndex) {
			var name = NodeTypeOptions[Mathf.Clamp(typeIndex, 0, NodeTypeOptions.Length - 1)];
			return string.Equals(name, "DialogueNode", StringComparison.OrdinalIgnoreCase);
		}

		private List<ImageFieldingAsset> ResolveChain(ImageFieldingAsset root) {
			var visited = new HashSet<ImageFieldingAsset>();
			var viaMethod = TryResolveChainViaMethod(root, visited);
			if (viaMethod.Count > 0)
				return viaMethod;

			var viaMembers = TryResolveChainViaMembers(root, visited);
			if (viaMembers.Count > 0)
				return viaMembers;

			return new List<ImageFieldingAsset> { root };
		}

		private List<ImageFieldingAsset> TryResolveChainViaMethod(ImageFieldingAsset root, HashSet<ImageFieldingAsset> visited) {
			var collected = new List<ImageFieldingAsset>();
			foreach (var m in GetCandidateMethods()) {
				if (TryInvokeMethodVariants(m, root, collected, visited))
					return collected;
			}
			return collected;
		}

		private static IEnumerable<MethodInfo> GetCandidateMethods() {
			var t = typeof(ImageFieldingAsset);
			var candidateNames = new[] { "GetChainedLayouts", "GetChainedLayoutChain", "GetChain" };
			return t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
					.Where(m => candidateNames.Contains(m.Name));
		}

		private bool TryInvokeMethodVariants(MethodInfo m, ImageFieldingAsset root, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			try {
				var ps = m.GetParameters();
				object target = m.IsStatic ? null : root;

				if (ps.Length == 0)
					return TryNoArg(m, target, collected, visited);

				if (ps.Length == 1 && typeof(IList).IsAssignableFrom(ps[0].ParameterType))
					return TryOutListSingle(m, target, collected, visited);

				if (ps.Length == 1 && ps[0].ParameterType == typeof(bool))
					return TryBoolSingle(m, target, collected, visited);

				if (ps.Length == 2)
					return TryTwoParamCombos(m, target, ps, collected, visited);
			}
			catch {
				// ignore and try next method
			}
			return false;
		}

		private bool TryNoArg(MethodInfo m, object target, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			var res = m.Invoke(target, null);
			return AppendFromEnumerable(res, collected, visited);
		}

		private bool TryOutListSingle(MethodInfo m, object target, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			var list = (IList)Activator.CreateInstance(typeof(List<ImageFieldingAsset>));
			m.Invoke(target, new object[] { list });
			return AppendFromEnumerable(list, collected, visited);
		}

		private bool TryBoolSingle(MethodInfo m, object target, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			foreach (var flag in new[] { true, false }) {
				var res = m.Invoke(target, new object[] { flag });

				if (AppendFromEnumerable(res, collected, visited))
					return true;
			}

			return false;
		}

		private bool TryTwoParamCombos(MethodInfo m, object target, ParameterInfo[] ps, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			bool firstIsList = typeof(IList).IsAssignableFrom(ps[0].ParameterType);
			bool secondIsList = typeof(IList).IsAssignableFrom(ps[1].ParameterType);

			if (firstIsList && ps[1].ParameterType == typeof(bool))
				return TryListThenBool(m, target, collected, visited);

			if (ps[0].ParameterType == typeof(bool) && secondIsList)
				return TryBoolThenList(m, target, collected, visited);

			return false;
		}

		private bool TryListThenBool(MethodInfo m, object target, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			var list = (IList)Activator.CreateInstance(typeof(List<ImageFieldingAsset>));

			foreach (var flag in new[] { true, false }) {
				m.Invoke(target, new object[] { list, flag });

				if (AppendFromEnumerable(list, collected, visited))
					return true;

				list.Clear();
			}

			return false;
		}

		private bool TryBoolThenList(MethodInfo m, object target, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			var list = (IList)Activator.CreateInstance(typeof(List<ImageFieldingAsset>));

			foreach (var flag in new[] { true, false }) {
				m.Invoke(target, new object[] { flag, list });

				if (AppendFromEnumerable(list, collected, visited))
					return true;

				list.Clear();
			}

			return false;
		}

		private List<ImageFieldingAsset> TryResolveChainViaMembers(ImageFieldingAsset root, HashSet<ImageFieldingAsset> visited) {
			var chain = new List<ImageFieldingAsset>();
			var current = root;

			var singleNames = new[] { "next", "nextLayout", "nextInChain", "chainNext" };
			var multiNames = new[] { "chainedLayouts", "nextLayouts", "layouts", "pages", "chain", "layoutChain" };

			int guard = 0;
			while (current && visited.Add(current) && guard++ < 1024) {
				chain.Add(current);

				var list = FindFirstListOfLayouts(current, multiNames);
				if (list != null && list.Count > 0) {
					foreach (var la in list)
						if (la && visited.Add(la))
							chain.Add(la);
					break;
				}

				var next = FindFirstSingleNext(current, singleNames);
				if (next && !visited.Contains(next)) { current = next; continue; }
				break;
			}
			return chain;
		}

		private static bool AppendFromEnumerable(object enumerable, List<ImageFieldingAsset> outList, HashSet<ImageFieldingAsset> visited) {
			if (enumerable is IEnumerable en) {
				foreach (var obj in en) {
					if (obj is ImageFieldingAsset la && la && visited.Add(la))
						outList.Add(la);
				}
				return outList.Count > 0;
			}
			return false;
		}

		private static List<ImageFieldingAsset> FindFirstListOfLayouts(ImageFieldingAsset obj, string[] preferredNames) {
			var t = obj.GetType();

			foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				if (!typeof(IList).IsAssignableFrom(f.FieldType))
					continue;

				if (preferredNames.Any(n => string.Equals(f.Name, n, StringComparison.OrdinalIgnoreCase)) || IsIListOfLayouts(f.FieldType)) {
					var val = f.GetValue(obj) as IList;
					var list = ToLayoutList(val);

					if (list != null && list.Count > 0)
						return list;
				}
			}

			foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				if (!p.CanRead)
					continue;
				if (!typeof(IList).IsAssignableFrom(p.PropertyType))
					continue;
				if (preferredNames.Any(n => string.Equals(p.Name, n, StringComparison.OrdinalIgnoreCase)) || IsIListOfLayouts(p.PropertyType)) {
					var val = p.GetValue(obj, null) as IList;
					var list = ToLayoutList(val);

					if (list != null && list.Count > 0)
						return list;
				}
			}

			return null;
		}

		private static ImageFieldingAsset FindFirstSingleNext(ImageFieldingAsset obj, string[] preferredNames) {
			var t = obj.GetType();

			foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				if (typeof(ImageFieldingAsset).IsAssignableFrom(f.FieldType)) {
					if (preferredNames.Any(n => string.Equals(f.Name, n, StringComparison.OrdinalIgnoreCase)))
						return f.GetValue(obj) as ImageFieldingAsset;

					var val = f.GetValue(obj) as ImageFieldingAsset;
					if (val)
						return val;
				}
			}

			foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				if (!p.CanRead)
					continue;

				if (typeof(ImageFieldingAsset).IsAssignableFrom(p.PropertyType)) {
					if (preferredNames.Any(n => string.Equals(p.Name, n, StringComparison.OrdinalIgnoreCase)))
						return p.GetValue(obj, null) as ImageFieldingAsset;

					var val = p.GetValue(obj, null) as ImageFieldingAsset;
					if (val)
						return val;
				}
			}

			return null;
		}

		private static bool IsIListOfLayouts(Type t) {
			if (!typeof(IList).IsAssignableFrom(t))
				return false;

			if (t.IsGenericType && t.GetGenericArguments().Length == 1) {
				return typeof(ImageFieldingAsset).IsAssignableFrom(t.GetGenericArguments()[0]);
			}

			return false;
		}

		private static List<ImageFieldingAsset> ToLayoutList(IList list) {
			if (list == null)
				return null;

			var res = new List<ImageFieldingAsset>();
			foreach (var obj in list)
				if (obj is ImageFieldingAsset la && la)
					res.Add(la);

			return res;
		}

		private void CreateDialogueFromRows() {
			if (orderedKeys.Count == 0) {
				EditorUtility.DisplayDialog("No Rows", "No rows defined in Step 3.", "OK");
				return;
			}

			var graph = ScriptableObject.CreateInstance<DialogueGraph>();
			string defaultFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selectedLayout)).Replace("\\", "/");

			if (!AssetDatabase.IsValidFolder(defaultFolder)) {
				var parts = defaultFolder.Split('/');
				string acc = parts[0];

				for (int i = 1; i < parts.Length; i++) {
					string next = acc + "/" + parts[i];

					if (!AssetDatabase.IsValidFolder(next))
						AssetDatabase.CreateFolder(acc, parts[i]);
					acc = next;
				}
			}

			string savePath = EditorUtility.SaveFilePanelInProject(
				"Save Dialogue",
				$"{(string.IsNullOrEmpty(stateName) ? "State" : stateName)}Dialogue",
				"asset",
				"Save the dialogue as a ScriptableObject asset.",
				defaultFolder
			);

			if (string.IsNullOrEmpty(savePath))
				return;

			AssetDatabase.CreateAsset(graph, savePath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			var startNode = ScriptableObject.CreateInstance<StartNode>();
			startNode.name = "StartNode";
			startNode.graph = graph;
			AssetDatabase.AddObjectToAsset(startNode, graph);
			graph.nodes.Add(startNode);

			var endNode = ScriptableObject.CreateInstance<EndNode>();
			endNode.name = "EndNode";
			endNode.graph = graph;
			AssetDatabase.AddObjectToAsset(endNode, graph);
			graph.nodes.Add(endNode);

			var spriteEnumType = ResolveTypeBySimpleName("CharacterSpriteType");
			var voiceEnumType = ResolveTypeBySimpleName("VoiceLineType");
			var poseChoices = spriteEnumType?.IsEnum == true ? Enum.GetNames(spriteEnumType) : Array.Empty<string>();
			var voiceChoices = voiceEnumType?.IsEnum == true ? Enum.GetNames(voiceEnumType) : Array.Empty<string>();

			var created = new List<Node>();
			var byName = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);

			for (int i = 0; i < orderedKeys.Count; i++) {
				var key = orderedKeys[i];
				if (!fieldConfigs.TryGetValue(key, out var cfg))
					continue;

				var typeName = NodeTypeOptions[Mathf.Clamp(cfg.TypeIndex, 0, NodeTypeOptions.Length - 1)];
				var nodeType = ResolveNodeType(typeName) ?? typeof(InputNode);

				var node = (Node)ScriptableObject.CreateInstance(nodeType);
				node.name = string.IsNullOrEmpty(cfg.NodeName) ? key : cfg.NodeName;

				SetStringMember(node, "DialogueText", cfg.Dialog);
				SetStringMember(node, "Keyword", cfg.Keyword);

				if (string.Equals(typeName, "DialogueNode", StringComparison.OrdinalIgnoreCase)) {
					SetEnumMember(node, "SpriteType", spriteEnumType, cfg.PoseIndex, poseChoices);
					SetEnumMember(node, "VoiceLine", voiceEnumType, cfg.VoiceIndex, voiceChoices);
				}

				if (IsOptionsType(cfg.TypeIndex)) {
					var opts = new List<string>();

					if (!string.IsNullOrWhiteSpace(cfg.Opt1Label))
						opts.Add(cfg.Opt1Label);

					if (!string.IsNullOrWhiteSpace(cfg.Opt2Label))
						opts.Add(cfg.Opt2Label);

					SetOptionsIfPresent(node, opts.ToArray());

					var updatePorts = typeof(XNode.Node).GetMethod("UpdatePorts", BindingFlags.Instance | BindingFlags.NonPublic);

					updatePorts?.Invoke(node, null);
				}

				node.graph = graph;
				AssetDatabase.AddObjectToAsset(node, graph);
				graph.nodes.Add(node);

				if (IsOptionsType(cfg.TypeIndex)) {
					// Always create two options; use placeholders if labels are empty.
					var opt1 = string.IsNullOrWhiteSpace(cfg.Opt1Label) ? "Option 1" : cfg.Opt1Label;
					var opt2 = string.IsNullOrWhiteSpace(cfg.Opt2Label) ? "Option 2" : cfg.Opt2Label;

					SetOptionsIfPresent(node, new[] { opt1, opt2 });

					// Ensure dynamic "Options N" ports exist AFTER the node is attached to the graph.
					var updatePorts = typeof(XNode.Node).GetMethod("UpdatePorts", BindingFlags.Instance | BindingFlags.NonPublic);
					updatePorts?.Invoke(node, null);
					EditorUtility.SetDirty(node);
				}

				created.Add(node);

				if (!byName.ContainsKey(node.name))
					byName[node.name] = node;
			}

			// Layout positions
			Vector2 pos = new Vector2(-1600, 600);
			startNode.position = pos;

			for (int i = 0; i < created.Count; i++) {
				pos = new Vector2(pos.x + 500, pos.y);
				if (i % 4 == 0 && i != 0)
					pos = new Vector2(pos.x - 2000, pos.y + 600);
				(created[i] as XNode.Node).position = pos;
			}
			endNode.position = new Vector2(pos.x + 500, pos.y);

			// Start -> first (or End if none)
			try {
				if (created.Count > 0) {
					var first = created[0] as XNode.Node;

					startNode.GetOutputPort("Output").Connect(first.GetInputPort("Input"));
				} else {
					startNode.GetOutputPort("Output").Connect(endNode.GetInputPort("Input"));
				}
			}
			catch (Exception e) {
				Debug.LogWarning($"[StateCreationTool] Port connect error (Start->First): {e.Message}");
			}

			// Linear wiring (non-options)
			for (int i = 0; i < created.Count - 1; i++) {
				var key = orderedKeys[i];
				if (!fieldConfigs.TryGetValue(key, out var cfg))
					continue;
				if (IsOptionsType(cfg.TypeIndex))
					continue;

				var a = created[i] as XNode.Node;
				var b = created[i + 1] as XNode.Node;

				var outPort = a.GetOutputPort("Output");
				var inPort = (cfg.NextRowPort == PortSel.OverrideInput)
					? (b.GetInputPort("OverrideInput") ?? b.GetInputPort("InputOverride") ?? b.GetInputPort("Input"))
					: b.GetInputPort("Input");

				if (outPort != null && inPort != null) {
					try { outPort.Connect(inPort); }
					catch (Exception e) {
						Debug.LogWarning($"[StateCreationTool] Port connect error ({a.name} -> {b.name}): {e.Message}");
					}
				}
			}

			// Options wiring
			for (int i = 0; i < created.Count; i++) {
				var key = orderedKeys[i];

				if (!fieldConfigs.TryGetValue(key, out var cfg))
					continue;
				if (!IsOptionsType(cfg.TypeIndex))
					continue;

				var node = created[i] as XNode.Node;

				var updatePorts = typeof(XNode.Node).GetMethod("UpdatePorts", BindingFlags.Instance | BindingFlags.NonPublic);
				updatePorts?.Invoke(node, null);

				Node nextNode = (i + 1 < created.Count) ? created[i + 1] : endNode;
				var fallbackIn = (nextNode as XNode.Node).GetInputPort("Input");


				WireOption(node, byName, cfg.Opt1TargetName, cfg.Opt1TargetPort, 0, fallbackIn);
				WireOption(node, byName, cfg.Opt2TargetName, cfg.Opt2TargetPort, 1, fallbackIn);
			}

			// InputOverride back-edges
			for (int i = 0; i < created.Count; i++) {
				var key = orderedKeys[i];

				if (!fieldConfigs.TryGetValue(key, out var cfg))
					continue;
				if (string.IsNullOrWhiteSpace(cfg.InputOverride))
					continue;

				if (!byName.TryGetValue(cfg.InputOverride, out var source))
					continue;

				var current = created[i] as XNode.Node;
				var srcOut = source.GetOutputPort("Output");
				var dstIn = current.GetInputPort("OverrideInput") ?? current.GetInputPort("InputOverride");

				if (srcOut != null && dstIn != null) {
					try {
						if (!dstIn.IsConnected || !dstIn.GetConnections().Any(c => c.node == source))
							srcOut.Connect(dstIn);
					}
					catch (Exception e) {
						Debug.LogWarning($"[StateCreationTool] Port connect error (override {source.name} -> {current.name}): {e.Message}");
					}
				}
			}

			// Last -> End (only if still free)
			try {
				if (created.Count > 0) {
					var last = created.Last() as XNode.Node;
					var lastOut = last.GetOutputPort("Output");

					if (lastOut != null && !lastOut.IsConnected) {
						lastOut.Connect(endNode.GetInputPort("Input"));
					}
				}
			}
			catch (Exception e) {
				Debug.LogWarning($"[StateCreationTool] Port connect error (Last->End): {e.Message}");
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Done", $"Dialogue created with {created.Count} nodes.", "OK");
		}

		private void WireOption(
			XNode.Node node,
			Dictionary<string, Node> byName,
			string targetName, 
			PortSel targetPortSel,
			int optionIndex, 
			XNode.NodePort fallbackIn) {

			var updatePorts = typeof(XNode.Node).GetMethod("UpdatePorts", BindingFlags.Instance | BindingFlags.NonPublic);
			updatePorts?.Invoke(node, null);

			string portName = $"Options {optionIndex}";
			var outPort = node.GetOutputPort(portName);
			if (outPort == null) {
				Debug.LogWarning($"[StateCreationTool] '{node.name}' missing output port '{portName}'. " +
								 $"Ensure Options are set to length >= {optionIndex + 1}.");
				return;
			}

			// Pick destination: explicit target or fallback to next row's Input
			XNode.Node dest = null;
			if (!string.IsNullOrWhiteSpace(targetName)) {
				byName.TryGetValue(targetName, out dest);
			}

			var destIn = (dest != null)
				? (targetPortSel == PortSel.OverrideInput
					? (dest.GetInputPort("OverrideInput") ?? dest.GetInputPort("InputOverride") ?? dest.GetInputPort("Input"))
					: dest.GetInputPort("Input"))
				: fallbackIn;

			if (destIn == null) {
				Debug.LogWarning($"[StateCreationTool] No valid destination input for option {optionIndex} on '{node.name}'.");
				return;
			}

			try { outPort.Connect(destIn); }
			catch (Exception e) {
				Debug.LogWarning($"[StateCreationTool] Port connect error (option {optionIndex} from {node.name}): {e.Message}");
			}
		}


		private static void SetOptionsIfPresent(object node, string[] options) {
			if (node == null || options == null)
				return;
			var t = node.GetType();

			var p = t.GetProperty("Options", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (p != null && p.CanWrite) {
				if (p.PropertyType == typeof(string[])) { 
					p.SetValue(node, options, null);

					return; 
				}

				if (typeof(IList<string>).IsAssignableFrom(p.PropertyType)) {
					var list = (IList<string>)Activator.CreateInstance(p.PropertyType);

					foreach (var s in options)
						list.Add(s);
					p.SetValue(node, list, null);

					return;
				}
			}

			var f = t.GetField("Options", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (f != null) {
				if (f.FieldType == typeof(string[])) { 
					f.SetValue(node, options); 
					return; 
				}

				if (typeof(IList<string>).IsAssignableFrom(f.FieldType)) {
					var list = (IList<string>)Activator.CreateInstance(f.FieldType);

					foreach (var s in options)
						list.Add(s);

					f.SetValue(node, list);
				}
			}
		}

		private static Type ResolveNodeType(string simpleName) {
			if (string.IsNullOrEmpty(simpleName))
				return null;

			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
				try {
					var t = asm.GetTypes().FirstOrDefault(tt => tt.Name == simpleName && typeof(Node).IsAssignableFrom(tt));

					if (t != null)
						return t;
				}
				catch { }
			}
			return null;
		}
		private static Type ResolveTypeBySimpleName(string simpleName) {
			if (string.IsNullOrEmpty(simpleName))
				return null;

			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
				try {
					var t = asm.GetTypes().FirstOrDefault(tt => tt.Name == simpleName);

					if (t != null)
						return t;
				}
				catch { }
			}
			return null;
		}

		private static void SetStringMember(object target, string memberName, string value) {
			if (target == null)
				return;
			var t = target.GetType();

			var p = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (p != null && p.CanWrite && p.PropertyType == typeof(string)) { 
				p.SetValue(target, value, null); 
				return; 
			}

			var f = t.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (f != null && f.FieldType == typeof(string)) { 
				f.SetValue(target, value); 
			}
		}

		// TODO : Rename variables
		private static void SetEnumMember(object target, string memberName, Type enumType, int index, string[] names) {
			if (target == null || enumType == null || names == null || names.Length == 0)
				return;
			index = Mathf.Clamp(index, 0, names.Length - 1);
			var name = names[index];
			try {
				var value = Enum.Parse(enumType, name);
				var t = target.GetType();

				var p = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (p != null && p.CanWrite && p.PropertyType.IsEnum && p.PropertyType == enumType) {
					p.SetValue(target, value, null);
					return;
				}

				var f = t.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (f != null && f.FieldType.IsEnum && f.FieldType == enumType) {
					f.SetValue(target, value);
				}
			}
			catch { }
		}
	}
}
#endif
*/