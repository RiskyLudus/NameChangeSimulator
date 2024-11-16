using System;
using NCS.Enums;
using UnityEngine;

namespace NCS.Classes
{
    [Serializable]
    public class CharacterSpriteData
    {
        public CharacterPoseType emotionType = CharacterPoseType.Idle;
        public Sprite emotionSprite;
    }
}