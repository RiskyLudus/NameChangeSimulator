using UnityEngine;
using UnityEngine.Audio;

namespace NameChangeSimulator.Constructs.Options
{
    public class OptionsController : MonoBehaviour
    {
        [SerializeField] private GameObject optionsMenu;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private GameObject[] objectsToToggle;

        public void ChangeMasterVolume(float slidervalue)
        {
            mixer.SetFloat("MasterVolume", Mathf.Log10(slidervalue) * 20);
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
