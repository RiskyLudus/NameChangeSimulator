using Anarchy.Core.Common;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Background
{
	[CreateAssetMenu(fileName = "BackgroundData", menuName = "Anarchy/Constructs/Create BackgroundData")]
	public class BackgroundData : AnarchyData
	{
		// Add public fields for this construct's data
		// Go to Anarchy/Update Bindings to use events
        [SerializeField] internal Sprite BackgroundSprite;
	}
}
