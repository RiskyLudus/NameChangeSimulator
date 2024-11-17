using System;
using Anarchy.Shared;
using NameChangeSimulator.Shared;
using UnityEngine;

namespace NameChangeSimulator.Constructs.NodeLoader
{
    public class NodeLoaderController : MonoBehaviour
    {
        public void LoadNode(Node node)
        {
            if (node.CharacterName != "" && node.CharacterName != "" && node.Id != -1)
            {
                ConstructBindings.Send_ConversationData_DisplayConversation?.Invoke(node.CharacterName, node.ConversationText, node.Id);
            }

            if (node.IsInput)
            {
                ConstructBindings.Send_InputData_ShowInputWindow?.Invoke(node.Keyword, node.DataToInject[0][0], node.DataToInject[0][1]);
            }
            else
            {
                foreach (var data in node.DataToInject)
                {
                    ConstructBindings.Send_ChoicesData_AddChoice?.Invoke(data[0], data[1] == "True");
                }
                ConstructBindings.Send_ChoicesData_ShowChoicesWindow?.Invoke(node.Keyword);
            }
        }
    }
}
