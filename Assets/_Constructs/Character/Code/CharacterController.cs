using System;
using NCS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnarchyConstructFramework.Constructs.Character
{
    public class CharacterController : NCSBehaviour
    {
        [SerializeField] private Image characterImage;
        
        void OnEnable()
        {
            NCSEvents.DisplayCharacterSprite?.AddListener(DisplayCharacterSprite);
            NCSEvents.ClearCharacterSprite?.AddListener(ClearCharacterSprite);
        }

        private void OnDisable()
        {
            NCSEvents.DisplayCharacterSprite?.RemoveListener(DisplayCharacterSprite);
            NCSEvents.ClearCharacterSprite?.RemoveListener(ClearCharacterSprite);
        }
        
        private void DisplayCharacterSprite(Sprite sprite)
        {
            characterImage.sprite = sprite;
        }
        
        private void ClearCharacterSprite()
        {
            characterImage.sprite = null;
        }
    }
}
