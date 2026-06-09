using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
   public GameObject container;
   
    void Start()
    {
        container.SetActive(false);
        Time.timeScale = 1;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            EventSystem.current.SetSelectedGameObject(null);
            container.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void ResumeButton()
    {
        container.SetActive(false);
        Time.timeScale = 1;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void MainMenu()
    {
        //UnityEngine.SceneManagement.SceneManager.LoadScene("Scene 1");
         EventSystem.current.SetSelectedGameObject(null);
    }
}
