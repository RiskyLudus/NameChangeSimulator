using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.Options
{
    public class OptionsController : MonoBehaviour
    {
        [SerializeField] private GameObject optionsMenu;
        [SerializeField] private AudioMixer mixer;

        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider voVolumeSlider;

        void Start()
        {
            float value;

            if (mixer.GetFloat("MusicVolume", out value))
                musicVolumeSlider.value = Mathf.Pow(10f, value / 20f);

            if (mixer.GetFloat("SFXVolume", out value))
                sfxVolumeSlider.value = Mathf.Pow(10f, value / 20f);

            if (mixer.GetFloat("VOVolume", out value))
                voVolumeSlider.value = Mathf.Pow(10f, value / 20f);
        }

        public void ChangeVoiceOverVolume(float sliderValue)
        {
            mixer.SetFloat("VOVolume", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20);
        }

        public void ChangeMusicVolume(float sliderValue)
        {
            mixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20);
        }

        public void ChangeSFXVolume(float sliderValue)
        {
            mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20);
        }

        public void ShowOptionsMenu()
        {
            optionsMenu.SetActive(true);
        }

        public void CloseOptionsMenu()
        {
            optionsMenu.SetActive(false);
        }

        public void Restart()
        {
            SceneManager.LoadScene(0);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}