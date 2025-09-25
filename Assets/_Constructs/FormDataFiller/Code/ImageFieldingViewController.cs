


namespace NameChangeSimulator.Runtime.ImageFieldingBridge {
	using System;
	using UnityEngine;
	using UnityEngine.UI;

#if UNITY_EDITOR
	using UnityEditor;
#endif

	using DW.Tools;

	public class ImageFieldingViewController : MonoBehaviour {
		[Header("Target")]
		public RawImage previewTarget;

		[Header("Render Defaults")]
		public ImageFieldingAsset defaultLayout;
		public bool useBackgroundSize = true;
		public int widthPixels = 2048;
		public int heightPixels = 2048;

		Texture2D _lastPreview;

		void OnDisable() {
			if (_lastPreview) {
				Destroy(_lastPreview);
				_lastPreview = null;
			}
		}

		public void RenderPreview(
			ImageFieldingAsset layout,
			ImageFielding.RenderData renderData,
			int w,
			int h) {
			if (!layout)
				layout = defaultLayout;
			if (!layout)
				return;

			if (useBackgroundSize)
				layout.ComputeDefaultSize(out w, out h);
			else {
				w = Mathf.Max(8, w > 0 ? w : widthPixels);
				h = Mathf.Max(8, h > 0 ? h : heightPixels);
			}

			var tex = ImageFielding.AssetRenderer.RenderToTexture(layout, renderData, w, h);

			if (_lastPreview)
				Destroy(_lastPreview);
			_lastPreview = tex;

			if (previewTarget) {
				previewTarget.texture = tex;
				previewTarget.SetNativeSize();
			}
		}

		public void SavePng(ImageFieldingAsset layout, ImageFielding.RenderData renderData, int w, int h) {
			if (!layout)
				layout = defaultLayout;
			if (!layout)
				return;

			if (useBackgroundSize)
				layout.ComputeDefaultSize(out w, out h);
			else {
				w = Mathf.Max(8, w > 0 ? w : widthPixels);
				h = Mathf.Max(8, h > 0 ? h : heightPixels);
			}

#if UNITY_EDITOR
			var path = EditorUtility.SaveFilePanel("Save PNG", Application.dataPath, $"{layout.name}_Output", "png");
			if (string.IsNullOrEmpty(path))
				return;
#else // TODO : Make sure this works.
            var path = System.IO.Path.Combine(Application.persistentDataPath, $"{layout.name}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
#endif
			try {
				ImageFielding.AssetRenderer.RenderToFile(layout, renderData, w, h, path, ImageFielding.AssetRenderer.FileFormat.Png);
				Debug.Log($"[ImageFieldingView] Saved PNG: {path}");
			}
			catch (Exception e) { Debug.LogException(e); }
		}

		public void SavePdf(ImageFieldingAsset layout, ImageFielding.RenderData renderData, int w, int h, float pdfDpi = 300f) {
			if (!layout)
				layout = defaultLayout;
			if (!layout)
				return;

			if (useBackgroundSize)
				layout.ComputeDefaultSize(out w, out h);
			else {
				w = Mathf.Max(8, w > 0 ? w : widthPixels);
				h = Mathf.Max(8, h > 0 ? h : heightPixels);
			}

#if UNITY_EDITOR
			var path = EditorUtility.SaveFilePanel("Save PDF", Application.dataPath, $"{layout.name}_Output", "pdf");
			if (string.IsNullOrEmpty(path))
				return;
#else // TODO : Make sure this works.
            var path = System.IO.Path.Combine(Application.persistentDataPath, $"{layout.name}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
#endif
			try {
				ImageFielding.AssetRenderer.RenderToFile(layout, renderData, w, h, path, ImageFielding.AssetRenderer.FileFormat.Pdf);
				Debug.Log($"[ImageFieldingView] Saved PDF: {path}");
			}
			catch (Exception e) { Debug.LogException(e); }
		}
	}
}
