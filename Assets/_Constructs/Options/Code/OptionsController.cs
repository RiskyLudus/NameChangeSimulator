using UnityEngine;
using UnityEngine.Audio;

namespace NameChangeSimulator.Constructs.Options
{
    public class OptionsController : MonoBehaviour
    {
        [SerializeField] private GameObject optionsMenu;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private GameObject[] objectsToToggle;

        public void ChangeMasterVolume(float volume)
        {
            mixer.SetFloat("MasterVolume", volume);
        }
        
        public void ChangeMusicVolume(float volume)
        {
            mixer.SetFloat("MusicVolume", volume);
        }

        public void ChangeSFXVolume(float volume)
        {
            mixer.SetFloat("SFXVolume", volume);
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
