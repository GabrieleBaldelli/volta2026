using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ChangeScene : MonoBehaviour
{
    public string Scene;
    public AudioClip musicToPlay;

    public void OnTriggerEnter2D(Collider2D other)
   {
        if (other.CompareTag("Player"))
        {
            if (musicToPlay != null)
                BackgroundMusicManager.Instance.PlayMusic(musicToPlay);

            SceneManager.LoadScene(Scene);
            EventSystem.current.SetSelectedGameObject(null);
        }
        
   }
}
