using UnityEngine.Events;
using UnityEngine;

namespace Anarchy.Shared
{
    // This code is auto-generated. Please do not try to edit this file.
    public static class ConstructBindings
    {
        // Events for BackgroundData
        public static UnityEvent<Sprite> Send_BackgroundData_ChangeBackground = new UnityEvent<Sprite>();

        // Events for CharacterData
        public static UnityEvent<Sprite> Send_CharacterData_ChangeCharacterSprite = new UnityEvent<Sprite>();
        public static UnityEvent<string> Send_CharacterData_ChangeCharacterName = new UnityEvent<string>();
        public static UnityEvent<bool> Send_CharacterData_ToggleCharacterSprite = new UnityEvent<bool>();
        public static UnityEvent<string> Send_CharacterData_characterName_Changed = new UnityEvent<string>();

        // Events for ChoicesData
        public static UnityEvent<string> Send_ChoicesData_ShowChoicesWindow = new UnityEvent<string>();
        public static UnityEvent Send_ChoicesData_CloseChoicesWindow = new UnityEvent();
        public static UnityEvent<string, bool, string> Send_ChoicesData_AddChoice = new UnityEvent<string, bool, string>();
        public static UnityEvent Send_ChoicesData_ClearChoices = new UnityEvent();
        public static UnityEvent<string, bool, string> Send_ChoicesData_SubmitChoice = new UnityEvent<string, bool, string>();

        // Events for ConversationData
        public static UnityEvent<string, string, string, bool> Send_ConversationData_DisplayConversation = new UnityEvent<string, string, string, bool>();
        public static UnityEvent<bool> Send_ConversationData_ClearConversation = new UnityEvent<bool>();
        public static UnityEvent<string> Send_ConversationData_SubmitNode = new UnityEvent<string>();

        // Events for FormCheckerData
        public static UnityEvent<string> Send_FormCheckerData_ShowForm = new UnityEvent<string>();
        public static UnityEvent Send_FormCheckerData_CloseForm = new UnityEvent();

        // Events for FormDataFillerData
        public static UnityEvent<string> Send_FormDataFillerData_LoadFormFiller = new UnityEvent<string>();

        // Events for InputData
        public static UnityEvent<string, string> Send_InputData_ShowInputWindow = new UnityEvent<string, string>();
        public static UnityEvent Send_InputData_CloseInputWindow = new UnityEvent();
        public static UnityEvent<string, string, string> Send_InputData_SubmitInput = new UnityEvent<string, string, string>();

        // Events for MultiInputData
        public static UnityEvent<string, int, string> Send_MultiInputData_ShowMultiInputWindow = new UnityEvent<string, int, string>();
        public static UnityEvent Send_MultiInputData_CloseMultiInputWindow = new UnityEvent();
        public static UnityEvent<string, string, string, string> Send_MultiInputData_SubmitMultiInput = new UnityEvent<string, string, string, string>();

        // Events for NodeLoaderData
        public static UnityEvent<string> Send_NodeLoaderData_LoadDialogue = new UnityEvent<string>();

        // Events for StartScreenData

        // Events for StatePickerData
        public static UnityEvent Send_StatePickerData_ShowStatePickerWindow = new UnityEvent();
        public static UnityEvent Send_StatePickerData_CloseStatePickerWindow = new UnityEvent();
        public static UnityEvent<string> Send_StatePickerData_SendStateString = new UnityEvent<string>();

    }
}
