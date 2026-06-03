using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer sr;
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Collider")]
    [SerializeField] private BoxCollider2D boxCollider;

    private bool isOpen = false;
    private bool playerInRange = false;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();

        if (sr == null || boxCollider == null)
        {
            Debug.LogWarning("Chest: SpriteRenderer o BoxCollider della chest nullo.");
        }
    }

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

            if (isOpen)
            {
                Debug.Log("isOpen --> true!");
            }
            else
            {
                Debug.Log("isOpen --> false!");
            }

            //operatore ternario: if isOpen --> true: openSprite, false: closedSprite
            sr.sprite = isOpen ? openSprite : closedSprite;
        }
    }
}
