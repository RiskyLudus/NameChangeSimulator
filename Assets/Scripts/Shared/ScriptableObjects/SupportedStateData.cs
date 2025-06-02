using System.Collections.Generic;
using UnityEngine;

namespace NameChangeSimulator.Shared
{
    [CreateAssetMenu(fileName = "SupportedStateData", menuName = "Scriptable Objects/SupportedStateData")]
    public class SupportedStateData : ScriptableObject
    {
        [Header("Supported States (Folders with Content)")]
        public List<string> supportedStates = new List<string>();

        [Header("Non-Supported States (Empty Folders)")]
        public List<string> nonSupportedStates = new List<string>();

        /// <summary>
        /// Checks if a state is supported (has content).
        /// </summary>
        public bool IsStateSupported(string stateName)
        {
            return supportedStates.Contains(stateName);
        }

        /// <summary>
        /// Gets all states (both supported and non-supported).
        /// </summary>
        public List<string> GetAllStates()
        {
            var allStates = new List<string>(supportedStates);
            allStates.AddRange(nonSupportedStates);
            allStates.Remove("Introduction");
            allStates.Remove("Ending");
            return allStates;
        }
    }
}