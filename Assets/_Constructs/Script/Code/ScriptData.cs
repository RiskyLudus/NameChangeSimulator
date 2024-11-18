using Anarchy.Core.Common;
using NameChangeSimulator.Shared;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Script
{
	[CreateAssetMenu(fileName = "ScriptData", menuName = "Anarchy/Constructs/Create ScriptData")]
	public class ScriptData : AnarchyData
	{
		// Add public fields for this construct's data
		// Go to Anarchy/Update Bindings to use events
		public Node[] nodes;
	}
}
