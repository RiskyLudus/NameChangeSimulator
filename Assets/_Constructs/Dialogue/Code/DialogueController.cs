using System;
using System.Collections.Generic;
using System.Linq;

using Anarchy.Shared;

using DW.Tools;

using NameChangeSimulator.Constructs.Dialogue.ChoiceBox;
using NameChangeSimulator.Constructs.Dialogue.DialogueBox;
using NameChangeSimulator.Constructs.Dialogue.DropdownBox;
using NameChangeSimulator.Constructs.Dialogue.InputBox;
using NameChangeSimulator.Constructs.Dialogue.StatePickerBox;
using NameChangeSimulator.Shared;
using NameChangeSimulator.Shared.Node;

using UnityEngine;

using XNode;

namespace NameChangeSimulator.Constructs.Dialogue {
	public class DialogueController : MonoBehaviour {
		public string StateToLoad { get; set; }

		[SerializeField] private GameObject container;

		[SerializeField] private DialogueBoxController dialogueBox;
		[SerializeField] private NameInputBoxController deadNameInputBox;
		[SerializeField] private NameInputBoxController newNameInputBox;
		[SerializeField] private StatePickerBoxController statePickerBox;
		[SerializeField] private InputBoxController inputBox;
		[SerializeField] private DropdownBoxController dropdownBox;
		[SerializeField] private ChoiceBoxController choiceBox;
		[SerializeField] private GameObject quitBox;
		[SerializeField] private CharacterController characterController;

		[Header("Submission")]
		[Tooltip("If true, submit the node's saved value (if any) when it becomes current.")]
		[SerializeField] private bool submitOnSetCurrentNode = false;

		[Tooltip("Keys allowed to submit an empty string (to intentionally clear).")]
		[SerializeField] private string[] allowEmptySubmitKeys = Array.Empty<string>();

		private readonly HashSet<string> _allowEmpty = new(StringComparer.OrdinalIgnoreCase);

		private DialogueGraph _currentDialogue;
		private StartNode _startNode;
		private DialogueNode _currentNode;
		private ImageFieldingAsset FieldAssets;

		private bool _showingProgressBar;

		#region Editor Debug Helpers
#if UNITY_EDITOR
		private static string Stamp() =>
			$"<color=grey> at {DateTime.UtcNow:HH:mm:ss.fff} (t={Time.realtimeSinceStartup:F3}s)</color>";
#endif

		private void DumpFieldAssets(string whereTag) {
#if UNITY_EDITOR
			if (FieldAssets == null) {
				Debug.Log($"<color=red>[FIELD_ASSETS]</color> {whereTag}: FieldAssets is <null>");
				return;
			}

			var visited = new HashSet<ImageFieldingAsset>();
			var idx = 0;
			var cursor = FieldAssets;
			while (cursor != null && visited.Add(cursor)) {
				Debug.Log($"<color=grey>[FIELD_ASSETS]</color> {whereTag}: chain[{idx}] asset='{cursor.name}' fields={cursor.fields?.Count ?? 0}");
				if (cursor.fields != null) {
					for (int i = 0; i < cursor.fields.Count; i++) {
						var f = cursor.fields[i];
						var id = string.IsNullOrEmpty(f.ID) ? "(empty)" : f.ID;
						var txt = string.IsNullOrEmpty(f.text) ? "" : f.text;

						Debug.Log($"<color=grey>  └─</color> [{i}] ID='{id}' Type='{f.fieldType}' Text='{txt}'");
					}
				}
				cursor = cursor.next;
				idx++;
			}
#endif
		}

		private void DumpNodeIO(Node node, string where) {
#if UNITY_EDITOR
			if (node == null) { Debug.Log($"<color=magenta>[NODE_IO]</color> {where}: node is <null>"); return; }
			Debug.Log($"<color=magenta>[NODE_IO]</color> {where}: '{node.name}' ({node.GetType().Name})");
			foreach (var p in node.Ports) {
				var dir = p.IsInput ? "IN " : "OUT";
				var conn = p.IsConnected ? $"→ '{p.Connection.node?.name}'" : "• (not connected)";
				Debug.Log($"    {dir} port '{p.fieldName}' {conn}");
			}
#endif
		}

