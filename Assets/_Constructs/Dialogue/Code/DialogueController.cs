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

		private DialogueGraph _currentDialogue;
		private StartNode _startNode;
		private DialogueNode _currentNode;
		private ImageFieldingAsset FieldAssets;

		private bool _showingProgressBar;

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
		private void HndleShowProgressBar(int arg0, int arg1) => _showingProgressBar = true;

		private void OnLoad(string dialogueToLoad) {
			container.SetActive(true);

			AudioManager.Instance.PlayWhoAreYou_Music();
			Debug.Log($"[START]Loading Dialogue: {dialogueToLoad}");

			var graph = Resources.LoadAll<DialogueGraph>("States/" + dialogueToLoad).First();
			if (graph == null) {
				Debug.LogError($"Failed to load Dialogue {dialogueToLoad}");
			}

			if (FieldAssets == null) {
				var found = Resources.LoadAll<ImageFieldingAsset>("States/" + dialogueToLoad).FirstOrDefault();
				if (found != null)
					FieldAssets = found;
			}

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

			if (valueEntered != null) {
				Debug.Log($"<color=green>[SAVE]</color> : {_currentNode.name}={valueEntered}");
				ConstructBindings.Send_FormDataFillerData_Submit?.Invoke(_currentNode.name, valueEntered);
			}

			Node nextNode = _currentNode;

			if (_currentNode is ChoiceNode choiceNode) {
				var outputData = new Dictionary<string, string>();
				foreach (var iOutput in _currentNode.DynamicOutputs) {
					int index = int.Parse(iOutput.fieldName.Replace("Options ", string.Empty));
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

			/*if (!string.IsNullOrWhiteSpace(node.Keyword)) {
				var byKeywordText = NCS.Dialog.Selector.MatchText(node.Keyword);
				if (!string.IsNullOrWhiteSpace(byKeywordText))
					return byKeywordText;
			}*/

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
			ConstructBindings.Send_DialogueData_Load?.Invoke(StateToLoad);
			ConstructBindings.Send_FormDataFillerData_Load?.Invoke(StateToLoad);
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
			var typeName = node.GetType().Name;
			_currentNode = node as DialogueNode;

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
				Debug.LogError($"Unknown node type: {typeName}");
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

		[ContextMenu("Log Dialogue Progress")]
		private void LogDialogueProgress() {
			if (_currentDialogue == null || _currentNode == null) {
				Debug.LogWarning("Dialogue graph or current node is null.");
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
	}
}
