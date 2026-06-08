using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.UI;

public class Enemy1 : MonoBehaviour
{   
    [Header("Life bar")]
    public Image HealthImage;

    [Header("Player References")]
    public GameObject player;
    private Transform p;
    private PlayerMovement playerScript;

    [Header("Enemy Stats")]
    public float vitaMassima = 50f;
    private float vita;

    public float danno = 10f;

    public float stopDistance = 2f;
    public float chaseDistance = 4f;
    public float attackCooldown = 1f;
    public float attackDuration = 1f;
    public float attackHitDelay = 0.25f;

    public float knockbackForce = 7f;
    public float knockbackDuration = 0.15f;

    public AIPath aiPath;
    public float nextAttackTime = 0.5f;
    private Animator animator;
    private SpriteRenderer spriterenderer;
    private bool IsAttacking = false;
    private bool IsHurting;

    

    public bool IsAttackingSetGet
        {
            get 
            { 
                return IsAttacking; 
            }
            set 
            { 
                IsAttacking = value; 
            }
        }

    private string currentAnimation;
    

    //Inizializza componenti e statistiche del nemico
    void Start()
    {
        // Recupera i componenti necessari presenti sul GameObject
        animator = GetComponent<Animator>();
        spriterenderer = GetComponent<SpriteRenderer>();
        aiPath = GetComponent<AIPath>();

        // Memorizza il transform del player
        p = player.transform;
        // Recupera lo script del player
        playerScript = player.GetComponent<PlayerMovement>();

        // Imposta la vita iniziale del nemico
        vita = vitaMassima;

        // All'avvio il nemico non si muove
        aiPath.canMove = false;
    }

    //Gestisce inseguimento, attacco e stati del nemico
    void Update()
    {
        // Se il player non esiste, interrompe la logica del nemico
        if (p == null)
            return;

        // Durante l'attacco il nemico non può muoversi e la sua animazione rimane invariata
        if (IsAttacking)
        {
            aiPath.canMove = false;
            return;
        }
        // Durante l'animazione di danno il nemico rimane fermo e la sua animazione rimane invariata
        if (IsHurting)
        {
            aiPath.canMove = false;
            return;
        }

        // Calcola la distanza tra nemico e player
        float distance = Vector2.Distance(transform.position, p.position);

        // flip sprite
        if (p.position.x > transform.position.x)
            spriterenderer.flipX = true;
        else
            spriterenderer.flipX = false;

        //Se il nemico è troppo lontano, lui rimane fermo
        if (distance > chaseDistance)
        {
            aiPath.canMove = false;
            PlayAnimation("Nemico1_Idle");
            return;
        }

        // Se il player è abbastanza vicino inizia la fase di attacco
        if (distance <= stopDistance)
        {
            aiPath.canMove = false;

            // Controlla che il tempo di ricarica dell'attacco sia terminato
            if (Time.time >= nextAttackTime)
                StartCoroutine(AttackCoroutine());
            else
                PlayAnimation("Nemico1_Idle");

            return;
        }

        //Se il player è nel raggio di inseguimento ma non in quello di attacco,
        //il nemico lo segue usando il sistema A* Pathfinding
        PlayAnimation("Nemico1_Corsa");

        //Abilita il movimento automatico del componente AIPath
        aiPath.canMove = true;

        //Imposta la velocità massima di movimento del nemico
        aiPath.maxSpeed = 3f; // oppure usa una variabile speed

        //Se la vita arriva a zero distrugge il GameObject
        if(vita <=1)
            Destroy(gameObject);

    }

    //Esegue la sequenza completa dell'attacco
    private IEnumerator AttackCoroutine()
    {
        //Prende lo script del player
        playerScript = player.GetComponent<PlayerMovement>();
        
        //Impedisce al nemico di eseguire più attacchi contemporaneamente
        IsAttacking = true;
        //Calcola il momento in cui potrà attaccare di nuovo
        nextAttackTime = Time.time + attackDuration + attackCooldown;

        //Blocca il movimento durante l'attacco
        aiPath.canMove = false;

        //Avvia l'animazione di attacco
        PlayAnimation("Nemico1_Attacco");
        
        // Attende il frame in cui il colpo deve essere applicato
        yield return new WaitForSeconds(attackHitDelay);

        //Controlla che il player sia ancora a distanza di colpo
        //e che non stia usando lo scudo
        if (Vector2.Distance(transform.position, p.position) <= stopDistance + 0.4f && playerScript.IsShildingSetGet == false) 
        {
            HitPlayer();
        }

        //Attende la fine dell'animazione prima di permettere un nuovo attacco
        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - attackHitDelay));

        //Il nemico può tornare a muoversi e attaccare normalmente
        IsAttacking = false;
    }

    //Riproduce un'animazione evitando riavvii inutili
    private void PlayAnimation(string animationName)
    {
        //Evita di riavviare continuamente la stessa animazione
        if (currentAnimation == animationName)
            return;

        //Memorizza l'ultima animazione riprodotta
        currentAnimation = animationName;
        //Riproduce la nuova animazione richiesta
        animator.Play(animationName);
    }

    //Infligge danno e knockback al player
    private void HitPlayer()
    {
        //Recupero rigidbody e lo script del player
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        playerScript = player.GetComponent<PlayerMovement>();

        //Applica il knockback
        StartCoroutine(KnockbackPlayer(playerRb, playerScript));

        //Debug di controllo
        Debug.Log("Colpito");

        //Infligge il danno al giocatore
            StartCoroutine(playerScript.HurtCoroutine(danno)); // Danno al giocatore, implamentato nella classe HeroKnight
        
    }

    //Spinge il player all'indietro dopo un colpo
    private IEnumerator KnockbackPlayer(Rigidbody2D playerRb, PlayerMovement playerScript)
    {
        //Calcola la direzione in cui spingere il player
        Vector2 knockbackDirection = (p.position - transform.position).normalized;

        //Disabilita temporaneamente il controllo del player
        if (playerScript != null)
            playerScript.enabled = false;

        //Ferma eventuali movimenti precedenti del player
        playerRb.velocity = Vector2.zero;
        // Applica una forza che lo spinge
        playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        //Mantiene il player sotto l'effetto del knockback per alcuni istanti
        yield return new WaitForSeconds(knockbackDuration);

        //Arresta completamente il movimento residuo dopo la spinta
        playerRb.velocity = Vector2.zero;

        // Riabilita il controllo del player
        if (playerScript != null)
            playerScript.enabled = true;
    }

    //Gestisce la ricezione del danno da parte del nemico
     public IEnumerator HurtCoroutine(float danno)
    {
        //Il nemico entra nello stato di danno subito
        IsHurting = true;

        //Recupera lo script che gestisce la barra della vita del nemico
        LifeBar LifebarScript = transform.Find("Canvas/Life_Bar").GetComponent<LifeBar>();

        //rb.velocity = Vector2.zero;

        // Riproduce l'animazione di danno subito
        PlayAnimation("Enemy_Attacco_Subito");

        //Riduce la vita del nemico in base al danno ricevuto
        vita -=danno;

        // Aggiorna la barra della vita visivamente
        LifebarScript.UpdateLifeBar(vita, vitaMassima);

        // Attende la fine dell'animazione di danno
        yield return new WaitForSeconds(0.35f);

        // Il nemico può tornare ad agire normalmente
        IsHurting = false;

        //Debug di controllo, se il nemico viene colpito
        Debug.Log("viene colpito");
    }



}
