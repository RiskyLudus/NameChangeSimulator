using System;
using Anarchy.Core.Common;
using Anarchy.Shared;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NameChangeSimulator.Constructs.ScreenBlocker
{
    /// <summary>
    /// Blocks all user input
    /// </summary>
    public class ScreenBlockerController : AnarchyBehaviour
    {
        [SerializeField] private GameObject blocker;
        
        private EventSystem _eventSystem = null;
        
        private void OnEnable()
        {
            ConstructBindings.Send_ScreenBlockerData_ToggleScreenBlocker.AddListener(OnScreenBlockerToggle);
        }

        void OnDisable()
        {
            ConstructBindings.Send_ScreenBlockerData_ToggleScreenBlocker.RemoveListener(OnScreenBlockerToggle);
        }

        private void Start()
        {
            TryGetComponent(out _eventSystem);
        }

        private void OnScreenBlockerToggle(bool toggle)
        {
            blocker.SetActive(toggle);
            if (_eventSystem != null)
            {
                _eventSystem.gameObject.SetActive(toggle);
            }
        }
    }
}
