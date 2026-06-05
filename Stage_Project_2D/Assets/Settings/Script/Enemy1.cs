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
    


    void Start()
    {
        animator = GetComponent<Animator>();
        spriterenderer = GetComponent<SpriteRenderer>();
        aiPath = GetComponent<AIPath>();
        p = player.transform;
        playerScript = player.GetComponent<PlayerMovement>();

        vita = vitaMassima;

        // sicurezza iniziale
        aiPath.canMove = false;
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

        // flip sprite
        if (p.position.x > transform.position.x)
            spriterenderer.flipX = true;
        else
            spriterenderer.flipX = false;

        // fuori range
        if (distance > chaseDistance)
        {
            aiPath.canMove = false;
            PlayAnimation("Nemico1_Idle");
            return;
        }

        // attacco
        if (distance <= stopDistance)
        {
            aiPath.canMove = false;

            if (Time.time >= nextAttackTime)
                StartCoroutine(AttackCoroutine());
            else
                PlayAnimation("Nemico1_Idle");

            return;
        }

        // inseguimento con pathfinding
        PlayAnimation("Nemico1_Corsa");

        aiPath.canMove = true;
        aiPath.maxSpeed = 3f; // oppure usa una variabile speed


        if(vita <=1)
            Destroy(gameObject);

    }

    private IEnumerator AttackCoroutine()
    {
        playerScript = player.GetComponent<PlayerMovement>();

        IsAttacking = true;
        nextAttackTime = Time.time + attackDuration + attackCooldown;

        aiPath.canMove = false;

        PlayAnimation("Nemico1_Attacco");

        yield return new WaitForSeconds(attackHitDelay);

        if (Vector2.Distance(transform.position, p.position) <= stopDistance + 0.4f && playerScript.IsShildingSetGet == false) 
        {
            HitPlayer();
        }

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - attackHitDelay));

        IsAttacking = false;
    }

    private void PlayAnimation(string animationName)
    {
        if (currentAnimation == animationName)
            return;

        currentAnimation = animationName;
        animator.Play(animationName);
    }

    private void HitPlayer()
    {
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        playerScript = player.GetComponent<PlayerMovement>();

        StartCoroutine(KnockbackPlayer(playerRb, playerScript));

        Debug.Log("Colpito");

        StartCoroutine(playerScript.HurtCoroutine(danno)); // Danno al giocatore, implamentato nella classe HeroKnight
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

        LifeBar_Enemy LifebarScript = transform.Find("LifeBar/Canvas").GetComponent<LifeBar_Enemy>();

        //rb.velocity = Vector2.zero;

        PlayAnimation("Enemy_Attacco_Subito");

        vita -=danno;

        LifebarScript.UpdateLifeBar(vita, vitaMassima);

        yield return new WaitForSeconds(0.35f);

        IsHurting = false;

        Debug.Log("viene colpito");
    }



}
