using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock_Enemy : MonoBehaviour
{
    public GameObject[] Enemies;

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            foreach (GameObject enemy in Enemies)
            {
                if(enemy == null)
                    continue;

                enemy.SetActive(false);
            }
        }
    }
}
