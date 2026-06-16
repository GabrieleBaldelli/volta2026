using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CharacterAudioController))]
public class Wizard : Enemy
{
    // Parametri dell'attacco a distanza: animazione Attack1, danno vicino e spawn dei nemici.
    [Header("Summon Attack")]
    public GameObject[] enemyPrefabs;
    public Transform[] summonPoints;
    public int minEnemiesToSummon = 1;
    public int maxEnemiesToSummon = 2;
    public int maxSummonedEnemiesAlive = 5;
    public float summonCooldown = 4f;
    public float summonDelay = 0.35f;
    public float summonRadius = 2.5f;
    public float summonDamageRadius = 2f;
    public float summonPointRandomOffset = 1f;
    public bool useWizardChaseDistanceForSummons = true;
    public Transform enemyAstarPathParent;
    public string enemyAstarPathObjectName = "Enemy_AstarPath";

    // Parametri della fuga: il wizard sceglie punti lontani dal player o posizioni casuali valide.
    [Header("Teleport")]
    public Transform[] teleportPoints;
    public float playerTooCloseDistance = 3f;
    public float teleportCooldown = 4f;
    public float damageBeforeTeleport = 35f;
    public float minTeleportDistanceFromPlayer = 6f;
    public float maxTeleportDistanceFromPlayer = 10f;
    public int teleportSearchAttempts = 16;
    public float teleportPointRandomOffset = 1f;
    public bool teleportAfterMeleeAttack = true;
    public LayerMask teleportBlockedLayers;

    // Piccole pause che impediscono al wizard di concatenare azioni senza mai tornare in idle.
    [Header("Timing")]
    public float initialIdleDelay = 1.5f;
    public float idleDelayAfterAction = 1f;
    public float idleDelayAfterTeleport = 1.2f;
    public float hurtAnimationCooldown = 0.8f;

    // Colori usati solo nell'Editor per leggere le distanze principali del boss.
    [Header("Distance Gizmos")]
    public bool drawDistanceGizmos = true;
    public bool drawOnlyWhenSelected = false;
    public Color meleeAttackGizmoColor = new Color(1f, 0.2f, 0.2f, 0.8f);
    public Color summonDamageGizmoColor = new Color(1f, 0.55f, 0f, 0.8f);
    public Color teleportGizmoColor = new Color(0.35f, 0.65f, 1f, 0.8f);
    public Color summonGizmoColor = new Color(0.65f, 0.25f, 1f, 0.8f);

    // Seconda barra difensiva: finche' lo shield e' sopra 0, la vita non scende.
    [Header("Wizard Shield")]
    public Slider shieldSlider;
    public float shieldMassimo = 60f;
    public float shieldRegenDelay = 5f;
    public float shieldBrokenRegenDelay = 8f;
    public float shieldRegenPerSecond = 12f;
    public bool regenerateShieldOnlyWhenFar = true;
    public float shieldRegenMinPlayerDistance = 5f;

    // Cache dei componenti e riferimenti runtime. Sono separati da Enemy perche' molti campi base sono private.
    private Transform playerTransform;
    private PlayerMovement playerScript;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animazioni animazioni;
    private CharacterAudioController characterAudio;
    private LifeBar lifeBar;

    private readonly List<GameObject> summonedEnemies = new List<GameObject>();

    // Stato interno del boss: cooldown, vita/shield e blocchi anti-spam per coroutine e animazioni.
    private float vita;
    private float shield;
    private float lastDamageTime;
    private float damageTakenSinceTeleport;
    private float nextTeleportTime;
    private float nextSummonTime;
    private float nextMeleeTime;
    private float nextActionTime;
    private float lastHurtAnimationTime = -999f;

    private bool isAttacking;
    private bool isHurting;
    private bool isDying;
    private bool isTeleporting;
    private bool shieldBroken;
    private bool missingEnemyParentWarningShown;
    private Transform lastTeleportPoint;
    private Vector3 lastTeleportPosition;
    private bool hasLastTeleportPosition;

