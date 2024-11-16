using NCS.Core;

namespace AnarchyConstructFramework.Constructs.Conversation
{
    public class NextButtonController : NCSBehaviour
    {
        public void Click()
        {
            NCSEvents.NextButtonPressed?.Invoke();
        }
    }
}
