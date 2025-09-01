

namespace DW.Tools {
	using System;
	using System.Collections.Generic;
	using System.IO;

	using UnityEditor;

	using UnityEngine;

	using DW.Tools;

	public partial class ImageFielding {
		public partial class Editor {
			public static class State {
				public static Texture2D Background;
				public static List<FieldViewModle> Fields = new();
				public static int SelectedIndex = -1;

				public static ImageFieldingAsset ActiveAsset;

				public static DefaultAsset JsonFolderAsset;
				public static string JsonFileName = "ImageFielding.json";

				public static event Action StateChanged;

				public static void BindFromAsset(ImageFieldingAsset asset) {
					ActiveAsset = asset;
					if (asset == null) { MarkDirty(); return; }

					Background = asset.backgroundImage;
					Fields = FromAssetToVM(asset);
					SelectedIndex = Fields.Count > 0 ? 0 : -1;
					MarkDirty();
				}

				public static void SaveToAsset(ImageFieldingAsset asset) {
					if (asset == null)
						return;

					asset.backgroundImage = Background;
					asset.fields.Clear();

					for (int i = 0; i < Fields.Count; i++) {
						var vm = Fields[i];
						var topLeft = vm.topLeft;
						var btmRight = vm.bottomRight;
						ClampAndOrder(ref topLeft, ref btmRight);

						var rect = Rect.MinMaxRect(topLeft.x, topLeft.y, btmRight.x, btmRight.y);
						var fld = new Field(vm.id ?? string.Empty, vm.type, rect);
						fld.text = vm.text;
						fld.image = vm.image;
						asset.fields.Add(fld);
					}

					EditorUtility.SetDirty(asset);
					AssetDatabase.SaveAssets();
				}

				public static void WriteJsonToAbsolutePath(string absolutePath) {
					var dto = new LayoutJson {
						backgroundAssetPath = Background ? AssetDatabase.GetAssetPath(Background) : string.Empty,
						fields = new List<FieldJson>()
					};

					for (int i = 0; i < Fields.Count; i++) {
						var vm = Fields[i];
						dto.fields.Add(new FieldJson {
							type = vm.type.ToString(),
							id = vm.id,
							label = vm.label,
							topLeft = vm.topLeft,
							bottomRight = vm.bottomRight,
							text = vm.text,
							imageAssetPath = vm.image ? AssetDatabase.GetAssetPath(vm.image) : string.Empty
						});
					}

					var json = JsonUtility.ToJson(dto, true);
					Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
					File.WriteAllText(absolutePath, json);
					AssetDatabase.Refresh();
				}

				public static bool LoadJsonFromAbsolutePath(string absolutePath) {
					if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath))
						return false;

					string json = File.ReadAllText(absolutePath);
					var data = JsonUtility.FromJson<LayoutJson>(json);
					if (data == null)
						return false;

					Background = !string.IsNullOrEmpty(data.backgroundAssetPath)
						? AssetDatabase.LoadAssetAtPath<Texture2D>(data.backgroundAssetPath)
						: null;

					Fields.Clear();
					if (data.fields != null) {
						for (int i = 0; i < data.fields.Count; i++) {
							var fldJson = data.fields[i];
							var vm = new FieldViewModle {
								id = fldJson.id,
								label = fldJson.label,
								topLeft = fldJson.topLeft,
								bottomRight = fldJson.bottomRight,
								type = ParseTypeSafe(fldJson.type),
								text = fldJson.text,
								image = !string.IsNullOrEmpty(fldJson.imageAssetPath)
									? AssetDatabase.LoadAssetAtPath<Texture2D>(fldJson.imageAssetPath)
									: null
							};
							ClampAndOrder(ref vm.topLeft, ref vm.bottomRight);
							Fields.Add(vm);
						}
					}
					SelectedIndex = Fields.Count > 0 ? 0 : -1;
					MarkDirty();
					return true;
				}

				public static void Save() {
					if (ActiveAsset) {
						SaveToAsset(ActiveAsset);

						var assetPath = AssetDatabase.GetAssetPath(ActiveAsset);
						var absAssetPath = ToAbsoluteProjectPath(assetPath);
						var jsonAbs = Path.ChangeExtension(absAssetPath, "json");

						WriteJsonToAbsolutePath(jsonAbs);

						var jsonRel = ToProjectRelativePath(jsonAbs);
						if (!string.IsNullOrEmpty(jsonRel)) {
							AssetDatabase.ImportAsset(jsonRel);
							var ta = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonRel);
							ActiveAsset.jsonExport = ta;
							EditorUtility.SetDirty(ActiveAsset);
							AssetDatabase.SaveAssets();
						}

						MarkDirty();
					} else {
						SaveAs();
					}
				}

