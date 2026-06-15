using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Settings : MonoBehaviour
{
   public AudioSource buttonAudioSource;

   [Range(0f, 1f)]
   public float buttonVolume = 1f;

   public float sceneChangeDelay = 0.1f;

   public void MainMenu()
   {
        StartCoroutine(MainMenuCoroutine());
   }

   private IEnumerator MainMenuCoroutine()
   {
        if (buttonAudioSource != null && buttonAudioSource.clip != null)
        {
            buttonAudioSource.volume = buttonVolume;
            buttonAudioSource.PlayOneShot(buttonAudioSource.clip);
        }

        yield return new WaitForSecondsRealtime(sceneChangeDelay);

        SceneManager.LoadScene("Main Menu");

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
   }
}
