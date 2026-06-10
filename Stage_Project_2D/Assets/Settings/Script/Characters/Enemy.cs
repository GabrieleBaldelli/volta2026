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

    void Update()
    {
        if (p == null || aiPath == null || animazioni == null || IsDying)
            return;

        if (IsAttacking)
        {
            aiPath.canMove = false;
            return;
        }

        if (IsHurting)
        {
            aiPath.canMove = false;
            return;
        }

        float distance = Vector2.Distance(transform.position, p.position);

        if (spriterenderer != null)
            spriterenderer.flipX = p.position.x > transform.position.x;

        if (distance > chaseDistance)
        {
            aiPath.canMove = false;
            animazioni.Idle();
            return;
        }

        if (distance <= stopDistance)
        {
            aiPath.canMove = false;

            if (Time.time >= nextAttackTime)
                StartCoroutine(AttackCoroutine());
            else
                animazioni.Idle();

            return;
        }

        animazioni.Corsa();

        aiPath.canMove = true;
        aiPath.maxSpeed = 3f;

        if(vita <= 1)
            Destroy(gameObject);
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

        yield return new WaitForSeconds(attackHitDelay);

        if (IsDying)
        {
            IsAttacking = false;
            yield break;
        }

        if (playerScript != null && Vector2.Distance(transform.position, p.position) <= stopDistance + 0.4f && playerScript.IsShildingSetGet == false)
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
        IsHurting = true;

        Transform lifeBarTransform = transform.Find("Canvas/Life_Bar");
        LifeBar lifebarScript = null;

        if(lifeBarTransform != null)
            lifebarScript = lifeBarTransform.GetComponent<LifeBar>();

        if(animazioni != null)
            animazioni.Danno();

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

            if(animazioni != null)
                animazioni.Morte();

            yield return new WaitForSeconds(0.35f);
            Destroy(gameObject);
            yield break;
        }

        yield return new WaitForSeconds(0.35f);

        IsHurting = false;

        Debug.Log("viene colpito");
    }
}
