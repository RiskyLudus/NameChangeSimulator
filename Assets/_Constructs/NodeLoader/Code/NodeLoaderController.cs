using System;
using System.Linq;
using Anarchy.Shared;
using UnityEngine;
using XNode;

namespace NameChangeSimulator.Constructs.NodeLoader
{
    public class NodeLoaderController : MonoBehaviour
    {
        private Node _currentNode = null;

        private void OnEnable()
        {
            ConstructBindings.Send_NodeLoaderData_LoadDialogue?.AddListener(OnLoadDialogue);
            ConstructBindings.Send_ConversationData_SubmitNode?.AddListener(OnSubmitNode);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_NodeLoaderData_LoadDialogue?.RemoveListener(OnLoadDialogue);
            ConstructBindings.Send_ConversationData_SubmitNode?.RemoveListener(OnSubmitNode);
        }

        private void OnLoadDialogue(string stateName)
        {
            LoadGraph(Resources.LoadAll<DialogueGraph>($"States/{stateName}/").First());
            ConstructBindings.Send_FormDataFillerData_LoadFormFiller?.Invoke(stateName);
        }
        
        private void OnSubmitNode(string nodeField)
        {
            Debug.Log($"Current node is {_currentNode?.name} and the node field is {nodeField}");
            _currentNode = _currentNode.GetOutputPort(nodeField).Connection.node;
            GoToCurrentNode();
        }

        private void Start()
        {
            OnLoadDialogue("Oregon");
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
                default:
                    Debug.LogError($"Unknown node type: {typeName}");
                    break;
            }
        }

        private void SendDialogueNode()
        {
            var dialogueNode = _currentNode as DialogueNode;
            var conversationText = dialogueNode.DialogueText;
            var nextNodeFieldName = _currentNode.Outputs.First().fieldName;
            
            ConstructBindings.Send_ConversationData_DisplayConversation?.Invoke("Default-Chan", conversationText, nextNodeFieldName, true);
        }
        
        private void SendInputNode()
        {
            var inputNode = _currentNode as InputNode;
            var conversationText = inputNode.QuestionText;
            var keywordName = inputNode.Keyword;
            var nextNodeFieldName = _currentNode.Outputs.First().fieldName;

            Debug.Log(_currentNode.GetOutputPort(nextNodeFieldName).Connection.node.name);
            
            
            ConstructBindings.Send_ConversationData_DisplayConversation?.Invoke("Default-Chan", conversationText, "", false);
            ConstructBindings.Send_InputData_ShowInputWindow?.Invoke(keywordName, nextNodeFieldName);
        }

        private void SendChoiceNode()
        {
            var choiceNode = _currentNode as ChoiceNode;
            var conversationText = choiceNode.QuestionText;
            var keywordName = choiceNode.Keyword;
            
            ConstructBindings.Send_ChoicesData_ShowChoicesWindow?.Invoke(keywordName);
            ConstructBindings.Send_ConversationData_DisplayConversation?.Invoke("Default-Chan", conversationText, "", false);

            foreach (var choice in choiceNode.Choices)
            {
                ConstructBindings.Send_ChoicesData_AddChoice?.Invoke(choice.Prompt, choice.Value, choice.PortFieldName);
            }
        }

        private void SendMultiInputNode()
        {
            var inputNode = _currentNode as MultiInputNode;
            var conversationText = inputNode.QuestionText;
            var keywordName = inputNode.Keyword;
            var numberOfInputs = inputNode.Inputs.Count;
            var nextNodeFieldName = _currentNode.Outputs.First().fieldName;
            
            ConstructBindings.Send_ConversationData_DisplayConversation?.Invoke("Default-Chan", conversationText, "", false);
            ConstructBindings.Send_MultiInputData_ShowMultiInputWindow?.Invoke(keywordName, numberOfInputs, nextNodeFieldName);
        }

        private void SendEndNode()
        {
            ConstructBindings.Send_ConversationData_DisplayConversation?.Invoke("Default-Chan", "Congratulations on your name change! Let's get those forms ready for you...", "", false);
            ConstructBindings.Send_FormCheckerData_ShowForm?.Invoke("Oregon");
        }
    }
}
