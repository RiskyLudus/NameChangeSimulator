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

        // Events for InputData
        public static UnityEvent<string, string> Send_InputData_ShowInputWindow = new UnityEvent<string, string>();
        public static UnityEvent Send_InputData_CloseInputWindow = new UnityEvent();
        public static UnityEvent<string> Send_InputData_SubmitInput = new UnityEvent<string>();

    }
}
