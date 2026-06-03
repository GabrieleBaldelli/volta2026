using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    private Transform player;
    public float speed = 3f;
    public float stopDistance = 1f;
    public float chaseDistance = 4f;
    public float attackCooldown = 1f;
    public float attackDuration = 0.6f;
    public float attackHitDelay = 0.25f;
    public float knockbackForce = 7f;
    public float knockbackDuration = 0.15f;

    private Rigidbody2D rb;
    private float nextAttackTime = 0f;
    private Animator animator;
    private SpriteRenderer spriterenderer;
    private bool isAttacking = false;
    private string currentAnimation;
    

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriterenderer = GetComponent<SpriteRenderer>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
        {
            player = p.transform;
        }
        
    }

    void Update()
    {
        if (player == null)
            return;

        if (isAttacking)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (player.position.x > transform.position.x)
            spriterenderer.flipX = true;
        else if (player.position.x < transform.position.x)
            spriterenderer.flipX = false;
        
        if (distance > chaseDistance)
        {
            rb.velocity = Vector2.zero;
            PlayAnimation("Nemico1_Idle");
            return;
        }

        if (distance <= stopDistance)
        {
            rb.velocity = Vector2.zero;

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(AttackCoroutine());
            }
            else
            {
                PlayAnimation("Nemico1_Idle");
            }

            return;
        }

        PlayAnimation("Nemico1_Corsa");

        Vector2 direction = (player.position - transform.position).normalized;

        rb.velocity = direction * speed;
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackDuration + attackCooldown;
        rb.velocity = Vector2.zero;

        PlayAnimation("Nemico1_Attacco");

        yield return new WaitForSeconds(attackHitDelay);

        if (player != null && Vector2.Distance(transform.position, player.position) <= stopDistance + 0.4f)
        {
            HitPlayer();
        }

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - attackHitDelay));

        isAttacking = false;
    }

    private void PlayAnimation(string animationName)
    {
        if (currentAnimation == animationName)
        {
            return;
        }

        currentAnimation = animationName;
        animator.Play(animationName);
    }

    public void HitPlayer()
    {
        if (player == null)
        {
            return;
        }

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        if (playerRb != null)
        {
            StartCoroutine(KnockbackPlayer(playerRb, playerMovement));
        }
    }

    private IEnumerator KnockbackPlayer(Rigidbody2D playerRb, PlayerMovement playerMovement)
    {
        Vector2 knockbackDirection = (player.position - transform.position).normalized;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        playerRb.velocity = Vector2.zero;
        playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        playerRb.velocity = Vector2.zero;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    private void OnValidate()
    {
        stopDistance = Mathf.Max(0.1f, stopDistance);
        chaseDistance = Mathf.Max(stopDistance + 0.1f, chaseDistance);
        attackCooldown = Mathf.Max(0.1f, attackCooldown);
        attackDuration = Mathf.Max(0.1f, attackDuration);
        attackHitDelay = Mathf.Clamp(attackHitDelay, 0.01f, attackDuration);
        knockbackDuration = Mathf.Max(0.01f, knockbackDuration);
    }
}
        
