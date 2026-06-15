using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SettingsInGame : MonoBehaviour
{
   public AudioSource buttonAudioSource;

   [Range(0f, 1f)]
   public float buttonVolume = 1f;

   public float actionDelay = 0.1f;

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
        PlayButtonSound();
        yield return new WaitForSecondsRealtime(actionDelay);

        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");
        ClearSelectedObject();
   }

   private IEnumerator ResumeCoroutine()
   {
        PlayButtonSound();
        yield return new WaitForSecondsRealtime(actionDelay);

        SceneManager.UnloadSceneAsync("Settings In Game");
        ClearSelectedObject();
   }

   private void PlayButtonSound()
   {
        if (buttonAudioSource != null && buttonAudioSource.clip != null)
        {
            buttonAudioSource.volume = buttonVolume;
            buttonAudioSource.PlayOneShot(buttonAudioSource.clip);
        }
   }

   private void ClearSelectedObject()
   {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
   }
}
