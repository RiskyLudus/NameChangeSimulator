using NCS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnarchyConstructFramework.Constructs.Background
{
    public class BackgroundController : NCSBehaviour
    {
        [SerializeField] private Image backgroundImage;
        
        void OnEnable()
        {
            NCSEvents.DisplayBackgroundSprite?.AddListener(DisplayBackgroundSprite);
            NCSEvents.ClearBackgroundSprite?.AddListener(ClearBackgroundSprite);
        }

        private void OnDisable()
        {
            NCSEvents.DisplayBackgroundSprite?.RemoveListener(DisplayBackgroundSprite);
            NCSEvents.ClearBackgroundSprite?.RemoveListener(ClearBackgroundSprite);
        }

        private void DisplayBackgroundSprite(Sprite sprite)
        {
            backgroundImage.sprite = sprite;
        }

        private void ClearBackgroundSprite()
        {
            backgroundImage.sprite = null;
        }
    }
}
