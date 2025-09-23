namespace DW.Tools {
	using UnityEditor;

	using UnityEngine;

	using state = ImageFielding.Editor.State;

	public partial class ImageFielding {
		public partial class Editor {
			public class Window : UnityEditor.EditorWindow {
				private FieldsSection _fieldsSection;
				private EditingSection _canvasSection;
				private Vector2 _leftScroll;
				private int _copiedIndex = -1;

				private struct DeletedField { public state.FieldViewModle fld; public int index; }
				private System.Collections.Generic.List<DeletedField> _deletedStack = new System.Collections.Generic.List<DeletedField>();

				[MenuItem("Tools/DW/Image Fielding Editor")]
				public static Window Open() {
					var win = GetWindow<Window>("Image Fielding");
					win.minSize = new Vector2(760, 540);
					win.Show();
					return win;
				}

				public static void OpenWith(ImageFieldingAsset asset) {
					var win = Open();

					state.BindFromAsset(asset);

					win.Repaint();
				}

				private void OnEnable() {
					wantsMouseMove = true;
					_fieldsSection = new FieldsSection(this);
					_canvasSection = new EditingSection(this);

					state.StateChanged += Repaint;
				}

				private void OnDisable() {
					state.StateChanged -= Repaint;
				}

				private void OnGUI() {
					using (new EditorGUILayout.HorizontalScope()) {
						DrawLeftPanel();
						DrawRightPanel();
					}
					HandleKeyboardNudge();
				}

				private void DrawRightPanel() {
					_canvasSection.Draw();
				}

				private void DrawLeftPanel() {
					using (new EditorGUILayout.VerticalScope(GUILayout.Width(360), GUILayout.ExpandHeight(true))) {
						EditorGUILayout.Space(4);

						_fieldsSection.DrawFileManagement();

						EditorGUILayout.Space(6);

						_canvasSection.DrawBackgroundImage();

						EditorGUILayout.Space(6);

						_fieldsSection.DrawFieldMenu();

						using (var sv = new EditorGUILayout.ScrollViewScope(_leftScroll, GUILayout.ExpandHeight(true))) {
							_leftScroll = sv.scrollPosition;
							_fieldsSection.Draw();
							GUILayout.Space(8);
						}
					}
				}

				private void HandleKeyboardNudge() {
					var crntEvent = Event.current;
					if (crntEvent.type != EventType.KeyDown)
						return;

					if ((crntEvent.control || crntEvent.command) && crntEvent.keyCode == KeyCode.Z) {
						if (!Command_UndoDelete(crntEvent))
							return;
					}

					if (!EditorGUIUtility.editingTextField) {
						if (!Command_Copy(crntEvent))
							return;
						if (!Command_Paste(crntEvent))
							return;
						if (!Command_Duplicate(crntEvent))
							return;
						if (!Command_Delete(crntEvent))
							return;
					}

					if (!Command_ArrowFieldNudge(crntEvent))
						return;
				}


				public void OnFieldDeleted(state.FieldViewModle fld, int index) {
					_deletedStack.Add(new DeletedField { fld = fld, index = index });
				}

				// COMMANDS -- START
				#region COMMANDS
				private bool Command_ArrowFieldNudge(Event crntEvent) {
					if (state.SelectedIndex < 0 || state.SelectedIndex >= state.Fields.Count)
						return false;

					float step = crntEvent.shift ? 0.01f : 0.0025f;
					bool changed = false;

					var topLeft = state.Fields[state.SelectedIndex].topLeft;
					var btmRight = state.Fields[state.SelectedIndex].bottomRight;

					if (crntEvent.keyCode == KeyCode.LeftArrow) { 
						topLeft.x -= step; 
						btmRight.x -= step; 
						changed = true;
					}
					if (crntEvent.keyCode == KeyCode.RightArrow) { 
						topLeft.x += step; 
						btmRight.x += step; 
						changed = true; 
					}
					if (crntEvent.keyCode == KeyCode.UpArrow) { 
						topLeft.y -= step; 
						btmRight.y -= step; 
						changed = true; 
					}
					if (crntEvent.keyCode == KeyCode.DownArrow) { 
						topLeft.y += step; 
						btmRight.y += step; 
						changed = true; 
					}

					if (changed) {
						state.ClampAndOrder(ref topLeft, ref btmRight);
						state.Fields[state.SelectedIndex].topLeft = topLeft;
						state.Fields[state.SelectedIndex].bottomRight = btmRight;
						
						Repaint();
						crntEvent.Use();
					}

					return true;
				}

				private bool Command_Delete(Event crntEvent) {
					if (crntEvent.keyCode == KeyCode.Delete || crntEvent.keyCode == KeyCode.Backspace) {
						if (state.SelectedIndex >= 0 && state.SelectedIndex < state.Fields.Count) {
							int i = state.SelectedIndex;
							OnFieldDeleted(state.Fields[i], i);
							
							state.Fields.RemoveAt(i);
							state.SelectedIndex = state.Fields.Count > 0 ? Mathf.Clamp(i - 1, 0, state.Fields.Count - 1) : -1;
							
							Repaint();
							crntEvent.Use();
							return false;
						}
					}

					return true;
				}


				private bool Command_UndoDelete(Event crntEvent) {
					if ((crntEvent.control || crntEvent.command) && crntEvent.keyCode == KeyCode.Z) {
						if (_deletedStack.Count > 0) {
							var last = _deletedStack[_deletedStack.Count - 1];
							int insertAt = Mathf.Clamp(last.index, 0, state.Fields.Count);

							state.Fields.Insert(insertAt, last.fld);
							state.SelectedIndex = insertAt;

							_deletedStack.RemoveAt(_deletedStack.Count - 1);

							Repaint();
							crntEvent.Use();
							return false;
						}
					}
					return true;
				}

				private bool Command_Duplicate(Event crntEvent) {
					if ((crntEvent.control || crntEvent.command) && crntEvent.keyCode == KeyCode.D) {
						if (state.SelectedIndex >= 0 && state.SelectedIndex < state.Fields.Count) {
							var fld = state.Fields[state.SelectedIndex];
							var topLeft = fld.topLeft + new Vector2(0.01f, 0.01f);
							var btmRght = fld.bottomRight + new Vector2(0.01f, 0.01f);

							state.ClampAndOrder(ref topLeft, ref btmRght);
							state.Fields.Add(new state.FieldViewModle {
								type = fld.type,
								id = fld.id,
								label = fld.label,
								topLeft = topLeft,
								bottomRight = btmRght,
								text = fld.text,
								image = fld.image
							});
							state.SelectedIndex = state.Fields.Count - 1;

							Repaint();
							crntEvent.Use();
							return false;
						}
					}

					return true;
				}

				private bool Command_Paste(Event crntEvent) {
					if ((crntEvent.control || crntEvent.command) && crntEvent.keyCode == KeyCode.V) {
						if (_copiedIndex >= 0 && _copiedIndex < state.Fields.Count) {
							var src = state.Fields[_copiedIndex];
							var size = src.bottomRight - src.topLeft;
							var center = _canvasSection.GetViewportCenterNormalized();
							var topLeft = center - size * 0.5f;
							var btmRght = center + size * 0.5f;

							state.ClampAndOrder(ref topLeft, ref btmRght);
							var copy = new state.FieldViewModle {
								type = src.type,
								id = src.id,
								label = src.label,
								topLeft = topLeft,
								bottomRight = btmRght,
								text = src.text,
								image = src.image
							};
							state.Fields.Add(copy);
							state.SelectedIndex = state.Fields.Count - 1;

							Repaint();
							crntEvent.Use();
						}
						return false;
					}

					return true;
				}

				private bool Command_Copy(Event crntEvent) {
					if ((crntEvent.control || crntEvent.command) && crntEvent.keyCode == KeyCode.C) {
						_copiedIndex = state.SelectedIndex;
						crntEvent.Use();
						return false;
					}

					return true;
				}
				#endregion
				// COMMANDS -- END
			}
		}
	}
}
