using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public Transform interactionArea;
    public float interactionRadius = 1f;
    public LayerMask interactableLayers = ~0;
    public KeyCode interactKey = KeyCode.E;

    // Oggetto interagibile più vicino trovato in questo frame.
    private Interactable currentInteractable;

    // Oggetto con cui stiamo già interagendo, per continuare un dialogo già aperto
    private Interactable activeInteractable;    // Anche quando CanInteract() torna false.

    public Interactable activeInteractableSetGet
    {
        get { return activeInteractable; }
        set { activeInteractable = value; }
    }

    void Update()
    {
        // Se non siamo già dentro un'interazione, cerca l'oggetto interagibile più vicino.
        if(activeInteractable == null)
        {
            currentInteractable = FindNearInteractable(); //restituisce l'interactable + vicino
        }
        else 
        // 
        {
            // Prendo la posizione dell' GameObject di activeInteractable
            MonoBehaviour mb = activeInteractable as MonoBehaviour;
            GameObject activeInteractableObject = mb.gameObject;
            Transform interactableTransform = activeInteractableObject.transform;
            float distance = Vector2.Distance(interactableTransform.position, transform.position);

            if(!activeInteractable.CanInteract() && distance <= interactionRadius)
                return;

            activeInteractable = null;
            activeInteractableObject.GetComponent<NPC>().EndDialogue();

            Debug.Log("Interazione interrotta perché troppo lontano");
        }

        if(Input.GetKeyDown(interactKey))
        {
            // Se un'interazione è già iniziata, continua quella.
            // Esempio: fa avanzare il dialogo dell'NPC alla prossima frase.
            if(activeInteractable != null)
            {
                activeInteractable.Interact();

                // Quando l'oggetto torna interagibile, considera l'interazione conclusa.
                // Nel tuo NPC succede dopo EndDialogue().
                if(activeInteractable != null)
                    if(activeInteractable.CanInteract())
                        activeInteractable = null;

                return;
            }

            // Se non c'è un'interazione attiva, prova a iniziarne una nuova.
            if(currentInteractable != null)
            {
                activeInteractable = currentInteractable;
                activeInteractable.Interact();

                // Se l'interazione finisce subito, libera activeInteractable.
                // Utile per oggetti semplici tipo leve, porte o casse.
                if(activeInteractable.CanInteract())
                    activeInteractable = null;
            }
        }
    }

    private Interactable FindNearInteractable()
    {
        // Usa Interaction Area se esiste, altrimenti usa la posizione del player.
        Vector2 center = new Vector2(interactionArea.position.x, interactionArea.position.y);

        // Trova tutti i Collider2D dentro il cerchio, ma solo sui layer scelti.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, interactionRadius, interactableLayers);

        Interactable nearInteractable = null;
        float bestDistance = float.MaxValue;

        foreach(Collider2D currentCollider in colliders)
        {
// GetInteractable( Collider2D ) prende lo script solo se il suo oggetto ha un layer Interactable
            Interactable interactable = GetInteractable(currentCollider);

            // Se non non ha trovato le funzioni, oppure non può interagire ora, lo salta.
            if(interactable == null || !interactable.CanInteract())
                continue;

            // Sceglie l'oggetto più vicino al centro dell'Interaction Area.
            float distance = Vector2.Distance(center, currentCollider.ClosestPoint(center));

            if(distance < bestDistance)
            {
                bestDistance = distance;
                nearInteractable = interactable;
            }
        }
            
        return nearInteractable;
    }

    private Interactable GetInteractable(Collider2D collider)
    {
        // Cerca prima sullo stesso GameObject del collider.
        MonoBehaviour[] behaviours = collider.GetComponents<MonoBehaviour>();

        foreach(MonoBehaviour behaviour in behaviours)
        {
            if(behaviour is Interactable interactable)
                return interactable;
        }

        // Se il collider è su un figlio, cerca anche nei parent.
        // Utile se il collider sta su un child ma lo script NPC sta sull'oggetto padre.
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
        // Disegna il cerchio di interazione nella Scene View quando selezioni il player.
        Vector3 center = interactionArea != null ? interactionArea.position : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, interactionRadius);
    }
}
