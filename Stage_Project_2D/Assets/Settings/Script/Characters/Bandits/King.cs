using System.Collections;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(Seeker))]
public class King : Enemy
{
    [Header("Rage Mode")]
    public Slider rageSlider;
    public float rageChargeDuration = 8f;
    public float rageDrainDuration = 6f;
    public float rageDamageMultiplier = 1.6f;
    public float rageAttackCooldownMultiplier = 0.55f;
    public float rageAttackDurationMultiplier = 0.65f;
    public float rageAttackHitDelayMultiplier = 0.65f;
    public Color rageColor = new Color(1f, 0.65f, 0.65f, 1f);

    [Header("Room UI")]
    public GameObject[] roomCanvases;
    public bool activateWhenPlayerIsNear = true;

    [Header("Defeated NPC")]
    [SerializeField] private NPC defeatedNpc;
    [SerializeField] private bool disableNpcUntilDefeated = true;
    [SerializeField] private string defeatedNpcLayerName = "Interactable";
    [SerializeField] private bool freezeRigidbodyWhenDefeated = true;

    private float rageAmount;
    private bool isRageMode;
    private bool rageCanCharge;
    private bool isRoomUnlocked;
    private float baseDamage;
    private float baseAttackCooldown;
    private float baseAttackDuration;
    private float baseAttackHitDelay;
    private Color baseColor = Color.white;
    private bool hasStoredBaseStats;
    private bool hasConvertedToNpc;

    public override bool ShouldRoomLockerControl
    {
        get { return !hasConvertedToNpc; }
    }

    protected override void OnEnable()
    {
        // Il King viene attivato davvero solo dal trigger della stanza.
    }

    protected override void Start()
    {
        base.Start();

        if(defeatedNpc == null)
            defeatedNpc = GetComponent<NPC>();

        if(defeatedNpc != null && disableNpcUntilDefeated)
            defeatedNpc.enabled = false;

        CacheRoomCanvases();

        baseDamage = danno;
        baseAttackCooldown = attackCooldown;
        baseAttackDuration = attackDuration;
        baseAttackHitDelay = attackHitDelay;
        hasStoredBaseStats = true;

        if(spriterenderer != null)
            baseColor = spriterenderer.color;

        if(rageSlider == null)
        {
            Transform rageBarTransform = transform.Find("Canvas/Slider");
            if(rageBarTransform != null)
                rageSlider = rageBarTransform.GetComponent<Slider>();
        }

        SetRageAmount(0f);
        SetRageMode(false);
        SetRoomCanvasesActive(isRoomUnlocked);

        if(animazioni != null)
        {
            animazioni.Configura(
                "King_Idle",
                "King_Run",
                "King_Attack1",
                "King_Attack2",
                "King_Hurt",
                "King_Death"
            );
        }
    }

    protected override void Update()
    {
        if(hasConvertedToNpc)
        {
            KeepDefeatedNpcIdle();
            return;
        }

        if(!isRoomUnlocked)
        {
            TryActivateFromPlayerDistance();

            if(!isRoomUnlocked)
            {
                StopRunSound();

                if(aiPath != null)
                    aiPath.canMove = false;

                if(animazioni != null)
                    animazioni.Idle();

                return;
            }
        }

        if(rageCanCharge)
            UpdateRageMode();

        if(IsDying)
            return;

        if(p == null)
            TryFindPlayer();

        if(p == null || aiPath == null || animazioni == null || IsDying)
        {
            StopRunSound();
            return;
        }

        if(IsAttacking)
        {
            StopRunSound();
            aiPath.canMove = false;
            return;
        }

        if(IsHurting)
        {
            StopRunSound();
            aiPath.canMove = false;
            return;
        }

        float distance = Vector2.Distance(transform.position, p.position);

        FacePlayer();

        if(distance > chaseDistance)
        {
            StopRunSound();
            aiPath.canMove = false;
            animazioni.Idle();
            return;
        }

        if(distance <= stopDistance)
        {
            StopRunSound();
            aiPath.canMove = false;

            if(Time.time >= nextAttackTime)
                StartCoroutine(KingAttackCoroutine());
            else
                animazioni.Idle();

            return;
        }

        animazioni.Corsa();
        PlayRunSound();

        aiPath.canMove = true;
        aiPath.maxSpeed = 3f;
    }

