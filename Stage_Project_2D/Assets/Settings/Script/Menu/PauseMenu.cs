using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
   public GameObject container;
   public AudioSource buttonAudioSource;
   public float sceneChangeDelay = 0.1f;

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
        StartCoroutine(ResumeCoroutine());
    }

    public void MainMenu()
    {
        StartCoroutine(MainMenuCoroutine());
    }

    public void Settings()
    {
        StartCoroutine(SettingsCoroutine());
    }

    private IEnumerator ResumeCoroutine()
    {
        PlayButtonClick();

        yield return new WaitForSecondsRealtime(sceneChangeDelay);

        container.SetActive(false);
        Time.timeScale = 1;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
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

    private IEnumerator SettingsCoroutine()
    {
        PlayButtonClick();

        yield return new WaitForSecondsRealtime(sceneChangeDelay);

        SceneManager.LoadScene("Settings In Game", LoadSceneMode.Additive);

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
}
