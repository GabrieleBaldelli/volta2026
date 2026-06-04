using System.Collections;
using UnityEngine;

public class IntroSequence : MonoBehaviour
{
    [Header("Player Settings")]
    public PlayerMovement playerMovement;
    public Transform player;
    public Transform targetPosition;
    public float walkSpeed = 2f;
    public float openDoorDelay = 1f;
    public float closeDoorDelay = 1f;

    [Header("Player Animation Settings")]
    public Animator playerAnimator;

    [Header("Door Settings")]
    public GameObject door;
    public Sprite openDoor;
    public Sprite closedDoor;

    private IEnumerator Start()
    {
        playerMovement = player.GetComponent<PlayerMovement>();
        playerAnimator = player.GetComponent<Animator>();
        Rigidbody2D playerRigidbody = player.GetComponent<Rigidbody2D>();
        SpriteRenderer SrDoor = door.GetComponent<SpriteRenderer>();
        BoxCollider2D doorCollider = door.GetComponent<BoxCollider2D>();

        playerRigidbody.velocity = Vector2.zero;
        playerMovement.enabled = false;

        SrDoor.sprite = openDoor;
        yield return new WaitForSeconds(openDoorDelay);
        playerAnimator.Play("Player_Run");

        doorCollider.isTrigger = true;

        while (Vector2.Distance(player.position, targetPosition.position) > 0.05f)
        {
            player.position = Vector2.MoveTowards(player.position, targetPosition.position, walkSpeed * Time.deltaTime);
            yield return null;
        }

        doorCollider.isTrigger = false;

        player.position = targetPosition.position;
        playerAnimator.Play("Player_Idle");
        playerRigidbody.velocity = Vector2.zero;

        SrDoor.sprite = closedDoor;

        yield return new WaitForSeconds(closeDoorDelay);

        playerMovement.enabled = true;
    }
}