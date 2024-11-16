using System;
using Anarchy.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.Background
{
    public class BackgroundController : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private BackgroundData backgroundData;
        
        private void OnEnable()
        {
            ConstructBindings.Send_BackgroundData_ChangeBackground?.AddListener(OnBackgroundDataChange);
        }

        private void OnDisable()
        {
            ConstructBindings.Send_BackgroundData_ChangeBackground?.RemoveListener(OnBackgroundDataChange);
        }

        private void Start()
        {
            LoadBackground();
        }

        private void OnBackgroundDataChange(Sprite backgroundSprite)
        {
            backgroundData.BackgroundSprite = backgroundSprite;
            LoadBackground();
        }

        private void LoadBackground()
        {
            backgroundImage.sprite = backgroundData.BackgroundSprite;
        }
    }
}
