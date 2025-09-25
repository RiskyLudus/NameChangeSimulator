

namespace DW.Tools {
	using UnityEditor;

	using UnityEngine;

	using state = ImageFielding.Editor.State;

	// TODO : Comments
	public class EditingSection {
		private readonly UnityEditor.EditorWindow _host;

		private enum GripDirection { None, Move, topLeft, top, topRight, right, bottomRight, bottom, bottomLeft, left }

		private const float MinZoom = 0.1f;
		private const float MaxZoom = 8f;
		private const float ZoomSpeed = 0.08f;

		private const float GRIP_VISUAL_SIZE_PX = 9f;
		private const float GRIP_PICK_SIZE_PX = 16f;
		private const float EDGE_PICK_PAD_PX = 8f;
		private const float MIN_PIXELS = 6f;

		private static GUIStyle s_FieldTextStyle;

		private Vector2 _scroll;
		private float _zoom = 1f;

		private Rect _viewRect;
		private Rect _contentRect;
		private Rect _imageRect;

		private bool _isPanning;
		private Vector2 _panStartMouse;
		private Vector2 _panStartScroll;

		private bool _isDraggingField;
		private GripDirection _activeGrip = GripDirection.None;
		private Vector2 _dragStartMouse;
		private Rect _dragStartFieldRect;

		private static readonly GripDirection[] s_GripOrder = new[] {
			GripDirection.topLeft, GripDirection.top, GripDirection.topRight,
			GripDirection.right, GripDirection.bottomRight, GripDirection.bottom,
			GripDirection.bottomLeft, GripDirection.left
		};

		public EditingSection(UnityEditor.EditorWindow host) { _host = host; }

		public void Draw() {
			_viewRect = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			GUI.Box(_viewRect, GUIContent.none);

			var bg = state.Background;
			if (!bg) {
				var centered = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
				EditorGUI.LabelField(_viewRect, "Select background Image", centered);
				return;
			}

			HandleZoomAndPan_PreScrollView();

			float cw = Mathf.Max(1f, bg.width * _zoom);
			float ch = Mathf.Max(1f, bg.height * _zoom);
			_contentRect = new Rect(0, 0, cw, ch);
			_imageRect = new Rect(0, 0, cw, ch);

			_scroll = GUI.BeginScrollView(_viewRect, _scroll, _contentRect, true, true);

			if (Event.current.type == EventType.Repaint) {
				GUI.DrawTexture(_imageRect, bg, ScaleMode.StretchToFill);
				for (int i = 0; i < state.Fields.Count; i++)
					DrawFieldRect(i);
			}

			UpdateCursorsForSelected();
			HandleFieldMouseInput();

			GUI.EndScrollView();
		}

		public void DrawBackgroundImage() {
			EditorGUILayout.LabelField("Background Image", EditorStyles.boldLabel);
			var prev = state.Background;
			state.Background = (Texture2D)EditorGUILayout.ObjectField(state.Background, typeof(Texture2D), false);
			if (prev != state.Background)
				_host.Repaint();
		}

		public Vector2 GetViewportCenterNormalized() {
			Vector2 viewCenter = _viewRect.center - _viewRect.position;
			Vector2 content = _scroll + viewCenter;

			float x = _imageRect.width > 0 ? content.x / _imageRect.width : 0.5f;
			float y = _imageRect.height > 0 ? content.y / _imageRect.height : 0.5f;

			return new Vector2(Mathf.Clamp01(x), Mathf.Clamp01(y));
		}