    protected override void OnDisable()
    {
        if(hasConvertedToNpc)
        {
            StopDefeatedNpcMovement();
            SetRoomCanvasesActive(false);
            StopRunSound();
            return;
        }

        rageCanCharge = false;
        isRoomUnlocked = false;
        SetRageAmount(0f);

        if(hasStoredBaseStats)
            SetRageMode(false);

        SetRoomCanvasesActive(false);

        base.OnDisable();
    }

    public override void PrepareForRoomLock()
    {
        if(hasConvertedToNpc)
        {
            KeepDefeatedNpcIdle();
            return;
        }

        rageCanCharge = false;
        isRoomUnlocked = false;
        SetRageAmount(0f);

        if(hasStoredBaseStats)
            SetRageMode(false);

        SetRoomCanvasesActive(false);
        base.PrepareForRoomLock();
    }

    public override void PrepareForRoomUnlock()
    {
        if(hasConvertedToNpc)
        {
            KeepDefeatedNpcIdle();
            return;
        }

        base.PrepareForRoomUnlock();

        ActivateKingRoom();
    }

    private void TryActivateFromPlayerDistance()
    {
        if(!activateWhenPlayerIsNear)
            return;

        if(p == null)
            TryFindPlayer();

        if(p == null)
            return;

        if(Vector2.Distance(transform.position, p.position) <= chaseDistance)
            ActivateKingRoom();
    }

    private void ActivateKingRoom()
    {
        isRoomUnlocked = true;
        rageCanCharge = true;
        SetRageAmount(0f);
        SetRageMode(false);
        SetRoomCanvasesActive(true);
    }

    private void UpdateRageMode()
    {
        if(isRageMode)
        {
            float drainSpeed = rageDrainDuration <= 0f ? 1f : 1f / rageDrainDuration;
            SetRageAmount(rageAmount - drainSpeed * Time.deltaTime);

            if(rageAmount <= 0f)
                SetRageMode(false);

            return;
        }

        float chargeSpeed = rageChargeDuration <= 0f ? 1f : 1f / rageChargeDuration;
        SetRageAmount(rageAmount + chargeSpeed * Time.deltaTime);

        if(rageAmount >= 1f)
            SetRageMode(true);
    }

    private void SetRageAmount(float value)
    {
        rageAmount = Mathf.Clamp01(value);

        if(rageSlider != null)
            rageSlider.value = rageAmount;
    }

    private void SetRageMode(bool active)
    {
        if(!hasStoredBaseStats)
            return;

        isRageMode = active;

        danno = active ? baseDamage * rageDamageMultiplier : baseDamage;
        attackCooldown = active ? baseAttackCooldown * rageAttackCooldownMultiplier : baseAttackCooldown;
        attackDuration = active ? baseAttackDuration * rageAttackDurationMultiplier : baseAttackDuration;
        attackHitDelay = active ? baseAttackHitDelay * rageAttackHitDelayMultiplier : baseAttackHitDelay;

        if(spriterenderer != null)
            spriterenderer.color = active ? rageColor : baseColor;
    }

    private void CacheRoomCanvases()
    {
        if(roomCanvases != null && roomCanvases.Length > 0)
            return;

        Transform canvasTransform = transform.Find("Canvas");
        if(canvasTransform != null)
            roomCanvases = new GameObject[] { canvasTransform.gameObject };
    }

    private void SetRoomCanvasesActive(bool active)
    {
        CacheRoomCanvases();

        if(roomCanvases == null)
            return;

        foreach(GameObject canvasObject in roomCanvases)
        {
            if(canvasObject != null)
                canvasObject.SetActive(active);
        }
    }