		private void DumpSynonyms(string key, IEnumerable<string> keys) {
#if UNITY_EDITOR
			var list = keys == null ? "(null)" : string.Join(", ", keys);
			Debug.Log($"<color=cyan>[SYNONYMS]</color> key='{key}' → keysToCheck=[{list}]");
#endif
		}
		#endregion

		void Awake() {
			var bb = new DW.Core.Collections.Blackboard();
			// seed allow-empty set from inspector
			_allowEmpty.Clear();
			if (allowEmptySubmitKeys != null) {
				foreach (var k in allowEmptySubmitKeys) {
					if (!string.IsNullOrWhiteSpace(k))
						_allowEmpty.Add(k.Trim());
				}
			}
		}

		private void OnEnable() {
			ConstructBindings.Send_DialogueData_Load?.AddListener(OnLoad);
			ConstructBindings.Send_ProgressBarData_ShowProgressBar?.AddListener(HndleShowProgressBar);
			ConstructBindings.Send_ProgressBarData_CloseProgressBar?.AddListener(HandleCloseProgressBar);
		}

		private void OnDisable() {
			ConstructBindings.Send_DialogueData_Load?.RemoveListener(OnLoad);
			ConstructBindings.Send_ProgressBarData_ShowProgressBar?.RemoveListener(HndleShowProgressBar);
			ConstructBindings.Send_ProgressBarData_CloseProgressBar?.RemoveListener(HandleCloseProgressBar);
		}

		private void HandleCloseProgressBar() => _showingProgressBar = false;
		private void HndleShowProgressBar(int _, int __) => _showingProgressBar = true;

		private void OnLoad(string dialogueToLoad) {
#if UNITY_EDITOR
			Debug.Log($"[START]Loading Dialogue: {dialogueToLoad}");
#endif
			container.SetActive(true);

			AudioManager.Instance.PlayWhoAreYou_Music();

			var graph = Resources.LoadAll<DialogueGraph>("States/" + dialogueToLoad).First();
			if (graph == null) {
#if UNITY_EDITOR
				Debug.Log($"Failed to load Dialogue {dialogueToLoad}");
#endif
			}

			var found = Resources.LoadAll<ImageFieldingAsset>("States/" + dialogueToLoad).FirstOrDefault();
			if (found != null)
				FieldAssets = found;

			DumpFieldAssets("OnLoad/after-Resources");

			foreach (var startNode in graph.nodes.OfType<StartNode>()) {
				_startNode = startNode;
				SetCurrentNode(startNode.GetOutputPort("Output").Connection.node);
			}

			_currentDialogue = graph;

			if (dialogueToLoad != "Introduction") {
				ConstructBindings.Send_ProgressBarData_ShowProgressBar?.Invoke(0, _currentDialogue.nodes.Count);
			}
		}

		public void GoToBack() {
			if (_currentNode == null)
				return;

			CloseAll();

			Node previousNode;
			try {
				previousNode = _currentNode.GetInputPort("Input").Connection.node;
			}
			catch (NullReferenceException) {
				previousNode = _currentNode.GetInputPort("OverrideInput").Connection.node;
			}

			SetCurrentNode(previousNode);
		}

		public void GoToNext(string valueEntered = null) {
			if (_currentNode == null)
				return;

			// Always record the user's submission for this node (with empty handling).
			var key = _currentNode.name;
			var allowEmpty = _allowEmpty.Contains(key);

			// If this is an InputNode, also allow empty when CanLeaveBlank is true.
			if (_currentNode is InputNode inp && inp.CanLeaveBlank)
				allowEmpty = true;

			// Log/save
			Debug.Log($"<color=green>[SAVE]</color> : {key}={(valueEntered ?? string.Empty)}");
			SubmitToFormDataFiller(key, valueEntered ?? string.Empty, allowEmpty);

			// Determine next node
			Node nextNode = _currentNode;

			if (_currentNode is ChoiceNode choiceNode) {
				var outputData = new Dictionary<string, string>();
				foreach (var iOutput in _currentNode.DynamicOutputs) {
					string fieldName = iOutput.fieldName.Replace("Options ", string.Empty);
					int index = int.Parse(fieldName);
					string optionValue = choiceNode.Options[index];
					outputData.Add(iOutput.fieldName, optionValue);
				}
				foreach (var output in outputData.Where(output => output.Value == valueEntered)) {
					nextNode = _currentNode.DynamicOutputs.First(op => op.fieldName == output.Key).Connection.node;
				}
			} else {
				nextNode = _currentNode.GetOutputPort("Output").Connection.node;
			}

			SetCurrentNode(nextNode);
		}