		private void HandleZoomAndPan_PreScrollView() {
			var evnt = Event.current;
			if (!_viewRect.Contains(evnt.mousePosition))
				return;

			if (evnt.type == EventType.ScrollWheel) {
				float old = _zoom;
				float factor = Mathf.Pow(1f + ZoomSpeed, -evnt.delta.y);
				_zoom = Mathf.Clamp(old * factor, MinZoom, MaxZoom);

				if (!Mathf.Approximately(_zoom, old)) {
					Vector2 mouseInView = evnt.mousePosition - _viewRect.position;

					float oldW = state.Background.width * old;
					float oldH = state.Background.height * old;

					Vector2 mouseInContent = _scroll + mouseInView;

					Vector2 rel = new Vector2(
						oldW > 0 ? mouseInContent.x / oldW : 0f,
						oldH > 0 ? mouseInContent.y / oldH : 0f);

					float newW = state.Background.width * _zoom;
					float newH = state.Background.height * _zoom;

					Vector2 newMouseInContent = new Vector2(rel.x * newW, rel.y * newH);
					_scroll = newMouseInContent - mouseInView;

					ClampScroll(_viewRect.size, new Vector2(newW, newH), ref _scroll);
					_host.Repaint();
				}
				evnt.Use();
				return;
			}

			if (evnt.type == EventType.MouseDown && evnt.button == 2) {
				_isPanning = true;
				_panStartMouse = evnt.mousePosition;
				_panStartScroll = _scroll;

				evnt.Use();
				return;
			}

			if (_isPanning && evnt.type == EventType.MouseDrag) {
				Vector2 delta = evnt.mousePosition - _panStartMouse;

				_scroll = _panStartScroll - delta;
				ClampScroll(_viewRect.size, new Vector2(state.Background.width * _zoom, state.Background.height * _zoom), ref _scroll);

				_host.Repaint();
				evnt.Use();
				return;
			}

			if (_isPanning && evnt.type == EventType.MouseUp && evnt.button == 2) {
				_isPanning = false;
				evnt.Use();
				return;
			}
		}

		private static void ClampScroll(Vector2 viewSize, Vector2 contentSize, ref Vector2 scroll) {
			float maxX = Mathf.Max(0f, contentSize.x - viewSize.x);
			float maxY = Mathf.Max(0f, contentSize.y - viewSize.y);

			scroll.x = Mathf.Clamp(scroll.x, 0f, maxX);
			scroll.y = Mathf.Clamp(scroll.y, 0f, maxY);
		}

		private void DrawFieldRect(int index) {
			var fld = state.Fields[index];
			Rect rect = NormalizedToDisplay(fld.topLeft, fld.bottomRight, _imageRect);

			if (s_FieldTextStyle == null) {
				s_FieldTextStyle = new GUIStyle(EditorStyles.boldLabel) {
					alignment = TextAnchor.MiddleCenter,
					wordWrap = true,
					clipping = TextClipping.Clip
				};
			}

			var fill = fld.type == ImageFieldingTypes.String ? new Color(0f, 0.6f, 1f, 0.10f) : new Color(1f, 0.6f, 0f, 0.10f);
			EditorGUI.DrawRect(rect, fill);

			if (fld.type == ImageFieldingTypes.Image && fld.image) {
				var pad = 2f;
				var content = new Rect(rect.x + pad, rect.y + pad, rect.width - pad * 2f, rect.height - pad * 2f);
				GUI.DrawTexture(content, fld.image, ScaleMode.ScaleToFit, true);

			} else if (fld.type == ImageFieldingTypes.String) {
				string txt = string.IsNullOrEmpty(fld.text) ? fld.id : fld.text;
				var pad = 4f;
				var content = new Rect(rect.x + pad, rect.y + pad, rect.width - pad * 2f, rect.height - pad * 2f);
				GUI.Label(content, txt, s_FieldTextStyle);

			} else {
				var pad = 4f;
				var content = new Rect(rect.x + pad, rect.y + pad, rect.width - pad * 2f, 18f);
				GUI.Label(content, string.IsNullOrEmpty(fld.id) ? "Image" : fld.id, EditorStyles.boldLabel);
			}

			var outline = index == state.SelectedIndex ? Color.yellow : new Color(1f, 1f, 1f, 0.75f);
			Handles.DrawSolidRectangleWithOutline(rect, new Color(0, 0, 0, 0), outline);

			if (index == state.SelectedIndex)
				DrawGrips(rect);
		}

		private void DrawGrips(Rect rect) {
			foreach (var grip in s_GripOrder) {
				var rec = GetGripDrawRect(rect, grip);
				EditorGUI.DrawRect(rec, Color.white);
				EditorGUI.DrawRect(new Rect(rec.x + 1, rec.y + 1, rec.width - 2, rec.height - 2), new Color(0.2f, 0.2f, 0.2f, 1f));
			}
		}

