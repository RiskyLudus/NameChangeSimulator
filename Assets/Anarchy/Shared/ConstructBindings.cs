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

        // Events for DialogueData
        public static UnityEvent<string> Send_DialogueData_Load = new UnityEvent<string>();

        // Events for FormCheckerData
        public static UnityEvent<string> Send_FormCheckerData_ShowForm = new UnityEvent<string>();
        public static UnityEvent Send_FormCheckerData_CloseForm = new UnityEvent();

        // Events for FormDataFillerData
        public static UnityEvent<string> Send_FormDataFillerData_Load = new UnityEvent<string>();
        public static UnityEvent<string, string> Send_FormDataFillerData_Submit = new UnityEvent<string, string>();
        public static UnityEvent Send_FormDataFillerData_ApplyToPDF = new UnityEvent();

        // Events for OptionsData

        // Events for ProgressBarData
        public static UnityEvent<int, int> Send_ProgressBarData_ShowProgressBar = new UnityEvent<int, int>();
        public static UnityEvent Send_ProgressBarData_CloseProgressBar = new UnityEvent();
        public static UnityEvent<int> Send_ProgressBarData_UpdateProgress = new UnityEvent<int>();

        // Events for StartScreenData

    }
}
