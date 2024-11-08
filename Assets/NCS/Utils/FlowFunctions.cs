using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using NCS.Classes;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine.Networking;

namespace NCS.Utils
{
    public static class FlowFunctions
    {
        /// <summary>
        /// Loads the flow data from the specified state and county.
        /// </summary>
        /// <param name="stateName">The name of the state for the flow.</param>
        /// <param name="countyName">The name of the county for the flow (optional).</param>
        /// <returns>An array of ConversationNode objects representing the flow.</returns>
        public static async Task<ConversationNode[]> LoadFlow(string stateName, string countyName = "")
        {
            Debug.Log("Loading Flow");

            // Build the path based on whether a county is provided
            string filePath;
            if (!string.IsNullOrEmpty(countyName))
            {
                filePath = Path.Combine(Application.streamingAssetsPath, "Flows", stateName, "Counties", countyName, "flow.json");
            }
            else
            {
                filePath = Path.Combine(Application.streamingAssetsPath, "Flows", stateName, "flow.json");
            }

            // Load the file asynchronously and parse it as JSON
            string json = await LoadFlowFileAsync(filePath);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("Flow data not found or empty.");
                return null;
            }

            Debug.Log("Flow data loaded successfully.");
            return ParseFlow(json);
        }

        /// <summary>
        /// Asynchronously loads a JSON file from the specified file path.
        /// </summary>
        /// <param name="filePath">The path to the JSON file in StreamingAssets.</param>
        /// <returns>The content of the JSON file as a string.</returns>
        private static async Task<string> LoadFlowFileAsync(string filePath)
        {
            Debug.Log($"Attempting to load file from path: {filePath}");
            using (UnityWebRequest request = UnityWebRequest.Get(filePath))
            {
                var asyncOp = request.SendWebRequest();

                while (!asyncOp.isDone)
                {
                    await Task.Yield(); // Yield to avoid blocking the main thread
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("File loaded successfully.");
                    return request.downloadHandler.text;
                }
                else
                {
                    Debug.LogError($"Error loading file: {request.error}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Parses the JSON string into an array of ConversationNode objects.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <returns>An array of ConversationNode objects.</returns>
        private static ConversationNode[] ParseFlow(string json)
        {
            try
            {
                ConversationData data = JsonConvert.DeserializeObject<ConversationData>(json);

                if (data == null || data.conversation == null)
                {
                    Debug.LogError("Failed to parse conversation data. JSON structure may be incorrect.");
                    return null;
                }

                Debug.Log("JSON parsed successfully with Newtonsoft.");
                return data.conversation;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception occurred during JSON parsing: {e.Message}");
                return null;
            }
        }
    }
}
