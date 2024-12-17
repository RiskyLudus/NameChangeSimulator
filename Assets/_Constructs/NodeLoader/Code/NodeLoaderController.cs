using System;
using System.IO;
using System.Linq;
using Anarchy.Shared;
using NameChangeSimulator.Shared;
using UnityEngine;
using XNode;

namespace NameChangeSimulator.Constructs.NodeLoader
{
    public class NodeLoaderController : MonoBehaviour
    {
        [SerializeField] private IntroductionStateData introductionStateData;
        private Node _currentNode = null;

        private void OnEnable()
        {
            ConstructBindings.Send_NodeLoaderData_LoadDialogue?.AddListener(OnLoadDialogue);
            ConstructBindings.Send_ConversationData_SubmitPrevNode?.AddListener(OnSubmitPrevNode);
            ConstructBindings.Send_ConversationData_SubmitNextNode?.AddListener(OnSubmitNextNode);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_NodeLoaderData_LoadDialogue?.RemoveListener(OnLoadDialogue);
            ConstructBindings.Send_ConversationData_SubmitPrevNode?.RemoveListener(OnSubmitPrevNode);
            ConstructBindings.Send_ConversationData_SubmitNextNode?.RemoveListener(OnSubmitNextNode);
        }

        private void Start()
        {
            OnLoadDialogue("Introduction");
        }

        private void OnLoadDialogue(string stateName)
        {
            Debug.Log($"Loading Dialogue States/{stateName}");
            var graph = Resources.LoadAll<DialogueGraph>("States/" + stateName).First();
            if (graph == null)
            {
                Debug.LogError($"Failed to load Dialogue {stateName}");
            }
            else
            {
                LoadGraph(graph);
            }
            ConstructBindings.Send_FormDataFillerData_LoadFormFiller?.Invoke(stateName);
        }
        
        private void OnSubmitPrevNode(string nodeField)
        {
            Debug.Log($"Current node is {_currentNode?.name} and the node field is {nodeField}");
            ConstructBindings.Send_NodeLoaderData_CloseAllWindows?.Invoke();
            if (_currentNode != null) _currentNode = _currentNode.GetInputPort(nodeField).Connection.node;
            GoToCurrentNode();
        }
        
        private void OnSubmitNextNode(string nodeField)
        {
            Debug.Log($"Current node is {_currentNode?.name} and the node field is {nodeField}");
            if (_currentNode != null) _currentNode = _currentNode.GetOutputPort(nodeField).Connection.node;
            GoToCurrentNode();
        }

        private void LoadGraph(DialogueGraph graph)
        {
            // Get the start node to begin our flow
            foreach (var startNode in graph.nodes.OfType<StartNode>())
            {
                _currentNode = startNode.GetOutputPort("Output").Connection.node as Node;
            }
            
            GoToCurrentNode();
        }

        // Check what kind of node we're getting and parse it for data
        private void GoToCurrentNode()
        {
            var typeName = _currentNode.GetType().Name;

            switch (typeName)
            {
                case "DialogueNode":
                    SendDialogueNode();
                    break;
                case "InputNode":
                    SendInputNode();
                    break;
                case "ChoiceNode":
                    SendChoiceNode();
                    break;
                case "MultiInputNode":
                    SendMultiInputNode();
                    break;
                case "EndNode":
                    SendEndNode();
                    break;
                case "ShowStatePickerNode":
                    SendShowStatePickerNode();
                    break;
                case "LoadStateGraphNode":
                    SendLoadStateGraphNode();
                    break;
                default:
                    Debug.LogError($"Unknown node type: {typeName}");
                    break;
            }
        }

        // A standard conversation with back and next buttons active.
        private void SendDialogueNode()
        {
            var dialogueNode = _currentNode as DialogueNode;
            var conversationText = dialogueNode.DialogueText;
            var prevNodeFieldName = _currentNode.Inputs.First().fieldName;
            var nextNodeFieldName = _currentNode.Outputs.First().fieldName;
            
            ConstructBindings.Send_ConversationData_DisplayConversation?.Invoke("Default-Chan", conversationText, prevNodeFieldName, nextNodeFieldName);
            
            // Handle case to remove back button if the last node was the Start Node
            ConstructBindings.Send_ConversationData_ToggleButtons?.Invoke(!(_currentNode.GetInputPort("Input").Connection.node is StartNode), true);
        }
        
