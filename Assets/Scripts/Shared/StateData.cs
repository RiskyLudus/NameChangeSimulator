using System.Linq;
using UnityEngine;

namespace NameChangeSimulator.Shared
{
    [CreateAssetMenu(fileName = "New StateData", menuName = "NCS/Create StateData", order = 1)]
    public class StateData : ScriptableObject
    {
        public GameObject formFieldObject;
        public Sprite formSprite;
        public Field[] fields;
    
        public int GetCompletedFields()
        {
            return fields.Count(t => t.Value != string.Empty);
        }
    }
}
