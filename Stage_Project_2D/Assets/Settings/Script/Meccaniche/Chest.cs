using UnityEngine;
using UnityEngine.Events;

public class Chest : MonoBehaviour
{
    public Sprite openSprite;
 
    public UnityEvent openChest = new UnityEvent();
    
    private BoxCollider2D boxCollider;
    private SpriteRenderer sr;
    private bool playerInRange = false;
    private bool isOpen = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        boxCollider = GetComponentInChildren<BoxCollider2D>();

        openChest.AddListener(openChestEvent);
    }

    private void Update()
    {
        if (playerInRange && !isOpen && Input.GetKeyDown(KeyCode.E))
        {
            openChest.Invoke();
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

    public void openChestEvent()
    {
        isOpen = true;
        Debug.Log("La cassa è stata aperta!");

        sr.sprite = openSprite;

        enabled = false;
    }
}
