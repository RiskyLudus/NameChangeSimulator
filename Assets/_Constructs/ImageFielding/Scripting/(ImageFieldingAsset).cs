

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

		public void PrintToFile(
			RenderData renderData,
			int outputWidthPixels,
			int outputHeightPixels,
			string filePath,
			ImageFielding.AssetRenderer.FileFormat fileFormat = ImageFielding.AssetRenderer.FileFormat.Png) {

			ImageFielding.AssetRenderer.RenderToFile(
				this,
				renderData,
				outputWidthPixels,
				outputHeightPixels,
				filePath,
				fileFormat);
		}

		public List<ImageFieldingAsset> GetChainedLayouts(bool includeSelf = true, int maxHops = 128) {

			var list = new List<ImageFieldingAsset>();
			var seen = new HashSet<ImageFieldingAsset>();
			var cur = includeSelf ? this : next;
			int hops = 0;

			while (cur != null && hops++ < maxHops && seen.Add(cur)) {
				list.Add(cur);
				cur = cur.next;
			}

			return list;
		}


#if UNITY_EDITOR
		public void PrintPngViaDialog() {
			var path = UnityEditor.EditorUtility.SaveFilePanel("Save PNG", Application.dataPath, "output", "png");
			if (string.IsNullOrEmpty(path))
				return;

			var renderData = CreateDefaultRenderDataWithPlaceholders();
			ComputeDefaultSize(out int w, out int h);

			PrintToFile(renderData, w, h, path, ImageFielding.AssetRenderer.FileFormat.Png);
			Debug.Log("Saved PNG: " + path);
		}

		public void PrintChainedPdfViaDialog(float pdfDpi = 300f) {
			var path = UnityEditor.EditorUtility.SaveFilePanel("Save PDF", Application.dataPath, "output", "pdf");
			if (string.IsNullOrEmpty(path))
				return;

			var layouts = GetChainedLayouts(includeSelf: true);
			if (layouts == null || layouts.Count == 0)
				return;

			var rds = new List<ImageFielding.RenderData>(layouts.Count);
			foreach (var la in layouts)
				rds.Add(la.CreateDefaultRenderDataWithPlaceholders());

			layouts[0].ComputeDefaultSize(out int w, out int h);

			ImageFielding.AssetRenderer.RenderLayoutsToPdf(
				layouts,
				rds,
				w, h,
				path,
				pdfDpi: pdfDpi
			);

			Debug.Log("Saved chained PDF: " + path);
		}

		public void PrintPdfViaDialog(float pdfDpi = 300f) {
			var path = UnityEditor.EditorUtility.SaveFilePanel("Save PDF", Application.dataPath, "output", "pdf");
			if (string.IsNullOrEmpty(path))
				return;

			var layouts = GetChainedLayouts(includeSelf: true);
			if (layouts == null || layouts.Count == 0)
				layouts = new List<ImageFieldingAsset> { this };

			var renderDataList = new List<ImageFielding.RenderData>(layouts.Count);
			for (int i = 0; i < layouts.Count; i++)
				renderDataList.Add(layouts[i].CreateDefaultRenderDataWithPlaceholders());

			layouts[0].ComputeDefaultSize(out int w, out int h);

			ImageFielding.AssetRenderer.RenderLayoutsToPdf(
				layouts,
				renderDataList,
				w, h,
				path,
				pdfDpi: pdfDpi
			);

			Debug.Log($"Saved PDF with {layouts.Count} page(s): " + path);
		}


		private RenderData CreateDefaultRenderDataWithPlaceholders() {
			var rd = new RenderData {
				textFontSizePixels = 32,
				textColor = Color.black,
				textAlignment = TextAnchor.MiddleCenter,
				drawPlaceholderBoxesForMissingValues = true,
				placeholderOutlineThicknessPixels = 2
			};

			for (int i = 0; i < fields.Count; i++) {
				var f = fields[i];
				if (f.fieldType == ImageFieldingTypes.String) {
					var val = string.IsNullOrEmpty(f.text) ? f.ID : f.text;
					if (!string.IsNullOrEmpty(f.ID))
						rd.fieldStringValues[f.ID] = val;
				} else if (f.fieldType == ImageFieldingTypes.Image) {
					if (!string.IsNullOrEmpty(f.ID) && f.image != null)
						rd.fieldImageValues[f.ID] = f.image;
				}
			}

			return rd;
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
}
