using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using DW.Tools;
using Anarchy.Shared;

namespace NameChangeSimulator.Runtime.ImageFieldingBridge {
	[DefaultExecutionOrder(100)]
	public class FormDataFiller_ImageFielding : MonoBehaviour {
		[Serializable]
		public struct StateLayoutBinding {
			public string stateName;
			public ImageFieldingAsset layout;
		}

		public ImageFieldingAsset defaultLayout;
		public List<StateLayoutBinding> stateLayouts = new List<StateLayoutBinding>();
		public bool updateAssetTextFieldsLive = true;
#if UNITY_EDITOR
		public bool autoSaveAssetInEditor = true;
#endif
		public ImageFieldingViewController viewController;
		public bool useBackgroundSize = true;
		public int outputWidthPixels = 2048;
		public int outputHeightPixels = 2048;
		public float pdfDpi = 300f;

		private ImageFieldingAsset _activeLayout;
		private string _stateName;

		private string _deadFirstName, _deadMiddleName, _deadLastName;
		private string _newFirstName, _newMiddleName, _newLastName;

		private readonly Dictionary<string, string> _strings =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private readonly Dictionary<string, string> _pendingStrings =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private string FullDeadName => $"{_deadFirstName} {_deadMiddleName} {_deadLastName}".Trim();
		private string FullNewName => $"{_newFirstName} {_newMiddleName} {_newLastName}".Trim();

		private void OnEnable() {
			ConstructBindings.Send_FormDataFillerData_Load?.AddListener(OnLoad);
			ConstructBindings.Send_FormDataFillerData_Submit?.AddListener(OnSubmit);
			ConstructBindings.Send_FormDataFillerData_ApplyToPDF?.AddListener(OnApplyToPDF);
		}

		private void OnDisable() {
			ConstructBindings.Send_FormDataFillerData_Load?.RemoveListener(OnLoad);
			ConstructBindings.Send_FormDataFillerData_Submit?.RemoveListener(OnSubmit);
			ConstructBindings.Send_FormDataFillerData_ApplyToPDF?.RemoveListener(OnApplyToPDF);
		}

		private void OnLoad(string stateName) {
			_stateName = stateName;
			_activeLayout = ResolveLayoutForState(stateName) ?? defaultLayout;

			if (_activeLayout == null) {
				Debug.LogError($"[FormDataFiller] No ImageFieldingLayoutAsset for state '{stateName}'. Inputs will be stored and applied when a layout is available.");
				_strings.Clear();
				_deadFirstName = _deadMiddleName = _deadLastName = string.Empty;
				_newFirstName = _newMiddleName = _newLastName = string.Empty;
				return;
			}

			_strings.Clear();
			_deadFirstName = _deadMiddleName = _deadLastName = string.Empty;
			_newFirstName = _newMiddleName = _newLastName = string.Empty;

			SetOverride("IsAdult", "Yes");
			SetOverride("FullDeadName", FullDeadName);
			SetOverride("DeadFirstName", _deadFirstName);
			SetOverride("DeadMiddleName", _deadMiddleName);
			SetOverride("DeadLastName", _deadLastName);
			SetOverride("FullNewName", FullNewName);
			SetOverride("NewFirstName", _newFirstName);
			SetOverride("NewMiddleName", _newMiddleName);
			SetOverride("NewLastName", _newLastName);

			if (useBackgroundSize)
				_activeLayout.ComputeDefaultSize(out outputWidthPixels, out outputHeightPixels);

			if (_pendingStrings.Count > 0) {
				foreach (var kv in _pendingStrings)
					_strings[kv.Key] = kv.Value;
				_pendingStrings.Clear();
			}

			if (updateAssetTextFieldsLive)
				PushStringsIntoLayoutAsset(_activeLayout, _strings);

			if (Application.isEditor && viewController != null && _activeLayout != null)
				viewController.RenderPreview(_activeLayout, BuildRenderDataForOne(_activeLayout), outputWidthPixels, outputHeightPixels);
		}

		private ImageFieldingAsset ResolveLayoutForState(string stateName) {
			if (!string.IsNullOrEmpty(stateName)) {
				for (int i = 0; i < stateLayouts.Count; i++) {
					var s = stateLayouts[i];
					if (!string.IsNullOrEmpty(s.stateName) &&
						string.Equals(s.stateName, stateName, StringComparison.OrdinalIgnoreCase) &&
						s.layout)
						return s.layout;
				}
			}

			if (!string.IsNullOrEmpty(stateName)) {
				var all = Resources.LoadAll<ImageFieldingAsset>($"States/{stateName}");
				if (all != null && all.Length > 0) {
					var set = new HashSet<ImageFieldingAsset>(all);
					var pointedTo = new HashSet<ImageFieldingAsset>();
					for (int i = 0; i < all.Length; i++) {
						var a = all[i];
						if (a != null && a.next != null && set.Contains(a.next))
							pointedTo.Add(a.next);
					}
					for (int i = 0; i < all.Length; i++) {
						var a = all[i];
						if (a != null && !pointedTo.Contains(a))
							return a;
					}
					return all[0];
				}
			}

			return defaultLayout;
		}

