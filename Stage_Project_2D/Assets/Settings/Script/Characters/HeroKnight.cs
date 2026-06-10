using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Pathfinding;

public class PlayerMovement : MonoBehaviour
{
    [Header("Stats Player")]
    public const float vitaMassima = 100f;
    private float vita = vitaMassima;
    public float Vita 
    {
        get 
        { 
            return vita; 
        }
        set 
        { 
            vita = value; 
        }
    }
    public float danno = 20f;
    public float speed = 5f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.15f;
    public float comboResetTime = 0.8f;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    [Header("Shield Settings")]
    public float shieldCheckRange = 2.5f;
    
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
    private bool IsPerfectShielding;
    private bool IsHurting;
    private bool IsDying;

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
    public bool IsPerfectShildingSetGet
    {
        get 
        { 
            return IsPerfectShielding; 
        }
        set 
        { 
            IsPerfectShielding = value; 
        }
    }

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

        if(anim == null)
            anim = GetComponentInChildren<Animator>();

        if(spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if(rb == null)
            Debug.LogError("Rigidbody2D mancante sul Player", this);

        if(anim == null)
            Debug.LogError("Animator mancante sul Player o nei suoi figli", this);

        if(spriteRenderer == null)
            Debug.LogError("SpriteRenderer mancante sul Player o nei suoi figli", this);
    }

    void Update()
    {
        if (Time.timeScale == 0)
            return;

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

        if (IsDying)
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
        //INPUT di parata con il tasto DX del mouse
        if (Input.GetMouseButton(1) && IsShielding == false && IsAttacking == false && IsDashing == false)
        {
            StartCoroutine(ShieldCoroutine());
        }

        //INPUT del dash con il tasto "Shift"
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.R))
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
        if (IsDying)
        {
            return;
        }

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
        if(attackPoint == null)
            return;

        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        
        foreach(Collider2D enemyCollider in enemiesHit)
        {
            if(enemyCollider == null)
                continue;

            Enemy enemyScript = enemyCollider.GetComponent<Enemy>();

            if(enemyScript == null)
                continue;

            Rigidbody2D enemyRb = enemyCollider.GetComponent<Rigidbody2D>();
            if(enemyRb == null)
                continue;

            StartCoroutine(Knockback(enemyRb, enemyScript.aiPath, enemyCollider.transform));
            StartCoroutine(enemyScript.HurtCoroutine(danno));

            Debug.Log("Nemico Colpito");
        }
    }
    
    private IEnumerator Knockback(Rigidbody2D enemyRb, AIPath enemyPath, Transform e)                                         //Guardo
    {
        if(enemyRb == null || e == null)
            yield break;

        Vector2 knockbackDirection = (e.position - transform.position).normalized;

        if (enemyPath != null)
        {
            enemyPath.canMove = false;
            enemyPath.enabled = false;
        }

        enemyRb.velocity = knockbackDirection * knockbackForce;

        yield return new WaitForSeconds(knockbackDuration);

        if(enemyRb == null)
            yield break;

        enemyRb.velocity = Vector2.zero;

        if (enemyPath != null)
        {
            enemyPath.enabled = true;
            enemyPath.canMove = true;
        }
    }

    public IEnumerator HurtCoroutine(float danno)
    {
        IsHurting = true;

        //rb.velocity = Vector2.zero;

        //Recupera lo script che gestisce la barra della vita del nemico
        LifeBar LifebarScript = transform.Find("Life_Canvas/Life_Bar").GetComponent<LifeBar>();

        //rb.velocity = Vector2.zero;

        //Riduce la vita del nemico in base al danno ricevuto
        vita -=danno;

        // Aggiorna la barra della vita visivamente
        LifebarScript.UpdateLifeBar(vita, vitaMassima);

        if(vita <= 1)
        {
            StartCoroutine(Die());
        }
        else
        {

            // Riproduce l'animazione di danno subito
            anim.Play("Player_Attacco_Subito");


            Debug.Log(vita);

           

            yield return new WaitForSeconds(0.3f);

            IsHurting = false;

            Debug.Log("viene colpito");

        }
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
        ShieldBar shieldBarScript = GetComponentInChildren<ShieldBar>();
        if(shieldBarScript != null && shieldBarScript.shieldSetGet  == 0)
        {
            yield break; // Esce dalla coroutine se lo scudo è esaurito
        }

        if(rb == null || anim == null)
            yield break;

        IsShielding = true;

        bool EnemyIsAttacking = IsAnyEnemyAttacking();

        if(IsShielding && EnemyIsAttacking)
        {
            IsPerfectShielding = true;

            rb.velocity = Vector2.zero;

            anim.Play("Player_PerfectShield");

            yield return new WaitForSeconds(0.3f);

            IsPerfectShielding = false;
        }
        else
        {
            rb.velocity = Vector2.zero;

            anim.Play("Player_Shield");

            yield return new WaitForSeconds(0.5f);
        }

        IsShielding = false;
    }

    public bool IsAnyEnemyAttacking()
    {
        Collider2D[] enemiesNearPlayer = Physics2D.OverlapCircleAll(transform.position, shieldCheckRange, enemyLayer);

        foreach(Collider2D enemyCollider in enemiesNearPlayer)
        {
            Enemy enemyScript = enemyCollider.GetComponent<Enemy>();

            if(enemyScript != null && enemyScript.IsAttackingSetGet)
                return true;
        }

        return false;
    }

    public IEnumerator Die()
    {
        IsDying = true;

        Debug.Log("Morte");

        transform.rotation = Quaternion.Euler(0, 0, -90);

        anim.Play("Player_Death");

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        IsDying = false;

       
    }

}
