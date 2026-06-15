using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SettingsInGame : MonoBehaviour
{
   public void SetCharacterVolume(float volume)
   {
        CharacterAudioController.SetCharacterVolume(volume);
   }

   public void MainMenu()
   {
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");
        EventSystem.current.SetSelectedGameObject(null);
   }

   public void Resume()
   {
        SceneManager.UnloadSceneAsync("Settings In Game");
        EventSystem.current.SetSelectedGameObject(null);
   }
}
