using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Settings : MonoBehaviour
{
   public AudioSource buttonAudioSource;
   public float sceneChangeDelay = 0.1f;

   public void SetCharacterVolume(float volume)
   {
        CharacterAudioController.SetCharacterVolume(volume);
   }

   public void MainMenu()
   {
        StartCoroutine(MainMenuCoroutine());
   }

   private IEnumerator MainMenuCoroutine()
   {
        if (buttonAudioSource != null)
        {
            buttonAudioSource.Play();
        }

        yield return new WaitForSecondsRealtime(sceneChangeDelay);

        SceneManager.LoadScene("Main Menu");

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
   }
}
