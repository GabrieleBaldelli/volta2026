using UnityEngine;

[DisallowMultipleComponent]
public class Animazioni : MonoBehaviour
{
    [Header("Animation State Names")]
    public string idleAnimation;
    public string ShadowAnimation;
    public string runAnimation;
    public string attackAnimation1;
    public string attackAnimation2;
    public string hurtAnimation;
    public string deathAnimation;
    

    private Animator animator;
    private string currentAnimation;

    void Awake()
    {
        EnsureDefaultAttackAnimation();

        animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogError("Animator mancante sul nemico o nei suoi figli", this);
    }

    private void OnValidate()
    {
        EnsureDefaultAttackAnimation();
    }

    public void Idle()
    {
        PlayAnimation(idleAnimation);
    }

    public void Shadow()
    {
        PlayAnimation(ShadowAnimation);
    }

    public void Corsa()
    {
        PlayAnimation(runAnimation);
    }

    public void Attacco1()
    {
        PlayAnimation(attackAnimation1);
    }

    public void Attacco2()
    {
        PlayAnimation(attackAnimation2);
    }

    public void Danno()
    {
        PlayAnimation(hurtAnimation);
    }

    public void Morte()
    {
        PlayAnimation(deathAnimation);
    }

    public void Configura(string idle, string run, string attack1, string attack2, string hurt, string death = "")
    {
        idleAnimation = idle;
        runAnimation = run;
        attackAnimation1 = attack1;
        attackAnimation2 = attack2;
        hurtAnimation = hurt;
        deathAnimation = death;
    }

    public void PlayAnimation(string animationName)
    {
        if (animator == null || string.IsNullOrEmpty(animationName))
            return;

        if (currentAnimation == animationName)
            return;

        currentAnimation = animationName;
        animator.Play(animationName);
    }

    public void ResetCurrentAnimation()
    {
        currentAnimation = "";
    }

    private void EnsureDefaultAttackAnimation()
    {
        if (!string.IsNullOrEmpty(attackAnimation1))
            return;

        if (idleAnimation == "Nemico1_Idle")
            attackAnimation1 = "Nemico1_Attacco";
        else if (idleAnimation == "Nemico2_Idle")
            attackAnimation1 = "Nemico2_Attacco";
    }
}
