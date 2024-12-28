using System.Collections;
using NameChangeSimulator.Shared;
using TMPro;
using UnityEngine;

namespace NameChangeSimulator.Constructs.Dialogue.DialogueBox
{
    public class DialogueBoxController : MonoBehaviour
    {
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private float scollSpeed = 0.03f;
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_Text conversationPromptText;
        [SerializeField] private GameObject backButton;
        [SerializeField] private GameObject nextButton;

        private bool _textIsScrolling = false;
        private string _currentText = string.Empty;
        private Coroutine _coroutine = null;

        public void DisplayConversation(string dialogueText, bool showBackButton, bool showNextButton)
        {
            Debug.Log($"Showing Dialogue node");
            AudioManager.Instance.PlayShowWindow_SFX();
            conversationPromptText.text = string.Empty;
            _currentText = dialogueText;
            ScrollText(_currentText);
            container.gameObject.SetActive(true);
            ToggleButtons(showBackButton, showNextButton);
        }

        public void CloseDialogueBox()
        {
            AudioManager.Instance.PlayCloseWindow_SFX();
            container.gameObject.SetActive(false);
        }

        private void ToggleButtons(bool showBackButton, bool showNextButton)
        {
            backButton.SetActive(showBackButton);
            nextButton.SetActive(showNextButton);
        }
        
        public void SubmitBack()
        {
            dialogueController.GoToBack();
        }

        public void SubmitNext()
        {
            if (_textIsScrolling)
            {
                _textIsScrolling = !_textIsScrolling;
                ClearCoroutine();
                conversationPromptText.text = _currentText;
            }
            else
            {
                dialogueController.GoToNext();
            }
        }

        private void ClearCoroutine()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        private void ScrollText(string textToScroll)
        {
            ClearCoroutine();
            _coroutine = StartCoroutine(ScrollText_Co(textToScroll));
        }

        private IEnumerator ScrollText_Co(string textToScroll)
        {
            _textIsScrolling = true;
            
            WaitForSeconds wait = new WaitForSeconds(scollSpeed);
            var textChars = textToScroll.ToCharArray();
            bool playSound = false;
            
            foreach (var character in textChars)
            {
                conversationPromptText.text += character;
                playSound = !playSound;
                if (playSound)
                {
                    AudioManager.Instance.PlayTextBlip_SFX();
                }
                yield return wait;
            }

            _textIsScrolling = false;
        }
    }
}
