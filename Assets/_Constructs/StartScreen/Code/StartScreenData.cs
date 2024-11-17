using Anarchy.Core.Common;
using UnityEngine;

namespace NameChangeSimulator.Constructs.StartScreen
{
	[CreateAssetMenu(fileName = "StartScreenData", menuName = "Anarchy/Constructs/Create StartScreenData")]
	public class StartScreenData : AnarchyData
	{
		// Add public fields for this construct's data
		// Go to Anarchy/Update Bindings to use events
		[SerializeField] internal float flavorTextSpeed = 1.0f;
		[SerializeField] internal float flavorTextScaleFactor = 1.3f;
		[SerializeField] internal string[] flavorTextStrings;
	}
}