		private void SetDialogueNode(DialogueNode dialogueNode, bool showBackButton = true, bool showNextButton = true) {
			if (dialogueNode.GetInputPort("Input").Connection.node == _startNode) {
				showBackButton = false;
			}

			if (dialogueNode != null)
				dialogueBox.DisplayConversation(ResolveDialogueText(dialogueNode), showBackButton, showNextButton);

			if (dialogueNode.VoiceLine != VoiceLineType.None) {
				AudioManager.Instance.PlayVoiceOver(dialogueNode.VoiceLine.ToString());
			}

			ConstructBindings.Send_CharacterData_ChangeCharacterSprite?.Invoke(dialogueNode.SpriteType.ToString());
		}

		private string ResolveDialogueText(DialogueNode node) {
			if (node == null)
				return string.Empty;

			if (!string.IsNullOrWhiteSpace(node.DialogueText))
				return node.DialogueText;

			string byName = NCS.Dialog.Selector.MatchText(node.name);
			if (byName != null && !string.IsNullOrWhiteSpace(byName))
				return byName;

			if (FieldAssets != null) {
				var fld = FieldAssets.fields.FirstOrDefault(ff => string.Equals(ff.ID, node.name, StringComparison.OrdinalIgnoreCase));
				if (!string.IsNullOrWhiteSpace(fld.text))
					return fld.text;
			}

			return node.name;
		}

		private void SetDeadNameInputNode(DeadNameInputNode deadNameInputNode) {
			SetDialogueNode(deadNameInputNode, true, false);
			deadNameInputBox.DisplayNameInputWindow();
		}

		private void SetNewNameInputNode(NewNameInputNode newNameInputNode) {
			SetDialogueNode(newNameInputNode, true, false);
			newNameInputBox.DisplayNameInputWindow();
		}

		private void SetShowStatePickerNode(ShowStatePickerNode statePickerNode) {
			SetDialogueNode(statePickerNode, false, false);
			statePickerBox.DisplayStatePicker();
		}

		private void SetLoadStateGraphNode() {
			ConstructBindings.Send_FormDataFillerData_Load?.Invoke(StateToLoad);
#if UNITY_EDITOR
			Debug.Log($"<color=green>[SAVE]</color> : Show State Picker=");
#endif
			ConstructBindings.Send_DialogueData_Load?.Invoke(StateToLoad);
		}

		private void SetInputNode(InputNode inputNode) {
			SetDialogueNode(inputNode, true, false);
			inputBox.DisplayInputWindow(inputNode.CanLeaveBlank);
		}

		private void SetDropdownNode(DropdownNode dropdownNode) {
			SetDialogueNode(dropdownNode, true, false);
			dropdownBox.DisplayDropdownWindow(dropdownNode.Options);
		}

		private void SetChoiceNode(ChoiceNode choiceNode) {
			SetDialogueNode(choiceNode, true, false);
			choiceBox.DisplayChoicesWindow(choiceNode.Options);
		}

		private void SetQuitNode() {
			CloseAll();
			quitBox.SetActive(true);
		}

