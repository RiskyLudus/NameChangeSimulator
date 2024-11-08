using System;

namespace NCS.Classes
{
    [Serializable]
    public class ChoicesData
    {
        public string text;
        public string[] choices;
        public bool allowMultipleChoice = false;
    }
}
