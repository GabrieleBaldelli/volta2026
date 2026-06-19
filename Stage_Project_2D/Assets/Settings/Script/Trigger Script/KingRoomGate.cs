using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KingRoomGate : MonoBehaviour
{
    [Header("Gate")]
    [SerializeField] private bool openUntilPlayerEnters = true;
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private GameObject openVisual;

    [Header("Prompt")]
    [SerializeField] private GameObject promptObject;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string lockedMessage = "Sconfiggi il Re per uscire";

    private BoxCollider2D gateCollider;
    private bool hasClosedBehindPlayer;

    private void Awake()
    {
        gateCollider = GetComponent<BoxCollider2D>();
        SetGateOpen(openUntilPlayerEnters || King.HasBeenDefeated);
        SetPromptActive(false);
    }

    private void Update()
    {
        if(King.HasBeenDefeated && !gateCollider.isTrigger)
            SetGateOpen(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(!other.CompareTag("Player") || hasClosedBehindPlayer || King.HasBeenDefeated)
            return;

        hasClosedBehindPlayer = true;
        SetGateOpen(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!collision.gameObject.CompareTag("Player") || King.HasBeenDefeated)
            return;

        ShowPrompt(lockedMessage);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
            SetPromptActive(false);
    }

    private void SetGateOpen(bool open)
    {
        gateCollider.isTrigger = open;

        if(openVisual != null)
            openVisual.SetActive(open);

        if(closedVisual != null)
            closedVisual.SetActive(!open);

        if(open)
            SetPromptActive(false);
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
