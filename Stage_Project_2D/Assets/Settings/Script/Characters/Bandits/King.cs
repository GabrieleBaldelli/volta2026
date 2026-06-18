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

    private float rageAmount;
    private bool isRageMode;
    private float baseDamage;
    private float baseAttackCooldown;
    private float baseAttackDuration;
    private float baseAttackHitDelay;
    private Color baseColor = Color.white;
    private bool hasStoredBaseStats;

    protected override void Start()
    {
        base.Start();

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
        if(hasStoredBaseStats)
            SetRageMode(false);

        base.OnDisable();
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
