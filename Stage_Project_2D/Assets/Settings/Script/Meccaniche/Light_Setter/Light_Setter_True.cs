using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light_Setter_True : MonoBehaviour
{
    public GameObject DarkRoom;

    void Start()
    {
        if (DarkRoom == null)
            Debug.Log("DarkRoom non assegnata allo script \"Light_Room\".");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DarkRoom.SetActive(true);
        }
    }
}