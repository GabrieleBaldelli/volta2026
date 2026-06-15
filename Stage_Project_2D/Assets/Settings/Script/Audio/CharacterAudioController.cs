using UnityEngine;

public class CharacterAudioController : MonoBehaviour
{
    private const string CharacterVolumeKey = "CharacterVolume";

    [Header("Audio Sources")]
    public AudioSource loopAudioSource;
    public AudioSource oneShotAudioSource;

    [Header("Movement")]
    public AudioClip runSound;
    [Range(0f, 3f)]
    public float runVolume = 1f;

    [Header("Attack")]
    public AudioClip swordSwingSound;
    public AudioClip attackSound;
    public AudioClip attackEffortSound;
    [Range(0f, 3f)]
    public float swordSwingVolume = 1f;
    [Range(0f, 3f)]
    public float attackVolume = 1f;
    [Range(0f, 3f)]
    public float attackEffortVolume = 1f;

    [Header("Damage")]
    public AudioClip hurtSound;
    public AudioClip deathSound;
    [Range(0f, 3f)]
    public float hurtVolume = 1f;
    [Range(0f, 3f)]
    public float deathVolume = 1f;

    [Header("Shield")]
    public AudioClip shieldSound;
    public AudioClip perfectShieldSound;
    [Range(0f, 3f)]
    public float shieldVolume = 1f;
    [Range(0f, 3f)]
    public float perfectShieldVolume = 1f;

    private float currentLoopVolume = 1f;

    public static float CharacterVolume
    {
        get { return PlayerPrefs.GetFloat(CharacterVolumeKey, 1f); }
    }

    private void Awake()
    {
        if(loopAudioSource == null)
            loopAudioSource = GetComponent<AudioSource>();

        if(loopAudioSource == null)
            loopAudioSource = gameObject.AddComponent<AudioSource>();

        if(oneShotAudioSource == null)
            oneShotAudioSource = gameObject.AddComponent<AudioSource>();

        SetupAudioSource(loopAudioSource);
        SetupAudioSource(oneShotAudioSource);
    }

    private void Update()
    {
        if(loopAudioSource != null && loopAudioSource.isPlaying)
            loopAudioSource.volume = currentLoopVolume * CharacterVolume;
    }

    private void SetupAudioSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    public static void SetCharacterVolume(float volume)
    {
        PlayerPrefs.SetFloat(CharacterVolumeKey, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }

    public void SetVolumeFromSettings(float volume)
    {
        SetCharacterVolume(volume);
    }

    public void PlayRunSound()
    {
        PlayLoop(runSound, runVolume);
    }

    public void StopRunSound()
    {
        StopLoop(runSound);
    }

    public void PlaySwordSwingSound()
    {
        PlayOneShot(swordSwingSound, swordSwingVolume);
    }

    public void PlayAttackSound()
    {
        PlayOneShot(attackSound, attackVolume);
    }

    public void PlayAttackEffortSound()
    {
        PlayOneShot(attackEffortSound, attackEffortVolume);
    }

    public void PlayHurtSound()
    {
        PlayOneShot(hurtSound, hurtVolume);
    }

    public void PlayDeathSound()
    {
        PlayOneShot(deathSound, deathVolume);
    }

    public void PlayShieldSound()
    {
        PlayOneShot(shieldSound, shieldVolume);
    }

    public void PlayPerfectShieldSound()
    {
        PlayOneShot(perfectShieldSound, perfectShieldVolume);
    }

    private void PlayLoop(AudioClip clip, float volume)
    {
        if(loopAudioSource == null || clip == null)
            return;

        currentLoopVolume = volume;

        if(loopAudioSource.clip == clip && loopAudioSource.isPlaying)
            return;

        loopAudioSource.clip = clip;
        loopAudioSource.loop = true;
        loopAudioSource.volume = volume * CharacterVolume;
        loopAudioSource.Play();
    }

    private void StopLoop(AudioClip clip)
    {
        if(loopAudioSource == null)
            return;

        if(loopAudioSource.clip == clip && loopAudioSource.isPlaying)
        {
            loopAudioSource.Stop();
            loopAudioSource.loop = false;
            loopAudioSource.clip = null;
        }
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if(oneShotAudioSource == null || clip == null)
            return;

        oneShotAudioSource.volume = 1f;
        oneShotAudioSource.PlayOneShot(clip, volume * CharacterVolume);
    }
}
