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
        private List<Rect> inputFields = new List<Rect>();
        private List<Rect> choiceFields = new List<Rect>();

        private bool isDraggingBox = false;
        private bool isResizingBox = false;
        private int selectedBoxIndex = -1;
        private Vector2 dragOffset;
        private Vector2 resizeStart;

        private enum ResizeDirection { None, Left, Right, Top, Bottom, TopLeft, TopRight, BottomLeft, BottomRight }
        private ResizeDirection resizeDirection = ResizeDirection.None;

        [MenuItem("NCS/Create a Flow!")]
        public static void ShowWindow()
        {
            NCSFlowMakerWizard window = GetWindow<NCSFlowMakerWizard>("Flow Wizard");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Flow Wizard", EditorStyles.boldLabel);

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
                DrawImageDisplayStep(currentStep - 3);
            }

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

            // Display all images as thumbnails
            EditorGUILayout.LabelField("Uploaded Images:");
            EditorGUILayout.BeginHorizontal();
            foreach (var image in images)
            {
                if (GUILayout.Button(image, GUILayout.Width(100), GUILayout.Height(100)))
                {
                    // Clicking a thumbnail sets the view to that specific image step
                    currentStep = 3 + images.IndexOf(image);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawImageDisplayStep(int imageIndex)
        {
            if (imageIndex < 0 || imageIndex >= images.Count) return;

            Texture2D imageToDisplay = images[imageIndex];

            // Set image size to 1024x1024 and center it
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(imageToDisplay, GUILayout.Width(1024), GUILayout.Height(1024));
            GUILayout.FlexibleSpace();

            // Add Input Field and Choice Field buttons on the right side
            EditorGUILayout.BeginVertical();

            if (GUILayout.Button("Add Input Field", GUILayout.Width(120)))
            {
                // Add a yellow input field box
                inputFields.Add(new Rect(100, 100, 100, 50)); // Default position and size for simplicity
            }

            if (GUILayout.Button("Add Choice Field", GUILayout.Width(120)))
            {
                // Add a red choice field box
                choiceFields.Add(new Rect(150, 150, 50, 50)); // Default position and size for simplicity
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            // Draw boxes on the image
            DrawBoxes();
            HandleBoxInteraction();
        }

        private void DrawBoxes()
        {
            // Draw input fields as yellow boxes
            for (int i = 0; i < inputFields.Count; i++)
            {
                EditorGUI.DrawRect(inputFields[i], new Color(1, 1, 0, 0.5f));
            }

            // Draw choice fields as red boxes
            foreach (var rect in choiceFields)
            {
                EditorGUI.DrawRect(rect, new Color(1, 0, 0, 0.5f));
            }
        }

        private void HandleBoxInteraction()
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                for (int i = 0; i < inputFields.Count; i++)
                {
                    if (inputFields[i].Contains(e.mousePosition))
                    {
                        // Check for resizing near edges or corners
                        selectedBoxIndex = i;
                        resizeDirection = GetResizeDirection(inputFields[i], e.mousePosition);
                        if (resizeDirection != ResizeDirection.None)
                        {
                            isResizingBox = true;
                            resizeStart = e.mousePosition;
                            e.Use();
                            return;
                        }

                        // Start dragging if no resize direction detected
                        isDraggingBox = true;
                        dragOffset = e.mousePosition - new Vector2(inputFields[i].x, inputFields[i].y);
                        e.Use();
                        return;
                    }
                }
            }

            if (e.type == EventType.MouseDrag && isDraggingBox && selectedBoxIndex != -1)
            {
                // Retrieve, modify, and reassign the box
                Rect box = inputFields[selectedBoxIndex];
                box.position = e.mousePosition - dragOffset;
                inputFields[selectedBoxIndex] = box;
                e.Use();
                Repaint();
            }
            else if (e.type == EventType.MouseDrag && isResizingBox && selectedBoxIndex != -1)
            {
                ResizeBox(e.mousePosition);
                e.Use();
                Repaint();
            }

            if (e.type == EventType.MouseUp)
            {
                isDraggingBox = false;
                isResizingBox = false;
                selectedBoxIndex = -1;
                resizeDirection = ResizeDirection.None;
                e.Use();
            }

            // Change cursor based on hover position
            if (e.type == EventType.MouseMove && selectedBoxIndex == -1)
            {
                for (int i = 0; i < inputFields.Count; i++)
                {
                    ResizeDirection direction = GetResizeDirection(inputFields[i], e.mousePosition);
                    if (direction != ResizeDirection.None)
                    {
                        SetCursorForDirection(direction);
                        Repaint();
                        return;
                    }
                }
                EditorGUIUtility.AddCursorRect(new Rect(e.mousePosition, Vector2.one), MouseCursor.Arrow);
            }
        }


        private ResizeDirection GetResizeDirection(Rect box, Vector2 mousePos)
        {
            float edgeThreshold = 10f;

            bool nearLeft = Mathf.Abs(mousePos.x - box.x) < edgeThreshold;
            bool nearRight = Mathf.Abs(mousePos.x - (box.x + box.width)) < edgeThreshold;
            bool nearTop = Mathf.Abs(mousePos.y - box.y) < edgeThreshold;
            bool nearBottom = Mathf.Abs(mousePos.y - (box.y + box.height)) < edgeThreshold;

            if (nearLeft && nearTop) return ResizeDirection.TopLeft;
            if (nearRight && nearTop) return ResizeDirection.TopRight;
            if (nearLeft && nearBottom) return ResizeDirection.BottomLeft;
            if (nearRight && nearBottom) return ResizeDirection.BottomRight;
            if (nearLeft) return ResizeDirection.Left;
            if (nearRight) return ResizeDirection.Right;
            if (nearTop) return ResizeDirection.Top;
            if (nearBottom) return ResizeDirection.Bottom;

            return ResizeDirection.None;
        }

        private void ResizeBox(Vector2 mousePos)
        {
            // Retrieve the struct by value, modify it, then store it back in the list
            Rect box = inputFields[selectedBoxIndex];

            switch (resizeDirection)
            {
                case ResizeDirection.Left:
                    box.xMin = mousePos.x;
                    break;
                case ResizeDirection.Right:
                    box.xMax = mousePos.x;
                    break;
                case ResizeDirection.Top:
                    box.yMin = mousePos.y;
                    break;
                case ResizeDirection.Bottom:
                    box.yMax = mousePos.y;
                    break;
                case ResizeDirection.TopLeft:
                    box.xMin = mousePos.x;
                    box.yMin = mousePos.y;
                    break;
                case ResizeDirection.TopRight:
                    box.xMax = mousePos.x;
                    box.yMin = mousePos.y;
                    break;
                case ResizeDirection.BottomLeft:
                    box.xMin = mousePos.x;
                    box.yMax = mousePos.y;
                    break;
                case ResizeDirection.BottomRight:
                    box.xMax = mousePos.x;
                    box.yMax = mousePos.y;
                    break;
            }

            inputFields[selectedBoxIndex] = box; // Assign the modified struct back to the list
        }


        private void SetCursorForDirection(ResizeDirection direction)
        {
            MouseCursor cursor = MouseCursor.Arrow;

            switch (direction)
            {
                case ResizeDirection.Left:
                case ResizeDirection.Right:
                    cursor = MouseCursor.ResizeHorizontal;
                    break;
                case ResizeDirection.Top:
                case ResizeDirection.Bottom:
                    cursor = MouseCursor.ResizeVertical;
                    break;
                case ResizeDirection.TopLeft:
                case ResizeDirection.BottomRight:
                    cursor = MouseCursor.ResizeUpLeft;
                    break;
                case ResizeDirection.TopRight:
                case ResizeDirection.BottomLeft:
                    cursor = MouseCursor.ResizeUpRight;
                    break;
            }

            EditorGUIUtility.AddCursorRect(new Rect(Event.current.mousePosition, Vector2.one), cursor);
        }
    }
}
