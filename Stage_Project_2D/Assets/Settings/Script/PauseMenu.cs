using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
   public GameObject container;
   public AudioSource buttonAudioSource;

   [Range(0f, 1f)]
   public float buttonVolume = 1f;

   public float actionDelay = 0.1f;

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
        PlayButtonSound();
        yield return new WaitForSecondsRealtime(actionDelay);

        container.SetActive(false);
        Time.timeScale = 1;
        ClearSelectedObject();
    }

    private IEnumerator MainMenuCoroutine()
    {
        PlayButtonSound();
        yield return new WaitForSecondsRealtime(actionDelay);

        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");
        ClearSelectedObject();
    }

    private IEnumerator SettingsCoroutine()
    {
        PlayButtonSound();
        yield return new WaitForSecondsRealtime(actionDelay);

        SceneManager.LoadScene("Settings In Game", LoadSceneMode.Additive);
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
