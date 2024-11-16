using System.Collections.Generic;
using Anarchy.Core.Common;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Choices
{
	[CreateAssetMenu(fileName = "ChoicesData", menuName = "Anarchy/Constructs/Create ChoicesData")]
	public class ChoicesData : AnarchyData
	{
		// Add public fields for this construct's data
		// Go to Anarchy/Update Bindings to use events
		[SerializeField] internal List<GameObject> choices;
	}
}
