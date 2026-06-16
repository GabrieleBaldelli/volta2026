using System.Collections;
using UnityEngine;
using Pathfinding;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CharacterAudioController))]
public class Enemy : MonoBehaviour
{
    [Header("Life bar")]
    public Image HealthImage;

    [Header("Player References")]
    public GameObject player;
    protected Transform p;
    protected PlayerMovement playerScript;

    [Header("Enemy Stats")]
    public float vitaMassima = 50f;
    protected float vita;

    // XP data al player quando questo nemico muore.
    public float xp = 100f;
    
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
    public bool invertFlipX = false;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriterenderer;
    protected Animazioni animazioni;

    // Script unico che contiene i clip e i volumi audio del nemico.
    // Ogni bandit puo' avere clip diversi assegnati nel suo CharacterAudioController.
    protected CharacterAudioController characterAudio;

    [Header("Stati dell'Enemy")]
    // Questi stati impediscono al nemico di inseguire mentre attacca, prende danno o muore.
    protected bool IsAttacking = false;
    protected bool IsHurting;
    protected bool IsDying;

    // Evita di dare XP piu' volte durante la stessa morte.
    private bool hasGivenXP;

    public virtual bool IsAttackingSetGet
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

    protected Transform PlayerTransform
    {
        get { return p; }
    }

    protected virtual void OnEnable()
    {
        // Quando i sistemi delle stanze riattivano il nemico, resetta AI e animazione.
        PrepareForRoomUnlock();
    }

    protected virtual void OnDisable()
    {
        // Quando il nemico viene spento dalla stanza, ferma movimento e suoni.
        if(rb != null)
            rb.velocity = Vector2.zero;

        if(aiPath != null)
            aiPath.canMove = false;

        StopRunSound();
    }

    protected virtual void Start()
    {
        // Recupera i componenti necessari al movimento, animazione e audio.
        rb = GetComponent<Rigidbody2D>();

        spriterenderer = GetComponent<SpriteRenderer>();
        if(spriterenderer == null)
            spriterenderer = GetComponentInChildren<SpriteRenderer>();

        if(aiPath == null)
            aiPath = GetComponent<AIPath>();

        animazioni = GetComponent<Animazioni>();

        if (animazioni == null)
            animazioni = gameObject.AddComponent<Animazioni>();

        // Prende il controller audio del bandit.
        characterAudio = GetComponent<CharacterAudioController>();

        // Se manca, lo aggiunge automaticamente per evitare NullReferenceException.
        if(characterAudio == null)
            characterAudio = gameObject.AddComponent<CharacterAudioController>();

        if(player != null)
        {
            // Se il player e' gia' assegnato dall'Inspector, usa quello come target.
            p = player.transform;
            playerScript = player.GetComponent<PlayerMovement>();
            SetAIDestinationTarget();
        }
        else
        {
            // Fallback: cerca il player nella scena se non e' stato assegnato.
            playerScript = FindObjectOfType<PlayerMovement>();

            if(playerScript != null)
            {
                player = playerScript.gameObject;
                p = player.transform;
                SetAIDestinationTarget();
            }
        }

        vita = vitaMassima;

        if(aiPath != null)
            aiPath.canMove = false;

    }

    protected void PlayRunSound()
    {
        // Avvia il suono in loop della corsa/inseguimento del bandit.
        if(characterAudio == null)
            return;

        characterAudio.PlayRunSound();
    }

    protected void StopRunSound()
    {
        // Ferma il suono di corsa quando il bandit e' fermo, attacca o prende danno.
        if(characterAudio == null)
            return;

        characterAudio.StopRunSound();
    }

    protected void PlayAttackSound()
    {
        // Prima ferma la corsa, poi riproduce il suono del colpo.
        StopRunSound();
        characterAudio.PlayAttackSound();
    }

    protected void PlayAttackEffortSound()
    {
        // Suono della voce/sforzo del bandit durante l'attacco.
        StopRunSound();
        characterAudio.PlayAttackEffortSound();
    }

    protected void PlayHurtSound()
    {
        // Quando il bandit subisce danno, ferma la corsa e riproduce il suono di dolore.
        StopRunSound();
        characterAudio.PlayHurtSound();
    }

    protected void PlayDeathSound()
    {
        StopRunSound();
        characterAudio.PlayDeathSound();
    }

    protected virtual void Update()
    {
        if(IsDying)
        {
            return;
        }
        
        if(p == null)
            TryFindPlayer();

        if (p == null || aiPath == null || animazioni == null || IsDying)
        {
            StopRunSound();
            return;
        }

        if (IsAttacking)
        {
            // Durante l'attacco resta fermo e lascia finire la coroutine.
            StopRunSound();
            aiPath.canMove = false;
            return;
        }

        if (IsHurting)
        {
            // Mentre subisce danno non insegue il player.
            StopRunSound();
            aiPath.canMove = false;
            return;
        }

        float distance = Vector2.Distance(transform.position, p.position);

        FacePlayer();

        if (distance > chaseDistance)
        {
            // Troppo lontano: non insegue e torna in idle.
            StopRunSound();
            aiPath.canMove = false;
            animazioni.Idle();
            return;
        }

        if (distance <= stopDistance)
        {
            // Abbastanza vicino: si ferma e prova ad attaccare rispettando il cooldown.
            StopRunSound();
            aiPath.canMove = false;

            if (Time.time >= nextAttackTime)
                StartCoroutine(AttackCoroutine());
            else
                animazioni.Idle();

            return;
        }

        // Dentro la chaseDistance ma fuori dalla stopDistance: corre verso il player.
        animazioni.Corsa();
        PlayRunSound();

        aiPath.canMove = true;
        aiPath.maxSpeed = 3f;

        if(vita <= 1)
        {
            StopRunSound();
            Destroy(gameObject);
        }
    }