		private void UpdateCursorsForSelected() {
			if (state.SelectedIndex < 0 || state.SelectedIndex >= state.Fields.Count)
				return;

			Rect rectSlct = NormalizedToDisplay(
				state.Fields[state.SelectedIndex].topLeft,
				state.Fields[state.SelectedIndex].bottomRight,
				_imageRect);

			foreach (var grip in s_GripOrder)
				EditorGUIUtility.AddCursorRect(GetGripHitRect(rectSlct, grip), GetCursorForGrip(grip));

			EditorGUIUtility.AddCursorRect(rectSlct, MouseCursor.MoveArrow);
		}

		private void HandleFieldMouseInput() {
			var evnt = Event.current;

			Rect actionArea = _imageRect;
			actionArea.xMin -= GRIP_PICK_SIZE_PX;
			actionArea.yMin -= GRIP_PICK_SIZE_PX;
			actionArea.xMax += GRIP_PICK_SIZE_PX;
			actionArea.yMax += GRIP_PICK_SIZE_PX;

			if (!actionArea.Contains(evnt.mousePosition))
				return;

			switch (evnt.type) {
			case EventType.MouseDown:
				if (evnt.button == 0) {
					if (TryBeginDragSelected(evnt))
						return;

					int hit = PickTopMostFieldIncludingGrips(evnt.mousePosition, out var picked);
					state.SelectedIndex = hit;

					if (hit >= 0) {
						Rect rect = NormalizedToDisplay(
							state.Fields[hit].topLeft,
							state.Fields[hit].bottomRight,
							_imageRect);

						_activeGrip = picked == GripDirection.None ? GripDirection.Move : picked;
						_isDraggingField = true;
						_dragStartMouse = evnt.mousePosition;
						_dragStartFieldRect = rect;

						GUI.FocusControl(null);
						evnt.Use();
					} else {
						_activeGrip = GripDirection.None;
						_isDraggingField = false;
						_host.Repaint();
					}
				}
				break;

			case EventType.MouseDrag:
				if (_isDraggingField && state.SelectedIndex >= 0) {
					Vector2 delta = evnt.mousePosition - _dragStartMouse;
					Rect newRect = _dragStartFieldRect;

					if (_activeGrip == GripDirection.Move)
						newRect.position += delta;
					else
						ResizeByGrip(ref newRect, _activeGrip, delta);

					newRect = ConstrainToArea(newRect, _imageRect, MIN_PIXELS);

					Rect norRect = DisplayToNormalized(newRect, _imageRect);
					Vector2 topLeft = new Vector2(norRect.xMin, norRect.yMin);
					Vector2 btmRight = new Vector2(norRect.xMax, norRect.yMax);
					state.ClampAndOrder(ref topLeft, ref btmRight);

					var index = state.SelectedIndex;
					state.Fields[index].topLeft = topLeft;
					state.Fields[index].bottomRight = btmRight;

					_host.Repaint();
					evnt.Use();
				}
				break;

			case EventType.MouseUp:
				if (_isDraggingField) {
					_isDraggingField = false;
					_activeGrip = GripDirection.None;
					evnt.Use();
				}
				break;
			}
		}

		private bool TryBeginDragSelected(Event evnt) {
			int index = state.SelectedIndex;
			if (index < 0 || index >= state.Fields.Count)
				return false;

			Rect selected = NormalizedToDisplay(
				state.Fields[index].topLeft,
				state.Fields[index].bottomRight,
				_imageRect);

			if (TryGetGripAt(selected, evnt.mousePosition, out var pickedOnSelected)) {
				_activeGrip = pickedOnSelected == GripDirection.None ? GripDirection.Move : pickedOnSelected;
				_isDraggingField = true;
				_dragStartMouse = evnt.mousePosition;
				_dragStartFieldRect = selected;

				GUI.FocusControl(null);
				evnt.Use();
				return true;
			}
			return false;
		}

		private int PickTopMostFieldIncludingGrips(Vector2 mousePos, out GripDirection picked) {
			for (int i = state.Fields.Count - 1; i >= 0; i--) {
				Rect rect = NormalizedToDisplay(state.Fields[i].topLeft, state.Fields[i].bottomRight, _imageRect);

				foreach (var grip in s_GripOrder) {
					if (GetGripHitRect(rect, grip).Contains(mousePos)) {
						picked = grip;
						return i;
					}
				}

				if (rect.Contains(mousePos)) {
					picked = GripDirection.Move;
					return i;
				}
			}
			picked = GripDirection.None;

			return -1;
		}

