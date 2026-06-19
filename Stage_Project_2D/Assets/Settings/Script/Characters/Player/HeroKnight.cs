using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Pathfinding;

[RequireComponent(typeof(CharacterAudioController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Stats Player")]
    public const float vitaMassimaIniziale = 100f;
    public float vitaMassima = vitaMassimaIniziale;
    private float vita = vitaMassimaIniziale;

    public float Vita 
    {
        get { return vita; }
        set { vita = value; }
    }

    public float VitaMassima
    {
        get { return vitaMassima; }
    }

    public float danno = 20f;
    public float speed = 5f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.15f;
    public float comboResetTime = 0.8f;

    private int coin;

    public int CoinSetGet
    {
        get {return coin;}
        set { coin = value;}
    }


    // XP accumulata dal player uccidendo i nemici.
    private float xp;

    // Proprieta' usata dagli altri script per leggere o aggiungere XP.
    // Quando viene assegnato un valore, passa da AddXP cosi' controlla anche il level up.
    public float AddXp
    {
        get { return xp; }
        set { AddXP(value); }
    }

    // Livello attuale del player. Viene letto dal menu upgrade.
    private float livelloAttuale = 1f;
    public float livello
    {
        get { return livelloAttuale; }
    }

    // Moltiplicatore della XP richiesta: livello 1 = 100 XP, poi aumenta a ogni level up.
    private float LivelloSuccessivo = 1f;

    // Valori letti dal menu upgrade per mostrare "XP attuale / XP prossimo livello".
    public float XpAttuale
    {
        get { return xp; }
    }

    public float XpProssimoLivello
    {
        get { return 100f * LivelloSuccessivo; }
    }

    public float MoltiplicatoreLivelloSuccessivo
    {
        get { return LivelloSuccessivo; }
    }

    public void RestoreProgression(float savedXp, float savedLevel, float savedNextLevelMultiplier)
    {
        xp = Mathf.Max(0f, savedXp);
        livelloAttuale = Mathf.Max(1f, savedLevel);
        LivelloSuccessivo = Mathf.Max(1f, savedNextLevelMultiplier);
    }

    public void AddXP(float xpToAdd)
    {
        if(xpToAdd <= 0f)
            return;

        xp += xpToAdd;

        Debug.Log("xp attuale: " + xp);

        // Se l'XP supera la soglia, il player sale di livello.
        // Il while permette di gestire anche tanta XP ricevuta tutta insieme.
        while(xp >= 100f * LivelloSuccessivo)
        {
            livelloAttuale++;

            // Togli solo l'XP usata per il level up, lasciando quella in piu'.
            xp -= 100f * LivelloSuccessivo;

            // Ogni nuovo livello da 2 punti spendibili nel menu upgrade.
            AddUpgradePoints(2);

            // Aumenta la XP richiesta per il prossimo livello.
            LivelloSuccessivo += 0.3f;

            Debug.Log("livello attuale: " + livelloAttuale);
            Debug.Log("xp rimanente: " + xp);
        }
    }

    private void AddUpgradePoints(int points)
    {
        PlayerUpgradeStats upgradeStats = GetComponent<PlayerUpgradeStats>();

        // Se il player ha il componente degli upgrade, aggiunge i punti da spendere.
        if(upgradeStats != null)
            upgradeStats.upgradePoints += points;
    }

    public void IncreaseMaxHealth(float amount, bool refillHealth)
    {
        vitaMassima += amount;

        if(refillHealth)
            vita = vitaMassima;
        else
            vita = Mathf.Min(vita, vitaMassima);

        LifeBar lifeBar = GetComponentInChildren<LifeBar>();

        if(lifeBar != null)
            lifeBar.UpdateLifeBar(vita, vitaMassima);
    }


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

    // Script unico che contiene tutti i clip e i volumi audio del personaggio.
    // I suoni non vengono piu' assegnati direttamente qui, ma nel CharacterAudioController.
    private CharacterAudioController characterAudio;
   
    [Header("Stati del Player")]
    private bool IsMoving;
    private bool IsAttacking;
    private bool IsDashing;
    private bool IsShielding;
    private bool IsPerfectShielding;
    private bool IsHurting;
    private bool IsDying;

    public bool IsShieldingSetGet
    {
        get { return IsShielding; }
        set { IsShielding = value; }
    }
    public bool IsPerfectShieldingSetGet
    {
        get { return IsPerfectShielding; }
        set { IsPerfectShielding = value; }
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

        // Prende il controller audio collegato all'HeroKnight.
        characterAudio = GetComponent<CharacterAudioController>();

        // Se manca, lo aggiunge automaticamente per evitare errori quando si chiamano i suoni.
        if(characterAudio == null)
            characterAudio = gameObject.AddComponent<CharacterAudioController>();

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
        {
            StopGrassRunSound();
            return;
        }

        //Il return esce dall'update per far finire le animazioni
        if (IsDashing)
        {
            StopGrassRunSound();
            return;
        }
        
        if (IsHurting)
        {
            StopGrassRunSound();
            return;
        }
        
        if (IsDying)
        {
            StopGrassRunSound();
            return;
        }

        if (IsShielding)
        {
            StopGrassRunSound();
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

        HandleGrassRunSound();

        //INPUT di attacco con il tasto SX del mouse
        if (Input.GetMouseButtonDown(0) && IsAttacking == false)
        {
            HandleAttackInput();
        }

        //INPUT di parata con il tasto DX del mouse
        if (Input.GetMouseButtonDown(1) && IsShielding == false && IsAttacking == false && IsDashing == false && IsHurting == false && IsDying == false)
        {
            StartCoroutine(ShieldCoroutine());
        }

        //INPUT del dash con il tasto "Shift"
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(DashCoroutine());
        }

        // TIMER DELLA COMBO
        if (comboTimer > 0)     //Se il timer combo è attivo
        {
            comboTimer -= Time.deltaTime;   //Scala il tempo
            
            if (comboTimer <= 0)    //Se il tempo è finito resetta la combo
                comboStep = 0; 
        }



        //Gestione delle animazioni
        Animazioni();
    }

    private void HandleGrassRunSound()
    {
        // Se il player si sta muovendo e non e' bloccato da altre azioni, avvia il suono di corsa.
        if (IsMoving && !IsAttacking && !IsDashing && !IsShielding && !IsHurting && !IsDying)
        {
            characterAudio.PlayRunSound();
        }
        else
        {
            // Se il player si ferma, attacca, para, dasha o prende danno, ferma la corsa.
            StopGrassRunSound();
        }
    }

    private void StopGrassRunSound()
    {
        // Ferma solo il loop della corsa gestito dal CharacterAudioController.
        characterAudio.StopRunSound();
    }

    private void PlaySwordSwingSound()
    {
        // Suono della lama/spadata quando parte l'attacco.
        characterAudio.PlaySwordSwingSound();
    }

    private void PlayAttackEffortSound()
    {
        // Suono della voce/sforzo dell'HeroKnight quando attacca.
        characterAudio.PlayAttackEffortSound();
    }

    private void PlayHurtSound()
    {
        // Suono riprodotto quando l'HeroKnight subisce danno.
        characterAudio.PlayHurtSound();
    }

    private void PlayShieldSound()
    {
        // Suono della parata normale.
        characterAudio.PlayShieldSound();
    }

    private void PlayPerfectShieldSound()
    {
        // Suono della parata perfetta.
        characterAudio.PlayPerfectShieldSound();
    }

    private void PlayDeathSound()
    {
        // Suono riprodotto quando l'HeroKnight muore.
        characterAudio.PlayDeathSound();
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

        PlaySwordSwingSound();
        PlayAttackEffortSound();

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

            PassiveSpellManager passiveSpellManager = GetComponent<PassiveSpellManager>();

            if(passiveSpellManager != null)
                passiveSpellManager.NotifyAttackHit();

            Debug.Log("Nemico Colpito");
        }
    }
    
    private IEnumerator Knockback(Rigidbody2D enemyRb, AIPath enemyPath, Transform e)                                         //Guardo
    {
        if(enemyRb == null || e == null)
            yield break;

        Enemy enemyScript = e.GetComponent<Enemy>();
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

        if(enemyScript != null && (!enemyScript.enabled || enemyScript.IsDyingSetGet))
            yield break;

        if (enemyPath != null)
        {
            enemyPath.enabled = true;
            enemyPath.canMove = true;
        }
    }

    public IEnumerator HurtCoroutine(float danno)
    {
        IsHurting = true;
        IsShielding = false;
        IsPerfectShielding = false;

        //rb.velocity = Vector2.zero;

        //Recupera lo script che gestisce la barra della vita del nemico
        LifeBar LifebarScript = transform.Find("Life_Canvas/Life_Bar").GetComponent<LifeBar>();

        //rb.velocity = Vector2.zero;

        //Riduce la vita del nemico in base al danno ricevuto
        vita -=danno;

        PlayHurtSound();

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
        if(shieldBarScript != null && shieldBarScript.shieldSetGet <= 0)
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

            PlayPerfectShieldSound();

            anim.Play("Player_PerfectShield");

            yield return new WaitForSeconds(0.3f);

            if(IsHurting || IsDying)
            {
                IsShielding = false;
                IsPerfectShielding = false;
                yield break;
            }

            while(IsAnyEnemyAttacking() && Input.GetMouseButton(1) && !IsHurting && !IsDying)
            {
                rb.velocity = Vector2.zero;
                yield return null;
            }

            IsPerfectShielding = false;
        }
        else
        {
            rb.velocity = Vector2.zero;

            PlayShieldSound();

            anim.Play("Player_Shield");
        }

        if(Input.GetMouseButton(1) && shieldBarScript != null && shieldBarScript.shieldSetGet > 0 && !IsHurting && !IsDying)
        {
            anim.Play("Player_Shield");
        }

        while(Input.GetMouseButton(1) && !IsAttacking && !IsDashing && !IsHurting && !IsDying)
        {
            if(shieldBarScript != null && shieldBarScript.shieldSetGet <= 0)
                break;

            rb.velocity = Vector2.zero;
            yield return null;
        }

        IsShielding = false;
        IsPerfectShielding = false;
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

        transform.rotation = Quaternion.Euler(0, 0, 90);

        PlayDeathSound();

        anim.Play("Player_Death");

        yield return new WaitForSeconds(1f);

        ResetStats();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        IsDying = false;
    }

    public void ResetStats()
    {
        if(IsDying)
        {
            // Reset delle statistiche di progressione quando il player muore.
            xp = 0;
            livelloAttuale = 1;
            LivelloSuccessivo = 1f;
            PlayerUpgradeStats upgradeStats = GetComponent<PlayerUpgradeStats>();

            if(upgradeStats != null)
                upgradeStats.ResetProgress();

        }
    }
}
