using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NameChangeSimulator.Constructs.StartScreen
{
    public class StartScreenController : MonoBehaviour
    {
        [SerializeField] private StartScreenData startScreenData;
        [SerializeField] private TMP_Text flavorText;

        private Coroutine _co = null;
        
        private void Start()
        {
            GenerateRandomFlavorText();
            ClearCoroutine();
            _co = StartCoroutine(PingPongFlavorText());
        }

        private void GenerateRandomFlavorText()
        {
            int rand = Random.Range(0, startScreenData.flavorTextStrings.Length - 1);
            flavorText.text = startScreenData.flavorTextStrings[rand];
        }

        public void StartGame()
        {
            ClearCoroutine();
            gameObject.SetActive(false);
        }

        private void ClearCoroutine()
        {
            if (_co == null) return;
            StopCoroutine(_co);
            _co = null;
        }

        private IEnumerator PingPongFlavorText()
        {
            Vector3 originalScale = flavorText.transform.localScale;
            float timeElapsed = 0.0f;

            while (true)
            {
                float scaleFactor = Mathf.Lerp(1.0f, startScreenData.flavorTextScaleFactor, Mathf.PingPong(timeElapsed * startScreenData.flavorTextSpeed, 1.0f));
                flavorText.transform.localScale = originalScale * scaleFactor;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
