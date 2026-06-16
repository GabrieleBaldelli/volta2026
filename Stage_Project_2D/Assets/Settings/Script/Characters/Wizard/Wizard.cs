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

    // Cooldown tra un attacco di evocazione e l'altro, 
    // che include il tempo dell'animazione e una pausa dopo.
    public float summonCooldown = 4f;
    // Tempo tra l'inizio dell'animazione e il momento in cui il danno viene applicato 
    // e i nemici vengono evocati.
    public float summonDelay = 0.35f;
    
    // Raggio attorno al wizard in cui possono comparire i nemici evocati se non ci sono summon point assegnati o sono meno del numero da evocare.
    public float summonRadius = 2.5f; 

    // Raggio in cui il player subisce danno se e' troppo vicino al wizard durante l'attacco di evocazione.
    public float nearAttackArea = 2f; 
    // Offset casuale aggiuntivo alla posizione di spawn dei nemici evocati 
    // per renderli meno prevedibili, anche quando si usano summon point fissi.
    public float summonRandomPoint = 1f;

    // Se true, i nemici evocati useranno la stessa distanza di inseguimento del wizard, 
    // altrimenti useranno quella definita nei loro prefab.
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

    // Riferimenti specifici del wizard. Player, vita, animazioni, audio e movimento arrivano da Enemy.
    private LifeBar lifeBar;

    private readonly List<GameObject> summonedEnemies = new List<GameObject>();

    // Stato interno del boss: cooldown, vita/shield e blocchi anti-spam per coroutine e animazioni.
    private float shield;
    private float lastDamageTime;
    private float damageTakenSinceTeleport;
    private float nextTeleportTime;
    private float nextSummonTime;
    private float nextMeleeTime;
    private float nextActionTime;
    private float lastHurtAnimationTime = -999f;

    private bool isTeleporting;
    private bool shieldBroken;
    private bool missingEnemyParentWarningShown;
    private Transform lastTeleportPoint;
    private Vector3 lastTeleportPosition;
    private bool hasLastTeleportPosition;

    protected override void Start()
    {
        // Setup iniziale: Enemy prepara componenti/player/vita; il wizard aggiunge barre, summon e shield.
        base.Start();
        FindBars();
        FindEnemyAstarPathParent();

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
        if(IsDying)
            return;

        if(PlayerTransform == null)
            TryFindPlayer();

        if(PlayerTransform == null || animazioni == null)
            return;

        // Lo shield si rigenera passivamente quando il boss non subisce danni da un po' e quando il player e' lontano.
        RegenerateShield();

        if(IsAttacking || IsHurting || isTeleporting)
            return;

        float distance = Vector2.Distance(transform.position, PlayerTransform.position);

        // Il wizard guarda sempre il player, ma attacca o si muove solo se e' abbastanza vicino e se i cooldown lo permettono.
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

        if(distance > chaseDistance * 1.5f)
        {
            animazioni.Idle();
            transform.Find("Life_Canvas").gameObject.SetActive(false);
            return;
        }
        else
        {
            transform.Find("Life_Canvas").gameObject.SetActive(true);
        }
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

    private IEnumerator SummonAttackCoroutine()
    {
        // Attack1: resta fermo, aspetta il frame di impatto, danneggia vicino e poi evoca.
        if(IsDying || PlayerTransform == null)
            yield break;

        IsAttacking = true;
        nextSummonTime = Time.time + attackDuration + summonCooldown;
        nextActionTime = Time.time + attackDuration + idleDelayAfterAction;

        StopMovement();
        animazioni.Attacco1();
        PlayAttackSound();
        PlayAttackEffortSound();

        yield return new WaitForSeconds(summonDelay);

        if(IsDying)
        {
            IsAttacking = false;
            yield break;
        }

        DamagePlayerIfNear(nearAttackArea);
        SummonEnemies();

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - summonDelay));

        IsAttacking = false;
        nextActionTime = Mathf.Max(nextActionTime, Time.time + idleDelayAfterAction);
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        // Attack2: colpo ravvicinato, poi opzionalmente si teletrasporta per allontanarsi.
        if(IsDying || PlayerTransform == null)
            yield break;

        IsAttacking = true;
        nextMeleeTime = Time.time + attackDuration + attackCooldown;
        nextActionTime = Time.time + attackDuration + idleDelayAfterAction;

        StopMovement();
        animazioni.Attacco2();
        PlayAttackSound();
        PlayAttackEffortSound();

        yield return new WaitForSeconds(attackHitDelay);

        if(!IsDying)
            DamagePlayerIfNear(stopDistance + 0.4f);

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - attackHitDelay));

        IsAttacking = false;

        if(teleportAfterMeleeAttack && !IsDying)
        {
            StartCoroutine(TeleportCoroutine());
            yield break;
        }

        nextActionTime = Mathf.Max(nextActionTime, Time.time + idleDelayAfterAction);
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
                destinationSetter.target = PlayerTransform;
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
                return point.position + GetRandomOffset(summonRandomPoint);
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
        if(IsDying)
            yield break;

        bool canPlayHurtFeedback = !IsHurting && Time.time >= lastHurtAnimationTime + hurtAnimationCooldown;

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
            IsHurting = true;
            StopMovement();
            animazioni.Danno();
            lastHurtAnimationTime = Time.time;
        }

        if(canPlayHurtFeedback)
            PlayHurtSound();

        if(damageTakenSinceTeleport >= damageBeforeTeleport && Time.time >= nextTeleportTime)
        {
            StartCoroutine(TeleportCoroutine());
            yield break;
        }

        if(!canPlayHurtFeedback)
            yield break;

        yield return new WaitForSeconds(0.25f);

        IsHurting = false;
    }

    private void RegenerateShield()
    {
        // Lo shield torna solo dopo un periodo senza danni, e se richiesto quando il player e' lontano.
        if(shieldMassimo <= 0f || shield >= shieldMassimo)
            return;

        float delay = shieldBroken ? shieldBrokenRegenDelay : shieldRegenDelay;
        if(Time.time < lastDamageTime + delay)
            return;

        if(regenerateShieldOnlyWhenFar && PlayerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, PlayerTransform.position);
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
        if(isTeleporting || IsDying)
            yield break;

        isTeleporting = true;
        IsAttacking = false;
        IsHurting = false;
        damageTakenSinceTeleport = 0f;
        nextTeleportTime = Time.time + teleportCooldown;

        StopMovement();

        if(spriterenderer != null)
            spriterenderer.color = new Color(1f, 1f, 1f, 0.25f);

        yield return new WaitForSeconds(0.15f);

        Vector3 teleportPosition = FindTeleportPosition();
        if(aiPath != null)
            aiPath.Teleport(teleportPosition);
        else
            transform.position = teleportPosition;

        yield return new WaitForSeconds(0.15f);

        if(spriterenderer != null)
            spriterenderer.color = Color.white;

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

        if(PlayerTransform == null)
            return transform.position;

        for(int i = 0; i < teleportSearchAttempts; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            if(direction.sqrMagnitude < 0.01f)
                direction = (transform.position - PlayerTransform.position).normalized;

            float distance = Random.Range(minTeleportDistanceFromPlayer, maxTeleportDistanceFromPlayer);
            Vector3 candidate = PlayerTransform.position + (Vector3)(direction * distance);

            if(IsTeleportPositionFree(candidate) && !IsSameAsLastTeleport(candidate))
            {
                lastTeleportPoint = null;
                lastTeleportPosition = candidate;
                hasLastTeleportPosition = true;
                return candidate;
            }
        }

        Vector3 fallbackDirection = (transform.position - PlayerTransform.position).normalized;
        if(fallbackDirection.sqrMagnitude < 0.01f)
            fallbackDirection = Vector3.right;

        Vector3 fallback = PlayerTransform.position + fallbackDirection * minTeleportDistanceFromPlayer;
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
        if(PlayerTransform == null)
            return true;

        return Vector2.Distance(position, PlayerTransform.position) >= minTeleportDistanceFromPlayer;
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
        if(IsDying)
            yield break;

        IsDying = true;
        IsAttacking = false;
        IsHurting = false;
        isTeleporting = false;

        StopMovement();
        PlayDeathSound();

        if(animazioni != null)
            animazioni.Morte();

        yield return new WaitForSeconds(0.8f);

        if(this != null)
            Destroy(gameObject);
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
        DrawWireCircle(position, nearAttackArea, summonDamageGizmoColor);
        DrawWireCircle(position, playerTooCloseDistance, teleportGizmoColor);
        DrawWireCircle(position, chaseDistance, summonGizmoColor);

        Transform targetPlayer = PlayerTransform;
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
