using UnityEngine;

namespace AnarchyConstructFramework.Core.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Data", menuName = "Anarchy/Create Settings", order = 1)]
    public class AnarchySettings : ScriptableObject
    {
        public string RootNamespace = "AnarchyConstructFramework";
        public string PathToAnarchyConstructFramework = "Assets/Anarchy-Construct-Framework";
        public string PathToConstructs = "Assets/Anarchy-Construct-Framework/_Constructs";
    }
}