    public override bool IsAttackingSetGet
    {
        get { return isAttacking; }
        set { isAttacking = value; }
    }

    protected override void Start()
    {
        // Setup iniziale: componenti locali, player, barre UI e parent per i nemici evocati.
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        animazioni = GetComponent<Animazioni>();
        if(animazioni == null)
            animazioni = gameObject.AddComponent<Animazioni>();

        characterAudio = GetComponent<CharacterAudioController>();
        if(characterAudio == null)
            characterAudio = gameObject.AddComponent<CharacterAudioController>();

        if(aiPath == null)
            aiPath = GetComponent<AIPath>();

        if(aiPath != null)
            aiPath.canMove = false;

        FindPlayer();
        FindBars();
        FindEnemyAstarPathParent();

        vita = vitaMassima;
        shield = shieldMassimo;
        lastDamageTime = Time.time;
        nextActionTime = Time.time + initialIdleDelay;
        nextSummonTime = nextActionTime;
        nextMeleeTime = nextActionTime;

        UpdateLifeBar();
        UpdateShieldBar();
    }

    protected override void Update()
    {
        // Ciclo decisionale del wizard: idle, melee, teleport, summon o rigenerazione shield.
        if(isDying)
            return;

        FindPlayer();

        if(playerTransform == null || animazioni == null)
            return;

        RegenerateShield();

        if(isAttacking || isHurting || isTeleporting)
            return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        FacePlayer();

        if(Time.time < nextActionTime)
        {
            animazioni.Idle();
            return;
        }

        if(distance <= stopDistance)
        {
            if(Time.time >= nextMeleeTime)
                StartCoroutine(MeleeAttackCoroutine());
            else
                animazioni.Idle();

            return;
        }

        if(distance <= playerTooCloseDistance && Time.time >= nextTeleportTime)
        {
            StartCoroutine(TeleportCoroutine());
            return;
        }

        if(distance <= chaseDistance)
        {
            if(Time.time >= nextSummonTime)
                StartCoroutine(SummonAttackCoroutine());
            else
                animazioni.Idle();

            return;
        }

        if(Time.time >= nextTeleportTime && damageTakenSinceTeleport >= damageBeforeTeleport)
        {
            StartCoroutine(TeleportCoroutine());
            return;
        }

        if(distance > chaseDistance)
        {
            animazioni.Idle();
            return;
        }
    }

    private void FindPlayer()
    {
        // Mantiene aggiornato il riferimento al player anche se non e' stato assegnato nel prefab.
        if(player != null)
        {
            playerTransform = player.transform;
            playerScript = player.GetComponent<PlayerMovement>();
            return;
        }

        playerScript = FindObjectOfType<PlayerMovement>();
        if(playerScript == null)
            return;

        player = playerScript.gameObject;
        playerTransform = player.transform;
    }

    private void FindBars()
    {
        // Cerca le barre con i nomi usati nei prefab/scena, poi ripiega sui figli del wizard.
        if(lifeBar == null)
        {
            Transform lifeBarTransform = transform.Find("Canvas/Life_Bar");
            if(lifeBarTransform == null)
                lifeBarTransform = transform.Find("Life_Canvas/Life_Bar");

            if(lifeBarTransform != null)
                lifeBar = lifeBarTransform.GetComponent<LifeBar>();
        }

        if(lifeBar == null)
            lifeBar = GetComponentInChildren<LifeBar>();

        if(shieldSlider == null)
        {
            Transform shieldBarTransform = transform.Find("Canvas/Shield_Bar");
            if(shieldBarTransform == null)
                shieldBarTransform = transform.Find("Life_Canvas/Shield_Bar");

            if(shieldBarTransform != null)
                shieldSlider = shieldBarTransform.GetComponent<Slider>();
        }
    }

