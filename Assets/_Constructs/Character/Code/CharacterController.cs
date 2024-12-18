using System;
using Anarchy.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.Character
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField] private CharacterData characterData;
        [SerializeField] private Image characterImage;

        private void OnEnable()
        {
            ConstructBindings.Send_CharacterData_ChangeCharacterSprite.AddListener(OnCharacterSpriteChange);
            ConstructBindings.Send_CharacterData_ChangeCharacterName.AddListener(OnCharacterNameChange);
            ConstructBindings.Send_CharacterData_ToggleCharacterSprite?.AddListener(OnToggleCharacterSprite);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_CharacterData_ChangeCharacterSprite.RemoveListener(OnCharacterSpriteChange);
            ConstructBindings.Send_CharacterData_ChangeCharacterName.RemoveListener(OnCharacterNameChange);
            ConstructBindings.Send_CharacterData_ToggleCharacterSprite?.RemoveListener(OnToggleCharacterSprite);
        }

        private void Start()
        {
            LoadCharacterSprite();
        }
        
        private void OnCharacterSpriteChange(Sprite characterSprite)
        {
            characterData.characterSprite = characterSprite;
            LoadCharacterSprite();
        }
        
        private void OnCharacterNameChange(string characterName)
        {
            characterData.characterName = characterName;
        }
        
        private void OnToggleCharacterSprite(bool toggle)
        {
            characterImage.enabled = toggle;
        }
        
        private void LoadCharacterSprite()
        {
            characterImage.sprite = characterData.characterSprite;
        }
    }
}
