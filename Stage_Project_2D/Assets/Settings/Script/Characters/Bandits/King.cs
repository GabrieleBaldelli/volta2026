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

    protected override void OnEnable()
    {
        // Il King viene attivato davvero solo dal trigger della stanza.
    }

    protected override void Start()
    {
        base.Start();

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
