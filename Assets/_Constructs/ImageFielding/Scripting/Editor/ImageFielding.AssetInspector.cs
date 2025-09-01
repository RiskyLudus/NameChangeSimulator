

namespace DW.Tools {
	using System.Collections.Generic;

	using UnityEditor;

	using UnityEngine;

	[CustomEditor(typeof(ImageFieldingAsset))]
	public class ImageFieldingAsset_Inspector : UnityEditor.Editor {
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

		// If you see this commented code, and the project isn't exploding. DELETE
/*
		private static void CreateDialogFlowForImageFieldingAsset(DW.ImageFieldingAsset asset) {
			string fileName = string.IsNullOrEmpty(asset.name) ? "Dialogue" : asset.name + "_Dialogue";
			string assetFolder = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(asset));

			if (string.IsNullOrEmpty(assetFolder))
				assetFolder = "Assets";

			string path = EditorUtility.SaveFilePanelInProject(
				"Save Dialogue Graph",
				fileName,
				"asset",
				"Select where to save the Dialogue Graph asset.",
				assetFolder
			);

			if (string.IsNullOrEmpty(path))
				return;

			var graph = ScriptableObject.CreateInstance("DialogueGraph");
			AssetDatabase.CreateAsset(graph, path);

			// Create Start node
			var startNode = ScriptableObject.CreateInstance("StartNode");
			SetPropertyOrField(startNode, "name", "StartNode");
			SetPropertyOrField(startNode, "graph", graph);
			AssetDatabase.AddObjectToAsset(startNode, graph);
			AddNodeToGraph(graph, startNode);

			// Make nodes from String fields (skip images)
			// TODO : Figure out how to handle image flow.
			var dialogNodes = new System.Collections.Generic.List<ScriptableObject>();

			foreach (Field field in asset.fields) {
				if (field.fieldType != DW.ImageFieldingTypes.String)
					continue;

				var node = ScriptableObject.CreateInstance("InputNode");

				if (!string.IsNullOrEmpty(field.ID))
					SetPropertyOrField(node, "Keyword", field.ID);

				// Dialogue text: DialogSO.Dialog if we find a matching Keyword, else fieldName or text
				string dialogText = null;
				if (!string.IsNullOrEmpty(field.ID))
					TryGetDialogText(field.ID, out dialogText);

				if (string.IsNullOrEmpty(dialogText))
					dialogText = string.IsNullOrEmpty(field.text) ? field.ID : field.text;

				SetPropertyOrField(node, "DialogueText", dialogText);

				// Attach to graph
				SetPropertyOrField(node, "name", string.IsNullOrEmpty(field.ID) ? "Input" : field.ID);
				SetPropertyOrField(node, "graph", graph);

				AssetDatabase.AddObjectToAsset(node, graph);
				AddNodeToGraph(graph, node);

				dialogNodes.Add(node);
			}

			var endNode = ScriptableObject.CreateInstance("EndNode");
			SetPropertyOrField(endNode, "name", "EndNode");
			SetPropertyOrField(endNode, "graph", graph);
			AssetDatabase.AddObjectToAsset(endNode, graph);
			AddNodeToGraph(graph, endNode);

			// Layout & chain: start -> each node -> end
			var allChain = new System.Collections.Generic.List<ScriptableObject>();
			allChain.Add(startNode);
			allChain.AddRange(dialogNodes);
			allChain.Add(endNode);

			Vector2 pos = Vector2.zero;
			for (int i = 0; i < allChain.Count; i++) {
				var node = allChain[i];

				// Stagger positions in a grid-ish pattern
				pos = new Vector2(pos.x + 500f, pos.y);
				if (i % 4 == 0)
					pos = new Vector2(pos.x - 2000f, pos.y + 600f);
				SetPropertyOrField(node, "position", pos);

				if (i >= allChain.Count - 1)
					continue;
				
				var next = allChain[i + 1];
				TryConnectNodes(node, "Output", next, "Input");
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorGUIUtility.PingObject(graph);
			Debug.Log($"Created Dialogue Graph with {dialogNodes.Count} node(s): {path}");
		}

		private static void SetPropertyOrField(object obj, string name, object value) {
			if (obj == null || string.IsNullOrEmpty(name))
				return;
			var type = obj.GetType();

			var prprty = type.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			if (prprty != null && prprty.CanWrite) {
				try { prprty.SetValue(obj, value, null); return; }
				catch { }
			}

			var fld = type.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (fld != null) {
				try { fld.SetValue(obj, value); return; }
				catch { }
			}
		}

		private static void AddNodeToGraph(object graph, object node) {
			if (graph == null || node == null)
				return;
			var grphType = graph.GetType();

			var nodeFld = grphType.GetField("nodes");
			if (nodeFld != null) {
				var list = nodeFld.GetValue(graph) as System.Collections.IList;
				if (list != null && !list.Contains(node))
					list.Add(node);
				return;
			}

			var nodePrprty = grphType.GetProperty("nodes");
			if (nodePrprty != null && nodePrprty.CanRead) {
				var list = nodePrprty.GetValue(graph, null) as System.Collections.IList;
				if (list != null && !list.Contains(node))
					list.Add(node);
			}
		}

		private static bool TryConnectNodes(object fromNode, string fromPortName, object toNode, string toPortName) {
			if (fromNode == null || toNode == null)
				return false;

			var frmNodeType = fromNode.GetType();

			MethodInfo fromPortMthd = frmNodeType.GetMethod("GetOutputPort");
			MethodInfo toPortMthd = toNode.GetType().GetMethod("GetInputPort");
			if (fromPortMthd == null || toPortMthd == null)
				return false;

			object fromPort = null, toPort = null;
			try { fromPort = fromPortMthd.Invoke(fromNode, new object[] { fromPortName }); }
			catch { }
			try { toPort = toPortMthd.Invoke(toNode, new object[] { toPortName }); }
			catch { }

			if (fromPort == null || toPort == null)
				return false;

			var portType = fromPort.GetType();
			var connect = portType.GetMethod("Connect");
			if (connect == null)
				return false;

			try { connect.Invoke(fromPort, new object[] { toPort }); return true; }
			catch { return false; }
		}

		private static bool TryGetDialogText(string keyword, out string dialogText) {
			dialogText = null;
			if (string.IsNullOrEmpty(keyword))
				return false;

			// If dialogSO matches selected
			// TODO : Maybe implement the blackboard here
			var dlgSOs = Resources.LoadAll("Dialogs", typeof(ScriptableObject));
			foreach (Object dlgSO in dlgSOs) {
				if (TryReadKeywordAndDialog(dlgSO, out var key, out var dlg)) {
					if (string.Equals(key, keyword, System.StringComparison.OrdinalIgnoreCase)) {
						dialogText = dlg;
						return true;
					}
				}
			}

			// Editor-wide search fallback
			var guids = AssetDatabase.FindAssets("t:ScriptableObject");
			for (int i = 0; i < guids.Length; i++) {
				var path = AssetDatabase.GUIDToAssetPath(guids[i]);
				var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
				if (!so)
					continue;
				if (TryReadKeywordAndDialog(so, out var k, out var d)) {
					if (string.Equals(k, keyword, System.StringComparison.OrdinalIgnoreCase)) {
						dialogText = d;
						return true;
					}
				}
			}

			return false;
		}

		// TODO : Maybe just have a cast so we don't have to use reflection?
		private static bool TryReadKeywordAndDialog(Object obj, out string keyword, out string dialog) {
			keyword = null;
			dialog = null;

			if (!obj)
				return false;

			var t = obj.GetType();

			System.Reflection.FieldInfo keyFld = t.GetField("Keyword");
			if (keyFld != null && keyFld.FieldType == typeof(string))
				keyword = (string)keyFld.GetValue(obj);
			else {
				var keyPrpt = t.GetProperty("Keyword");
				if (keyPrpt != null && keyPrpt.PropertyType == typeof(string))
					keyword = (string)keyPrpt.GetValue(obj, null);
			}

			System.Reflection.FieldInfo dialogFld = t.GetField("Dialog");
			if (dialogFld != null && dialogFld.FieldType == typeof(string))
				dialog = (string)dialogFld.GetValue(obj);
			else {
				var dialogPrpt = t.GetProperty("Dialog");
				if (dialogPrpt != null && dialogPrpt.PropertyType == typeof(string))
					dialog = (string)dialogPrpt.GetValue(obj, null);
			}

			return !string.IsNullOrEmpty(keyword) && !string.IsNullOrEmpty(dialog);
		}*/

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