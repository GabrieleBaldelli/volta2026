using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
   
    private bool IsMoving;
    private bool IsAttacking;

    private int comboStep = 0;
    private float comboTimer = 0f;
    public float comboResetTime = 0.8f;

    private float movementX;
    private float movementY;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // INPUT
        movementX = Input.GetAxisRaw("Horizontal");
        movementY = Input.GetAxisRaw("Vertical");

        rb.velocity = new Vector2(movementX * speed, movementY * speed);
        
        
       
        if (rb.velocity.x > 0 )

        {
            IsMoving = true;
            spriteRenderer.flipX = false;
        }
        else if (rb.velocity.x < 0)
        {
            IsMoving = true;
            spriteRenderer.flipX = true;
        }
        else
        {
            IsMoving = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleAttackInput();
        }

        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;

            if (comboTimer <= 0)
            {
                comboStep = 0; // reset combo
            }
        }

        Animazioni();
    }

   private void Animazioni()
{
        if (IsAttacking)
        {
            // opzionale: puoi aspettare fine animazione con coroutine
            return;
        }

    if (IsMoving)
    {
        anim.Play("Player_Run");
    
        
    }
    else if(IsMoving == false)
    {
        anim.Play("Player_Idle");
    }
}


private void HandleAttackInput()
{
    if (IsAttacking) return;

    comboStep++;
    if (comboStep > 3)
        comboStep = 1;

    StartCoroutine(AttackCoroutine(comboStep));
}


private IEnumerator AttackCoroutine(int attackIndex)
{
    IsAttacking = true;

    rb.velocity = Vector2.zero;

    comboTimer = comboResetTime; // reset finestra combo

    switch (attackIndex)
    {
        case 1:
            anim.Play("Attacco1");
            break;

        case 2:
            anim.Play("Attacco2");
            break;

        case 3:
            anim.Play("Attacco3");
            break;
    }

    yield return new WaitForSeconds(0.3f); // durata animazione

    IsAttacking = false;
}

   
}