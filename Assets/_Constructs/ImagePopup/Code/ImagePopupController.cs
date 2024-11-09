using NCS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnarchyConstructFramework.Constructs.ImagePopup
{
    public class ImagePopupController : NCSBehaviour
    {
        [SerializeField] private GameObject imagePopupContainer;
        [SerializeField] private Image popupImage;
        
        private void OnEnable()
        {
            NCSEvents.DisplayImagePopup?.AddListener(DisplayImagePopup);
            NCSEvents.CloseImagePopup?.AddListener(CloseImagePopup);
        }

        private void OnDisable()
        {
            NCSEvents.DisplayImagePopup?.RemoveListener(DisplayImagePopup);
            NCSEvents.CloseImagePopup?.RemoveListener(CloseImagePopup);
        }
        
        private void DisplayImagePopup(Sprite imageSprite)
        {
            popupImage.sprite = imageSprite;
            imagePopupContainer.gameObject.SetActive(true);
        }
        
        private void CloseImagePopup()
        {
            popupImage.sprite = null;
            imagePopupContainer.gameObject.SetActive(false);
        }
    }
}
