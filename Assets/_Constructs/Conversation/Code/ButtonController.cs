using UnityEngine;
using UnityEngine.Events;

namespace NameChangeSimulator.Constructs.Conversation
{
    public class ButtonController : MonoBehaviour
    {
        private static readonly int HoverOn = Animator.StringToHash("HoverOn");
        private static readonly int HoverOff = Animator.StringToHash("HoverOff");
        private static readonly int Click = Animator.StringToHash("Click");

        [SerializeField] private UnityEvent onClick;
        [SerializeField] private Animator animator;

        private void OnMouseEnter()
        {
            animator.SetTrigger(HoverOn);
        }

        private void OnMouseExit()
        {
            animator.SetTrigger(HoverOff);
        }

        private void OnMouseDown()
        {
            animator.SetTrigger(Click);
            onClick?.Invoke();
        }
    }
}