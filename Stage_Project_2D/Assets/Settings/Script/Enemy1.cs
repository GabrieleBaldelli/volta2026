using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    private Transform player;
    public float speed = 3f;
    public float stopDistance = 1f;
    public float chaseDistance = 4f;
    public float attackCooldown = 0.3f;
    public float knockbackForce = 50f;

    private Rigidbody2D rb;
    private float nextAttackTime = 1f;
    private Animator animator;
    private SpriteRenderer spriterenderer;
    

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

        float distance = Vector2.Distance(transform.position, player.position);
        
        if (distance > chaseDistance)
        {
            rb.velocity = Vector2.zero;
            animator.Play("Nemico1_Idle");
        }

       if (distance <= stopDistance)
        {
            rb.velocity = Vector2.zero;

            if (Time.time >= nextAttackTime)
            {
                animator.Play("Nemico1_Attacco");
                nextAttackTime = Time.time + attackCooldown;   
            }
            
        }

        if (distance > stopDistance && distance < chaseDistance)
        {
            if (player.position.x > transform.position.x)
                spriterenderer.flipX = true;
            else
                spriterenderer.flipX = false;

            animator.Play("Nemico1_Corsa");

            Vector2 direction =
                (player.position - transform.position).normalized;

            rb.velocity = direction * speed;
        }
    }

    public void HitPlayer()
    {
        Debug.Log("Colpito");

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            Vector2 knockbackDirection =
                (player.position - transform.position).normalized;

            playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }
}
        

