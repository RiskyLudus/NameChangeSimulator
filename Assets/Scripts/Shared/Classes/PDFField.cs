namespace NameChangeSimulator.Shared.Shared.Classes
{
    [System.Serializable]
    public class PDFField
    {
        public string fieldName;
        public string fieldValue;
        public string fieldType;
        public string[] options; // Dropdown or radio button options or other kind of field that requires multiple inputs
    }
}