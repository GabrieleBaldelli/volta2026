using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public Transform interactionArea;
    public float interactionRadius = 1f;
    public LayerMask interactableLayers = ~0;
    public KeyCode interactKey = KeyCode.E;

    private Interactable currentInteractable;
    private Interactable activeInteractable;

    void Awake()
    {
        if(interactionArea == null)
        {
            Transform foundInteractionArea = transform.Find("Interaction Area");

            if(foundInteractionArea != null)
                interactionArea = foundInteractionArea;
        }
    }

    void Update()
    {
        if(activeInteractable == null)
            currentInteractable = FindBestInteractable();

        if(Input.GetKeyDown(interactKey))
        {
            if(activeInteractable != null)
            {
                activeInteractable.Interact();

                if(activeInteractable.CanInteract())
                    activeInteractable = null;

                return;
            }

            if(currentInteractable != null)
            {
                activeInteractable = currentInteractable;
                activeInteractable.Interact();

                if(activeInteractable.CanInteract())
                    activeInteractable = null;
            }
        }
    }

    private Interactable FindBestInteractable()
    {
        Vector2 center = interactionArea != null ? interactionArea.position : transform.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, interactionRadius, interactableLayers);

        Interactable bestInteractable = null;
        float bestDistance = float.MaxValue;

        foreach(Collider2D collider in colliders)
        {
            Interactable interactable = GetInteractable(collider);

            if(interactable == null || !interactable.CanInteract())
                continue;

            float distance = Vector2.Distance(center, collider.ClosestPoint(center));

            if(distance < bestDistance)
            {
                bestDistance = distance;
                bestInteractable = interactable;
            }
        }

        return bestInteractable;
    }

    private Interactable GetInteractable(Collider2D collider)
    {
        MonoBehaviour[] behaviours = collider.GetComponents<MonoBehaviour>();

        foreach(MonoBehaviour behaviour in behaviours)
        {
            if(behaviour is Interactable interactable)
                return interactable;
        }

        behaviours = collider.GetComponentsInParent<MonoBehaviour>();

        foreach(MonoBehaviour behaviour in behaviours)
        {
            if(behaviour is Interactable interactable)
                return interactable;
        }

        return null;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = interactionArea != null ? interactionArea.position : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, interactionRadius);
    }
}
