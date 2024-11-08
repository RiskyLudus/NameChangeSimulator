using UnityEngine.Events;

namespace AnarchyConstructFramework.Core.Common
{
    public static class AnarchyEvents
    {
        public static UnityEvent TestEvent1 = new UnityEvent();
        public static UnityEvent<int> TestEvent2 = new UnityEvent<int>();
        public static UnityEvent<int,string> TestEvent3 = new UnityEvent<int, string>();
        public static UnityEvent<int, string, bool> TestEvent4 = new UnityEvent<int, string, bool>();
    }
}
