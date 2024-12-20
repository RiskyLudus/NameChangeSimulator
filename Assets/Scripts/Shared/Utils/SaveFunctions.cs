using UnityEngine;
using System.IO;
using System.Windows.Forms; // Requires System.Windows.Forms reference
using Application = System.Windows.Forms.Application;

namespace NameChangeSimulator.Shared.Utils
{
    public static class SaveFunctions
    {
        public static void SaveOnWindows(string filePathOfObjectToSave)
        {
            // Ensure the file to save exists
            if (!File.Exists(filePathOfObjectToSave))
            {
                Debug.LogError($"File not found at path: {filePathOfObjectToSave}");
                return;
            }

            // Initialize SaveFileDialog
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Save PDF File";
                saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                saveFileDialog.FileName = Path.GetFileName(filePathOfObjectToSave); // Default file name
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(filePathOfObjectToSave); // Default folder

                // Show the dialog and process the result
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = saveFileDialog.FileName;
                    try
                    {
                        // Copy the file to the selected location
                        File.Copy(filePathOfObjectToSave, selectedPath, overwrite: true);
                        Debug.Log($"File saved successfully to: {selectedPath}");
                    }
                    catch (IOException ex)
                    {
                        Debug.LogError($"Error saving file: {ex.Message}");
                    }
                }
                else
                {
                    Debug.Log("Save operation canceled by the user.");
                }
            }
        }
    }
}