				public static void SaveAs() {
					string projPath = EditorUtility.SaveFilePanelInProject(
						"Save Layout", "ImageFieldingLayout", "asset", "");

					if (string.IsNullOrEmpty(projPath))
						return;

					var existing = AssetDatabase.LoadAssetAtPath<ImageFieldingAsset>(projPath);
					if (!existing) {
						existing = ScriptableObject.CreateInstance<ImageFieldingAsset>();
						AssetDatabase.CreateAsset(existing, projPath);
						AssetDatabase.ImportAsset(projPath);
					}

					ActiveAsset = existing;
					SaveToAsset(existing);

					var absAssetPath = ToAbsoluteProjectPath(projPath);
					var jsonAbs = Path.ChangeExtension(absAssetPath, "json");
					WriteJsonToAbsolutePath(jsonAbs);

					var jsonRel = ToProjectRelativePath(jsonAbs);
					if (!string.IsNullOrEmpty(jsonRel)) {
						AssetDatabase.ImportAsset(jsonRel);
						var ta = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonRel);
						existing.jsonExport = ta;
						EditorUtility.SetDirty(existing);
						AssetDatabase.SaveAssets();
					}

					MarkDirty();
				}

				public static void Load() {
					string abs = EditorUtility.OpenFilePanelWithFilters(
						"Load Layout",
						Application.dataPath,
						new[] { "Layout Asset", "asset", "Layout JSON", "json" });

					if (string.IsNullOrEmpty(abs))
						return;

					if (abs.EndsWith(".asset")) {
						var rel = ToProjectRelativePath(abs);
						if (string.IsNullOrEmpty(rel))
							return;

						var asset = AssetDatabase.LoadAssetAtPath<ImageFieldingAsset>(rel);
						if (asset) {
							BindFromAsset(asset);
							MarkDirty();
						}
					} else if (abs.EndsWith(".json") && LoadJsonFromAbsolutePath(abs)) {
						ActiveAsset = null;
						MarkDirty();
					}
				}

				// HELPERS -- START
				#region HELPERS
				public static void MarkDirty() => StateChanged?.Invoke();

				public static void ClampAndOrder(ref Vector2 topLeft, ref Vector2 btmRight) {
					topLeft = new Vector2(Mathf.Clamp01(topLeft.x), Mathf.Clamp01(topLeft.y));
					btmRight = new Vector2(Mathf.Clamp01(btmRight.x), Mathf.Clamp01(btmRight.y));

					float topX = Mathf.Min(topLeft.x, btmRight.x);
					float leftY = Mathf.Min(topLeft.y, btmRight.y);
					float btmX = Mathf.Max(topLeft.x, btmRight.x);
					float rightY = Mathf.Max(topLeft.y, btmRight.y);
					topLeft = new Vector2(topX, leftY);
					btmRight = new Vector2(btmX, rightY);

					const float minN = 0.002f;
					if (btmRight.x - topLeft.x < minN)
						btmRight.x = Mathf.Min(1f, topLeft.x + minN);
					if (btmRight.y - topLeft.y < minN)
						btmRight.y = Mathf.Min(1f, topLeft.y + minN);
				}

				public static ImageFieldingTypes ParseTypeSafe(string typeStr) {
					if (Enum.TryParse<ImageFieldingTypes>(typeStr, true, out var typeEnum))
						return typeEnum;
					return string.Equals(typeStr, "text", StringComparison.OrdinalIgnoreCase) ? ImageFieldingTypes.String : ImageFieldingTypes.Image;
				}

				public static string ToAbsoluteProjectPath(string projectRelativePath) {
					string projectRoot = Directory.GetParent(Application.dataPath).FullName;
					return Path.GetFullPath(Path.Combine(projectRoot, projectRelativePath));
				}

				public static string ToProjectRelativePath(string absolutePath) {
					var relativePath = FileUtil.GetProjectRelativePath(absolutePath);
					return string.IsNullOrEmpty(relativePath) ? null : relativePath.Replace('\\', '/');
				}

				private static List<FieldViewModle> FromAssetToVM(ImageFieldingAsset asset) {
					var list = new List<FieldViewModle>();
					if (asset == null)
						return list;

					for (int i = 0; i < asset.fields.Count; i++) {
						var fld = asset.fields[i];
						var topLeft = new Vector2(fld.normalizedRect.x, fld.normalizedRect.y);
						var btmRight = new Vector2(fld.normalizedRect.x + fld.normalizedRect.width, fld.normalizedRect.y + fld.normalizedRect.height);
						ClampAndOrder(ref topLeft, ref btmRight);
						list.Add(new FieldViewModle {
							type = fld.fieldType,
							id = fld.ID,
							label = fld.fieldType == ImageFieldingTypes.String ? fld.ID : null,
							topLeft = topLeft,
							bottomRight = btmRight,
							text = fld.text,
							image = fld.image
						});
					}
					return list;
				}
				#endregion
				// HELPERS -- END

				[Serializable]
				public class FieldViewModle {
					public ImageFieldingTypes type;
					public string id;
					public string label;
					public Vector2 topLeft = new(0.10f, 0.10f);
					public Vector2 bottomRight = new(0.30f, 0.20f);
					public string text;
					public Texture2D image;
				}

				[Serializable]
				private class LayoutJson {
					public string backgroundAssetPath;
					public List<FieldJson> fields = new();
				}

				[Serializable]
				private class FieldJson {
					public string type;
					public string id;
					public string label;
					public Vector2 topLeft;
					public Vector2 bottomRight;
					public string text;
					public string imageAssetPath;
				}

				public static void New() {
					ActiveAsset = null;
					Background = null;
					Fields.Clear();
					SelectedIndex = -1;
					MarkDirty();
				}
			}
		}
	}
}