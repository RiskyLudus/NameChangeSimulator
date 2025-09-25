#if UNITY_EDITOR
namespace NameChangeSimulator.Editor {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;

	using DW.Tools;

	using UnityEditor;

	using UnityEngine;

	public partial class StateCreationToolEditor {
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

		private void ShowStep3() {
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

			var nodeNameChoices = GetNodeNameChoicesCached();
			var poseChoices = _poseChoices;
			var voiceChoices = _voiceChoices;

			var keySnapshot = orderedKeys.ToArray();
			int count = keySnapshot.Length;

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(kViewHeight));

			float approxRow = Mathf.Max(24f, _avgRowH);
			int approxPerScreen = Mathf.Max(1, Mathf.CeilToInt(kViewHeight / approxRow));
			int start = Mathf.Clamp(Mathf.FloorToInt(scrollPosition.y / approxRow) - 3, 0, Mathf.Max(0, count - 1));
			int end = Mathf.Min(count, start + approxPerScreen + 6);

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

				using (new EditorGUILayout.HorizontalScope()) {
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

					string oldName = cfg.NodeName;
					cfg.NodeName = PlaceholderTextField(cfg.NodeName ?? key, "Node Name", _w.nameW);
					if (!string.Equals(oldName, cfg.NodeName, StringComparison.Ordinal))
						_nodeNameChoicesDirty = true;

					GUILayout.Label(GOutputArrow, GUILayout.Width(_w.portLabelW));
					cfg.NextRowPort = PortSelPopup(cfg.NextRowPort, _w.connectW, false);

					int ovIdx = IndexOfChoice(nodeNameChoices, cfg.InputOverride);
					ovIdx = EditorGUILayout.Popup(ovIdx, nodeNameChoices, GUILayout.Width(_w.overrideW));
					cfg.InputOverride = (ovIdx <= 0) ? string.Empty : nodeNameChoices[ovIdx];
				}

				using (new EditorGUILayout.HorizontalScope()) {
					cfg.Dialog = DelayedTextWithPlaceholder(cfg.Dialog, "Dialog", _w.dialogW);
					cfg.Keyword = PlaceholderTextField(cfg.Keyword, "Keyword", _w.keywordW);
					GUILayout.Label(GTypeEquals, GUILayout.Width(_w.typeLabelW));
					cfg.TypeIndex = EditorGUILayout.Popup(cfg.TypeIndex, NodeTypeOptions, GUILayout.Width(_w.typeW));
				}

				if (isDialogue) {
					using (new EditorGUILayout.HorizontalScope()) {
						GUILayout.Label(GPoseEquals, GUILayout.Width(_w.poseLabelW));
						cfg.PoseIndex = EditorGUILayout.Popup(
							Mathf.Clamp(cfg.PoseIndex, 0, Mathf.Max(0, poseChoices.Length - 1)),
							poseChoices.Length > 0 ? poseChoices : new[] { "(no CharacterSpriteType enum found)" },
							GUILayout.Width(_w.poseW)
						);

						GUILayout.Label(GVoiceEquals, GUILayout.Width(_w.voiceLabelW));
						cfg.VoiceIndex = EditorGUILayout.Popup(
							Mathf.Clamp(cfg.VoiceIndex, 0, Mathf.Max(0, voiceChoices.Length - 1)),
							voiceChoices.Length > 0 ? voiceChoices : new[] { "(no VoiceLineType enum found)" },
							GUILayout.Width(_w.voiceW)
						);
					}
				}

				if (isOptions) {
					using (new EditorGUILayout.HorizontalScope()) {
						cfg.Opt1Label = PlaceholderTextField(cfg.Opt1Label, "Option 1 label", _w.optLabelW);
						int t1 = IndexOfChoice(nodeNameChoices, cfg.Opt1TargetName);
						t1 = EditorGUILayout.Popup(t1, nodeNameChoices, GUILayout.Width(_w.optTargetW));
						cfg.Opt1TargetName = (t1 <= 0) ? string.Empty : nodeNameChoices[t1];
						cfg.Opt1TargetPort = PortSelPopup(cfg.Opt1TargetPort, _w.optPortW, true);
					}
					using (new EditorGUILayout.HorizontalScope()) {
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

			float bottomPad = 0f;
			for (int i = end; i < count; i++) {
				if (!fieldConfigs.TryGetValue(keySnapshot[i], out var cfg2))
					continue;
				bottomPad += ApproxRowHeight(cfg2);
			}
			GUILayout.Space(bottomPad);

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

			float fixedRow1 = controlsW + portLabelW + connectW;
			float remRow1 = Mathf.Max(280f, total - fixedRow1);
			nameW = Mathf.Max(160f, remRow1 * 0.50f);
			overrideW = Mathf.Max(220f, remRow1 - nameW);

			float fixedRow2 = typeLabelW + typeW;
			float remRow2 = Mathf.Max(260f, total - fixedRow2);
			dialogW = Mathf.Max(220f, remRow2 * 0.58f);
			keywordW = Mathf.Max(120f, remRow2 - dialogW);

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
	}
}
#endif
