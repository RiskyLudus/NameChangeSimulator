using System;
using Anarchy.Shared;
using NameChangeSimulator.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.Character
{
    [Serializable]
    public class CharacterSprite
    {
        public CharacterSpriteType spriteType = CharacterSpriteType.None;
        public GameObject spriteObject;
    }
    
    public class CharacterController : MonoBehaviour
    {
        [SerializeField] private GameObject currentCharacterSprite;
        [SerializeField] private CharacterSprite[] characterSprites;

        private void OnEnable()
        {
            ConstructBindings.Send_CharacterData_ChangeCharacterSprite.AddListener(OnCharacterSpriteChange);
            ConstructBindings.Send_CharacterData_ToggleCharacterSprite?.AddListener(OnToggleCharacterSprite);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_CharacterData_ChangeCharacterSprite.RemoveListener(OnCharacterSpriteChange);
            ConstructBindings.Send_CharacterData_ToggleCharacterSprite?.RemoveListener(OnToggleCharacterSprite);
        }

        private void OnCharacterSpriteChange(string characterSpriteTypeString)
        {
            if (characterSpriteTypeString != CharacterSpriteType.None.ToString())
            {
                foreach (var characterSprite in characterSprites)
                {
                    characterSprite.spriteObject.SetActive(false);
                    if (characterSprite.spriteType.ToString() == characterSpriteTypeString)
                    {
                        currentCharacterSprite = characterSprite.spriteObject;
                        currentCharacterSprite.SetActive(true);
                    }
                }
            }
            else
            {
                foreach (var characterSprite in characterSprites)
                {
                    characterSprite.spriteObject.SetActive(false);
                }
            }
        }

        private void OnToggleCharacterSprite(bool toggle)
        {
            currentCharacterSprite.SetActive(toggle);
        }
    }
}
