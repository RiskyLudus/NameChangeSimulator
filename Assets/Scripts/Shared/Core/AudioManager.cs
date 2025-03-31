using System;
using UnityEngine;
using UnityEngine.Audio;
using Utils;
using System.Collections.Generic;
using System.Reflection;

namespace NameChangeSimulator.Shared
{
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource voAudioSource;
        [SerializeField] private GameObject audioSourcePrefab; // Prefab with an AudioSource component
        private List<AudioSource> sfxAudioSources = new List<AudioSource>();
        private Queue<AudioSource> availableSfxSources = new Queue<AudioSource>();

        [Header("Pool Settings")]
        [SerializeField] private int initialPoolSize = 3; // Number of SFX sources to preallocate

        [Header("Music Clips")]
        [SerializeField] private AudioClip ncsMusicClip;
        [SerializeField] private AudioClip whoareyouMusicClip;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip startSound;
        [SerializeField] private AudioClip textBlipSound;
        [SerializeField] private AudioClip showWindowSound;
        [SerializeField] private AudioClip closeWindowSound;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip formSound;
        [SerializeField] private AudioClip burnSound;
        [SerializeField] private AudioClip sparkleSound;
        [SerializeField] private AudioClip uiCancelSound;
        [SerializeField] private AudioClip uiConfirmSound;
        [SerializeField] private AudioClip uiHoverSound;
        [SerializeField] private AudioClip uiHoverExitSound;

        [Header("VO Clips")]
        [SerializeField] private AudioClip voAh;
        [SerializeField] private AudioClip voHmmm;
        [SerializeField] private AudioClip voMhm;
        [SerializeField] private AudioClip voMm;
        [SerializeField] private AudioClip voMmQ;

        protected override void Awake()
        {
            base.Awake();
            InitializeAudioSourcePool();
        }

        private void InitializeAudioSourcePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewAudioSource();
            }
        }

        private void CreateNewAudioSource()
        {
            GameObject audioSourceObj = Instantiate(audioSourcePrefab, AudioManager.Instance.transform);
            audioSourceObj.SetActive(true);
            AudioSource audioSource = audioSourceObj.GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            sfxAudioSources.Add(audioSource);
            availableSfxSources.Enqueue(audioSource);
        }

        private AudioSource GetAvailableAudioSource()
        {
            if (availableSfxSources.Count == 0)
            {
                CreateNewAudioSource();
            }

            AudioSource source = availableSfxSources.Dequeue();
            return source;
        }

        private void ReturnAudioSourceToPool(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            availableSfxSources.Enqueue(source);
        }

        public void PlaySfx(AudioClip clip)
        {
            AudioSource audioSource = GetAvailableAudioSource();
            audioSource.clip = clip;
            audioSource.Play();
            StartCoroutine(ReturnToPoolAfterPlayback(audioSource));
        }

        private System.Collections.IEnumerator ReturnToPoolAfterPlayback(AudioSource source)
        {
            yield return new WaitUntil(() => !source.isPlaying);
            ReturnAudioSourceToPool(source);
        }

        public void StopAllSfx()
        {
            foreach (var source in sfxAudioSources)
            {
                source.Stop();
            }
        }

        // Music playback methods remain the same
        public void PlayMusic(AudioClip clip)
        {
            musicAudioSource.clip = clip;
            musicAudioSource.Play();
        }

        public void StopMusic()
        {
            musicAudioSource.Stop();
        }

        public void PlayVoiceOver(AudioClip clip)
        {
            voAudioSource.clip = clip;
            voAudioSource.Play();
        }

        public void PlayVoiceOver(string voiceLine)
        {
            if (string.IsNullOrEmpty(voiceLine) || voiceLine == "None")
                return;
            
            string fullVoiceLine = "vo" + voiceLine;
            Type thisClass = this.GetType();

            try
            {
                FieldInfo voiceLineField = thisClass.GetField(fullVoiceLine,
                    BindingFlags.NonPublic | BindingFlags.Instance);
                object voiceLineValue = voiceLineField.GetValue(this);
                PlayVoiceOver(voiceLineValue as AudioClip);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not find voice over {fullVoiceLine}: {e.Message}");
                PlayVO_Ah();
            }
            
        }

        public void StopVoiceOver()
        {
            voAudioSource.Stop();
        }

        #region Music Sounds
        public void PlayNCS_Music() => PlayMusic(ncsMusicClip);
        public void PlayWhoAreYou_Music() => PlayMusic(whoareyouMusicClip);
        #endregion

        #region SFX Sounds
        public void PlayStartSound_SFX() => PlaySfx(startSound);
        public void PlayTextBlip_SFX() => PlaySfx(textBlipSound);
        public void PlayShowWindow_SFX() => PlaySfx(showWindowSound);
        public void PlayCloseWindow_SFX() => PlaySfx(closeWindowSound);
        public void PlaySuccess_SFX() => PlaySfx(successSound);
        public void PlayForm_SFX() => PlaySfx(formSound);
        public void PlayBurn_SFX() => PlaySfx(burnSound);
        public void PlaySparkle_SFX() => PlaySfx(sparkleSound);
        public void PlayUICancel_SFX() => PlaySfx(uiCancelSound);
        public void PlayUIConfirm_SFX() => PlaySfx(uiConfirmSound);
        public void PlayUIHover_SFX() => PlaySfx(uiHoverSound);
        public void PlayUIHoverExit_SFX() => PlaySfx(uiHoverExitSound);
        #endregion

        #region VO Sounds
        public void PlayVO_Ah() => PlayVoiceOver(voAh);
        public void PlayVO_Hmmm() => PlayVoiceOver(voHmmm);
        public void PlayVO_Mhm1() => PlayVoiceOver(voMhm);
        public void PlayVO_Mm1() => PlayVoiceOver(voMm);
        public void PlayVO_MmQ() => PlayVoiceOver(voMmQ);
        #endregion
    }
}
