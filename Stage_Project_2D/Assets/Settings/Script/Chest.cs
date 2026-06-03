using UnityEngine;

public class Chest : MonoBehaviour
{
    public Sprite closedSprite;
    public Sprite openSprite;

    [SerializeField] private SpriteRenderer sr;

    private bool isOpen = false;
    private bool playerInRange = false;

    // Quando entro nell'area del trigger posso premere "E" per aprire o chiudere il forziere
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    // Quando esco dall'area del trigger non posso più interagire con il forziere
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (sr == null)
            {
                Debug.LogWarning("Chest: assegna lo SpriteRenderer della chest nello script.");
                return;
            }

            isOpen = !isOpen;

            //operatore ternario: if isOpen --> true: openSprite, false: closedSprite
            sr.sprite = isOpen ? openSprite : closedSprite;
        }
    }
}
