using UnityEngine;
using UnityEngine.Audio;
using Utils;

namespace NameChangeSimulator.Shared
{
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;

        public void PlayMusic(AudioClip clip)
        {
            musicAudioSource.clip = clip;
            musicAudioSource.Play();
        }

        public void PlaySfx(AudioClip clip)
        {
            sfxAudioSource.clip = clip;
            sfxAudioSource.Play();
        }
    }
}
