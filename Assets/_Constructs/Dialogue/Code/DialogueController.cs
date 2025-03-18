using System;
using System.Collections.Generic;
using System.Linq;
using Anarchy.Shared;
using NameChangeSimulator.Constructs.Dialogue.ChoiceBox;
using NameChangeSimulator.Constructs.Dialogue.DialogueBox;
using NameChangeSimulator.Constructs.Dialogue.DropdownBox;
using NameChangeSimulator.Constructs.Dialogue.InputBox;
using NameChangeSimulator.Constructs.Dialogue.StatePickerBox;
using NameChangeSimulator.Shared;
using NameChangeSimulator.Shared.Node;
using UnityEngine;
using XNode;

namespace NameChangeSimulator.Constructs.Dialogue
{
    public class DialogueController : MonoBehaviour
    {
        public string StateToLoad { get; set; }
        
        [SerializeField] private DialogueBoxController dialogueBox;
        [SerializeField] private DeadNameInputBoxController deadNameInputBox;
        [SerializeField] private NewNameInputBoxController newNameInputBox;
        [SerializeField] private StatePickerBoxController statePickerBox;
        [SerializeField] private InputBoxController inputBox;
        [SerializeField] private DropdownBoxController dropdownBox;
        [SerializeField] private ChoiceBoxController choiceBox;
        
        private DialogueGraph _currentDialogue;
        private StartNode _startNode;
        private DialogueNode _currentNode;
        
        private void OnEnable()
        {
            ConstructBindings.Send_DialogueData_Load?.AddListener(OnLoad);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_DialogueData_Load?.RemoveListener(OnLoad);
        }

        private void OnLoad(string dialogueToLoad)
        {
            AudioManager.Instance.PlayWhoAreYou_Music();
            Debug.Log($"Loading Dialogue: {dialogueToLoad}");
            
            // Load the Dialogue Graph
            var graph = Resources.LoadAll<DialogueGraph>("States/" + dialogueToLoad).First();
            if (graph == null)
            {
                Debug.LogError($"Failed to load Dialogue {dialogueToLoad}");
            }
            
            // Find our Start Node so we know where we start.
            foreach (var startNode in graph.nodes.OfType<StartNode>())
            {
                _startNode = startNode;
                SetCurrentNode(startNode.GetOutputPort("Output").Connection.node);
            }
        }

        public void GoToBack()
        {
            if (_currentNode == null) return;
         
            CloseAll();
            
            var previousNode = _currentNode.GetInputPort("Input").Connection.node;

            if (previousNode is DialogueNode dialogueNode)
            {
                // We are checking to see if the node has a keyword. This is dynamically given a value.
                // We then iterate to the previous node and try again.
                if (dialogueNode.Keyword != string.Empty)
                {
                    Debug.Log($"Going back to {previousNode.GetInputPort("OverrideInput").Connection.node.name}");
                    previousNode = previousNode.GetInputPort("OverrideInput").Connection.node;
                    _currentNode = previousNode as DialogueNode;
                    GoToBack();
                }
                else
                {
                    SetCurrentNode(previousNode);
                }
            }
            else
            {
                SetCurrentNode(previousNode);
            }
        }

        public void GoToNext(string valueEntered = null)
        {
            if (_currentNode == null) return;

            if (valueEntered != null)
            {
                ConstructBindings.Send_FormDataFillerData_Submit?.Invoke(_currentNode.name, valueEntered);
            }

            Node nextNode = _currentNode;
            
            if (_currentNode is ChoiceNode choiceNode)
            {
                Dictionary<string, string> outputData = new Dictionary<string, string>();
            
                foreach (var iOutput in _currentNode.DynamicOutputs)
                {
                    int index = int.Parse(iOutput.fieldName.Replace("Options ", string.Empty));
                    string optionValue = choiceNode.Options[index];
                    
                    Debug.Log($"Adding option {iOutput.fieldName} to outputdata: {optionValue}");
                    outputData.Add(iOutput.fieldName, optionValue);
                }
                
                foreach (var output in outputData.Where(output => output.Value == valueEntered))
                {
                    nextNode = _currentNode.DynamicOutputs.First(op => op.fieldName == output.Key).Connection.node;
                }
            }
            else
            {
                nextNode = _currentNode.GetOutputPort("Output").Connection.node;
            }

            if (nextNode is DialogueNode dialogueNode)
            {
                // We are checking to see if the node has a keyword. This is dynamically given a value.
                // We then iterate to the previous node and try again.
                if (dialogueNode.Keyword != string.Empty)
                {
                    _currentNode = dialogueNode;
                    GoToNext();
                }
                else
                {
                    SetCurrentNode(nextNode);
                }
            }
            else
            {
                SetCurrentNode(nextNode);
            }
        }
        
        // A standard conversation with back and next buttons.
        private void SetDialogueNode(DialogueNode dialogueNode, bool showBackButton = true, bool showNextButton = true)
        {
            if (dialogueNode.GetInputPort("Input").Connection.node == _startNode)
            {
                showBackButton = false;
            }
            if (dialogueNode != null) dialogueBox.DisplayConversation(dialogueNode.DialogueText, showBackButton, showNextButton);

            if (dialogueNode.VoiceLine != VoiceLineType.None)
            {
                AudioManager.Instance.PlayVoiceOver(dialogueNode.VoiceLine.ToString());
            }
        }

        private void SetDeadNameInputNode(DeadNameInputNode deadNameInputNode)
        {
            SetDialogueNode(deadNameInputNode, true, false);
            deadNameInputBox.DisplayDeadNameInputWindow();
        }

        private void SetNewNameInputNode(NewNameInputNode newNameInputNode)
        {
            SetDialogueNode(newNameInputNode, true, false);
            newNameInputBox.DisplayNewNameInputWindow();
        }
        
        // A state picker conversation. We ask the Player to select a state from a dropdown. We have the next button on the form itself. We have a back button active in its normal place.
        private void SetShowStatePickerNode(ShowStatePickerNode statePickerNode)
        {
            SetDialogueNode(statePickerNode, false, false);
            statePickerBox.DisplayStatePicker();
        }
        
        private void SetLoadStateGraphNode()
        {
            ConstructBindings.Send_DialogueData_Load?.Invoke(StateToLoad);
            ConstructBindings.Send_FormDataFillerData_Load?.Invoke(StateToLoad);
        }
        
        // An input conversation. We ask the Player to input some text in a window. We have the next button on the form itself. We have a back button active in its normal place.
        private void SetInputNode(InputNode inputNode)
        {
            SetDialogueNode(inputNode, true, false);
            inputBox.DisplayInputWindow();
        }

        private void SetDropdownNode(DropdownNode dropdownNode)
        {
            SetDialogueNode(dropdownNode, true, false);
            dropdownBox.DisplayDropdownWindow(dropdownNode.Options);
        }

        private void SetChoiceNode(ChoiceNode choiceNode)
        {
            SetDialogueNode(choiceNode, true, false);
            choiceBox.DisplayChoicesWindow(choiceNode.Options);
        }
        
        // Check what kind of node we're getting and parse it for data
        private void SetCurrentNode(Node node)
        {
            var typeName = node.GetType().Name;
            _currentNode = node as DialogueNode;
            
            switch (typeName)
            {
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
                default:
                    Debug.LogError($"Unknown node type: {typeName}");
                    break;
            }
        }

        private void CloseAll()
        {
            dialogueBox.CloseDialogueBox();
            deadNameInputBox.Close();
            newNameInputBox.Close();
            statePickerBox.Close();
            inputBox.Close();
            dropdownBox.Close();
            choiceBox.Close();
        }
    }
}
