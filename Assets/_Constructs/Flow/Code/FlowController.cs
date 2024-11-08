using System;
using NCS.Classes;
using NCS.Core;
using NCS.Utils;
using UnityEngine;

namespace AnarchyConstructFramework.Constructs.Flow
{
    public class FlowController : NCSBehaviour
    {
        private async void Start()
        {
            ConversationNode[] nodes = await FlowFunctions.LoadFlow("Introduction");

            if (nodes == null)
            {
                Debug.LogError("Failed to load conversation nodes. Please check the JSON file and path.");
                return;
            }
            
            NCSEvents.DisplayConversationDialogue?.Invoke(nodes[0]);

            foreach (ConversationNode node in nodes)
            {
                Debug.Log($"Node ID: {node.id}");
                Debug.Log($"Type: {node.type}");
                Debug.Log($"Character: {node.character}");
                Debug.Log($"Sprite: {node.sprite}");
                Debug.Log($"Text: {node.text}");
                
                // Only log choicesData if it exists
                if (node.choicesData != null)
                {
                    Debug.Log("Choices Data:");
                    Debug.Log($"- Prompt Text: {node.choicesData.text}");
                    Debug.Log($"- Allow Multiple Choice: {node.choicesData.allowMultipleChoice}");
                    foreach (string choice in node.choicesData.choices)
                    {
                        Debug.Log($"- Choice: {choice}");
                    }
                }
                else
                {
                    Debug.Log("Choices Data: None");
                }
                
                // Only log inputData if it exists
                if (node.inputData != null)
                {
                    Debug.Log("Input Data:");
                    Debug.Log($"- Prompt Text: {node.inputData.text}");
                    Debug.Log($"- Placeholder Text: {node.inputData.placeholderText}");
                }
                else
                {
                    Debug.Log("Input Data: None");
                }

                // Log next node ID if it exists
                if (!string.IsNullOrEmpty(node.next))
                {
                    Debug.Log($"Next Node ID: {node.next}");
                }
                else
                {
                    Debug.Log("Next Node ID: None");
                }

                Debug.Log("-------------------------------");
            }
        }
    }
}
