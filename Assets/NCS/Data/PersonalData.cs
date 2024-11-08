using UnityEngine;

namespace NCS.Data
{
    [CreateAssetMenu(fileName = "PersonalData", menuName = "NCS/Create Personal Data")]
    public class PersonalData : ScriptableObject
    {
        public string CurrentName;
        public string NewFirstName;
        public string NewMiddleName;
        public string NewLastName;
        public string Email;
        public string[] Nicknames;
        public string StreetAddress;
        public string StreetAddress2;
        public string PhoneNumber;
        public string City;
        public string State;
        public string Zip;
        public Texture2D Signature;
    }
}
