using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.Options
{
    public class OptionsController : MonoBehaviour
    {
        [SerializeField] private GameObject optionsMenu;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private GameObject[] objectsToToggle;
        
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider voVolumeSlider;

        private void OnEnable()
        {
            mixer.GetFloat("MusicVolume", out float musicVolume);
            musicVolumeSlider.value = musicVolume;
            
            mixer.GetFloat("SFXVolume", out float sfxVolume);
            sfxVolumeSlider.value = sfxVolume;
            
            mixer.GetFloat("VOVolume", out float voVolume);
            voVolumeSlider.value = voVolume;
        }

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
            ToggleObjects(false);
            optionsMenu.SetActive(true);
        }

        public void CloseOptionsMenu()
        {
            ToggleObjects(true);
            optionsMenu.SetActive(false);
        }

        private void ToggleObjects(bool toggle)
        {
            foreach (var toggleObject in objectsToToggle)
            {
                toggleObject.SetActive(toggle);
            }
        }
    }
}
