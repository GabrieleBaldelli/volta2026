using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Settings()
    {
        SceneManager.LoadScene("Settings In Game", LoadSceneMode.Additive);
        EventSystem.current.SetSelectedGameObject(null);
    }
}
