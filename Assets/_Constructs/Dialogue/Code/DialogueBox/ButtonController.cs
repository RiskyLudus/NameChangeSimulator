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
        [SerializeField] private Button nextButton;
        [SerializeField] private Button backButton;

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
            if (input.backButton)
            AudioManager.Instance.PlayUICancel_SFX();
            else
            {            
                AudioManager.Instance.PlayUIConfirm_SFX();
            }
            onClick?.Invoke();

        }
    }
}