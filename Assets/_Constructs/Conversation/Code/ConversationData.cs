using Anarchy.Core.Common;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Conversation
{
	[CreateAssetMenu(fileName = "ConversationData", menuName = "Anarchy/Constructs/Create ConversationData")]
	public class ConversationData : AnarchyData
	{
		// Add public fields for this construct's data
		// Go to Anarchy/Update Bindings to use events
		public int node = 0;
	}
}