		private bool TryGetGripAt(Rect fieldDrawRect, Vector2 mousePos, out GripDirection result) {
			foreach (var grip in s_GripOrder) {
				if (GetGripHitRect(fieldDrawRect, grip).Contains(mousePos)) { result = grip; return true; }
			}

			float hrzDistToLeft = Mathf.Abs(mousePos.x - fieldDrawRect.x);
			float hrzDistToRight = Mathf.Abs(mousePos.x - fieldDrawRect.xMax);
			float vrtDistToTop = Mathf.Abs(mousePos.y - fieldDrawRect.y);
			float vrtDistToBtm = Mathf.Abs(mousePos.y - fieldDrawRect.yMax);

			bool nearLeft = hrzDistToLeft <= EDGE_PICK_PAD_PX && 
				mousePos.x <= fieldDrawRect.x && 
				mousePos.y >= fieldDrawRect.y - EDGE_PICK_PAD_PX && 
				mousePos.y <= fieldDrawRect.yMax + EDGE_PICK_PAD_PX;

			bool nearRight = hrzDistToRight <= EDGE_PICK_PAD_PX && 
				mousePos.x >= fieldDrawRect.xMax && 
				mousePos.y >= fieldDrawRect.y - EDGE_PICK_PAD_PX && 
				mousePos.y <= fieldDrawRect.yMax + EDGE_PICK_PAD_PX;

			bool nearTop = vrtDistToTop <= EDGE_PICK_PAD_PX && 
				mousePos.y <= fieldDrawRect.y && 
				mousePos.x >= fieldDrawRect.x - EDGE_PICK_PAD_PX && 
				mousePos.x <= fieldDrawRect.xMax + EDGE_PICK_PAD_PX;

			bool nearBottom = vrtDistToBtm <= EDGE_PICK_PAD_PX && 
				mousePos.y >= fieldDrawRect.yMax && 
				mousePos.x >= fieldDrawRect.x - EDGE_PICK_PAD_PX && 
				mousePos.x <= fieldDrawRect.xMax + EDGE_PICK_PAD_PX;

			if (nearTop && nearLeft) {		result = GripDirection.topLeft;		return true; }
			if (nearTop && nearRight) {		result = GripDirection.topRight;	return true; }
			if (nearTop) {					result = GripDirection.top;			return true; }
			if (nearBottom && nearRight) {	result = GripDirection.bottomRight;	return true; }
			if (nearBottom && nearLeft) {	result = GripDirection.bottomLeft;	return true; }
			if (nearBottom) {				result = GripDirection.bottom;		return true; }
			if (nearLeft) {					result = GripDirection.left;		return true; }
			if (nearRight) {				result = GripDirection.right;		return true; }

			if (fieldDrawRect.Contains(mousePos)) { result = GripDirection.Move; return true; }

			result = GripDirection.None;
			return false;
		}

		private static Rect NormalizedToDisplay(Vector2 tl, Vector2 br, Rect displayRect) {
			float x = displayRect.x + tl.x * displayRect.width;
			float y = displayRect.y + tl.y * displayRect.height;
			float w = (br.x - tl.x) * displayRect.width;
			float h = (br.y - tl.y) * displayRect.height;
			return new Rect(x, y, w, h);
		}

		private static Rect DisplayToNormalized(Rect rect, Rect displayRect) {
			float xMin = Mathf.InverseLerp(displayRect.x, displayRect.xMax, rect.xMin);
			float yMin = Mathf.InverseLerp(displayRect.y, displayRect.yMax, rect.yMin);
			float xMax = Mathf.InverseLerp(displayRect.x, displayRect.xMax, rect.xMax);
			float yMax = Mathf.InverseLerp(displayRect.y, displayRect.yMax, rect.yMax);

			return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
		}

		private static Rect ConstrainToArea(Rect rect, Rect area, float min) {
			if (rect.width < min)
				rect.width = min;

			if (rect.height < min)
				rect.height = min;

			if (rect.x < area.x)
				rect.x = area.x;

			if (rect.y < area.y)
				rect.y = area.y;

			if (rect.xMax > area.xMax)
				rect.x = area.xMax - rect.width;

			if (rect.yMax > area.yMax)
				rect.y = area.yMax - rect.height;

			return rect;
		}

