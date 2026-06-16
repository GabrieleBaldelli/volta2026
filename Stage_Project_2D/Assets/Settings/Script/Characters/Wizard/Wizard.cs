using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CharacterAudioController))]
public class Wizard : Enemy
{
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
    public Transform enemyAstarPathParent;
    public string enemyAstarPathObjectName = "Enemy_AstarPath";

    [Header("Teleport")]
    public Transform[] teleportPoints;
    public float playerTooCloseDistance = 3f;
    public float teleportCooldown = 4f;
    public float damageBeforeTeleport = 35f;
    public float minTeleportDistanceFromPlayer = 6f;
    public float maxTeleportDistanceFromPlayer = 10f;
    public int teleportSearchAttempts = 16;
    public LayerMask teleportBlockedLayers;

    [Header("Wizard Shield")]
    public Slider shieldSlider;
    public float shieldMassimo = 60f;
    public float shieldRegenDelay = 5f;
    public float shieldBrokenRegenDelay = 8f;
    public float shieldRegenPerSecond = 12f;
    public bool regenerateShieldOnlyWhenFar = true;
    public float shieldRegenMinPlayerDistance = 5f;

    private Transform playerTransform;
    private PlayerMovement playerScript;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animazioni animazioni;
    private CharacterAudioController characterAudio;
    private LifeBar lifeBar;

    private readonly List<GameObject> summonedEnemies = new List<GameObject>();

    private float vita;
    private float shield;
    private float lastDamageTime;
    private float damageTakenSinceTeleport;
    private float nextTeleportTime;

    private bool isAttacking;
    private bool isHurting;
    private bool isDying;
    private bool isTeleporting;
    private bool shieldBroken;
    private bool missingEnemyParentWarningShown;

    public override bool IsAttackingSetGet
    {
        get { return isAttacking; }
        set { isAttacking = value; }
    }

    protected override void Start()
    {
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

        UpdateLifeBar();
        UpdateShieldBar();
    }

    protected override void Update()
    {
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

        if(Time.time >= nextAttackTime && distance <= stopDistance)
        {
            StartCoroutine(MeleeAttackCoroutine());
            return;
        }

        if(distance <= playerTooCloseDistance && Time.time >= nextTeleportTime)
        {
            StartCoroutine(TeleportCoroutine());
            return;
        }

        if(Time.time < nextAttackTime)
        {
            animazioni.Idle();
            return;
        }

        if(distance <= chaseDistance)
        {
            StartCoroutine(SummonAttackCoroutine());
            return;
        }

        animazioni.Idle();
    }

    private void FindPlayer()
    {
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
        if(spriteRenderer != null && playerTransform != null)
            spriteRenderer.flipX = playerTransform.position.x > transform.position.x;
    }

    private IEnumerator SummonAttackCoroutine()
    {
        if(isDying || playerTransform == null)
            yield break;

        isAttacking = true;
        nextAttackTime = Time.time + attackDuration + summonCooldown;

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
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        if(isDying || playerTransform == null)
            yield break;

        isAttacking = true;
        nextAttackTime = Time.time + attackDuration + attackCooldown;

        StopMovement();
        animazioni.Attacco2();
        characterAudio.PlayAttackSound();
        characterAudio.PlayAttackEffortSound();

        yield return new WaitForSeconds(attackHitDelay);

        if(!isDying)
            DamagePlayerIfNear(stopDistance + 0.4f);

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - attackHitDelay));

        isAttacking = false;
    }

    private void DamagePlayerIfNear(float radius)
    {
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
                enemyScript.player = player;
        }
    }

    private Transform FindEnemyAstarPathParent()
    {
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
        if(summonPoints != null && summonPoints.Length > 0)
        {
            Transform point = summonPoints[index % summonPoints.Length];
            if(point != null)
                return point.position;
        }

        Vector2 offset = Random.insideUnitCircle;
        if(offset.sqrMagnitude < 0.01f)
            offset = Vector2.right;

        return transform.position + (Vector3)(offset.normalized * summonRadius);
    }

    private void RemoveMissingSummonedEnemies()
    {
        for(int i = summonedEnemies.Count - 1; i >= 0; i--)
        {
            if(summonedEnemies[i] == null)
                summonedEnemies.RemoveAt(i);
        }
    }

    public override IEnumerator HurtCoroutine(float danno)
    {
        if(isDying)
            yield break;

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

        if(animazioni != null)
            animazioni.Danno();

        characterAudio.PlayHurtSound();

        if(damageTakenSinceTeleport >= damageBeforeTeleport && Time.time >= nextTeleportTime)
        {
            StartCoroutine(TeleportCoroutine());
            yield break;
        }

        isHurting = true;
        StopMovement();

        yield return new WaitForSeconds(0.25f);

        isHurting = false;
    }

    private void RegenerateShield()
    {
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
        animazioni.Idle();
    }

    private Vector3 FindTeleportPosition()
    {
        if(teleportPoints != null && teleportPoints.Length > 0)
        {
            Transform bestPoint = null;
            float bestDistance = float.MinValue;

            foreach(Transform point in teleportPoints)
            {
                if(point == null || !IsTeleportPositionFree(point.position))
                    continue;

                float distance = playerTransform == null ? 0f : Vector2.Distance(point.position, playerTransform.position);
                if(distance > bestDistance)
                {
                    bestDistance = distance;
                    bestPoint = point;
                }
            }

            if(bestPoint != null)
                return bestPoint.position;
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

            if(IsTeleportPositionFree(candidate))
                return candidate;
        }

        Vector3 fallbackDirection = (transform.position - playerTransform.position).normalized;
        if(fallbackDirection.sqrMagnitude < 0.01f)
            fallbackDirection = Vector3.right;

        return playerTransform.position + fallbackDirection * minTeleportDistanceFromPlayer;
    }

    private bool IsTeleportPositionFree(Vector3 position)
    {
        if(teleportBlockedLayers.value == 0)
            return true;

        return Physics2D.OverlapCircle(position, 0.4f, teleportBlockedLayers) == null;
    }

    private IEnumerator DieCoroutine()
    {
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
        if(rb != null)
            rb.velocity = Vector2.zero;

        if(aiPath != null)
            aiPath.canMove = false;
    }

    private void UpdateLifeBar()
    {
        if(lifeBar != null)
            lifeBar.UpdateLifeBar(vita, vitaMassima);

        if(HealthImage != null)
            HealthImage.fillAmount = vita / vitaMassima;
    }

    private void UpdateShieldBar()
    {
        if(shieldSlider != null)
            shieldSlider.value = shieldMassimo <= 0f ? 0f : shield / shieldMassimo;
    }
}
