using System.Collections;
using TMPro;
using UnityEngine;

namespace NameChangeSimulator.Constructs.PDFViewer
{
    [RequireComponent(typeof(TMP_Text))]
    public class WaitingTextController : MonoBehaviour
    {
        [SerializeField] private string waitingText = "Please wait while we process your forms";
        [SerializeField] private char ellipsis = '.';
        [SerializeField] private float waitTime = 0.1f;

        private TMP_Text _text;
        private Coroutine _waitingTextCoroutine;

        private void OnEnable()
        {
            _text = GetComponent<TMP_Text>();
            StartWaitingText();
        }

        private void OnDisable()
        {
            StopWaitingText();
        }

        private void StartWaitingText()
        {
            StopWaitingText();
            _waitingTextCoroutine = StartCoroutine(RunWaitingText());
        }

        private void StopWaitingText()
        {
            if (_waitingTextCoroutine != null)
            {
                StopCoroutine(_waitingTextCoroutine);
                _waitingTextCoroutine = null;
            }
        }

        private IEnumerator RunWaitingText()
        {
            WaitForSeconds wait = new WaitForSeconds(waitTime);
            int dotCount = 0;

            while (true)
            {
                dotCount = (dotCount + 1) % 4; // 0, 1, 2, 3 -> loops back to 0 after 3
                string dots = new string(ellipsis, dotCount);
                _text.text = waitingText + dots;
                yield return wait;
            }
        }
    }
}