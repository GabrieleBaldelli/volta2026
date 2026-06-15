using System.Collections;
using UnityEngine;
using Pathfinding;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
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
    private SpriteRenderer spriterenderer;
    private Animazioni animazioni;
    private AudioSource enemyAudioSource;

    [Header("Audio")]
    public AudioClip runSound;
    public AudioClip attackSound;
    public AudioClip attackEffortSound;
    public AudioClip hurtSound;
    [Range(0f, 3f)]
    public float runVolume = 1f;
    [Range(0f, 3f)]
    public float attackVolume = 1f;
    [Range(0f, 3f)]
    public float attackEffortVolume = 1f;
    [Range(0f, 3f)]
    public float hurtVolume = 1f;

    [Header("Stati dell'Enemy")]
    private bool IsAttacking = false;
    private bool IsHurting;
    private bool IsDying;

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

    void Start()
    {
        spriterenderer = GetComponent<SpriteRenderer>();
        if(spriterenderer == null)
            spriterenderer = GetComponentInChildren<SpriteRenderer>();

        if(aiPath == null)
            aiPath = GetComponent<AIPath>();

        animazioni = GetComponent<Animazioni>();

        if (animazioni == null)
            animazioni = gameObject.AddComponent<Animazioni>();

        enemyAudioSource = GetComponent<AudioSource>();

        if(enemyAudioSource == null)
            enemyAudioSource = gameObject.AddComponent<AudioSource>();

        enemyAudioSource.playOnAwake = false;
        enemyAudioSource.loop = false;
        enemyAudioSource.spatialBlend = 0f;

        if(player != null)
        {
            p = player.transform;
            playerScript = player.GetComponent<PlayerMovement>();
        }
        else
        {
            playerScript = FindObjectOfType<PlayerMovement>();

            if(playerScript != null)
            {
                player = playerScript.gameObject;
                p = player.transform;
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
        if(enemyAudioSource != null && runSound != null && enemyAudioSource.isPlaying == false)
        {
            enemyAudioSource.clip = runSound;
            enemyAudioSource.loop = true;
            enemyAudioSource.volume = runVolume;
            enemyAudioSource.Play();
        }
    }

    private void StopRunSound()
    {
        if(enemyAudioSource != null && enemyAudioSource.clip == runSound && enemyAudioSource.isPlaying)
        {
            enemyAudioSource.Stop();
            enemyAudioSource.loop = false;
            enemyAudioSource.clip = null;
        }
    }

    private void PlayAttackSound()
    {
        if(enemyAudioSource != null && attackSound != null)
        {
            StopRunSound();
            enemyAudioSource.volume = 1f;
            enemyAudioSource.PlayOneShot(attackSound, attackVolume);
        }
    }

    private void PlayAttackEffortSound()
    {
        if(enemyAudioSource != null && attackEffortSound != null)
        {
            StopRunSound();
            enemyAudioSource.volume = 1f;
            enemyAudioSource.PlayOneShot(attackEffortSound, attackEffortVolume);
        }
    }

    private void PlayHurtSound()
    {
        if(enemyAudioSource != null && hurtSound != null)
        {
            StopRunSound();
            enemyAudioSource.volume = 1f;
            enemyAudioSource.PlayOneShot(hurtSound, hurtVolume);
        }
    }

    void Update()
    {
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
            spriterenderer.flipX = p.position.x > transform.position.x;

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

        animazioni.Attacco();
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

    public IEnumerator HurtCoroutine(float danno)
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
}
