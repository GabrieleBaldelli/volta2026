using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KeyDoor : MonoBehaviour, Interactable
{
    [Header("Key")]
    [SerializeField] private string requiredKeyId = "FinalRoomKey";
    [SerializeField] private bool consumeKeyOnUse;

    [Header("Prompt")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject promptObject;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string useKeyMessage = "Premi E per usare la chiave";
    [SerializeField] private string missingKeyMessage = "Ti manca la chiave";
    [SerializeField] private string unlockedMessage = "Passaggio sbloccato";

    private BoxCollider2D doorCollider;
    private bool playerIsNear;
    private bool isUnlocked;

    public bool CanInteract()
    {
        return enabled && playerIsNear && !isUnlocked;
    }

    public void Interact()
    {
        if(isUnlocked)
            return;

        if(!StoryKeyInventory.HasKey(requiredKeyId))
        {
            ShowPrompt(missingKeyMessage);
            return;
        }

        UnlockPassage();
    }

    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        doorCollider.isTrigger = false;
        SetPromptActive(false);
    }

    private void Update()
    {
        if(playerIsNear && !isUnlocked && Input.GetKeyDown(interactKey))
            Interact();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TrySetPlayerNear(collision.gameObject, true);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TrySetPlayerNear(collision.gameObject, true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        TrySetPlayerNear(collision.gameObject, false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TrySetPlayerNear(other.gameObject, true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TrySetPlayerNear(other.gameObject, false);
    }

    private void TrySetPlayerNear(GameObject other, bool near)
    {
        if(other == null || !other.CompareTag("Player"))
            return;

        playerIsNear = near;

        if(!playerIsNear || isUnlocked)
        {
            SetPromptActive(false);
            return;
        }

        RefreshPrompt();
    }

    private void RefreshPrompt()
    {
        ShowPrompt(StoryKeyInventory.HasKey(requiredKeyId) ? useKeyMessage : missingKeyMessage);
    }

    private void UnlockPassage()
    {
        isUnlocked = true;

        if(consumeKeyOnUse)
            StoryKeyInventory.ConsumeKey(requiredKeyId);

        doorCollider.isTrigger = true;
        ShowPrompt(unlockedMessage);
    }

    private void ShowPrompt(string message)
    {
        if(promptText != null)
            promptText.SetText(message);

        SetPromptActive(true);
    }

    private void SetPromptActive(bool active)
    {
        if(promptObject != null)
            promptObject.SetActive(active);
    }
}
