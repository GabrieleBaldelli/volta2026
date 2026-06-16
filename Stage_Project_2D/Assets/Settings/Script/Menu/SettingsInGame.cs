using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsInGame : MonoBehaviour
{
   public AudioSource buttonAudioSource;
   public float sceneChangeDelay = 0.1f;

   [Header("Audio Mixer")]
   public AudioMixer audioMixer;
   public string masterVolumeParameter = AudioSettingsStore.MasterVolumeKey;
   public string musicVolumeParameter = AudioSettingsStore.MusicVolumeKey;
   public string sfxVolumeParameter = AudioSettingsStore.SFXVolumeKey;
   public Slider masterVolumeSlider;
   public Slider musicVolumeSlider;
   public Slider sfxVolumeSlider;

   private bool isSynchronizingAudioUI;

   private void Awake()
   {
        SyncAudioControlsFromSavedValues();
   }

   public void SetCharacterVolume(float volume)
   {
        CharacterAudioController.SetCharacterVolume(volume);
   }

   public void SetMasterVolume(float volume)
   {
        if (isSynchronizingAudioUI)
            return;

        AudioSettingsStore.SetVolume(audioMixer, masterVolumeParameter, volume);
   }

   public void SetMusicVolume(float volume)
   {
        if (isSynchronizingAudioUI)
            return;

        AudioSettingsStore.SetVolume(audioMixer, musicVolumeParameter, volume);
   }

   public void SetSFXVolume(float volume)
   {
        if (isSynchronizingAudioUI)
            return;

        AudioSettingsStore.SetVolume(audioMixer, sfxVolumeParameter, volume);
   }

   public void ResetAudioSettings()
   {
        AudioSettingsStore.ResetVolumes(audioMixer);
        SyncAudioControlsFromSavedValues();
   }

   public void MainMenu()
   {
        StartCoroutine(MainMenuCoroutine());
   }

   public void Resume()
   {
        StartCoroutine(ResumeCoroutine());
   }

   private IEnumerator MainMenuCoroutine()
   {
        PlayButtonClick();

        yield return new WaitForSecondsRealtime(sceneChangeDelay);

        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
   }

   private IEnumerator ResumeCoroutine()
   {
        PlayButtonClick();

        yield return new WaitForSecondsRealtime(sceneChangeDelay);

        SceneManager.UnloadSceneAsync("Settings In Game");

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
   }

   private void PlayButtonClick()
   {
        if (buttonAudioSource != null)
        {
            buttonAudioSource.Play();
        }
   }

   private void SetMixerVolume(string parameterName, float volume)
   {
        AudioSettingsStore.ApplyVolume(audioMixer, parameterName, volume);
   }

   private void SyncAudioControlsFromSavedValues()
   {
        isSynchronizingAudioUI = true;

        AudioSettingsStore.EnsureInitialized();

        float masterVolume = AudioSettingsStore.GetVolume(masterVolumeParameter);
        float musicVolume = AudioSettingsStore.GetVolume(musicVolumeParameter);
        float sfxVolume = AudioSettingsStore.GetVolume(sfxVolumeParameter);

        SetSliderValue(masterVolumeSlider, masterVolume);
        SetSliderValue(musicVolumeSlider, musicVolume);
        SetSliderValue(sfxVolumeSlider, sfxVolume);

        SetMixerVolume(masterVolumeParameter, masterVolume);
        SetMixerVolume(musicVolumeParameter, musicVolume);
        SetMixerVolume(sfxVolumeParameter, sfxVolume);

        isSynchronizingAudioUI = false;
   }

   private void SetSliderValue(Slider slider, float volume)
   {
        if (slider != null)
        {
            slider.SetValueWithoutNotify(Mathf.Clamp01(volume));
        }
   }
}
