

namespace DW.Tools {
	using System;
	using System.Collections.Generic;

	using UnityEngine;

	using static DW.Tools.ImageFielding;

	[CreateAssetMenu(fileName = "ImageFieldingLayout", menuName = "DW/Image Field Layout", order = 1000)]
	public class ImageFieldingAsset : ScriptableObject {
		public ImageFieldingAsset next;
		public TextAsset jsonExport;
		public Texture2D backgroundImage;
		public List<Field> fields = new List<Field>();

		public int ClearAllFieldValues(bool alsoImages = false, bool logPerField = false) {
			int clearedTextCount = 0;
			int clearedImagesCount = 0;

			for (int i = 0; i < fields.Count; i++) {
				var fieldEntry = fields[i];

				if (fieldEntry.fieldType == ImageFieldingTypes.String) {
					if (!string.IsNullOrEmpty(fieldEntry.text)) {
						fieldEntry.text = string.Empty;
						fields[i] = fieldEntry;
						clearedTextCount++;
					}
				} else if (fieldEntry.fieldType == ImageFieldingTypes.Image && alsoImages) {
					if (fieldEntry.image != null) {
						fieldEntry.image = null;
						fields[i] = fieldEntry;
						clearedImagesCount++;
					}
				}
			}

#if UNITY_EDITOR
			if ((clearedTextCount > 0 || clearedImagesCount > 0) && UnityEditor.EditorUtility.IsPersistent(this)) {
				UnityEditor.EditorUtility.SetDirty(this);
				UnityEditor.AssetDatabase.SaveAssets();
			}
#endif

			if (clearedTextCount + clearedImagesCount > 0) {
				Debug.Log("<color=cyan>[ImageFielding]</color><color=orange>[RESET]</color> Cleared text:"
					+ clearedTextCount + " image:" + clearedImagesCount + " in asset '" + name + "'");
			}

			return clearedTextCount + clearedImagesCount;
		}

		public void PrintToFile(
			RenderData renderData,
			int outputWidthPixels,
			int outputHeightPixels,
			string filePath,
			ImageFielding.AssetRenderer.FileFormat fileFormat = ImageFielding.AssetRenderer.FileFormat.Png
		) {
			ImageFielding.AssetRenderer.RenderToFile(
				this,
				renderData,
				outputWidthPixels,
				outputHeightPixels,
				filePath,
				fileFormat
			);
		}

		public List<ImageFieldingAsset> GetChainedLayouts(bool includeSelf = true, int maxHops = 128) {
			var result = new List<ImageFieldingAsset>();
			var seen = new HashSet<ImageFieldingAsset>();

			var current = includeSelf ? this : next;
			int hops = 0;

			while (current != null && hops++ < maxHops && seen.Add(current)) {
				result.Add(current);
				current = current.next;
			}

			return result;
		}

#if UNITY_EDITOR
		public void PrintPngViaDialog() {
			var filePath = UnityEditor.EditorUtility.SaveFilePanel("Save PNG", Application.dataPath, "output", "png");
			if (string.IsNullOrEmpty(filePath)) {
				return;
			}

			var renderData = CreateDefaultRenderDataWithPlaceholders();

			ComputeDefaultSize(out int width, out int height);

			PrintToFile(renderData, width, height, filePath, ImageFielding.AssetRenderer.FileFormat.Png);

			Debug.Log("<color=cyan>[ImageFielding]</color><color=green>[OUTPUT]</color> Saved PNG: " + filePath);
		}

		public void PrintChainedPdfViaDialog(float pdfDpi = 300f) {
			var filePath = UnityEditor.EditorUtility.SaveFilePanel("Save PDF", Application.dataPath, "output", "pdf");
			if (string.IsNullOrEmpty(filePath)) {
				return;
			}

			var layouts = GetChainedLayouts(includeSelf: true);
			if (layouts == null || layouts.Count == 0) {
				return;
			}

			var renderDataList = new List<ImageFielding.RenderData>(layouts.Count);
			for (int i = 0; i < layouts.Count; i++) {
				renderDataList.Add(layouts[i].CreateDefaultRenderDataWithPlaceholders());
			}

			layouts[0].ComputeDefaultSize(out int width, out int height);

			ImageFielding.AssetRenderer.RenderLayoutsToPdf(
				layouts,
				renderDataList,
				width,
				height,
				filePath,
				pdfDpi: pdfDpi
			);

			Debug.Log("<color=cyan>[ImageFielding]</color><color=green>[OUTPUT]</color> Saved chained PDF: " + filePath);
		}

