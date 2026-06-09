using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Settings : MonoBehaviour
{
   public void MainMenu()
   {
        SceneManager.LoadScene("Main Menu");
        EventSystem.current.SetSelectedGameObject(null);
   }
}
