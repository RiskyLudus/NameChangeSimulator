using System;
using System.Linq;
using UnityEngine;

namespace NameChangeSimulator.Shared
{
    [CreateAssetMenu(fileName = "New StateData", menuName = "NCS/Create StateData", order = 1)]
    public class StateData : ScriptableObject
    {
        public bool ResetFieldValues = false;
        public GameObject formFieldObject;
        public Sprite formSprite;
        public Field[] fields;
    
        public int GetCompletedFields()
        {
            return fields.Count(t => t.Value != string.Empty);
        }

        private void OnValidate()
        {
            if (!ResetFieldValues) return;
            ResetFieldValues = false;
            foreach (var field in fields)
            {
                field.ParentStateData = this;
                field.Value = string.Empty;
            }
        }
    }
}
