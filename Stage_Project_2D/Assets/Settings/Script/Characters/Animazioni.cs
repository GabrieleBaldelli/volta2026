using UnityEngine;

[DisallowMultipleComponent]
public class Animazioni : MonoBehaviour
{
    [Header("Animation State Names")]
    public string idleAnimation;
    public string runAnimation;
    public string attackAnimation;
    public string hurtAnimation;
    public string deathAnimation;

    private Animator animator;
    private string currentAnimation;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogError("Animator mancante sul nemico o nei suoi figli", this);
    }

    public void Idle()
    {
        PlayAnimation(idleAnimation);
    }

    public void Corsa()
    {
        PlayAnimation(runAnimation);
    }

    public void Attacco()
    {
        PlayAnimation(attackAnimation);
    }

    public void Danno()
    {
        PlayAnimation(hurtAnimation);
    }

    public void Morte()
    {
        PlayAnimation(deathAnimation);
    }

    public void Configura(string idle, string run, string attack, string hurt, string death = "")
    {
        idleAnimation = idle;
        runAnimation = run;
        attackAnimation = attack;
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
}
