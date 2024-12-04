using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NameChangeSimulator.Shared
{
    [CreateAssetMenu(fileName = "New Introduction Data", menuName = "NCS/Create Introduction StateData", order = 2)]
    public class IntroductionStateData : StateData
    {
        /// <summary>
        /// Adds the CityStateZip keyword to the fields.
        /// </summary>
        public void AddCityStateZip()
        {
            var fieldsList = new HashSet<Field>(fields)
            {
                new()
                {
                    IsText = true,
                    Name = "CityStateZip",
                    Value = GetCityStateZip()
                }
            };
            fields = fieldsList.ToArray();
        }
        
        private string GetCityStateZip()
        {
            string city = "", state = "", zip = "";
            foreach (var field in fields)
            {
                switch (field.Name)
                {
                    case "City":
                        city = field.Value.ToString();
                        break;
                    case "State":
                        state = field.Value.ToString();
                        break;
                    case "Zip":
                        zip = field.Value.ToString();
                        break;
                }
            }
            return $"{city}, {state}, {zip}";
        }

        public string GetState()
        {
            var state = "";
            foreach (var field in fields)
            {
                if (field.Name == "State")
                {
                    state =  field.Value.ToString();
                }
            }
            return state;
        }
    }
}