using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ChangeScene : MonoBehaviour
{
    public string Scene;

    public void OnTriggerEnter2D(Collider2D other)
   {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(Scene);
            EventSystem.current.SetSelectedGameObject(null);
        }
        
   }
}
