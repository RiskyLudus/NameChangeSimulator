using System.Linq;
using NameChangeSimulator.Shared.Shared.Classes;
using UnityEngine;
using UnityEngine.Serialization;

namespace NameChangeSimulator.Shared.Shared.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PDFFields", menuName = "NCS/Create PDFFieldData", order = 1)]
    public class PDFFieldData : ScriptableObject
    {
        public PDFField[] Fields
        {
            get => fields;
            set
            {
                fields = value;
                keywords = fields.Select(field => field.fieldName).ToArray();
            }
        }
        [SerializeField] private PDFField[] fields;
        
        public string[] keywords;

        public void SetValue(string keyword, string value)
        {
            foreach (var field in fields)
            {
                if (field.fieldName != keyword) continue;
                field.fieldValue = value;
                return;
            }
        }
    }
}