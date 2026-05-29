using UnityEngine;

public class ChangeRoom : MonoBehaviour
{
    [Header("Camere")]
    public GameObject cameraDaSpegnere;
    public GameObject cameraDaAccendere;

    public GameObject player;

    public GameObject spawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            cameraDaSpegnere.SetActive(false);
            cameraDaAccendere.SetActive(true);

            player.transform.position = spawnPoint.transform.position;
        }
    }
}