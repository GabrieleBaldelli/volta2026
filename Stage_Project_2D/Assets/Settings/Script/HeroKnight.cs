using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool IsMoving;

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
        
        
        Animazioni(rb);
       

        // MOVIMENTO ORIZZONTALE
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
        else if(rb.velocity.y > 0)
        {
            IsMoving = true;

        }

         else if(rb.velocity.y < 0)
        {
            IsMoving = true;

        }
        
        else
        {
            IsMoving = false;
        }

    }

   private void Animazioni(Rigidbody2D rb)
{
    if (IsMoving == true)
    {
        anim.Play("Player_Run");
    }
    else
    {
        anim.Play("Player_Idle");
    }
}



 
   
}