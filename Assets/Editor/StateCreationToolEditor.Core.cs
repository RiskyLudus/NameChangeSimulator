#if UNITY_EDITOR
namespace NameChangeSimulator.Editor {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;
	using XNode;
	using DW.Tools;

	public partial class StateCreationToolEditor : EditorWindow {
		private string stateName;
		private ImageFieldingAsset selectedLayout;

		private readonly List<ImageFieldingAsset> resolvedChain = new List<ImageFieldingAsset>();
		private readonly List<Field> fieldsList = new List<Field>();
		private readonly HashSet<string> fieldNamesSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private Vector2 scrollPosition;
		private int stepNumber = 1;
		private string[] folderNames;
		private int selectedFolderIndex = 0;

		private bool step3Scanned = false;

		private readonly Dictionary<string, FieldConfig> fieldConfigs = new Dictionary<string, FieldConfig>(StringComparer.OrdinalIgnoreCase);
		private readonly List<string> orderedKeys = new List<string>();
		private int newRowCounter = 0;

		private enum PortSel { Input, OverrideInput }

		private static readonly string[] NodeTypeOptions = new[] {
			"DialogueNode",
			"DeadNameInputNode",
			"NewNameInputNode",
			"ShowStatePickerNode",
			"LoadStateGraphNode",
			"InputNode",
			"DropdownNode",
			"ChoiceNode",
			"EndNode",
			"StartNode",
			"QuitNode"
		};

		private class FieldConfig {
			public string NodeName;
			public PortSel NextRowPort = PortSel.Input;
			public string InputOverride;

			public string Dialog;
			public string Keyword;
			public int TypeIndex;

			public int PoseIndex;
			public int VoiceIndex;

			public string Opt1Label;
			public PortSel Opt1TargetPort = PortSel.Input;
			public string Opt1TargetName;

			public string Opt2Label;
			public PortSel Opt2TargetPort = PortSel.Input;
			public string Opt2TargetName;
		}

		private const float kViewHeight = 520f;
		private float _avgRowH = 64f;

		private string[] _nodeNameChoices = Array.Empty<string>();
		private bool _nodeNameChoicesDirty = true;

		private string[] _poseChoices = Array.Empty<string>();
		private string[] _voiceChoices = Array.Empty<string>();

		private float _lastW = -1f;
		private struct Widths {
			public float controlsW, nameW, portLabelW, connectW, overrideW;
			public float dialogW, keywordW, typeLabelW, typeW;
			public float poseLabelW, poseW, voiceLabelW, voiceW;
			public float optLabelW, optPortW, optTargetW;
		}
		private Widths _w;

		private static readonly GUIContent GOutputArrow = new GUIContent("output ->");
		private static readonly GUIContent GTypeEquals = new GUIContent("type=");
		private static readonly GUIContent GPoseEquals = new GUIContent("pose=");
		private static readonly GUIContent GVoiceEquals = new GUIContent("voice=");

		private static GUIStyle _box;
		private static GUIStyle Box => _box ??= new GUIStyle("box") {
			margin = new RectOffset(4, 4, 2, 2),
			padding = new RectOffset(6, 6, 6, 6)
		};

		private enum PendingOp { None, InsertAfter, RemoveAt, MoveUp, MoveDown }
		private PendingOp _pendingOp = PendingOp.None;
		private int _pendingIndex = -1;

		[MenuItem("Tools/State Creation Tool")]
		public static void ShowWindow() => GetWindow<StateCreationToolEditor>("State Creation Tool");

		private void OnGUI() {
			GUILayout.Space(10);
			EditorGUILayout.LabelField("State Creation Tool", EditorStyles.boldLabel);
			GUILayout.Space(15);

			switch (stepNumber) {
			case 1:
				ShowStep1();
				break;
			case 2:
				ShowStep2();
				break;
			case 3:
				ShowStep3();
				break;
			case 4:
				ShowStep4();
				break;
			}

			GUILayout.Space(25);
			EditorGUILayout.LabelField("Step: " + stepNumber);
			GUILayout.Space(15);
		}

		private void ApplyPendingOpIfAny() {
			if (_pendingOp == PendingOp.None)
				return;

			switch (_pendingOp) {
			case PendingOp.InsertAfter:
				InsertRowAfter(Mathf.Clamp(_pendingIndex, -1, orderedKeys.Count - 1));
				break;
			case PendingOp.RemoveAt:
				if (orderedKeys.Count > 0)
					RemoveRowAt(Mathf.Clamp(_pendingIndex, 0, orderedKeys.Count - 1));
				break;
			case PendingOp.MoveUp:
				MoveRow(Mathf.Clamp(_pendingIndex, 0, orderedKeys.Count - 1), -1);
				break;
			case PendingOp.MoveDown:
				MoveRow(Mathf.Clamp(_pendingIndex, 0, orderedKeys.Count - 1), +1);
				break;
			}

			_pendingOp = PendingOp.None;
			_pendingIndex = -1;
			_nodeNameChoicesDirty = true;
			Repaint();
		}

		private string NewKey() { newRowCounter++; return $"__new_{newRowCounter}"; }

		private bool IsOptionsType(int typeIndex) {
			var name = NodeTypeOptions[Mathf.Clamp(typeIndex, 0, NodeTypeOptions.Length - 1)];
			return string.Equals(name, "ChoiceNode", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(name, "DropdownNode", StringComparison.OrdinalIgnoreCase);
		}

		private bool IsDialogueType(int typeIndex) {
			var name = NodeTypeOptions[Mathf.Clamp(typeIndex, 0, NodeTypeOptions.Length - 1)];
			return string.Equals(name, "DialogueNode", StringComparison.OrdinalIgnoreCase);
		}
	}
}
#endif
