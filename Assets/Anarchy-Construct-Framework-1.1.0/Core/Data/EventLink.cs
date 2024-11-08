using System;
using UnityEngine.Events;

namespace AnarchyConstructFramework.Core.Data
{
    // EventLink struct with 4 generics (probably should be our limit lol)
    public struct EventLink<T1, T2, T3, T4>
    {
        public UnityEvent<T1, T2, T3, T4> UnityEvent;
        public Action<T1, T2, T3, T4> Action;

        public EventLink(UnityEvent<T1, T2, T3, T4> unityEvent, Action<T1, T2, T3, T4> action)
        {
            UnityEvent = unityEvent;
            Action = action;
        }
    }

    // EventLink struct with 3 generics
    public struct EventLink<T1, T2, T3>
    {
        public UnityEvent<T1, T2, T3> UnityEvent;
        public Action<T1, T2, T3> Action;

        public EventLink(UnityEvent<T1, T2, T3> unityEvent, Action<T1, T2, T3> action)
        {
            UnityEvent = unityEvent;
            Action = action;
        }
    }

    // EventLink struct with 2 generics
    public struct EventLink<T1,T2>
    {
        public UnityEvent<T1,T2> UnityEvent;
        public Action<T1, T2> Action;

        public EventLink(UnityEvent<T1, T2> unityEvent, Action<T1, T2> action)
        {
            UnityEvent = unityEvent;
            Action = action;
        }
    }

    // EventLink struct with 1 generic
    public struct EventLink<T>
    {
        public UnityEvent<T> UnityEvent;
        public Action<T> Action;

        public EventLink(UnityEvent<T> unityEvent, Action<T> action)
        {
            UnityEvent = unityEvent;
            Action = action;
        }
    }

    // Simple EventLink struct
    public struct EventLink
    {
        public UnityEvent UnityEvent;
        public Action Action;

        public EventLink(UnityEvent unityEvent, Action action)
        {
            UnityEvent = unityEvent;
            Action = action;
        }
    }
}