		private void OnSubmit(string keyword, string value) {
			if (string.IsNullOrEmpty(keyword))
				return;

			if (keyword.Contains("&")) {
				var parts = keyword.Split('&');
				for (int i = 0; i < parts.Length; i++)
					OnSubmit(parts[i].Trim(), value);
				return;
			}

			switch (keyword) {
			case "Dead Name Input": {
				string[] parsed = (value ?? string.Empty).Split('~');
				_deadFirstName = parsed.Length > 0 ? parsed[0] : string.Empty;
				_deadMiddleName = parsed.Length > 1 ? parsed[1] : string.Empty;
				_deadLastName = parsed.Length > 2 ? parsed[2] : string.Empty;

				SetOverride("FullDeadName", FullDeadName);
				SetOverride("DeadFirstName", _deadFirstName);
				SetOverride("DeadMiddleName", _deadMiddleName);
				SetOverride("DeadLastName", _deadLastName);
				break;
			}
			case "New Name Input": {
				string[] parsed = (value ?? string.Empty).Split('~');
				_newFirstName = parsed.Length > 0 ? parsed[0] : string.Empty;
				_newMiddleName = parsed.Length > 1 ? parsed[1] : string.Empty;
				_newLastName = parsed.Length > 2 ? parsed[2] : string.Empty;

				PlayerPrefs.SetString("NewFirstName", _newFirstName);

				SetOverride("FullNewName", FullNewName);
				SetOverride("NewFirstName", _newFirstName);
				SetOverride("NewMiddleName", _newMiddleName);
				SetOverride("NewLastName", _newLastName);
				break;
			}
			default:
				_strings[keyword] = value ?? string.Empty;
				if (_activeLayout == null)
					_pendingStrings[keyword] = value ?? string.Empty;
				break;
			}

			if (_activeLayout != null) {
				if (_pendingStrings.Count > 0) {
					foreach (var kv in _pendingStrings)
						_strings[kv.Key] = kv.Value;
					_pendingStrings.Clear();
				}
				if (updateAssetTextFieldsLive)
					PushStringsIntoLayoutAsset(_activeLayout, _strings);

				if (viewController != null)
					viewController.RenderPreview(_activeLayout, BuildRenderDataForOne(_activeLayout), outputWidthPixels, outputHeightPixels);
			}
		}

		private void SetOverride(string id, string val) {
			if (string.IsNullOrEmpty(id))
				return;
			_strings[id] = val ?? string.Empty;
			if (_activeLayout == null)
				_pendingStrings[id] = val ?? string.Empty;
		}

		private void OnApplyToPDF() {
			if (_activeLayout == null) {
				Debug.LogError("[FormDataFiller] ApplyToPDF called without an active layout.");
				return;
			}

			var chain = CollectChain(_activeLayout);
			if (chain.Count == 0) {
				Debug.LogError("[FormDataFiller] No layouts to render.");
				return;
			}

			int w = Mathf.Max(8, outputWidthPixels);
			int h = Mathf.Max(8, outputHeightPixels);
			if (useBackgroundSize && chain[0] != null) {
				chain[0].ComputeDefaultSize(out w, out h);
			}

			var merged = BuildRenderDataForChain(chain);
			var rdList = new System.Collections.Generic.List<ImageFielding.RenderData>(chain.Count);
			for (int i = 0; i < chain.Count; i++)
				rdList.Add(merged);

			string baseName = string.IsNullOrEmpty(_stateName) ? (chain[0] ? chain[0].name : "Layout") : _stateName;
			string dir = Application.persistentDataPath;
			System.IO.Directory.CreateDirectory(dir);
			string outPath = System.IO.Path.Combine(dir, $"Updated_{baseName}.pdf");

			StartCoroutine(GeneratePdf_Co(chain, rdList, w, h, outPath));
		}

