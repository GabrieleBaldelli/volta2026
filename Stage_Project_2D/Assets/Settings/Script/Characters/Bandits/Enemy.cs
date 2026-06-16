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
    private Transform p;
    private PlayerMovement playerScript;

    [Header("Enemy Stats")]
    public float vitaMassima = 50f;
    private float vita;
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
    private SpriteRenderer spriterenderer;
    private Animazioni animazioni;

    // Script unico che contiene i clip e i volumi audio del nemico.
    // Ogni bandit puo' avere clip diversi assegnati nel suo CharacterAudioController.
    private CharacterAudioController characterAudio;

    [Header("Stati dell'Enemy")]
    private bool IsAttacking = false;
    private bool IsHurting;
    private bool IsDying;
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

    protected virtual void Start()
    {
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
            p = player.transform;
            playerScript = player.GetComponent<PlayerMovement>();
            SetAIDestinationTarget();
        }
        else
        {
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
        else
            Debug.LogError("AIPath mancante sul nemico", this);
    }

    private void PlayRunSound()
    {
        // Avvia il suono in loop della corsa/inseguimento del bandit.
        characterAudio.PlayRunSound();
    }

    private void StopRunSound()
    {
        // Ferma il suono di corsa quando il bandit e' fermo, attacca o prende danno.
        characterAudio.StopRunSound();
    }

    private void PlayAttackSound()
    {
        // Prima ferma la corsa, poi riproduce il suono del colpo.
        StopRunSound();
        characterAudio.PlayAttackSound();
    }

    private void PlayAttackEffortSound()
    {
        // Suono della voce/sforzo del bandit durante l'attacco.
        StopRunSound();
        characterAudio.PlayAttackEffortSound();
    }

    private void PlayHurtSound()
    {
        // Quando il bandit subisce danno, ferma la corsa e riproduce il suono di dolore.
        StopRunSound();
        characterAudio.PlayHurtSound();
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
            StopRunSound();
            aiPath.canMove = false;
            return;
        }

        if (IsHurting)
        {
            StopRunSound();
            aiPath.canMove = false;
            return;
        }

        float distance = Vector2.Distance(transform.position, p.position);

        if (spriterenderer != null)
        {
            bool flipTowardPlayer = p.position.x > transform.position.x;
            spriterenderer.flipX = invertFlipX ? !flipTowardPlayer : flipTowardPlayer;
        }

        if (distance > chaseDistance)
        {
            StopRunSound();
            aiPath.canMove = false;
            animazioni.Idle();
            return;
        }

        if (distance <= stopDistance)
        {
            StopRunSound();
            aiPath.canMove = false;

            if (Time.time >= nextAttackTime)
                StartCoroutine(AttackCoroutine());
            else
                animazioni.Idle();

            return;
        }

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

        playerScript = player.GetComponent<PlayerMovement>();

        IsAttacking = true;
        nextAttackTime = Time.time + attackDuration + attackCooldown;

        aiPath.canMove = false;

        animazioni.Attacco1();
        PlayAttackSound();
        PlayAttackEffortSound();

        yield return new WaitForSeconds(attackHitDelay);

        if (IsDying)
        {
            IsAttacking = false;
            yield break;
        }

        if (playerScript != null && Vector2.Distance(transform.position, p.position) <= stopDistance + 0.4f && playerScript.IsShieldingSetGet == false)
        {
            HitPlayer();
        }

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - attackHitDelay));

        IsAttacking = false;
    }

    private void HitPlayer()
    {
        if(IsDying)
            return;

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        playerScript = player.GetComponent<PlayerMovement>();

        if(playerRb != null)
            StartCoroutine(KnockbackPlayer(playerRb));

        Debug.Log("Colpito");

        if(playerScript != null)
            StartCoroutine(playerScript.HurtCoroutine(danno));
    }

    private IEnumerator KnockbackPlayer(Rigidbody2D playerRb)
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

    private void TryFindPlayer()
    {
        playerScript = FindObjectOfType<PlayerMovement>();
        if(playerScript == null)
            return;

        player = playerScript.gameObject;
        p = player.transform;
        SetAIDestinationTarget();
    }

    private void GiveXPOnce()
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

    private void SetAIDestinationTarget()
    {
        AIDestinationSetter destinationSetter = GetComponent<AIDestinationSetter>();
        if(destinationSetter != null)
            destinationSetter.target = p;
    }
}
