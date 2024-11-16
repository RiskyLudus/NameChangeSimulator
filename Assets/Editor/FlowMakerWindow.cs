using UnityEditor;
using UnityEngine;

namespace NameChangeSimulator.Editor
{
    public class FlowMakerWindow : EditorWindow
    {
        [MenuItem("NCS/Flow Editor")]
        public static void ShowWindow()
        {
            GetWindow<FlowMakerWindow>("Flow Editor");
        }

        private void OnGUI()
        {
            GUILayout.Label("Flow Editor", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
        }
    }
}
