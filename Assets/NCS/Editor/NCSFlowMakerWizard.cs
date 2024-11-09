using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NCSEditor
{
    public class NCSFlowMakerWizard : EditorWindow
    {
        private int currentStep = 0;
        private string state = "";
        private string county = "";
        private List<Texture2D> images = new List<Texture2D>();

        [MenuItem("NCS/Create a Flow!")]
        public static void ShowWindow()
        {
            NCSFlowMakerWizard window = GetWindow<NCSFlowMakerWizard>("Flow Wizard");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Flow Wizard", EditorStyles.boldLabel);

            // Display the appropriate step based on the currentStep index
            if (currentStep == 0)
            {
                DrawWelcomeStep();
            }
            else if (currentStep == 1)
            {
                DrawLocationStep();
            }
            else if (currentStep == 2)
            {
                DrawImageUploadStep();
            }
            else
            {
                DrawImageDisplayStep(currentStep - 3); // Offset for welcome, location, and image upload steps
            }

            // Navigation buttons
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Previous") && currentStep > 0)
            {
                currentStep--;
            }

            if (GUILayout.Button("Next") && currentStep < 2 + images.Count)
            {
                currentStep++;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawWelcomeStep()
        {
            EditorGUILayout.LabelField("Hello! Welcome to the Flow Wizard.");
            EditorGUILayout.LabelField("Use the Next button to continue.");
        }

        private void DrawLocationStep()
        {
            EditorGUILayout.LabelField("Step 2: Location Information", EditorStyles.boldLabel);
            state = EditorGUILayout.TextField("State", state);
            county = EditorGUILayout.TextField("County", county);
        }

        private void DrawImageUploadStep()
        {
            EditorGUILayout.LabelField("Step 3: Upload Images", EditorStyles.boldLabel);

            if (GUILayout.Button("Add Image"))
            {
                string path = EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
                if (!string.IsNullOrEmpty(path))
                {
                    Texture2D texture = new Texture2D(2, 2);
                    byte[] imageData = System.IO.File.ReadAllBytes(path);
                    texture.LoadImage(imageData);
                    images.Add(texture);
                }
            }

            // Display images horizontally
            EditorGUILayout.BeginHorizontal();
            foreach (var image in images)
            {
                GUILayout.Label(image, GUILayout.Width(256), GUILayout.Height(256));
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawImageDisplayStep(int imageIndex)
        {
            if (imageIndex >= 0 && imageIndex < images.Count)
            {
                EditorGUILayout.LabelField($"Step {imageIndex + 4}: Display Image", EditorStyles.boldLabel);
                GUILayout.Label(images[imageIndex], GUILayout.Width(256), GUILayout.Height(256));
            }
        }
    }
}