    private void FacePlayer()
    {
        // Orienta lo sprite verso il player senza usare la logica di movimento di Enemy.
        if(spriteRenderer != null && playerTransform != null)
            spriteRenderer.flipX = playerTransform.position.x > transform.position.x;
    }

    private IEnumerator SummonAttackCoroutine()
    {
        // Attack1: resta fermo, aspetta il frame di impatto, danneggia vicino e poi evoca.
        if(isDying || playerTransform == null)
            yield break;

        isAttacking = true;
        nextSummonTime = Time.time + attackDuration + summonCooldown;
        nextActionTime = Time.time + attackDuration + idleDelayAfterAction;

        StopMovement();
        animazioni.Attacco1();
        characterAudio.PlayAttackSound();
        characterAudio.PlayAttackEffortSound();

        yield return new WaitForSeconds(summonDelay);

        if(isDying)
        {
            isAttacking = false;
            yield break;
        }

        DamagePlayerIfNear(summonDamageRadius);
        SummonEnemies();

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - summonDelay));

        isAttacking = false;
        nextActionTime = Mathf.Max(nextActionTime, Time.time + idleDelayAfterAction);
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        // Attack2: colpo ravvicinato, poi opzionalmente si teletrasporta per allontanarsi.
        if(isDying || playerTransform == null)
            yield break;

        isAttacking = true;
        nextMeleeTime = Time.time + attackDuration + attackCooldown;
        nextActionTime = Time.time + attackDuration + idleDelayAfterAction;

        StopMovement();
        animazioni.Attacco2();
        characterAudio.PlayAttackSound();
        characterAudio.PlayAttackEffortSound();

        yield return new WaitForSeconds(attackHitDelay);

        if(!isDying)
            DamagePlayerIfNear(stopDistance + 0.4f);

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - attackHitDelay));

        isAttacking = false;

        if(teleportAfterMeleeAttack && !isDying)
        {
            StartCoroutine(TeleportCoroutine());
            yield break;
        }

        nextActionTime = Mathf.Max(nextActionTime, Time.time + idleDelayAfterAction);
    }

    private void DamagePlayerIfNear(float radius)
    {
        // Usato da entrambi gli attacchi: applica danno e knockback solo se il player e' nel raggio.
        if(player == null || playerTransform == null)
            return;

        playerScript = player.GetComponent<PlayerMovement>();
        if(playerScript == null)
            return;

        if(Vector2.Distance(transform.position, playerTransform.position) > radius)
            return;

        if(playerScript.IsShieldingSetGet)
            return;

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if(playerRb != null)
            StartCoroutine(KnockbackPlayer(playerRb));

        StartCoroutine(playerScript.HurtCoroutine(danno));
    }

    private IEnumerator KnockbackPlayer(Rigidbody2D playerRb)
    {
        // Versione locale del knockback, duplicata perche' quella di Enemy oggi e' privata.
        if(playerTransform == null)
            yield break;

        Vector2 knockbackDirection = (playerTransform.position - transform.position).normalized;

        playerRb.velocity = Vector2.zero;
        playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        if(playerRb != null)
            playerRb.velocity = Vector2.zero;
    }

    private void SummonEnemies()
    {
        // Instanzia i prefab sotto Enemy_AstarPath e passa subito il target ai componenti AI.
        if(enemyPrefabs == null || enemyPrefabs.Length == 0)
            return;

        RemoveMissingSummonedEnemies();

        int freeSlots = maxSummonedEnemiesAlive - summonedEnemies.Count;
        if(freeSlots <= 0)
            return;

        int enemiesToSummon = Random.Range(minEnemiesToSummon, maxEnemiesToSummon + 1);
        enemiesToSummon = Mathf.Clamp(enemiesToSummon, 0, freeSlots);

        for(int i = 0; i < enemiesToSummon; i++)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            if(prefab == null)
                continue;

            Transform parent = FindEnemyAstarPathParent();
            GameObject summonedEnemy = Instantiate(prefab, GetSummonPosition(i), Quaternion.identity, parent);
            summonedEnemies.Add(summonedEnemy);

            Enemy enemyScript = summonedEnemy.GetComponent<Enemy>();
            if(enemyScript != null)
            {
                enemyScript.player = player;

                if(useWizardChaseDistanceForSummons)
                    enemyScript.chaseDistance = Mathf.Max(enemyScript.chaseDistance, chaseDistance);
            }

            AIDestinationSetter destinationSetter = summonedEnemy.GetComponent<AIDestinationSetter>();
            if(destinationSetter != null)
                destinationSetter.target = playerTransform;
        }
    }

    private Transform FindEnemyAstarPathParent()
    {
        // Cerca una sola volta il contenitore di scena usato per organizzare i nemici evocati.
        if(enemyAstarPathParent != null)
            return enemyAstarPathParent;

        GameObject enemyAstarPathObject = GameObject.Find(enemyAstarPathObjectName);
        if(enemyAstarPathObject != null)
        {
            enemyAstarPathParent = enemyAstarPathObject.transform;
            return enemyAstarPathParent;
        }

        if(!missingEnemyParentWarningShown)
        {
            Debug.LogWarning("Enemy_AstarPath non trovato: i nemici evocati verranno spawnati senza parent.", this);
            missingEnemyParentWarningShown = true;
        }

        return null;
    }

    private Vector3 GetSummonPosition(int index)
    {
        // Preferisce gli spawn point assegnati; se mancano, usa una posizione casuale attorno al wizard.
        if(summonPoints != null && summonPoints.Length > 0)
        {
            Transform point = summonPoints[index % summonPoints.Length];
            if(point != null)
                return point.position + GetRandomOffset(summonPointRandomOffset);
        }

        Vector2 offset = Random.insideUnitCircle * summonRadius;
        if(offset.sqrMagnitude < 0.01f)
            offset = Vector2.right;

        return transform.position + (Vector3)offset;
    }

    private void RemoveMissingSummonedEnemies()
    {
        // Pulisce la lista quando un nemico evocato e' stato distrutto.
        for(int i = summonedEnemies.Count - 1; i >= 0; i--)
        {
            if(summonedEnemies[i] == null)
                summonedEnemies.RemoveAt(i);
        }
    }

    public override IEnumerator HurtCoroutine(float danno)
    {
        // Prima consuma lo shield, poi la vita. L'animazione Hit ha un cooldown anti-spam.
        if(isDying)
            yield break;

        bool canPlayHurtFeedback = !isHurting && Time.time >= lastHurtAnimationTime + hurtAnimationCooldown;

        lastDamageTime = Time.time;
        damageTakenSinceTeleport += danno;

        if(shield > 0f)
        {
            shield = Mathf.Max(0f, shield - danno);
            shieldBroken = shield <= 0f;
            UpdateShieldBar();
        }
        else
        {
            vita -= danno;
            UpdateLifeBar();
        }

        if(vita <= 1f)
        {
            StartCoroutine(DieCoroutine());
            yield break;
        }

        if(canPlayHurtFeedback && animazioni != null)
        {
            isHurting = true;
            StopMovement();
            animazioni.Danno();
            lastHurtAnimationTime = Time.time;
        }

        if(canPlayHurtFeedback)
            characterAudio.PlayHurtSound();

        if(damageTakenSinceTeleport >= damageBeforeTeleport && Time.time >= nextTeleportTime)
        {
            StartCoroutine(TeleportCoroutine());
            yield break;
        }

        if(!canPlayHurtFeedback)
            yield break;

        yield return new WaitForSeconds(0.25f);

        isHurting = false;
    }

    private void RegenerateShield()
    {
        // Lo shield torna solo dopo un periodo senza danni, e se richiesto quando il player e' lontano.
        if(shieldMassimo <= 0f || shield >= shieldMassimo)
            return;

        float delay = shieldBroken ? shieldBrokenRegenDelay : shieldRegenDelay;
        if(Time.time < lastDamageTime + delay)
            return;

        if(regenerateShieldOnlyWhenFar && playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if(distance < shieldRegenMinPlayerDistance)
                return;
        }

        shield = Mathf.Min(shieldMassimo, shield + shieldRegenPerSecond * Time.deltaTime);
        if(shield >= shieldMassimo)
            shieldBroken = false;

        UpdateShieldBar();
    }

    private IEnumerator TeleportCoroutine()
    {
        // Blocca azioni e movimento, svanisce, sposta il wizard, poi forza una piccola pausa idle.
        if(isTeleporting || isDying)
            yield break;

        isTeleporting = true;
        isAttacking = false;
        isHurting = false;
        damageTakenSinceTeleport = 0f;
        nextTeleportTime = Time.time + teleportCooldown;

        StopMovement();

        if(spriteRenderer != null)
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.25f);

        yield return new WaitForSeconds(0.15f);

        Vector3 teleportPosition = FindTeleportPosition();
        if(aiPath != null)
            aiPath.Teleport(teleportPosition);
        else
            transform.position = teleportPosition;

        yield return new WaitForSeconds(0.15f);

        if(spriteRenderer != null)
            spriteRenderer.color = Color.white;

        isTeleporting = false;
        nextActionTime = Time.time + idleDelayAfterTeleport;
        animazioni.Idle();
    }

    private Vector3 FindTeleportPosition()
    {
        // Sceglie prima tra i punti configurati; se non trova candidati validi, prova intorno al player.
        if(teleportPoints != null && teleportPoints.Length > 0)
        {
            List<Vector3> candidatePositions = new List<Vector3>();
            List<Transform> candidatePoints = new List<Transform>();

            foreach(Transform point in teleportPoints)
            {
                if(point == null)
                    continue;

                if(point == lastTeleportPoint && teleportPoints.Length > 1)
                    continue;

                int attempts = teleportPointRandomOffset > 0f ? 4 : 1;
                for(int i = 0; i < attempts; i++)
                {
                    Vector3 candidate = point.position + GetRandomOffset(teleportPointRandomOffset);
                    if(IsFarEnoughFromPlayer(candidate) && IsTeleportPositionFree(candidate))
                    {
                        candidatePositions.Add(candidate);
                        candidatePoints.Add(point);
                    }
                }
            }

            if(candidatePositions.Count > 0)
            {
                int index = Random.Range(0, candidatePositions.Count);
                lastTeleportPoint = candidatePoints[index];
                lastTeleportPosition = candidatePositions[index];
                hasLastTeleportPosition = true;
                return candidatePositions[index];
            }
        }

        if(playerTransform == null)
            return transform.position;

        for(int i = 0; i < teleportSearchAttempts; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            if(direction.sqrMagnitude < 0.01f)
                direction = (transform.position - playerTransform.position).normalized;

            float distance = Random.Range(minTeleportDistanceFromPlayer, maxTeleportDistanceFromPlayer);
            Vector3 candidate = playerTransform.position + (Vector3)(direction * distance);

            if(IsTeleportPositionFree(candidate) && !IsSameAsLastTeleport(candidate))
            {
                lastTeleportPoint = null;
                lastTeleportPosition = candidate;
                hasLastTeleportPosition = true;
                return candidate;
            }
        }

        Vector3 fallbackDirection = (transform.position - playerTransform.position).normalized;
        if(fallbackDirection.sqrMagnitude < 0.01f)
            fallbackDirection = Vector3.right;

        Vector3 fallback = playerTransform.position + fallbackDirection * minTeleportDistanceFromPlayer;
        lastTeleportPoint = null;
        lastTeleportPosition = fallback;
        hasLastTeleportPosition = true;
        return fallback;
    }

    private bool IsTeleportPositionFree(Vector3 position)
    {
        // Se non sono stati impostati layer bloccanti, ogni posizione e' considerata valida.
        if(teleportBlockedLayers.value == 0)
            return true;

        return Physics2D.OverlapCircle(position, 0.4f, teleportBlockedLayers) == null;
    }

    private bool IsFarEnoughFromPlayer(Vector3 position)
    {
        // Evita teleport troppo vicini al player.
        if(playerTransform == null)
            return true;

        return Vector2.Distance(position, playerTransform.position) >= minTeleportDistanceFromPlayer;
    }

    private bool IsSameAsLastTeleport(Vector3 position)
    {
        // Riduce il rischio di vedere il wizard comparire sempre nello stesso punto.
        if(!hasLastTeleportPosition)
            return false;

        return Vector2.Distance(position, lastTeleportPosition) < 0.75f;
    }

    private Vector3 GetRandomOffset(float radius)
    {
        // Offset 2D riusato da spawn e teleport point per non essere troppo prevedibili.
        if(radius <= 0f)
            return Vector3.zero;

        return (Vector3)(Random.insideUnitCircle * radius);
    }

    private IEnumerator DieCoroutine()
    {
        // Morte del boss: ferma tutto, lancia animazione/audio e distrugge l'oggetto.
        if(isDying)
            yield break;

        isDying = true;
        isAttacking = false;
        isHurting = false;
        isTeleporting = false;

        StopMovement();
        characterAudio.PlayDeathSound();

        if(animazioni != null)
            animazioni.Morte();

        yield return new WaitForSeconds(0.8f);

        if(this != null)
            Destroy(gameObject);
    }

    private void StopMovement()
    {
        // Spegne sia la fisica 2D sia il movimento A* quando il wizard deve restare fermo.
        if(rb != null)
            rb.velocity = Vector2.zero;

        if(aiPath != null)
            aiPath.canMove = false;
    }

    private void UpdateLifeBar()
    {
        // Aggiorna sia la LifeBar custom sia l'Image legacy ereditata da Enemy, se presenti.
        if(lifeBar != null)
            lifeBar.UpdateLifeBar(vita, vitaMassima);

        if(HealthImage != null)
            HealthImage.fillAmount = vita / vitaMassima;
    }

    private void UpdateShieldBar()
    {
        // La slider dello shield lavora in percentuale 0-1.
        if(shieldSlider != null)
            shieldSlider.value = shieldMassimo <= 0f ? 0f : shield / shieldMassimo;
    }

    private void OnDrawGizmos()
    {
        if(drawOnlyWhenSelected)
            return;

        DrawDistanceGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        DrawDistanceGizmos();
    }

    private void DrawDistanceGizmos()
    {
        // Mostra solo i range principali: melee, danno summon, fuga e area di evocazione.
        if(!drawDistanceGizmos)
            return;

        Vector3 position = transform.position;

        DrawWireCircle(position, stopDistance, meleeAttackGizmoColor);
        DrawWireCircle(position, summonDamageRadius, summonDamageGizmoColor);
        DrawWireCircle(position, playerTooCloseDistance, teleportGizmoColor);
        DrawWireCircle(position, chaseDistance, summonGizmoColor);

        Transform targetPlayer = playerTransform;
        if(targetPlayer == null && player != null)
            targetPlayer = player.transform;

        if(targetPlayer != null)
        {
            DrawWireCircle(targetPlayer.position, minTeleportDistanceFromPlayer, teleportGizmoColor);
            DrawWireCircle(targetPlayer.position, maxTeleportDistanceFromPlayer, teleportGizmoColor);
        }
    }

    private void DrawWireCircle(Vector3 center, float radius, Color color)
    {
        if(radius <= 0f)
            return;

        Gizmos.color = color;
        Gizmos.DrawWireSphere(center, radius);
    }
}
