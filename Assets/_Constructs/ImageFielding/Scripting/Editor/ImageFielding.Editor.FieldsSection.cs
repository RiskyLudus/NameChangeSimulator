namespace DW.Tools {
	using System;
	using System.Collections.Generic;

	using UnityEditor;

	using UnityEngine;

	using state = ImageFielding.Editor.State;

	public partial class ImageFielding {
		public partial class Editor {
			public class FieldsSection {
				private readonly Window _host;
				private Vector2 _scroll;

				private static string[] s_AllIds = Array.Empty<string>();
				private static double s_LastScanTime = -1d;
				private const double SCAN_COOLDOWN = 1.0;
				private readonly Dictionary<string, string> _searchCache =
					new Dictionary<string, string>(StringComparer.Ordinal);

				public FieldsSection(Window host) { _host = host; }

				public void Draw() {
					using (var sv = new EditorGUILayout.ScrollViewScope(_scroll, GUILayout.ExpandHeight(true))) {
						_scroll = sv.scrollPosition;
						for (int i = 0; i < state.Fields.Count; i++) {
							var vm = state.Fields[i];
							DrawFieldCard(i, ref vm);
							state.Fields[i] = vm;
							GUILayout.Space(4);
						}
					}
				}

				// DRAWERS -- START
				#region DRAWERS
				public void DrawFileManagement() {
					using (new EditorGUILayout.HorizontalScope()) {
						if (GUILayout.Button("New", GUILayout.Height(24)))
							state.New();
						if (GUILayout.Button("Save", GUILayout.Height(24)))
							state.Save();
						if (GUILayout.Button("Save As…", GUILayout.Height(24)))
							state.SaveAs();
						if (GUILayout.Button("Load…", GUILayout.Height(24)))
							state.Load();
					}
				}

				public void DrawFieldMenu() {
					EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);
					using (new EditorGUILayout.HorizontalScope()) {
						if (GUILayout.Button("Add Text Field", GUILayout.Height(22))) {
							state.Fields.Add(
								new state.FieldViewModle {
									type = ImageFieldingTypes.String,
									id = "NewText",
									label = "NewText",
									topLeft = new Vector2(0.10f, 0.10f),
									bottomRight = new Vector2(0.30f, 0.20f)
								}
							);
						}
						if (GUILayout.Button("Add Image Field", GUILayout.Height(22))) {
							state.Fields.Add(
								new state.FieldViewModle {
									type = ImageFieldingTypes.Image,
									id = "NewImage",
									label = null,
									topLeft = new Vector2(0.10f, 0.10f),
									bottomRight = new Vector2(0.30f, 0.20f)
								}
							);
						}
						if (GUILayout.Button("Copy Field", GUILayout.Height(22))) {
							int index = state.SelectedIndex;
							if (index >= 0 && index < state.Fields.Count) {
								var fld = state.Fields[index];
								var topLeft = fld.topLeft + new Vector2(0.01f, 0.01f);
								var btmRight = fld.bottomRight + new Vector2(0.01f, 0.01f);
								state.ClampAndOrder(ref topLeft, ref btmRight);

								state.Fields.Add(new state.FieldViewModle {
									type = fld.type,
									id = fld.id,
									label = fld.label,
									topLeft = topLeft,
									bottomRight = btmRight,
									text = fld.text,
									image = fld.image
								});
								state.SelectedIndex = state.Fields.Count - 1;
								_host.Repaint();
							}
						}
					}
				}

				private void DrawFieldCard(int index, ref state.FieldViewModle vm) {
					using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
						DrawFieldHeader(index, ref vm);

						bool expanded = index == state.SelectedIndex;
						if (!expanded)
							return;

						DrawIdPickerAndSearch(index, ref vm);
						DrawTypeSpecificEditors(ref vm);
						DrawRectEditors(ref vm);
					}
				}

				private void DrawFieldHeader(int index, ref state.FieldViewModle vm) {
					using (new EditorGUILayout.HorizontalScope()) {
						bool sel = GUILayout.Toggle(index == state.SelectedIndex, GUIContent.none, GUILayout.Width(18));
						if (sel)
							state.SelectedIndex = index;

						vm.type = (ImageFieldingTypes)EditorGUILayout.EnumPopup(vm.type);

						string newIdInline = EditorGUILayout.TextField(string.IsNullOrEmpty(vm.id) ? "" : vm.id);
						if (newIdInline != vm.id) { vm.id = newIdInline; _host.Repaint(); }

						GUILayout.FlexibleSpace();
						if (GUILayout.Button("X", GUILayout.Width(22))) {
							var removed = state.Fields[index];
							_host.OnFieldDeleted(removed, index);
							state.Fields.RemoveAt(index);
							if (state.SelectedIndex == index)
								state.SelectedIndex = -1;
							_host.Repaint();
							GUIUtility.ExitGUI();
						}
					}
				}

				private void DrawIdPickerAndSearch(int index, ref state.FieldViewModle vm) {
					const string PrefPrefix = "DW.ImageFielding.FieldsSection.IdSearch.";
					string prefKey = BuildSearchPrefKey(PrefPrefix, vm.id, index);

					// Cached EditorPrefs access + delayed field
					using (new EditorGUILayout.HorizontalScope()) {
						GUILayout.Label("Search IDs", GUILayout.Width(80));
						string cached = GetSearchText(prefKey);
						string newSearch = EditorGUILayout.DelayedTextField(cached);
						if (!string.Equals(newSearch, cached, StringComparison.Ordinal)) {
							SetSearchText(prefKey, newSearch);
							_host.Repaint();
						}
						if (GUILayout.Button("↻", GUILayout.Width(24))) {
							ForceRefreshAllIds();
							_host.Repaint();
						}
					}

					string[] allIds = GetDialogIdsFiltered(GetSearchText(prefKey));
					var options = BuildIdDropdownOptions(allIds);

					int currentIndex = GetCurrentIdIndex(allIds, vm.id);
					int chosen = EditorGUILayout.Popup("ID", currentIndex, options);

					if (chosen <= 0) {
						string newId = EditorGUILayout.TextField("New ID", vm.id ?? string.Empty);
						if (newId != vm.id) { vm.id = newId; _host.Repaint(); }
					} else {
						string picked = allIds[chosen - 1];
						if (!string.Equals(picked, vm.id, StringComparison.Ordinal)) { vm.id = picked; _host.Repaint(); }
					}
				}

				private void DrawTypeSpecificEditors(ref state.FieldViewModle vm) {
					if (vm.type == ImageFieldingTypes.String) {
						EditorGUI.BeginChangeCheck();
						var newText = EditorGUILayout.TextField("Text", vm.text);
						if (EditorGUI.EndChangeCheck()) { vm.text = newText; _host.Repaint(); }
					} else {
						vm.image = (Texture2D)EditorGUILayout.ObjectField("Image", vm.image, typeof(Texture2D), false);
					}
				}

				private void DrawRectEditors(ref state.FieldViewModle vm) {
					vm.topLeft = EditorGUILayout.Vector2Field("Top-Left (0..1)", vm.topLeft);
					vm.bottomRight = EditorGUILayout.Vector2Field("Bottom-Right (0..1)", vm.bottomRight);
					state.ClampAndOrder(ref vm.topLeft, ref vm.bottomRight);
				}
				#endregion
				// DRAWERS -- END

				// HELPERS -- START
				#region HELPERS
				private static string[] BuildIdDropdownOptions(string[] allIds) {
					var options = new string[allIds.Length + 1];
					options[0] = "— New ID —";
					for (int i = 0; i < allIds.Length; i++)
						options[i + 1] = allIds[i];
					return options;
				}

				private static int GetCurrentIdIndex(string[] allIds, string currentId) {
					if (string.IsNullOrEmpty(currentId))
						return 0;
					for (int i = 0; i < allIds.Length; i++) {
						if (string.Equals(allIds[i], currentId, System.StringComparison.OrdinalIgnoreCase))
							return i + 1; // +1 because index 0 is "— New ID —"
					}
					return 0;
				}

				private static string BuildSearchPrefKey(string prefix, string id, int indexFallback) {
					string raw = string.IsNullOrEmpty(id) ? indexFallback.ToString() : id;
					for (int i = 0; i < raw.Length; i++) {
						char chr = raw[i];
						if (!(char.IsLetterOrDigit(chr) || chr == '_' || chr == '-' || chr == '.'))
							raw = raw.Replace(chr, '_');
					}
					return prefix + raw;
				}

				private string GetSearchText(string key) {
					if (_searchCache.TryGetValue(key, out var v))
						return v;
					string loaded = EditorPrefs.GetString(key, string.Empty);
					_searchCache[key] = loaded;
					return loaded;
				}

				private void SetSearchText(string key, string value) {
					_searchCache[key] = value;
					EditorPrefs.SetString(key, value);
				}

				private static void ForceRefreshAllIds() {
					s_LastScanTime = -1d;
					RefreshAllIdsIfNeeded(true);
				}

				private static void RefreshAllIdsIfNeeded(bool force = false) {
					double now = EditorApplication.timeSinceStartup;
					if (!force && s_AllIds.Length > 0 && (now - s_LastScanTime) < SCAN_COOLDOWN)
						return;

					var list = new List<string>();
					var objs = Resources.LoadAll("Dialogs", typeof(ScriptableObject));
					AccumulateKeywords(objs, list);

					for (int i = 0; i < list.Count; i++) {
						string vi = list[i];
						for (int j = list.Count - 1; j > i; j--) {
							if (string.Equals(list[j], vi, StringComparison.OrdinalIgnoreCase))
								list.RemoveAt(j);
						}
					}

					list.Sort((a, b) => string.Compare(a, b, StringComparison.OrdinalIgnoreCase));
					s_AllIds = list.ToArray();
					s_LastScanTime = now;
				}

				private static string[] GetDialogIdsFiltered(string filter) {
					RefreshAllIdsIfNeeded();
					if (string.IsNullOrEmpty(filter))
						return s_AllIds;

					var result = new List<string>(s_AllIds.Length);
					for (int i = 0; i < s_AllIds.Length; i++) {
						string id = s_AllIds[i];
						if (!string.IsNullOrEmpty(id) && id.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
							result.Add(id);
					}
					return result.ToArray();
				}

				private static void AccumulateKeywords(UnityEngine.Object[] objs, List<string> list) {
					for (int i = 0; i < objs.Length; i++) {
						var obj = objs[i];
						if (!obj)
							continue;
						if (TryGetKeywordFast(obj, out var kw))
							AddUniqueIgnoreCase(list, kw);
					}
				}

				private static bool TryGetKeywordFast(UnityEngine.Object obj, out string keyword) {
					keyword = null;
					if (!obj)
						return false;

					if (obj is IHasKeyword hasKeyword) {
						keyword = hasKeyword.Keyword;
						return !string.IsNullOrEmpty(keyword);
					}

#if UNITY_EDITOR
					try {
						var so = new SerializedObject(obj);
						var prop = so.FindProperty("Keyword");
						if (prop != null && prop.propertyType == SerializedPropertyType.String) {
							keyword = prop.stringValue;
							return !string.IsNullOrEmpty(keyword);
						}
					}
					catch { /* ignore */ }
#endif
					return false;
				}

				private static void AddUniqueIgnoreCase(List<string> list, string value) {
					if (string.IsNullOrEmpty(value))
						return;
					for (int i = 0; i < list.Count; i++)
						if (string.Equals(list[i], value, StringComparison.OrdinalIgnoreCase))
							return;
					list.Add(value);
				}
				#endregion
				// HELPERS -- END
			}
		}
	}
}