    public override IEnumerator HurtCoroutine(float danno)
    {
        if(IsDying || hasConvertedToNpc)
            yield break;

        IsHurting = true;

        if(playerScript == null)
            TryFindPlayer();

        Transform lifeBarTransform = transform.Find("Canvas/Life_Bar");
        LifeBar lifebarScript = null;

        if(lifeBarTransform != null)
            lifebarScript = lifeBarTransform.GetComponent<LifeBar>();

        bool fatalHit = vita - danno <= 1f;

        if(animazioni != null && !fatalHit)
            animazioni.Danno();

        if(!fatalHit)
            PlayHurtSound();

        vita -= danno;

        if(lifebarScript != null)
            lifebarScript.UpdateLifeBar(vita, vitaMassima);

        if(vita <= 1)
        {
            IsDying = true;
            yield return StartCoroutine(ConvertToNpcAfterDefeat());
            yield break;
        }

        yield return new WaitForSeconds(0.35f);

        if(IsDying)
            yield break;

        IsHurting = false;
    }

    private IEnumerator ConvertToNpcAfterDefeat()
    {
        if(hasConvertedToNpc)
            yield break;

        hasConvertedToNpc = true;
        IsDying = true;
        IsAttacking = false;
        IsHurting = false;
        rageCanCharge = false;
        isRoomUnlocked = false;
        SetRageAmount(0f);

        if(hasStoredBaseStats)
            SetRageMode(false);

        if(playerScript == null)
            TryFindPlayer();

        if(playerScript != null)
        {
            GiveXPOnce();

            PassiveSpellManager passiveSpellManager = playerScript.GetComponent<PassiveSpellManager>();
            int coinReward = passiveSpellManager != null ? passiveSpellManager.GetCoinRewardWithPassives(coin) : coin;
            playerScript.CoinSetGet += coinReward;

            if(passiveSpellManager != null)
                passiveSpellManager.NotifyEnemyKilled();
        }

        StopRunSound();
        StopDefeatedNpcMovement();

        if(animazioni != null)
        {
            animazioni.ResetCurrentAnimation();
            animazioni.Morte();
        }

        yield return new WaitForSeconds(0.35f);

        if(this == null)
            yield break;

        SetRoomCanvasesActive(false);
        EnableDefeatedNpc();
        if(animazioni != null)
            animazioni.ResetCurrentAnimation();

        KeepDefeatedNpcIdle();
        enabled = false;
    }

    private void EnableDefeatedNpc()
    {
        if(defeatedNpc == null)
            defeatedNpc = GetComponent<NPC>();

        if(defeatedNpc != null)
            defeatedNpc.enabled = true;

        if(string.IsNullOrWhiteSpace(defeatedNpcLayerName))
            return;

        int layer = LayerMask.NameToLayer(defeatedNpcLayerName);

        if(layer >= 0)
        {
            gameObject.layer = layer;

            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
            foreach(Collider2D currentCollider in colliders)
            {
                if(currentCollider != null)
                    currentCollider.gameObject.layer = layer;
            }
        }
    }

    private void KeepDefeatedNpcIdle()
    {
        StopDefeatedNpcMovement();

        if(animazioni == null)
            return;

        animazioni.Idle();
    }

    private void StopDefeatedNpcMovement()
    {
        StopMovement();

        AIDestinationSetter destinationSetter = GetComponent<AIDestinationSetter>();
        if(destinationSetter != null)
            destinationSetter.enabled = false;

        if(aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.enabled = false;
        }

        Seeker seeker = GetComponent<Seeker>();
        if(seeker != null)
            seeker.enabled = false;

        if(rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            if(freezeRigidbodyWhenDefeated)
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private IEnumerator KingAttackCoroutine()
    {
        if(player == null || p == null || IsDying)
            yield break;

        playerScript = player.GetComponent<PlayerMovement>();

        IsAttacking = true;
        nextAttackTime = Time.time + attackDuration + attackCooldown;

        aiPath.canMove = false;

        animazioni.Attacco1();
        PlayAttackSound();
        PlayAttackEffortSound();

        yield return new WaitForSeconds(attackHitDelay);

        if(IsDying)
        {
            IsAttacking = false;
            yield break;
        }

        DamagePlayerIfNear(stopDistance + 0.4f);

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - attackHitDelay));

        IsAttacking = false;
    }
}
