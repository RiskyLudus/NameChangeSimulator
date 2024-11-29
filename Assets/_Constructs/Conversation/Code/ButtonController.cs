using UnityEngine;
using UnityEngine.Events;

namespace NameChangeSimulator.Constructs.Conversation
{
    public class ButtonController : MonoBehaviour
    {
        private static readonly int HoverOn = Animator.StringToHash("HoverOn");
        private static readonly int HoverOff = Animator.StringToHash("HoverOff");
        private static readonly int Click = Animator.StringToHash("Click");
        
        [SerializeField] private AudioSource onHoverAudioSource, offHoverAudioSource, onClickAudioSource;
        [SerializeField] private UnityEvent onClick;
        [SerializeField] private Animator animator;

        private void OnMouseEnter()
        {
            animator.SetTrigger(HoverOn);
            onHoverAudioSource.Play();
        }

        private void OnMouseExit()
        {
            animator.SetTrigger(HoverOff);
            offHoverAudioSource.Play();
        }

        private void OnMouseDown()
        {
            animator.SetTrigger(Click);
            onClickAudioSource.Play();
            onClick?.Invoke();
        }
    }
}