        // An input conversation. We ask the Player to input some text in a window. We have the next button on the form itself. We have a back button active in its normal place.
        private void SendInputNode()
        {
            var inputNode = _currentNode as InputNode;
            var conversationText = inputNode.DialogueText;
            var keywordName = inputNode.Keyword;
            var prevNodeFieldName = _currentNode.Inputs.First().fieldName;
            var nextNodeFieldName = _currentNode.Outputs.First().fieldName;

            Debug.Log(_currentNode.GetOutputPort(nextNodeFieldName).Connection.node.name);
            
            ConstructBindings.Send_ConversationData_DisplayConversation?.Invoke("Default-Chan", conversationText, prevNodeFieldName, "");
            ConstructBindings.Send_InputData_ShowInputWindow?.Invoke(keywordName, nextNodeFieldName);
            ConstructBindings.Send_ConversationData_ToggleButtons?.Invoke(true, false);
        }

        // A choice conversation. We ask the Player to make a button selection in a window. The button you select is also a next button. We have a back button active in its normal place.
        private void SendChoiceNode()
        {
            var choiceNode = _currentNode as ChoiceNode;
            var conversationText = choiceNode.DialogueText;
            var keywordName = choiceNode.Keyword;
            var prevNodeFieldName = _currentNode.Inputs.First().fieldName;
            
            ConstructBindings.Send_ChoicesData_ShowChoicesWindow?.Invoke(keywordName);
            ConstructBindings.Send_ConversationData_DisplayConversation?.Invoke("Default-Chan", conversationText, prevNodeFieldName, "");
            ConstructBindings.Send_ConversationData_ToggleButtons?.Invoke(true, false);
        }

        // A multi-input conversation. We ask the Player to make x number of inputs. We have the next button on the form itself. We have a back button active in its normal place.
        private void SendMultiInputNode()
        {
            var inputNode = _currentNode as MultiInputNode;
            var conversationText = inputNode.QuestionText;
            var keywordName = inputNode.Keyword;
            var numberOfInputs = inputNode.Inputs.Count;
            var prevNodeFieldName = _currentNode.Inputs.First().fieldName;
            var nextNodeFieldName = _currentNode.Outputs.First().fieldName;
            
            ConstructBindings.Send_ConversationData_DisplayConversation?.Invoke("Default-Chan", conversationText, prevNodeFieldName, "");
            ConstructBindings.Send_ConversationData_ToggleButtons?.Invoke(true, false);
            ConstructBindings.Send_MultiInputData_ShowMultiInputWindow?.Invoke(keywordName, numberOfInputs, nextNodeFieldName);
        }

        private void SendEndNode()
        {
            ConstructBindings.Send_ProgressBarData_CloseProgressBar?.Invoke();
            ConstructBindings.Send_ConversationData_ClearConversation?.Invoke(false);
            ConstructBindings.Send_FormCheckerData_ShowForm?.Invoke(introductionStateData.GetState());
        }

        // A state picker conversation. We ask the Player to select a state from a dropdown. We have the next button on the form itself. We have a back button active in its normal place.
        private void SendShowStatePickerNode()
        {
            var statePickerNode = _currentNode as ShowStatePickerNode;
            var prevNodeFieldName = _currentNode.Inputs.First().fieldName;
            var nextNodeFieldName = _currentNode.Outputs.First().fieldName;
            ConstructBindings.Send_StatePickerData_ShowStatePickerWindow?.Invoke(nextNodeFieldName);
            ConstructBindings.Send_ConversationData_DisplayConversation?.Invoke("Default-Chan", "Now finally please tell me what state form we should load?", prevNodeFieldName, "");
            ConstructBindings.Send_ConversationData_ToggleButtons?.Invoke(true, false);
        }

        private void SendLoadStateGraphNode()
        {
            ConstructBindings.Send_ProgressBarData_CloseProgressBar?.Invoke();
            ConstructBindings.Send_NodeLoaderData_LoadDialogue?.Invoke(introductionStateData.GetState());
        }
    }
}
