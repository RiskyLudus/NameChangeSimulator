using System;
using System.Collections.Generic;
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

		private ImageFieldingAsset activeLayout;
		private string stateName;

		private string deadFirstName;
		private string deadMiddleName;
		private string deadLastName;

		private string newFirstName;
		private string newMiddleName;
		private string newLastName;

		private readonly Dictionary<string, string> stringsById =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private readonly Dictionary<string, string> pendingStringsById =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private string FullDeadName => $"{deadFirstName} {deadMiddleName} {deadLastName}".Trim();
		private string FullNewName => $"{newFirstName} {newMiddleName} {newLastName}".Trim();

		private const string TagFiller = "<color=orange>[FormDataFiller]</color>";
		private const string TagOnLoad = "<color=cyan>[OnLoad]</color>";
		private const string TagSubmit = "<color=cyan>[OnSubmit]</color>";
		private const string TagOther = "<color=magenta>[Other]</color>";
		private const string TagAlias = "<color=yellow>[Alias]</color>";
		private const string TagOutput = "<color=green>[Output]</color>";
		private const string TagError = "<color=red>[Error]</color>";

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

		private void OnLoad(string requestedStateName) {
			stateName = requestedStateName;
			activeLayout = ResolveLayoutForState(requestedStateName) ?? defaultLayout;

			if (activeLayout == null) {
				Debug.LogError($"{TagFiller}{TagOnLoad}{TagError} No ImageFieldingLayoutAsset for state='{requestedStateName}'. Inputs will be stored and applied when a layout is available.");
				stringsById.Clear();
				deadFirstName = deadMiddleName = deadLastName = string.Empty;
				newFirstName = newMiddleName = newLastName = string.Empty;
				return;
			}

			Debug.Log($"{TagFiller}{TagOnLoad} state='{stateName}' layout='{activeLayout.name}' fields={activeLayout.fields.Count}");

			stringsById.Clear();
			deadFirstName = deadMiddleName = deadLastName = string.Empty;
			newFirstName = newMiddleName = newLastName = string.Empty;

			SetOverride("IsAdult", "Yes");
			SetOverride("FullDeadName", FullDeadName);
			SetOverride("DeadFirstName", deadFirstName);
			SetOverride("DeadMiddleName", deadMiddleName);
			SetOverride("DeadLastName", deadLastName);
			SetOverride("FullNewName", FullNewName);
			SetOverride("NewFirstName", newFirstName);
			SetOverride("NewMiddleName", newMiddleName);
			SetOverride("NewLastName", newLastName);

			if (useBackgroundSize)
				activeLayout.ComputeDefaultSize(out outputWidthPixels, out outputHeightPixels);

			if (pendingStringsById.Count > 0) {
				foreach (var pair in pendingStringsById)
					stringsById[pair.Key] = pair.Value;
				pendingStringsById.Clear();
				Debug.Log($"{TagFiller}{TagOnLoad} mergedPending count={stringsById.Count}");
			}

			void CopyIfPresent(string from, string to) {
				if (stringsById.TryGetValue(from, out var value) && !string.IsNullOrEmpty(value)) {
					if (!stringsById.TryGetValue(to, out var current) || current != value) {
						stringsById[to] = value;
						Debug.Log($"{TagFiller}{TagOnLoad}{TagAlias} '{from}' → '{to}' = '{value}'");
					}
				}
			}
			CopyIfPresent("Name_First_Dead", "DeadFirstName");
			CopyIfPresent("Name_Middle_Dead", "DeadMiddleName");
			CopyIfPresent("Name_Last_Dead", "DeadLastName");
			CopyIfPresent("Name_First_New", "NewFirstName");
			CopyIfPresent("Name_Middle_New", "NewMiddleName");
			CopyIfPresent("Name_Last_New", "NewLastName");

			deadFirstName = stringsById.TryGetValue("DeadFirstName", out var dfn) ? dfn : (stringsById.TryGetValue("Name_First_Dead", out var dfn2) ? dfn2 : "");
			deadMiddleName = stringsById.TryGetValue("DeadMiddleName", out var dmn) ? dmn : (stringsById.TryGetValue("Name_Middle_Dead", out var dmn2) ? dmn2 : "");
			deadLastName = stringsById.TryGetValue("DeadLastName", out var dln) ? dln : (stringsById.TryGetValue("Name_Last_Dead", out var dln2) ? dln2 : "");
			newFirstName = stringsById.TryGetValue("NewFirstName", out var nfn) ? nfn : (stringsById.TryGetValue("Name_First_New", out var nfn2) ? nfn2 : "");
			newMiddleName = stringsById.TryGetValue("NewMiddleName", out var nmn) ? nmn : (stringsById.TryGetValue("Name_Middle_New", out var nmn2) ? nmn2 : "");
			newLastName = stringsById.TryGetValue("NewLastName", out var nln) ? nln : (stringsById.TryGetValue("Name_Last_New", out var nln2) ? nln2 : "");

			stringsById["FullDeadName"] = FullDeadName;
			stringsById["FullNewName"] = FullNewName;

			Debug.Log($"{TagFiller}{TagOnLoad} Dead='{FullDeadName}' New='{FullNewName}' totalKeys={stringsById.Count}");

			if (updateAssetTextFieldsLive)
				PushStringsIntoLayoutAsset(activeLayout, stringsById);

			if (Application.isEditor && viewController != null && activeLayout != null)
				viewController.RenderPreview(activeLayout, BuildRenderDataForOne(activeLayout), outputWidthPixels, outputHeightPixels);
		}

		private ImageFieldingAsset ResolveLayoutForState(string targetStateName) {
			if (!string.IsNullOrEmpty(targetStateName)) {
				for (int i = 0; i < stateLayouts.Count; i++) {
					var binding = stateLayouts[i];
					if (!string.IsNullOrEmpty(binding.stateName) &&
						string.Equals(binding.stateName, targetStateName, StringComparison.OrdinalIgnoreCase) &&
						binding.layout)
						return binding.layout;
				}
			}

			if (!string.IsNullOrEmpty(targetStateName)) {
				var allAssets = Resources.LoadAll<ImageFieldingAsset>($"States/{targetStateName}");
				if (allAssets != null && allAssets.Length > 0) {
					var allSet = new HashSet<ImageFieldingAsset>(allAssets);
					var pointedTo = new HashSet<ImageFieldingAsset>();
					for (int i = 0; i < allAssets.Length; i++) {
						var asset = allAssets[i];
						if (asset != null && asset.next != null && allSet.Contains(asset.next))
							pointedTo.Add(asset.next);
					}
					for (int i = 0; i < allAssets.Length; i++) {
						var asset = allAssets[i];
						if (asset != null && !pointedTo.Contains(asset))
							return asset;
					}
					return allAssets[0];
				}
			}

			return defaultLayout;
		}

		private void OnSubmit(string keyword, string value) {
			string rawKey = keyword ?? "";
			string rawValue = value ?? "";

			if (string.IsNullOrWhiteSpace(rawKey))
				return;

			if (rawKey.Contains("&")) {
				var parts = rawKey.Split('&');
				for (int i = 0; i < parts.Length; i++)
					OnSubmit(parts[i].Trim(), rawValue);
				return;
			}

			string key = rawKey.Trim();
			string keyLower = key.ToLowerInvariant();

			switch (keyLower) {
			case "dead name input": {
				string[] parsed = rawValue.Split('~');
				deadFirstName = parsed.Length > 0 ? parsed[0] : string.Empty;
				deadMiddleName = parsed.Length > 1 ? parsed[1] : string.Empty;
				deadLastName = parsed.Length > 2 ? parsed[2] : string.Empty;

				SetOverride("Name_First_Dead", deadFirstName);
				SetOverride("Name_Middle_Dead", deadMiddleName);
				SetOverride("Name_Last_Dead", deadLastName);

				SetOverride("DeadFirstName", deadFirstName);
				SetOverride("DeadMiddleName", deadMiddleName);
				SetOverride("DeadLastName", deadLastName);

				SetOverride("FullDeadName", FullDeadName);

				Debug.Log($"{TagFiller}{TagSubmit} parsedDead F='{deadFirstName}' M='{deadMiddleName}' L='{deadLastName}' Full='{FullDeadName}'");
				break;
			}

			case "new name input": {
				string[] parsed = rawValue.Split('~');
				newFirstName = parsed.Length > 0 ? parsed[0] : string.Empty;
				newMiddleName = parsed.Length > 1 ? parsed[1] : string.Empty;
				newLastName = parsed.Length > 2 ? parsed[2] : string.Empty;

				PlayerPrefs.SetString("NewFirstName", newFirstName);

				SetOverride("Name_First_New", newFirstName);
				SetOverride("Name_Middle_New", newMiddleName);
				SetOverride("Name_Last_New", newLastName);

				SetOverride("NewFirstName", newFirstName);
				SetOverride("NewMiddleName", newMiddleName);
				SetOverride("NewLastName", newLastName);

				SetOverride("FullNewName", FullNewName);

				Debug.Log($"{TagFiller}{TagSubmit} parsedNew  F='{newFirstName}' M='{newMiddleName}' L='{newLastName}' Full='{FullNewName}'");
				break;
			}

			default:
				stringsById[key] = rawValue;
				if (activeLayout == null)
					pendingStringsById[key] = rawValue;
				Debug.Log($"{TagFiller}{TagSubmit}{TagOther} '{key}' = '{rawValue}' (activeLayout={(activeLayout ? activeLayout.name : "null")})");
				break;
			}

			if (activeLayout != null) {
				if (pendingStringsById.Count > 0) {
					foreach (var pair in pendingStringsById)
						stringsById[pair.Key] = pair.Value;
					pendingStringsById.Clear();
				}

				if (updateAssetTextFieldsLive)
					PushStringsIntoLayoutAsset(activeLayout, stringsById);

				if (viewController != null)
					viewController.RenderPreview(activeLayout, BuildRenderDataForOne(activeLayout), outputWidthPixels, outputHeightPixels);
			}

			Debug.Log($"{TagFiller}{TagSubmit} totals strings={stringsById.Count} pending={pendingStringsById.Count}");
		}

		private void SetOverride(string id, string value) {
			if (string.IsNullOrEmpty(id))
				return;

			string finalValue = value ?? string.Empty;
			stringsById[id] = finalValue;

			if (activeLayout == null)
				pendingStringsById[id] = finalValue;
		}

		private void OnApplyToPDF() {
			if (activeLayout == null) {
				Debug.LogError($"{TagFiller}{TagOutput}{TagError} ApplyToPDF called without an active layout.");
				return;
			}

			var chain = CollectChain(activeLayout);
			if (chain.Count == 0) {
				Debug.LogError($"{TagFiller}{TagOutput}{TagError} No layouts to render.");
				return;
			}

			int width = Mathf.Max(8, outputWidthPixels);
			int height = Mathf.Max(8, outputHeightPixels);
			if (useBackgroundSize && chain[0] != null)
				chain[0].ComputeDefaultSize(out width, out height);

			var mergedRenderData = BuildRenderDataForChain(chain);
			var renderDataList = new List<ImageFielding.RenderData>(chain.Count);
			for (int i = 0; i < chain.Count; i++)
				renderDataList.Add(mergedRenderData);

			string baseName = string.IsNullOrEmpty(stateName) ? (chain[0] ? chain[0].name : "Layout") : stateName;
			string directoryPath = Application.persistentDataPath;
			System.IO.Directory.CreateDirectory(directoryPath);
			string outputPath = System.IO.Path.Combine(directoryPath, $"Updated_{baseName}.pdf");

			StartCoroutine(GeneratePdf_Co(chain, renderDataList, width, height, outputPath));
		}

		private System.Collections.IEnumerator GeneratePdf_Co(
			IList<ImageFieldingAsset> chain,
			IList<ImageFielding.RenderData> renderDataList,
			int width,
			int height,
			string outputPath
		) {
			ConstructBindings.Send_ProgressBarData_ShowProgressBar?.Invoke(0, chain.Count);

			int lastShown = 0;
			Action<int, int, string> report = (current, total, phase) => {
				int step = Mathf.Clamp(current, 0, total);
				if (phase == "render")
					lastShown = step;
				ConstructBindings.Send_ProgressBarData_UpdateProgress?.Invoke(step);
			};

			yield return ImageFielding.AssetRenderer.RenderLayoutsToPdf_Co(
				chain, renderDataList, width, height, outputPath, Mathf.Max(36f, pdfDpi), 8.5, 11.0, 0.25, report
			);

			byte[] pdfBytes = System.IO.File.ReadAllBytes(outputPath);
			Debug.Log($"{TagFiller}{TagOutput} Saved PDF: {outputPath}");

			ConstructBindings.Send_ProgressBarData_CloseProgressBar?.Invoke();
			ConstructBindings.Send_PDFViewerData_Load?.Invoke(pdfBytes);
		}

		private List<ImageFieldingAsset> CollectChain(ImageFieldingAsset start) {
			var list = new List<ImageFieldingAsset>();
			var seen = new HashSet<ImageFieldingAsset>();
			var current = start;

			while (current != null && seen.Add(current)) {
				list.Add(current);
				current = current.next;
			}

			return list;
		}

		private ImageFielding.RenderData BuildRenderDataForOne(ImageFieldingAsset layout) {
			var renderData = new ImageFielding.RenderData {
				textFontSizePixels = 32,
				textColor = Color.black,
				textAlignment = TextAnchor.MiddleCenter,
				drawPlaceholderBoxesForMissingValues = false
			};

			if (layout == null)
				return renderData;

			for (int i = 0; i < layout.fields.Count; i++) {
				var fieldEntry = layout.fields[i];

				if (fieldEntry.fieldType == ImageFieldingTypes.String) {
					if (!string.IsNullOrEmpty(fieldEntry.ID)) {
						if (stringsById.TryGetValue(fieldEntry.ID, out var userValue) && !string.IsNullOrEmpty(userValue)) {
							renderData.fieldStringValues[fieldEntry.ID] = userValue;
						} else if (!string.IsNullOrEmpty(fieldEntry.text)) {
							renderData.fieldStringValues[fieldEntry.ID] = fieldEntry.text;
						}
					}
				} else if (fieldEntry.fieldType == ImageFieldingTypes.Image) {
					if (!string.IsNullOrEmpty(fieldEntry.ID) && fieldEntry.image != null)
						renderData.fieldImageValues[fieldEntry.ID] = fieldEntry.image;
				}
			}

			return renderData;
		}

		private ImageFielding.RenderData BuildRenderDataForChain(List<ImageFieldingAsset> chain) {
			var renderData = new ImageFielding.RenderData {
				textFontSizePixels = 32,
				textColor = Color.black,
				textAlignment = TextAnchor.MiddleCenter,
				drawPlaceholderBoxesForMissingValues = false
			};

			for (int i = 0; i < chain.Count; i++) {
				var layout = chain[i];
				if (layout == null)
					continue;

				for (int j = 0; j < layout.fields.Count; j++) {
					var fieldEntry = layout.fields[j];

					if (fieldEntry.fieldType == ImageFieldingTypes.String) {
						if (string.IsNullOrEmpty(fieldEntry.ID))
							continue;

						if (stringsById.TryGetValue(fieldEntry.ID, out var userValue) && !string.IsNullOrEmpty(userValue)) {
							renderData.fieldStringValues[fieldEntry.ID] = userValue;
						} else if (!string.IsNullOrEmpty(fieldEntry.text)) {
							if (!renderData.fieldStringValues.ContainsKey(fieldEntry.ID))
								renderData.fieldStringValues[fieldEntry.ID] = fieldEntry.text;
						}
					} else if (fieldEntry.fieldType == ImageFieldingTypes.Image) {
						if (!string.IsNullOrEmpty(fieldEntry.ID) && fieldEntry.image != null) {
							if (!renderData.fieldImageValues.ContainsKey(fieldEntry.ID))
								renderData.fieldImageValues[fieldEntry.ID] = fieldEntry.image;
						}
					}
				}
			}

			return renderData;
		}

		private void PushStringsIntoLayoutAsset(ImageFieldingAsset layout, Dictionary<string, string> inputs) {
			if (layout == null || inputs == null)
				return;

			bool changed = false;

			for (int i = 0; i < layout.fields.Count; i++) {
				var fieldEntry = layout.fields[i];
				if (fieldEntry.fieldType != ImageFieldingTypes.String)
					continue;
				if (string.IsNullOrEmpty(fieldEntry.ID))
					continue;

				if (inputs.TryGetValue(fieldEntry.ID, out var value)) {
					if (!string.Equals(fieldEntry.text, value, StringComparison.Ordinal)) {
						fieldEntry.text = value;
						layout.fields[i] = fieldEntry;
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
