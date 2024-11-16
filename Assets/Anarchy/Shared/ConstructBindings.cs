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
        public static UnityEvent<string, int> Send_ChoicesData_AddChoice = new UnityEvent<string, int>();
        public static UnityEvent Send_ChoicesData_ClearChoices = new UnityEvent();
        public static UnityEvent<int> Send_ChoicesData_SubmitChoice = new UnityEvent<int>();

        // Events for ConversationData
        public static UnityEvent<string, string, int> Send_ConversationData_DisplayConversation = new UnityEvent<string, string, int>();
        public static UnityEvent<bool> Send_ConversationData_ClearConversation = new UnityEvent<bool>();
        public static UnityEvent<int> Send_ConversationData_SubmitNode = new UnityEvent<int>();
        public static UnityEvent<int> Send_ConversationData_node_Changed = new UnityEvent<int>();

        // Events for InputData
        public static UnityEvent<string, string> Send_InputData_ShowInputWindow = new UnityEvent<string, string>();
        public static UnityEvent Send_InputData_CloseInputWindow = new UnityEvent();
        public static UnityEvent<string> Send_InputData_SubmitInput = new UnityEvent<string>();

        // Events for PlayerData
        public static UnityEvent<string> Send_PlayerData_CurrentName_Changed = new UnityEvent<string>();
        public static UnityEvent<string> Send_PlayerData_NewFirstName_Changed = new UnityEvent<string>();
        public static UnityEvent<string> Send_PlayerData_NewMiddleName_Changed = new UnityEvent<string>();
        public static UnityEvent<string> Send_PlayerData_NewLastName_Changed = new UnityEvent<string>();
        public static UnityEvent<string> Send_PlayerData_Email_Changed = new UnityEvent<string>();
        public static UnityEvent<string> Send_PlayerData_NicknameOne_Changed = new UnityEvent<string>();
        public static UnityEvent<string> Send_PlayerData_NicknameTwo_Changed = new UnityEvent<string>();
        public static UnityEvent<string> Send_PlayerData_StreetAddress_Changed = new UnityEvent<string>();
        public static UnityEvent<string> Send_PlayerData_StreetAddress2_Changed = new UnityEvent<string>();
        public static UnityEvent<string> Send_PlayerData_PhoneNumber_Changed = new UnityEvent<string>();
        public static UnityEvent<string> Send_PlayerData_City_Changed = new UnityEvent<string>();
        public static UnityEvent<string> Send_PlayerData_State_Changed = new UnityEvent<string>();
        public static UnityEvent<string> Send_PlayerData_Zip_Changed = new UnityEvent<string>();
        public static UnityEvent<Texture2D> Send_PlayerData_Signature_Changed = new UnityEvent<Texture2D>();

    }
}
