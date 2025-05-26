using System.Linq;
using NameChangeSimulator.Shared.Shared.Classes;
using UnityEngine;
using UnityEngine.Serialization;

namespace NameChangeSimulator.Shared.Shared.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PDFFields", menuName = "NCS/Create PDFFieldData", order = 1)]
    public class PDFFieldData : ScriptableObject
    {
        public string PdfFileName;
        
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

        public void ClearValues()
        {
            foreach (var field in fields)
            {
                field.fieldValue = string.Empty;
            }
        }

        public void SetValue(string keyword, string value)
        {
            foreach (var field in fields)
            {
                if (field.fieldName == keyword)
                {
                    field.fieldValue = value;
                }
            }
        }

        public void SetOverrideValue(string keyword, string value) {
	        bool matched = false;
	        foreach (var field in fields) {
		        if (field.overrideKey == keyword) {
			        field.fieldValue = value;
			        matched = true;
			        Debug.Log($"Override matched: {keyword} -> {field.fieldName}");
		        }
	        }
	        if (!matched) {
		        Debug.LogWarning($"Override key not matched: {keyword}");
	        }
        }
	}
}