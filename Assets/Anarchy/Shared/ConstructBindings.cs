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
        public static UnityEvent<string> Send_CharacterData_ChangeCharacterSprite = new UnityEvent<string>();
        public static UnityEvent<bool> Send_CharacterData_ToggleCharacterSprite = new UnityEvent<bool>();

        // Events for DialogueData
        public static UnityEvent<string> Send_DialogueData_Load = new UnityEvent<string>();

        // Events for FormDataFillerData
        public static UnityEvent<string> Send_FormDataFillerData_Load = new UnityEvent<string>();
        public static UnityEvent<string, string> Send_FormDataFillerData_Submit = new UnityEvent<string, string>();
        public static UnityEvent Send_FormDataFillerData_ApplyToPDF = new UnityEvent();

        // Events for OptionsData

        // Events for PDFViewerData
        public static UnityEvent<byte[]> Send_PDFViewerData_Load = new UnityEvent<byte[]>();

        // Events for ProgressBarData
        public static UnityEvent<int, int> Send_ProgressBarData_ShowProgressBar = new UnityEvent<int, int>();
        public static UnityEvent Send_ProgressBarData_CloseProgressBar = new UnityEvent();
        public static UnityEvent<int> Send_ProgressBarData_UpdateProgress = new UnityEvent<int>();

        // Events for ScreenBlockerData
        public static UnityEvent<bool> Send_ScreenBlockerData_ToggleScreenBlocker = new UnityEvent<bool>();

        // Events for StartScreenData

    }
}
