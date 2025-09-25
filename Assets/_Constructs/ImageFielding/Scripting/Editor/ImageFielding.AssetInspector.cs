

namespace DW.Tools {
	using System.Collections.Generic;

	using UnityEditor;

	using UnityEngine;

	[CustomEditor(typeof(ImageFieldingAsset))]
	public class ImageFieldingAsset_Inspector : Editor {
		const string PrefkeyWidth = "DW.ImgFldg.xprtWdth";
		const string PrefkeyHeight = "DW.ImgFldg.xprtHght";
		const string PrefkeyPdfDpi = "DW.ImgFldg.PdfDpi";

		int _xprtWdth, _xprtHght;
		float _pdfDpi;

		void OnEnable() {
			var asset = (ImageFieldingAsset)target;
			int bckgrndWdth = asset.backgroundImage ? asset.backgroundImage.width : 2048;
			int bckgrndHght = asset.backgroundImage ? asset.backgroundImage.height : 2048;

			_xprtWdth = EditorPrefs.GetInt(PrefkeyWidth, bckgrndWdth);
			_xprtHght = EditorPrefs.GetInt(PrefkeyHeight, bckgrndHght);
			_pdfDpi = Mathf.Max(1f, EditorPrefs.GetFloat(PrefkeyPdfDpi, 300f));
		}

		public override void OnInspectorGUI() {
			var asset = (ImageFieldingAsset)target;

			if (GUILayout.Button("Open Image Fielding Editor", GUILayout.Height(24)))
				ImageFielding.Editor.Window.OpenWith(asset);

			base.OnInspectorGUI();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);

			using (new EditorGUILayout.HorizontalScope()) {
				_xprtWdth = EditorGUILayout.IntField(new GUIContent("Width (px)"), Mathf.Max(8, _xprtWdth));
				_xprtHght = EditorGUILayout.IntField(new GUIContent("Height (px)"), Mathf.Max(8, _xprtHght));
			}

			using (new EditorGUILayout.HorizontalScope()) {
				if (GUILayout.Button("Use Background Size")) {
					if (asset.backgroundImage) {
						_xprtWdth = asset.backgroundImage.width;
						_xprtHght = asset.backgroundImage.height;
					}
				}
				if (GUILayout.Button("Swap WH")) { (_xprtWdth, _xprtHght) = (_xprtHght, _xprtWdth); }
			}

			_pdfDpi = EditorGUILayout.FloatField(new GUIContent("PDF DPI"), Mathf.Max(36f, _pdfDpi));

			EditorGUILayout.Space();
			using (new EditorGUILayout.HorizontalScope()) {
				if (GUILayout.Button("Print PNG...", GUILayout.Height(26)))
					EditorApplication.delayCall += () => DoPrint(asset, ImageFielding.AssetRenderer.FileFormat.Png);

				if (GUILayout.Button("Print PDF...", GUILayout.Height(26)))
					EditorApplication.delayCall += () => DoPrint(asset, ImageFielding.AssetRenderer.FileFormat.Pdf);
			}

			EditorGUILayout.Space();
			if (GUI.changed) {
				EditorPrefs.SetInt(PrefkeyWidth, _xprtWdth);
				EditorPrefs.SetInt(PrefkeyHeight, _xprtHght);
				EditorPrefs.SetFloat(PrefkeyPdfDpi, _pdfDpi);
			}
		}

		void DoPrint(ImageFieldingAsset asset, ImageFielding.AssetRenderer.FileFormat frmt) {
			string extention = frmt == ImageFielding.AssetRenderer.FileFormat.Png ? "png" : "pdf";

			string suggested = asset.backgroundImage ? asset.backgroundImage.name : "output";
			string path = EditorUtility.SaveFilePanel($"Save {extention.ToUpper()}", Application.dataPath, suggested, extention);
			if (string.IsNullOrEmpty(path))
				return;

			if (frmt == ImageFielding.AssetRenderer.FileFormat.Pdf) {
				var layouts = asset.GetChainedLayouts(true);
				if (layouts == null || layouts.Count == 0)
					return;

				var renderData = new List<ImageFielding.RenderData>(layouts.Count);
				for (int i = 0; i < layouts.Count; i++)
					renderData.Add(BuildDefaultRenderData(layouts[i]));

				layouts[0].ComputeDefaultSize(out int w, out int h);
				ImageFielding.AssetRenderer.RenderLayoutsToPdf(layouts, renderData, w, h, path, _pdfDpi, 8.5, 11.0, 0.0);

			} else {
				var rd = BuildDefaultRenderData(asset);
				ImageFielding.AssetRenderer.RenderToFile(asset, rd, _xprtWdth, _xprtHght, path, frmt);
			}
		}

		static ImageFielding.RenderData BuildDefaultRenderData(ImageFieldingAsset asset) {
			var renderData = new ImageFielding.RenderData {
				textFontSizePixels = 32,
				textColor = Color.black,
				textAlignment = TextAnchor.MiddleCenter,
				drawPlaceholderBoxesForMissingValues = false,
				placeholderOutlineThicknessPixels = 0
			};

			foreach (var fld in asset.fields) {
				if (fld.fieldType == ImageFieldingTypes.String && !string.IsNullOrEmpty(fld.ID)) {
					var val = !string.IsNullOrEmpty(fld.text) ? fld.text : fld.ID;

					renderData.fieldStringValues[fld.ID] = val;

				} else if (fld.fieldType == ImageFieldingTypes.Image && !string.IsNullOrEmpty(fld.ID) && fld.image) {
					renderData.fieldImageValues[fld.ID] = fld.image;
				}
			}

			return renderData;
		}
	}
}