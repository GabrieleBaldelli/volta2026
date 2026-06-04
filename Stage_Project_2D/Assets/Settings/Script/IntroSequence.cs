using System.Collections;
using UnityEngine;

public class IntroSequence : MonoBehaviour
{
    [Header("Player Settings")]
    public GameObject player;
    private PlayerMovement playerScript;
    private Animator playerAnimator;
    private Transform p;

    public Transform targetPosition;

    public float walkSpeed = 2f;
    public float openDoorDelay = 1f;
    public float closeDoorDelay = 1f;

    [Header("Door Settings")]
    public GameObject door;
    public Sprite openDoor;
    public Sprite closedDoor;

    private IEnumerator Start()
    {
        // Prendo il controllo dei componenti del player e della porta
        playerScript = player.GetComponent<PlayerMovement>();
        playerAnimator = player.GetComponent<Animator>();
        p = player.GetComponent<Transform>();
        Rigidbody2D playerRigidbody = player.GetComponent<Rigidbody2D>();
        SpriteRenderer SrDoor = door.GetComponent<SpriteRenderer>();
        BoxCollider2D doorCollider = door.GetComponent<BoxCollider2D>();

        // Disabilito il movimento del player
        playerRigidbody.velocity = Vector2.zero;
        playerScript.enabled = false;

        // Apro la porta
        SrDoor.sprite = openDoor;
        yield return new WaitForSeconds(openDoorDelay);
        playerAnimator.Play("Player_Run");

        // Rendo il collider della porta un trigger per permettere al player di attraversarla
        doorCollider.isTrigger = true;

        // Faccio camminare il player verso la posizione target
        while (Vector2.Distance(p.position, targetPosition.position) > 0.05f)
        {
            p.position = Vector2.MoveTowards(p.position, targetPosition.position, walkSpeed * Time.deltaTime);
            yield return null;
        }

        // Rendo il collider della porta di nuovo solido
        doorCollider.isTrigger = false;

        // Posiziono il player esattamente sulla posizione target, cambio l'animazione in idle e fermo il movimento
        p.position = targetPosition.position;
        playerAnimator.Play("Player_Idle");
        playerRigidbody.velocity = Vector2.zero;

        // Chiudo la porta e riabilito il movimento del player
        SrDoor.sprite = closedDoor;
        yield return new WaitForSeconds(closeDoorDelay);
        playerScript.enabled = true;
    }
}