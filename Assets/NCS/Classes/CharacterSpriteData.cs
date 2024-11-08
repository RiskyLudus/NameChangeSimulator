using System;
using NCS.Enums;
using UnityEngine;

namespace NCS.Classes
{
    [Serializable]
    public class CharacterSpriteData
    {
        public CharacterEmotionType emotionType = CharacterEmotionType.Neutral;
        public Sprite emotionSprite;
    }
}