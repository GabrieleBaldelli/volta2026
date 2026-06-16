using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unlock_Enemy : MonoBehaviour
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
                
                enemy.SetActive(true);

                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if(enemyScript != null)
                    enemyScript.PrepareForRoomUnlock();
            }
        }
    }
}
