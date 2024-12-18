namespace NameChangeSimulator.Shared.Shared.Classes
{
    [System.Serializable]
    public class PDFField
    {
        public string fieldName;
        public string fieldValue;
        public string fieldType;
        public string[] options; // Dropdown or Radio options
        public string overrideKey; // Custom override keyword
    }
}