    private IEnumerator AttackCoroutine()
    {
        if(player == null || p == null || IsDying)
            yield break;

        // Blocca altri comportamenti finche' l'attacco non finisce.
        playerScript = player.GetComponent<PlayerMovement>();

        IsAttacking = true;
        nextAttackTime = Time.time + attackDuration + attackCooldown;

        aiPath.canMove = false;

        animazioni.Attacco1();
        PlayAttackSound();
        PlayAttackEffortSound();

        // Aspetta il momento in cui il colpo deve effettivamente fare danno.
        yield return new WaitForSeconds(attackHitDelay);

        if (IsDying)
        {
            IsAttacking = false;
            yield break;
        }

        DamagePlayerIfNear(stopDistance + 0.4f);

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - attackHitDelay));

        IsAttacking = false;
    }

    protected void DamagePlayerIfNear(float radius)
    {
        if(IsDying || player == null || p == null)
            return;

        playerScript = player.GetComponent<PlayerMovement>();
        if(playerScript == null)
            return;

        if(Vector2.Distance(transform.position, p.position) > radius)
            return;

        // Se il player sta parando, il danno normale non viene applicato.
        if(playerScript.IsShieldingSetGet)
            return;

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if(playerRb != null)
            StartCoroutine(KnockbackPlayer(playerRb));

        Debug.Log("Colpito");

        StartCoroutine(playerScript.HurtCoroutine(danno));
    }

    protected void FacePlayer()
    {
        if(spriterenderer == null || p == null)
            return;

        bool flipTowardPlayer = p.position.x > transform.position.x;
        spriterenderer.flipX = invertFlipX ? !flipTowardPlayer : flipTowardPlayer;
    }

    protected IEnumerator KnockbackPlayer(Rigidbody2D playerRb)
    {
        Vector2 knockbackDirection = (p.position - transform.position).normalized;

        playerRb.velocity = Vector2.zero;
        playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        playerRb.velocity = Vector2.zero;
    }

    public virtual IEnumerator HurtCoroutine(float danno)
    {
        if(IsDying)
            yield break;

        // Il nemico si ferma mentre prende danno, cosi' non insegue durante l'hit reaction.
        IsHurting = true;

        Transform lifeBarTransform = transform.Find("Canvas/Life_Bar");
        LifeBar lifebarScript = null;

        if(lifeBarTransform != null)
            lifebarScript = lifeBarTransform.GetComponent<LifeBar>();

        if(animazioni != null)
            animazioni.Danno();

        PlayHurtSound();

        vita -= danno;

        if(lifebarScript != null)
            lifebarScript.UpdateLifeBar(vita, vitaMassima);

        if(vita <= 1)
        {
            // Morte del nemico: ferma AI, da XP una sola volta e poi distrugge l'oggetto.
            IsDying = true;
            IsAttacking = false;
            IsHurting = false;
            GiveXPOnce();

            if(aiPath != null)
                aiPath.canMove = false;

            StopRunSound();

            if(animazioni != null)
                animazioni.Morte();

            yield return new WaitForSeconds(0.35f);

            if(this == null)
                yield break;

            Destroy(gameObject);
            yield break;
        }

        yield return new WaitForSeconds(0.35f);

        if(IsDying)
            yield break;

        IsHurting = false;

        Debug.Log("viene colpito");
    }

    protected void TryFindPlayer()
    {
        // Usato come recupero se il riferimento al player non e' ancora disponibile.
        playerScript = FindObjectOfType<PlayerMovement>();
        if(playerScript == null)
            return;

        player = playerScript.gameObject;
        p = player.transform;
        SetAIDestinationTarget();
    }

    protected void GiveXPOnce()
    {
        if(hasGivenXP)
            return;

        if(playerScript == null)
            TryFindPlayer();

        if(playerScript == null)
            return;

        playerScript.AddXP(xp);
        hasGivenXP = true;
    }

    protected void SetAIDestinationTarget()
    {
        AIDestinationSetter destinationSetter = GetComponent<AIDestinationSetter>();
        if(destinationSetter != null)
            destinationSetter.target = p;
    }

    protected void StopMovement()
    {
        if(rb != null)
            rb.velocity = Vector2.zero;

        if(aiPath != null)
            aiPath.canMove = false;
    }

    public void PrepareForRoomLock()
    {
        // Chiamato dai trigger delle stanze prima di disattivare il nemico.
        IsAttacking = false;
        IsHurting = false;

        StopMovement();
        StopRunSound();

        if(animazioni != null)
            animazioni.ResetCurrentAnimation();
    }

    public void PrepareForRoomUnlock()
    {
        // Chiamato quando la stanza riattiva il nemico, per evitare animazioni/stati vecchi.
        IsAttacking = false;
        IsHurting = false;

        if(animazioni != null)
            animazioni.ResetCurrentAnimation();

        if(aiPath != null)
            aiPath.canMove = false;

        if(p == null)
            TryFindPlayer();
        else
            SetAIDestinationTarget();
    }
}
