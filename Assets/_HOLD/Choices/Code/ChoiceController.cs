using NCS.Core;
using UnityEngine;

namespace AnarchyConstructFramework.Constructs.Choices
{
    public class ChoiceController : NCSBehaviour
    {
        public void ToggleChoice(bool toggleState)
        {
            NCSEvents.ChoiceMade?.Invoke(this.name, toggleState);
        }
    }
}