		private static void ResizeByGrip(ref Rect rect, GripDirection grip, Vector2 mouseDelta) {
			switch (grip) {
			case GripDirection.topLeft:

				rect.x += mouseDelta.x;
				rect.width -= mouseDelta.x;
				rect.y += mouseDelta.y;
				rect.height -= mouseDelta.y;

				break;
			case GripDirection.top:

				rect.y += mouseDelta.y;
				rect.height -= mouseDelta.y;

				break;
			case GripDirection.topRight:

				rect.width += mouseDelta.x;
				rect.y += mouseDelta.y;
				rect.height -= mouseDelta.y;

				break;
			case GripDirection.right:

				rect.width += mouseDelta.x;

				break;
			case GripDirection.bottomRight:

				rect.width += mouseDelta.x;
				rect.height += mouseDelta.y;

				break;
			case GripDirection.bottom:

				rect.height += mouseDelta.y;

				break;
			case GripDirection.bottomLeft:

				rect.x += mouseDelta.x;
				rect.width -= mouseDelta.x;
				rect.height += mouseDelta.y;

				break;
			case GripDirection.left:

				rect.x += mouseDelta.x;
				rect.width -= mouseDelta.x;

				break;
			}
		}

		private static Rect GetGripDrawRect(Rect rect, GripDirection grip) {
			float s = GRIP_VISUAL_SIZE_PX;

			switch (grip) {
			case GripDirection.topLeft:
				return new Rect(rect.x - s * .5f, rect.y - s * .5f, s, s);
			case GripDirection.top:
				return new Rect(rect.center.x - s * .5f, rect.y - s * .5f, s, s);
			case GripDirection.topRight:
				return new Rect(rect.xMax - s * .5f, rect.y - s * .5f, s, s);
			case GripDirection.right:
				return new Rect(rect.xMax - s * .5f, rect.center.y - s * .5f, s, s);
			case GripDirection.bottomRight:
				return new Rect(rect.xMax - s * .5f, rect.yMax - s * .5f, s, s);
			case GripDirection.bottom:
				return new Rect(rect.center.x - s * .5f, rect.yMax - s * .5f, s, s);
			case GripDirection.bottomLeft:
				return new Rect(rect.x - s * .5f, rect.yMax - s * .5f, s, s);
			case GripDirection.left:
				return new Rect(rect.x - s * .5f, rect.center.y - s * .5f, s, s);
			default:
				return Rect.zero;
			}
		}

		private static Rect GetGripHitRect(Rect rect, GripDirection grip) {
			float pick = GRIP_PICK_SIZE_PX;

			switch (grip) {
			case GripDirection.topLeft:
				return new Rect(rect.x - pick, rect.y - pick, pick, pick);
			case GripDirection.top:
				return new Rect(rect.center.x - pick * .5f, rect.y - pick, pick, pick);
			case GripDirection.topRight:
				return new Rect(rect.xMax, rect.y - pick, pick, pick);
			case GripDirection.right:
				return new Rect(rect.xMax, rect.center.y - pick * .5f, pick, pick);
			case GripDirection.bottomRight:
				return new Rect(rect.xMax, rect.yMax, pick, pick);
			case GripDirection.bottom:
				return new Rect(rect.center.x - pick * .5f, rect.yMax, pick, pick);
			case GripDirection.bottomLeft:
				return new Rect(rect.x - pick, rect.yMax, pick, pick);
			case GripDirection.left:
				return new Rect(rect.x - pick, rect.center.y - pick * .5f, pick, pick);
			default:
				return Rect.zero;
			}
		}

		private static MouseCursor GetCursorForGrip(GripDirection grip) {
			switch (grip) {
			case GripDirection.left:
			case GripDirection.right:
				return MouseCursor.ResizeHorizontal;
			case GripDirection.top:
			case GripDirection.bottom:
				return MouseCursor.ResizeVertical;
			case GripDirection.topLeft:
			case GripDirection.bottomRight:
				return MouseCursor.ResizeUpLeft;
			case GripDirection.topRight:
			case GripDirection.bottomLeft:
				return MouseCursor.ResizeUpRight;
			default:
				return MouseCursor.Arrow;
			}
		}
	}
}
