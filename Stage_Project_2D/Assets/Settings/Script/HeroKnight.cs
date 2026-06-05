using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Life Bar")]
    public Image HealthImage;

    [Header("Stats Player")]
    public const float vitaMassima = 100f;
    private float vita = vitaMassima;
    public float danno = 20f;
    public float speed = 5f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.15f;
    public float comboResetTime = 0.8f;
    
    [Header("Knockback Settings")]
    public float knockbackForce = 60f;
    public float knockbackDuration = 0.2f;

    //Componenti del Player
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
   
    [Header("Stati del Player")]
    private bool IsMoving;
    private bool IsAttacking;
    private bool IsDashing;
    private bool IsShielding;
    private bool IsHurting;

    public bool IsShildingSetGet
    {
        get 
        { 
            return IsShielding; 
        }
        set 
        { 
            IsShielding = value; 
        }
    }

    [Header("Enemy References")]
    public GameObject enemy;
    private Enemy1 enemyScript;
    
    //Serve per memorizzare la direzione del movimento del player, per poi utilizzarla nel dash
    private Vector2 dashDirection;

    //Indica quale attaco della combo è il personaggio
    private int comboStep = 0;

    //Serve per capire se il player ha premuto in tempo per effettuare la combo
    private float comboTimer = 0f;

    //Movimenti del personaggio
    private float movementX;
    private float movementY;

    void Start()
    {
        //Prende automaticamente i componenti del gameobject
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        //Serve per dire che se sto dashando
        //Il return esce dall'update per far finire l'animazione
        if (IsDashing)
            {
                return;
            }

        if (IsShielding)
            {
                return;
            }

        if (IsHurting)
        {
            return;
        }

       
        //INPUT asse orizzontale (X)
        movementX = Input.GetAxisRaw("Horizontal");

        //INPUT asse verticale (Y)
        movementY = Input.GetAxisRaw("Vertical");

        //Prende la direzione attuale del movimento
        //(Utilizzato per il dash)
        dashDirection = new Vector2(movementX, movementY).normalized;

        //Movimento del player con il rigidbody
        rb.velocity = new Vector2(movementX * speed, movementY * speed);
        
        

       
        if (rb.velocity.x > 0 )

        {
            IsMoving = true;
            //Se il personaggio si muove a destra, lo sprite rimane normale
            spriteRenderer.flipX = false;
        }
        else if (rb.velocity.x < 0)
        {
            IsMoving = true;
            //Se il personaggio si muove a sinistra, lo sprite si ribalta
            spriteRenderer.flipX = true;
        }
        else if(rb.velocity.y > 0 || rb.velocity.y < 0)
        {
            IsMoving = true;
        }

        else
        {
            IsMoving = false;
        }

        //INPUT di attacco con il tasto SX del mouse
        if (Input.GetMouseButtonDown(0) && IsAttacking == false)
        {
            HandleAttackInput();
        }

        if (Input.GetMouseButtonDown(1) && IsShielding == false && IsAttacking == false && IsDashing == false)
        {
            StartCoroutine(ShieldCoroutine());
        }

        //INPUT del dash con il tasto "Shift"
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(DashCoroutine());
        }

         //INPUT del dash con il tasto "Q"
       /* if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(DashCoroutine());
        }*/
        

        // TIMER DELLA COMBO

        //Se il timer combo è attivo
        if (comboTimer > 0)
        {
            //Scala il tempo
            comboTimer -= Time.deltaTime;

            //Se il tempo è finito
            if (comboTimer <= 0)
            {
                //Reset della combo
                comboStep = 0; 
            }
        }

        //Gestione delle animazioni
        Animazioni();
    }

   private void Animazioni()
{
        if (IsHurting)
        {
            return;
        }

        //Se attacca, non cambiare animazione
        if (IsAttacking)
        {
            return;
        }

        if (IsShielding)
        {
            return;
        }


        //Se dasha, non cambiare animazione
        if (IsDashing)
        {
            return;
        }

    //Se il personaggio si muove
    if (IsMoving && IsHurting == false)
    {
        //Cambio animazione
        anim.Play("Player_Run");
    }

    //Altrimenti se è fermo
    else if(IsMoving == false && IsHurting == false)
    {
        //Cambio animzione
        anim.Play("Player_Idle");
    }
}


    //GESTIONE DELL'ATTACCO 

    private void HandleAttackInput()
    {
        //Incrementa il numero della combo
        comboStep++;

        //Se supera il terzo colpo, allora ricomincia la combo da capo
        if (comboStep > 3)
            comboStep = 1;

        //Avvio del tempo di attesa per la combo
        StartCoroutine(AttackCoroutine(comboStep));
    }


    private IEnumerator AttackCoroutine(int attackIndex)
    {
        float distanza = Vector2.Distance(transform.position, enemy.transform.position);

        //Sto attaccando 
        IsAttacking = true;

        //Immobilizza il mio personaggio
        rb.velocity = Vector2.zero;

        //resetta il timer della combo
        comboTimer = comboResetTime; // reset finestra combo

        //In base a quale step della combo sei, cambia l'animazione
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

         
        HitEnemy();

        //Aspetto che finisca l'animazione
        yield return new WaitForSeconds(0.3f); 

        //Attacco finito
        IsAttacking = false;
    }

    private void HitEnemy()
    {
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        enemyScript = enemy.GetComponent<Enemy1>();

        float distanza = Vector2.Distance(transform.position, enemy.transform.position);

        if (distanza > 1.5f) // raggio attacco
        return;

        StartCoroutine(KnockbackPlayer(enemyRb, enemyScript));

        Debug.Log("Nemico Colpito");

        StartCoroutine(enemyScript.HurtCoroutine(danno)); // Danno al nemico, implamentato nella classe HeroKnight
    }
    
    private IEnumerator KnockbackPlayer(Rigidbody2D enemyRb, Enemy1 enemyScript)                                         //Guardo
    {
        Transform e = enemy.transform;
        Vector2 knockbackDirection = (e.position - transform.position).normalized;

        if (enemyScript != null && enemyScript.aiPath != null)
        {
            enemyScript.aiPath.canMove = false;
            enemyScript.aiPath.enabled = false;
        }

        enemyRb.velocity = knockbackDirection * knockbackForce;

        yield return new WaitForSeconds(knockbackDuration);

        enemyRb.velocity = Vector2.zero;

        if (enemyScript != null && enemyScript.aiPath != null)
        {
            enemyScript.aiPath.enabled = true;
            enemyScript.aiPath.canMove = true;
        }
    }

    public IEnumerator HurtCoroutine(float danno)
    {
        IsHurting = true;

        //rb.velocity = Vector2.zero;

        anim.Play("Player_Attacco_Subito");

         vita -=danno;

        HealthImage.fillAmount = vita / vitaMassima;

        yield return new WaitForSeconds(0.3f);

        IsHurting = false;

        Debug.Log("viene colpito");
    }

    private IEnumerator DashCoroutine()
    {
        //Sto dashando
        IsDashing = true;

        //Incremento la sua velocità, per il dash
        rb.velocity = dashDirection * dashSpeed;

        //Se si sta muovendo
        if(IsMoving)
        {
            //Cambio animazione
            anim.Play("Player_Dash");
        }

        //Aspetto che l'animazione finisca
        yield return new WaitForSeconds(0.3f);

        //Imposto la velocità del personaggio a 0
        rb.velocity = Vector2.zero;

        //Fine dash
        IsDashing = false;
    } 

    private IEnumerator ShieldCoroutine()
    {
        enemyScript = enemy.GetComponent<Enemy1>();

        IsShielding = true;

        if(IsShielding == true && enemyScript.IsAttackingSetGet == true)
        {
            rb.velocity = Vector2.zero;

            anim.Play("Player_PerfectShield");

            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            rb.velocity = Vector2.zero;

            anim.Play("Player_Shield");

            yield return new WaitForSeconds(0.5f);
        }

        IsShielding = false;
    }
}
