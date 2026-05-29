using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    //Velocità del personaggio
    public float speed = 5f;

    //Vita del personaggio
    public float vita = 100f;

    //Componenti del Player
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
   
    //Stati del personaggio
    private bool IsMoving;
    private bool IsAttacking;
    private bool IsDashing;

    //Velocità del dash
    public float dashSpeed = 10f;

    //Durata del dash
    public float dashDuration = 0.15f;

    //Direzione del dash
    private Vector2 dashDirection;


    //Indica quale attaco della combo è il personaggio
    private int comboStep = 0;

    //Serve per capire se il player ha premuto in tempo per effettuare la combo
    private float comboTimer = 0f;

    //Tempo massimo per continuare la combo
    public float comboResetTime = 0.8f;

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
        else
        {
            IsMoving = false;
        }

        //INPUT di attacco con il tasto SX del mouse
        if (Input.GetMouseButtonDown(0))
        {
            HandleAttackInput();
        }

        //INPUT del dash con il tasto "Q"
        if (Input.GetKeyDown("q"))
        {
            StartCoroutine(DashCoroutine());
        }


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
        //Se attacca, non cambiare animazione
        if (IsAttacking)
        {
            return;
        }

        //Se dasha, non cambiare animazione
        if (IsDashing)
        {
            return;
        }

    //Se il personaggio si muove
    if (IsMoving)
    {
        //Cambio animazione
        anim.Play("Player_Run");
    }

    //Altrimenti se è fermo
    else if(IsMoving == false)
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

    //Aspetto che finisca l'animazione
    yield return new WaitForSeconds(0.3f); 

    //Attacco finito
    IsAttacking = false;
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


//PROSSIMA COSA DA FARE

// TROVARE TUTTI GLI ASSETS DA SOSTITUIRE A QUELLO ORIGINALE
   
}