		public void PrintPdfViaDialog(float pdfDpi = 300f) {
			var filePath = UnityEditor.EditorUtility.SaveFilePanel("Save PDF", Application.dataPath, "output", "pdf");
			if (string.IsNullOrEmpty(filePath)) {
				return;
			}

			var layouts = GetChainedLayouts(includeSelf: true);
			if (layouts == null || layouts.Count == 0) {
				layouts = new List<ImageFieldingAsset> { this };
			}

			var renderDataList = new List<ImageFielding.RenderData>(layouts.Count);
			for (int i = 0; i < layouts.Count; i++) {
				renderDataList.Add(layouts[i].CreateDefaultRenderDataWithPlaceholders());
			}

			layouts[0].ComputeDefaultSize(out int width, out int height);

			ImageFielding.AssetRenderer.RenderLayoutsToPdf(
				layouts,
				renderDataList,
				width,
				height,
				filePath,
				pdfDpi: pdfDpi
			);

			Debug.Log("<color=cyan>[ImageFielding]</color><color=green>[OUTPUT]</color> Saved PDF with "
				+ layouts.Count + " page(s): " + filePath);
		}

		private RenderData CreateDefaultRenderDataWithPlaceholders() {
			var renderData = new RenderData {
				textFontSizePixels = 32,
				textColor = Color.black,
				textAlignment = TextAnchor.MiddleCenter,
				drawPlaceholderBoxesForMissingValues = true,
				placeholderOutlineThicknessPixels = 2
			};

			for (int i = 0; i < fields.Count; i++) {
				var fieldEntry = fields[i];

				if (fieldEntry.fieldType == ImageFieldingTypes.String) {
					var value = string.IsNullOrEmpty(fieldEntry.text) ? fieldEntry.ID : fieldEntry.text;

					if (!string.IsNullOrEmpty(fieldEntry.ID)) {
						renderData.fieldStringValues[fieldEntry.ID] = value;
					}
				} else if (fieldEntry.fieldType == ImageFieldingTypes.Image) {
					if (!string.IsNullOrEmpty(fieldEntry.ID) && fieldEntry.image != null) {
						renderData.fieldImageValues[fieldEntry.ID] = fieldEntry.image;
					}
				}
			}

			return renderData;
		}

		public void ComputeDefaultSize(out int width, out int height) {
			if (backgroundImage != null) {
				width = backgroundImage.width;
				height = backgroundImage.height;
			} else {
				width = 2048;
				height = 2048;
			}
		}
#endif
	}

	public enum ImageFieldingTypes {
		String,
		Image
	}

	[Serializable]
	public struct Field {
		public string ID;
		public ImageFieldingTypes fieldType;
		public Rect normalizedRect;
		public string text;
		public Texture2D image;

		public Field(string name, ImageFieldingTypes type, Rect normalizedRect) {
			ID = name;
			fieldType = type;
			this.normalizedRect = normalizedRect;
			text = null;
			image = null;
		}
	}

	[Serializable]
	public class ImageFieldLayoutJsonDTO {
		public string backgroundGuid;
		public int backgroundWidth;
		public int backgroundHeight;
		public List<ImageFieldJsonDTO> fields = new List<ImageFieldJsonDTO>();
	}

	[Serializable]
	public class ImageFieldJsonDTO {
		public string fieldName;
		public string fieldType;
		public float x;
		public float y;
		public float width;
		public float height;
		public string text;
		public string imageAssetPath;
	}

	static class ImageFieldingAssetResetter {
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void ClearAllOnAppStart() {
			var allAssets = Resources.LoadAll<ImageFieldingAsset>(string.Empty);

			int totalAssets = allAssets?.Length ?? 0;
			int totalFieldsCleared = 0;

			if (allAssets != null) {
				for (int i = 0; i < allAssets.Length; i++) {
					var asset = allAssets[i];
					if (asset == null) {
						continue;
					}

					totalFieldsCleared += asset.ClearAllFieldValues(alsoImages: false, logPerField: false);
				}
			}

			Debug.Log("<color=cyan>[ImageFielding]</color><color=orange>[RESET]</color> Startup clear complete. assets="
				+ totalAssets + " fieldsCleared=" + totalFieldsCleared);
		}
	}

#if UNITY_EDITOR
	static class ImageFieldingEditorUtilities {
		[UnityEditor.MenuItem("Tools/DW/ImageFielding/Clear All Field Texts (Resources)")]
		static void ClearAllFieldTextsInResources() {
			var allAssets = Resources.LoadAll<ImageFieldingAsset>(string.Empty);

			int totalFieldsCleared = 0;

			for (int i = 0; i < allAssets.Length; i++) {
				var asset = allAssets[i];
				if (asset == null) {
					continue;
				}

				totalFieldsCleared += asset.ClearAllFieldValues(alsoImages: false, logPerField: false);
			}

			Debug.Log("<color=cyan>[ImageFielding]</color><color=orange>[RESET]</color> Editor clear complete. fieldsCleared="
				+ totalFieldsCleared);
		}
	}
#endif
}
