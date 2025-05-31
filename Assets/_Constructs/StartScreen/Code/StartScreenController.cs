using System;
using System.Collections;
using Anarchy.Shared;
using NameChangeSimulator.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NameChangeSimulator.Constructs.StartScreen
{
    public class StartScreenController : MonoBehaviour
    {
        [SerializeField] private float startDelayTime = 10.0f;
        [SerializeField] private float logoSpinStrength = 1.0f;

        [SerializeField] private StartScreenData startScreenData;
        [SerializeField] private TMP_Text flavorText;
        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject logo;
        [SerializeField] private SpriteRenderer fade;

        private Coroutine _co = null;
        private Vector3 _startingFlavorTextSize = Vector3.zero;

        private bool isStarting = false;

        private void OnEnable()
        {
            GenerateRandomFlavorText();
            ClearCoroutine();
            _co = StartCoroutine(PingPongFlavorText());
        }

        private void Start()
        {
            AudioManager.Instance.PlayNCS_Music();
        }

        public void PlayOnHoverSFX()
        {
            AudioManager.Instance.PlayUIHover_SFX();
        }

        public void PlayOnExitSFX()
        {
            if (!isStarting)
                AudioManager.Instance.PlayUIHoverExit_SFX();
        }

        public void GenerateRandomFlavorText()
        {
            int rand = Random.Range(0, startScreenData.flavorTextStrings.Length - 1);
            flavorText.text = startScreenData.flavorTextStrings[rand];
        }

        public void StartGame()
        {
            if (!isStarting)
            {
                isStarting = true;
                StartCoroutine(StartGameIntro());
            }
        }

        private void ClearCoroutine()
        {
            if (_co == null) return;
            StopCoroutine(_co);
            _co = null;
        }

        private IEnumerator PingPongFlavorText()
        {
            if (_startingFlavorTextSize == Vector3.zero)
            {
                _startingFlavorTextSize = logo.transform.localScale;
            }
            
            flavorText.transform.localScale = _startingFlavorTextSize;
            float timeElapsed = 0.0f;

            while (true)
            {
                float scaleFactor = Mathf.Lerp(1.0f, startScreenData.flavorTextScaleFactor, Mathf.PingPong(timeElapsed * startScreenData.flavorTextSpeed, 1.0f));
                flavorText.transform.localScale = _startingFlavorTextSize * scaleFactor;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        private IEnumerator StartGameIntro()
        {
            AudioManager.Instance.PlayStartSound_SFX();
            AudioManager.Instance.StopMusic();
            float t = 0.0f;
            Color startColor = fade.color;
            
            while (t < startDelayTime)
            {
                yield return new WaitForFixedUpdate();
                t += Time.deltaTime;
                logo.transform.Rotate(Vector3.up, logoSpinStrength * t);
                float newAlpha = Mathf.Lerp(startColor.a, 1.0f, t / startDelayTime);
                fade.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            }
            
            ConstructBindings.Send_DialogueData_Load?.Invoke("Introduction");
            Destroy(gameObject);
        }
    }
}
