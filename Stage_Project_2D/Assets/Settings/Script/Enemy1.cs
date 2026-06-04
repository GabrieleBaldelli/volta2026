using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy1 : MonoBehaviour
{
    private Transform player;

    public float stopDistance = 1f;
    public float chaseDistance = 4f;
    public float attackCooldown = 1f;
    public float attackDuration = 0.6f;
    public float attackHitDelay = 0.25f;
    public float knockbackForce = 7f;
    public float knockbackDuration = 0.15f;

    private AIPath aiPath;
    private float nextAttackTime = 0f;
    private Animator animator;
    private SpriteRenderer spriterenderer;
    private bool isAttacking = false;
    private string currentAnimation;
    


    void Start()
    {
        animator = GetComponent<Animator>();
        spriterenderer = GetComponent<SpriteRenderer>();
        aiPath = GetComponent<AIPath>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        
        
        if (p != null)
        {
            player = p.transform;
        }

        // sicurezza iniziale
        aiPath.canMove = false;
    }

    void Update()
    {
        if (player == null)
            return;

        if (isAttacking)
        {
            aiPath.canMove = false;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // flip sprite
        if (player.position.x > transform.position.x)
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

    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackDuration + attackCooldown;

        aiPath.canMove = false;

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
            return;

        currentAnimation = animationName;
        animator.Play(animationName);
    }

    public void HitPlayer()
    {
        if (player == null)
            return;

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
            playerMovement.enabled = false;

        playerRb.velocity = Vector2.zero;
        playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        playerRb.velocity = Vector2.zero;

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

}