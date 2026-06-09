using System.Collections;
using UnityEngine;
using Pathfinding;
using UnityEngine.UI;

public class Enemy2 : MonoBehaviour
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
        aiPath = GetComponent<AIPath>();
        animazioni = GetComponent<Animazioni>();

        if (animazioni == null)
            animazioni = gameObject.AddComponent<Animazioni>();
        
        ConfiguraAnimazioniEnemy2();

        p = player.transform;
        playerScript = player.GetComponent<PlayerMovement>();

        vita = vitaMassima;
        aiPath.canMove = false;
    }

    private void ConfiguraAnimazioniEnemy2()
    {
        if (string.IsNullOrEmpty(animazioni.idleAnimation) || animazioni.idleAnimation == "Nemico1_Idle")
            animazioni.idleAnimation = "Nemico2_Idle";

        if (string.IsNullOrEmpty(animazioni.runAnimation) || animazioni.runAnimation == "Nemico1_Corsa")
            animazioni.runAnimation = "Nemico2_Corsa";

        if (string.IsNullOrEmpty(animazioni.attackAnimation) || animazioni.attackAnimation == "Nemico1_Attacco")
            animazioni.attackAnimation = "Nemico2_Attacco";

        if (string.IsNullOrEmpty(animazioni.hurtAnimation) || animazioni.hurtAnimation == "Enemy_Attacco_Subito" || animazioni.hurtAnimation == "Nemico2_Attacco_Subito")
            animazioni.hurtAnimation = "Enemy2_Attacco_Subito";
    }

    void Update()
    {
        if (p == null)
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

        if (p.position.x > transform.position.x)
            spriterenderer.flipX = true;
        else
            spriterenderer.flipX = false;

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

        if (vita <= 1)
            Destroy(gameObject);
    }

    private IEnumerator AttackCoroutine()
    {
        playerScript = player.GetComponent<PlayerMovement>();

        IsAttacking = true;
        nextAttackTime = Time.time + attackDuration + attackCooldown;

        aiPath.canMove = false;
        animazioni.Attacco();

        yield return new WaitForSeconds(attackHitDelay);

        if (Vector2.Distance(transform.position, p.position) <= stopDistance + 0.4f && playerScript.IsShildingSetGet == false)
        {
            HitPlayer();
        }

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - attackHitDelay));

        IsAttacking = false;
    }

    private void HitPlayer()
    {
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        playerScript = player.GetComponent<PlayerMovement>();

        StartCoroutine(KnockbackPlayer(playerRb, playerScript));

        Debug.Log("Colpito");
        StartCoroutine(playerScript.HurtCoroutine(danno));
    }

    private IEnumerator KnockbackPlayer(Rigidbody2D playerRb, PlayerMovement playerScript)
    {
        Vector2 knockbackDirection = (p.position - transform.position).normalized;

        if (playerScript != null)
            playerScript.enabled = false;

        playerRb.velocity = Vector2.zero;
        playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        playerRb.velocity = Vector2.zero;

        if (playerScript != null)
            playerScript.enabled = true;
    }

    public IEnumerator HurtCoroutine(float danno)
    {
        IsHurting = true;

        LifeBar LifebarScript = transform.Find("Canvas/Life_Bar").GetComponent<LifeBar>();

        animazioni.Danno();

        vita -= danno;
        LifebarScript.UpdateLifeBar(vita, vitaMassima);

        yield return new WaitForSeconds(0.35f);

        IsHurting = false;

        Debug.Log("viene colpito");
    }
}
