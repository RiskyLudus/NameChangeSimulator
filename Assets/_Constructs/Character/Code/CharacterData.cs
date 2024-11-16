using Anarchy.Core.Common;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Character
{
	[CreateAssetMenu(fileName = "CharacterData", menuName = "Anarchy/Constructs/Create CharacterData")]
	public class CharacterData : AnarchyData
	{
		// Add public fields for this construct's data
		// Go to Anarchy/Update Bindings to use events
		public string characterName;
		[SerializeField] internal Sprite characterSprite;
	}
}
