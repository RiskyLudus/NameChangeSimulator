using NameChangeSimulator.Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.Conversation
{
    public class ButtonController : MonoBehaviour
    {
        private static readonly int HoverOn = Animator.StringToHash("HoverOn");
        private static readonly int HoverOff = Animator.StringToHash("HoverOff");
        private static readonly int Click = Animator.StringToHash("Click");
        [SerializeField] private UnityEvent onClick;
        [SerializeField] private Animator animator;
        [SerializeField] private ButtonSoundType buttonType = ButtonSoundType.Cancel;

        private void OnMouseEnter()
        {
            animator.SetTrigger(HoverOn);
            AudioManager.Instance.PlayUIHover_SFX();
        }

        private void OnMouseExit()
        {
            animator.SetTrigger(HoverOff);
            AudioManager.Instance.PlayUIHoverExit_SFX();
        }

        private void OnMouseDown()
        {
            animator.SetTrigger(Click);

            switch (buttonType)
            {
                case ButtonSoundType.Cancel:
                    AudioManager.Instance.PlayUICancel_SFX();
                    break;
                case ButtonSoundType.Confirm:
                    AudioManager.Instance.PlayUIConfirm_SFX();
                    break;
                default:
                    AudioManager.Instance.PlayUICancel_SFX();
                    break;
            }
            
            onClick?.Invoke();

        }
    }

    public enum ButtonSoundType
    {
        Cancel,
        Confirm,
    }
}