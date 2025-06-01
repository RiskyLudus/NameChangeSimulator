using System;
using System.Collections;
using Anarchy.Shared;
using NameChangeSimulator.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NameChangeSimulator.Constructs.StartScreen
{
    public class StartScreenController : MonoBehaviour
    {
        [SerializeField] private float fadeOutDelayTime = 3.5f;
        [SerializeField] private float fadeInDelayTime = 2.0f;
        [SerializeField] private float logoSpinStrength = 1.0f;

        [SerializeField] private StartScreenData startScreenData;
        [SerializeField] private TMP_Text flavorText;
        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject logo;
        [SerializeField] private SpriteRenderer fade;
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private GameObject mainGameContainer;
        [SerializeField] private GameObject creditsButton;
        [SerializeField] private GameObject readmeButton;

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
            fade.gameObject.SetActive(true);
            eventSystem.gameObject.SetActive(false);
            
            AudioManager.Instance.PlayStartSound_SFX();
            AudioManager.Instance.StopMusic();
            float t = 0.0f;
            SpriteRenderer logoSprite = logo.transform.GetChild(0).GetComponent<SpriteRenderer>();
            
            Color startFadeColor = fade.color;
            Color startLogoColor = logoSprite.color;
            
            while (t < fadeOutDelayTime)
            {
                yield return new WaitForFixedUpdate();
                t += Time.deltaTime;
                logo.transform.Rotate(Vector3.up, logoSpinStrength * t);
                float newFadeAlpha = Mathf.Lerp(startFadeColor.a, 1.0f, t / fadeOutDelayTime);
                float newLogoAlpha = Mathf.Lerp(startLogoColor.a, 0.0f, t / fadeOutDelayTime);
                fade.color = new Color(startFadeColor.r, startFadeColor.g, startFadeColor.b, newFadeAlpha);
                logoSprite.color = new Color(startLogoColor.r, startLogoColor.g, startLogoColor.b, newLogoAlpha);
            }
            
            logo.SetActive(false);
            creditsButton.SetActive(false);
            readmeButton.SetActive(false);
            mainGameContainer.SetActive(true);
            
            t = 0.0f;
            startFadeColor = fade.color;
            
            while (t < fadeInDelayTime)
            {
                yield return new WaitForFixedUpdate();
                t += Time.deltaTime;
                float newFadeAlpha = Mathf.Lerp(startFadeColor.a, 0.0f, t / fadeInDelayTime);
                fade.color = new Color(startFadeColor.r, startFadeColor.g, startFadeColor.b, newFadeAlpha);
            }
            
            eventSystem.gameObject.SetActive(true);
            ConstructBindings.Send_DialogueData_Load?.Invoke("Introduction");
            Destroy(gameObject);
        }
    }
}
