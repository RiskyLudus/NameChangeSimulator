using Anarchy.Core.Common;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Player
{
	[CreateAssetMenu(fileName = "PlayerData", menuName = "Anarchy/Constructs/Create PlayerData")]
	public class PlayerData : AnarchyData
	{
		// Add public fields for this construct's data
		// Go to Anarchy/Update Bindings to use events
		public string CurrentName;
		public string NewFirstName;
		public string NewMiddleName;
		public string NewLastName;
		public string Email;
		public string NicknameOne;
		public string NicknameTwo;
		public string StreetAddress;
		public string StreetAddress2;
		public string PhoneNumber;
		public string City;
		public string State;
		public string Zip;
		public Texture2D Signature;
		
	}
}