		private System.Collections.IEnumerator GeneratePdf_Co(
			System.Collections.Generic.IList<ImageFieldingAsset> chain,
			System.Collections.Generic.IList<ImageFielding.RenderData> rdList,
			int w, int h,
			string outPath
		) {
			ConstructBindings.Send_ProgressBarData_ShowProgressBar?.Invoke(0, chain.Count);

			int lastShown = 0;
			System.Action<int, int, string> report = (cur, total, phase) => {
				int step = Mathf.Clamp(cur, 0, total);
				if (phase == "render")
					lastShown = step;
				ConstructBindings.Send_ProgressBarData_UpdateProgress?.Invoke(step);
			};

			yield return ImageFielding.AssetRenderer.RenderLayoutsToPdf_Co(
				chain, rdList, w, h, outPath, Mathf.Max(36f, pdfDpi), 8.5, 11.0, 0.25, report
			);

			byte[] pdfBytes = System.IO.File.ReadAllBytes(outPath);
			Debug.Log($"[FormDataFiller] Saved: {outPath}");

			ConstructBindings.Send_ProgressBarData_CloseProgressBar?.Invoke();

			ConstructBindings.Send_PDFViewerData_Load?.Invoke(pdfBytes);
		}


		private List<ImageFieldingAsset> CollectChain(ImageFieldingAsset start) {
			var list = new List<ImageFieldingAsset>();
			var seen = new HashSet<ImageFieldingAsset>();
			var cur = start;
			while (cur != null && seen.Add(cur)) {
				list.Add(cur);
				cur = cur.next;
			}
			return list;
		}

		private ImageFielding.RenderData BuildRenderDataForOne(ImageFieldingAsset layout) {
			var rd = new ImageFielding.RenderData {
				textFontSizePixels = 32,
				textColor = Color.black,
				textAlignment = TextAnchor.MiddleCenter,
				drawPlaceholderBoxesForMissingValues = false
			};

			if (layout == null)
				return rd;

			for (int i = 0; i < layout.fields.Count; i++) {
				var f = layout.fields[i];

				if (f.fieldType == ImageFieldingTypes.String) {
					if (!string.IsNullOrEmpty(f.ID)) {
						if (_strings.TryGetValue(f.ID, out var userVal) && !string.IsNullOrEmpty(userVal)) {
							rd.fieldStringValues[f.ID] = userVal;
						} else if (!string.IsNullOrEmpty(f.text)) {
							rd.fieldStringValues[f.ID] = f.text;
						}
					}
				} else if (f.fieldType == ImageFieldingTypes.Image) {
					if (!string.IsNullOrEmpty(f.ID) && f.image != null)
						rd.fieldImageValues[f.ID] = f.image;
				}
			}

			return rd;
		}

		private ImageFielding.RenderData BuildRenderDataForChain(List<ImageFieldingAsset> chain) {
			var rd = new ImageFielding.RenderData {
				textFontSizePixels = 32,
				textColor = Color.black,
				textAlignment = TextAnchor.MiddleCenter,
				drawPlaceholderBoxesForMissingValues = false
			};

			foreach (var layout in chain) {
				if (layout == null)
					continue;
				for (int i = 0; i < layout.fields.Count; i++) {
					var f = layout.fields[i];

					if (f.fieldType == ImageFieldingTypes.String) {
						if (string.IsNullOrEmpty(f.ID))
							continue;

						if (_strings.TryGetValue(f.ID, out var userVal) && !string.IsNullOrEmpty(userVal)) {
							rd.fieldStringValues[f.ID] = userVal;
						} else if (!string.IsNullOrEmpty(f.text)) {
							if (!rd.fieldStringValues.ContainsKey(f.ID))
								rd.fieldStringValues[f.ID] = f.text;
						}
					} else if (f.fieldType == ImageFieldingTypes.Image) {
						if (!string.IsNullOrEmpty(f.ID) && f.image != null) {
							if (!rd.fieldImageValues.ContainsKey(f.ID))
								rd.fieldImageValues[f.ID] = f.image;
						}
					}
				}
			}

			return rd;
		}

		private void PushStringsIntoLayoutAsset(ImageFieldingAsset layout, Dictionary<string, string> inputs) {
			if (layout == null || inputs == null)
				return;

			bool changed = false;

			for (int i = 0; i < layout.fields.Count; i++) {
				var f = layout.fields[i];
				if (f.fieldType != ImageFieldingTypes.String)
					continue;
				if (string.IsNullOrEmpty(f.ID))
					continue;

				if (inputs.TryGetValue(f.ID, out var v)) {
					if (!string.Equals(f.text, v, StringComparison.Ordinal)) {
						f.text = v;
						layout.fields[i] = f;
						changed = true;
					}
				}
			}

#if UNITY_EDITOR
			if (changed && autoSaveAssetInEditor) {
				EditorUtility.SetDirty(layout);
				AssetDatabase.SaveAssets();
			}
#endif
		}
	}
}