		private void SetCurrentNode(Node node) {
			if (node == null)
				return;

			var typeName = node.GetType().Name;
			_currentNode = node as DialogueNode;

			// Existing blue LOADING_NODE log + timestamp
			string savedValueForLog = null;
			TryGetSavedFromFieldAssets_SynAware(node.name, out savedValueForLog);
			Debug.Log($"<color=blue>[LOADING_NODE]</color> Node='{node.name}' Value='{(savedValueForLog ?? string.Empty)}'");

			// Deep debug around skip decision
			DumpFieldAssets("SetCurrentNode/before-skip");
			DumpNodeIO(node, "SetCurrentNode/before-skip");

#if UNITY_EDITOR
			Debug.Log($"<color=magenta>[SKIP_CHECK]</color> node='{node.name}' type='{typeName}'");
#endif
			if (TrySkipIfValueExists(node)) {
#if UNITY_EDITOR
				Debug.Log($"<color=yellow>[SKIP_DECISION]</color> node='{node.name}' → skipped");
#endif
				return;
			}
#if UNITY_EDITOR
			Debug.Log($"<color=yellow>[SKIP_DECISION]</color> node='{node.name}' → NOT skipped");
#endif

			// Optional: push the saved value (if any) when node becomes current
			if (submitOnSetCurrentNode) {
				if (TryGetSavedFromFieldAssets_SynAware(node.name, out var saved) && !string.IsNullOrWhiteSpace(saved)) {
					SubmitToFormDataFiller(node.name, saved, allowEmpty: false);
				}
			}

			switch (typeName) {
			case "DialogueNode":
				SetDialogueNode(_currentNode);
				break;
			case "DeadNameInputNode":
				SetDeadNameInputNode(node as DeadNameInputNode);
				break;
			case "NewNameInputNode":
				SetNewNameInputNode(node as NewNameInputNode);
				break;
			case "ShowStatePickerNode":
				SetShowStatePickerNode(node as ShowStatePickerNode);
				break;
			case "LoadStateGraphNode":
				SetLoadStateGraphNode();
				break;
			case "InputNode":
				SetInputNode(node as InputNode);
				break;
			case "DropdownNode":
				SetDropdownNode(node as DropdownNode);
				break;
			case "ChoiceNode":
				SetChoiceNode(node as ChoiceNode);
				break;
			case "EndNode":
				dialogueBox.CloseDialogueBox();
				ConstructBindings.Send_FormDataFillerData_ApplyToPDF?.Invoke();
				break;
			case "StartNode":
				SetLoadStateGraphNode();
				break;
			case "QuitNode":
				SetQuitNode();
				break;
			default:
#if UNITY_EDITOR
				Debug.LogWarning($"Unknown node type: {typeName}");
#endif
				break;
			}

			LogDialogueProgress();
		}

		private void CloseAll() {
			dialogueBox.CloseDialogueBox();
			deadNameInputBox.Close();
			newNameInputBox.Close();
			statePickerBox.Close();
			inputBox.Close();
			dropdownBox.Close();
			choiceBox.Close();
		}

		/// <summary>
		/// Skip Input/Dropdown nodes when a saved value already exists (synonym-aware).
		/// NOTE: We never skip DeadNameInputNode / NewNameInputNode themselves.
		/// </summary>
		private bool TrySkipIfValueExists(Node node) {
			if (node == null)
				return false;

			// Don't skip the 3-part collectors themselves
			if (node is DeadNameInputNode || node is NewNameInputNode)
				return false;

			// Only skip simple input-like nodes
			if (node is InputNode || node is DropdownNode) {
				string saved;
#if UNITY_EDITOR
				Debug.Log($"<color=magenta>[SKIP_CHECK]</color> scanning FieldAssets for key='{node.name}'");
#endif
				var keys = BuildKeysForLookup(node.name);
				DumpSynonyms(node.name, keys);

				if (TryGetSavedFromFieldAssets_SynAware(node.name, out saved) && !string.IsNullOrWhiteSpace(saved)) {
#if UNITY_EDITOR
					Debug.Log($"<color=green>[MATCH]</color> node='{node.name}' savedValue='{saved}'");
#endif
					// Follow primary 'Output' connection
					var outPort = node.GetOutputPort("Output");
					if (outPort != null && outPort.IsConnected) {
						var next = outPort.Connection.node;
						DumpNodeIO(node, "TrySkipIfValueExists/before-jump");
#if UNITY_EDITOR
						Debug.Log($"<color=yellow>[SKIP]</color> '{node.name}' → '{next?.name}'  (has saved='{saved}')");
#endif
						SetCurrentNode(next);
						return true;
					}
#if UNITY_EDITOR
					Debug.LogWarning($"'{node.name}' had a saved value but no connected 'Output' port to follow.");
#endif
				} else {
#if UNITY_EDITOR
					Debug.Log($"<color=red>[NO_MATCH]</color> node='{node.name}' — no saved value found.");
#endif
				}
			}

			return false;
		}

		private IEnumerable<string> BuildKeysForLookup(string key) {
			var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { key };
			try {
				var syns = DW.Core.Collections.Blackboard.MatchLogic.Synonyms.GetSynonyms(key);
				if (syns != null)
					foreach (var s in syns)
						if (!string.IsNullOrWhiteSpace(s))
							keys.Add(s);
			}
			catch {
				// ignore
			}
			return keys;
		}

