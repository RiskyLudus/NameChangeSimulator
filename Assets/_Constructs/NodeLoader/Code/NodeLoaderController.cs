using System;
using Anarchy.Shared;
using UnityEngine;

namespace NameChangeSimulator.Constructs.NodeLoader
{
    public class NodeLoaderController : MonoBehaviour
    {
        [Header("DEBUG")] [SerializeField] private bool testDisplay = false;

        private void Update()
        {
            if (testDisplay)
            {
                testDisplay = false;
                LoadUI();
            }
        }

        public void LoadUI()
        {
            ConstructBindings.Send_ChoicesData_ShowChoicesWindow?.Invoke("keyword", "placeholder");
            ConstructBindings.Send_ChoicesData_AddChoice?.Invoke("prompt", true);
            ConstructBindings.Send_ChoicesData_AddChoice?.Invoke("prompt", false);
            ConstructBindings.Send_InputData_ShowInputWindow?.Invoke("keyword", "prompt", "placeholder");
            ConstructBindings.Send_ConversationData_DisplayConversation?.Invoke("character name", "prompt", 1);
            ConstructBindings.Send_CharacterData_ToggleCharacterSprite?.Invoke(true);
        }
    }
}
