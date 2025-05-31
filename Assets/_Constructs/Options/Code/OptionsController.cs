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

        public void ChangeVoiceOverVolume(float slidervalue)
        {
            mixer.SetFloat("VOVolume", Mathf.Log10(slidervalue) * 20);
        }
        
        public void ChangeMusicVolume(float slidervalue)
        {
            mixer.SetFloat("MusicVolume", Mathf.Log10(slidervalue) * 20);
        }

        public void ChangeSFXVolume(float slidervalue)
        {
            mixer.SetFloat("SFXVolume", Mathf.Log10(slidervalue) * 20);
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
