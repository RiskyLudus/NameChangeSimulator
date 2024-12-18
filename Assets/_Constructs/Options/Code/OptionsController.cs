using UnityEngine;
using UnityEngine.Audio;

namespace NameChangeSimulator.Constructs.Options
{
    public class OptionsController : MonoBehaviour
    {
        [SerializeField] private GameObject optionsMenu;
        [SerializeField] private AudioMixer mixer;

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
            optionsMenu.SetActive(true);
        }

        public void CloseOptionsMenu()
        {
            optionsMenu.SetActive(false);
        }
    }
}
