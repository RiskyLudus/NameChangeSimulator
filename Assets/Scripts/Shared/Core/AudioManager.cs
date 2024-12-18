using UnityEngine;
using UnityEngine.Audio;
using Utils;

namespace NameChangeSimulator.Shared
{
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField] private AudioSource voAudioSource;
        
        [Header("Music Clips")]
        [SerializeField] private AudioClip ncsMusicClip;
        [SerializeField] private AudioClip whoareyouMusicClip;
        
        [Header("SFX Clips")]
        [SerializeField] private AudioClip startSound;
        [SerializeField] private AudioClip textBlipSound;
        [SerializeField] private AudioClip buttonEnterSound;
        [SerializeField] private AudioClip buttonExitSound;
        [SerializeField] private AudioClip showWindowSound;
        [SerializeField] private AudioClip closeWindowSound;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip formSound;
        [SerializeField] private AudioClip uiCancelSound;
        [SerializeField] private AudioClip uiConfirmSound;
        [SerializeField] private AudioClip uiHoverSound;
        [SerializeField] private AudioClip uiHoverExitSound;
        
        [Header("VO Clips")]
        [SerializeField] private AudioClip voAh;
        [SerializeField] private AudioClip voDisgust;
        [SerializeField] private AudioClip voGuh;
        [SerializeField] private AudioClip voGuh2;
        [SerializeField] private AudioClip voLaugh;
        [SerializeField] private AudioClip voMhm1;
        [SerializeField] private AudioClip voMhm2;
        [SerializeField] private AudioClip voMm1;
        [SerializeField] private AudioClip voMm2;
        [SerializeField] private AudioClip voMm3;
        [SerializeField] private AudioClip voMmmm;
        [SerializeField] private AudioClip voMmQ;
        [SerializeField] private AudioClip voScoff;
        [SerializeField] private AudioClip voSigh;

        public void PlayMusic(AudioClip clip)
        {
            musicAudioSource.clip = clip;
            musicAudioSource.Play();
        }

        public void StopMusic()
        {
            musicAudioSource.Stop();
        }

        public void PlaySfx(AudioClip clip)
        {
            sfxAudioSource.clip = clip;
            sfxAudioSource.Play();
        }

        public void StopSfx()
        {
            sfxAudioSource.Stop();
        }
        
        public void PlayVoiceOver(AudioClip clip)
        {
            voAudioSource.clip = clip;
            voAudioSource.Play();
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
        public void PlayButtonEnter_SFX() => PlaySfx(buttonEnterSound);
        public void PlayButtonExit_SFX() => PlaySfx(buttonExitSound);
        public void PlayShowWindow_SFX() => PlaySfx(showWindowSound);
        public void PlayCloseWindow_SFX() => PlaySfx(closeWindowSound);
        public void PlaySuccess_SFX() => PlaySfx(successSound);
        public void PlayForm_SFX() => PlaySfx(formSound);
        public void PlayUICancel_SFX() => PlaySfx(uiCancelSound);
        public void PlayUIConfirm_SFX() => PlaySfx(uiConfirmSound);
        public void PlayUIHover_SFX() => PlaySfx(uiHoverSound);
        public void PlayUIHoverExit_SFX() => PlaySfx(uiHoverExitSound);

        #endregion

        #region Voice Over Sounds

        public void PlayVO_Ah() => PlayVoiceOver(voAh);
        public void PlayVO_Disgust() => PlayVoiceOver(voDisgust);
        public void PlayVO_Guh() => PlayVoiceOver(voGuh);
        public void PlayVO_Guh2() => PlayVoiceOver(voGuh2);
        public void PlayVO_Laugh() => PlayVoiceOver(voLaugh);
        public void PlayVO_Mhm1() => PlayVoiceOver(voMhm1);
        public void PlayVO_Mhm2() => PlayVoiceOver(voMhm2);
        public void PlayVO_Mm1() => PlayVoiceOver(voMm1);
        public void PlayVO_Mm2() => PlayVoiceOver(voMm2);
        public void PlayVO_Mm3() => PlayVoiceOver(voMm3);
        public void PlayVO_Mmmm() => PlayVoiceOver(voMmmm);
        public void PlayVO_MmQ() => PlayVoiceOver(voMmQ);
        public void PlayVO_Scoff() => PlayVoiceOver(voScoff);
        public void PlayVO_Sigh() => PlayVoiceOver(voSigh);
        
        #endregion
        
    }
}