		private bool TryGetSavedFromFieldAssets_SynAware(string key, out string value) {
			value = null;
			if (string.IsNullOrEmpty(key))
				return false;

			var keysToCheck = BuildKeysForLookup(key);

#if UNITY_EDITOR
			DumpSynonyms(key, keysToCheck);
#endif

			var asset = FieldAssets;
			var visited = new HashSet<ImageFieldingAsset>();
			while (asset != null && visited.Add(asset)) {
#if UNITY_EDITOR
				Debug.Log($"<color=grey>[SCAN]</color> asset='{asset.name}' fields={asset.fields?.Count ?? 0}");
#endif
				if (asset.fields != null) {
					for (int i = 0; i < asset.fields.Count; i++) {
						var f = asset.fields[i];
						if (f.fieldType != ImageFieldingTypes.String)
							continue;
						if (string.IsNullOrEmpty(f.ID))
							continue;

#if UNITY_EDITOR
						var matchHint = keysToCheck.Contains(f.ID) ? " (MATCH-ID)" : "";
						Debug.Log($"<color=grey>  · id='{f.ID}' txt='{f.text}'{matchHint}</color>");
#endif

						if (keysToCheck.Contains(f.ID) && !string.IsNullOrWhiteSpace(f.text)) {
#if UNITY_EDITOR
							Debug.Log($"<color=green>[FOUND]</color> id='{f.ID}' → '{f.text}'");
#endif
							value = f.text;
							return true;
						}
					}
				}
				asset = asset.next;
			}
			return false;
		}

		[ContextMenu("Log Dialogue Progress")]
		private void LogDialogueProgress() {
			if (_currentDialogue == null || _currentNode == null) {
#if UNITY_EDITOR
				Debug.LogWarning("Dialogue graph or current node is null.");
#endif
				return;
			}

			int pastCount = CountPreviousNodes(_currentNode);
			int futureCount = CountRemainingNodes(_currentNode);

			string graphName = _currentDialogue.name;
			string nodeName = _currentNode.name;

			if (_showingProgressBar)
				ConstructBindings.Send_ProgressBarData_UpdateProgress?.Invoke(_currentDialogue.nodes.Count - futureCount);

			Debug.Log($"<color=purple>[Dialogue Progress]</color> Graph='{graphName}', CurrentNode='{nodeName}', PreviousNodes={pastCount}, RemainingNodes={futureCount}");
		}

		private int CountRemainingNodes(Node current) {
			var visited = new HashSet<Node>();
			int count = 0;
			Node node = current;

			while (node != null) {
				var outputPorts = node.Ports.Where(p => p.IsOutput && p.IsConnected);
				var firstConnectedOutput = outputPorts.FirstOrDefault();
				if (firstConnectedOutput == null)
					break;

				var next = firstConnectedOutput.Connection.node as Node;
				if (next == null || visited.Contains(next))
					break;

				visited.Add(next);
				count++;
				node = next;
			}

			return count;
		}

		private int CountPreviousNodes(Node current) {
			var visited = new HashSet<Node>();
			int count = 0;
			Node node = current;

			while (node != null) {
				var inputPorts = node.Ports
					.Where(p => p.IsInput && p.IsConnected && !p.fieldName.ToLower().Contains("override"));

				var mainInput = inputPorts.FirstOrDefault();
				if (mainInput == null)
					break;

				var prev = mainInput.Connection.node as Node;
				if (prev == null || visited.Contains(prev))
					break;

				visited.Add(prev);
				count++;
				node = prev;
			}

			return count;
		}

		private void SubmitToFormDataFiller(string key, string value, bool allowEmpty) {
			var isEmpty = string.IsNullOrWhiteSpace(value);

#if UNITY_EDITOR
			if (isEmpty && !allowEmpty) {
				Debug.Log($"<color=grey>[DCTRL][SUBMIT→FDF][SKIP_EMPTY]</color> key='{key}'");
				return;
			}
			if (isEmpty)
				Debug.Log($"<color=#88f>[DCTRL][SUBMIT→FDF][EMPTY_OK]</color> key='{key}'");
			else
				Debug.Log($"<color=#88f>[DCTRL][SUBMIT→FDF]</color> key='{key}' value='{value}'");
#endif
			if (!isEmpty || allowEmpty) {
				ConstructBindings.Send_FormDataFillerData_Submit?.Invoke(key, value ?? string.Empty);
			}
		}
	}
}
