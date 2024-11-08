using System;
using NCS.Enums;
using UnityEngine;

namespace NCS.Classes
{
    [Serializable]
    public class CharacterData
    {
        public string characterName;
        public CharacterSpriteData[] characterSprites;

        public bool TryGetCharacterSprite(CharacterEmotionType emotion, out Sprite sprite)
        {
            sprite = null;
            if (characterSprites == null || characterSprites.Length <= 0) return false;
            foreach (var characterSprite in characterSprites)
            {
                if (characterSprite.emotionType != emotion) continue;
                sprite = characterSprite.emotionSprite;
                return true;
            }
            return false;
        }
    }
}
