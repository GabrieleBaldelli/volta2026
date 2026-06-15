using UnityEngine;

// Gestisce tutti gli audio dei personaggi: player, bandit e altri nemici.
public class CharacterAudioController : MonoBehaviour
{
    // Chiave usata da PlayerPrefs per salvare il volume globale dei personaggi.
    private const string CharacterVolumeKey = "CharacterVolume";

    [Header("Audio Sources")]
    // Source usata per suoni continui, come corsa o passi.
    public AudioSource loopAudioSource;

    // Source usata per suoni singoli, come attacchi, danni e parate.
    public AudioSource oneShotAudioSource;

    [Header("Movement")]
    // Clip riprodotto in loop quando il personaggio corre o si muove.
    public AudioClip runSound;

    // Volume specifico del suono di corsa.
    [Range(0f, 3f)]
    public float runVolume = 1f;

    [Header("Attack")]
    // Suono della lama/spada, usato soprattutto dall'HeroKnight.
    public AudioClip swordSwingSound;

    // Suono dell'attacco del nemico o di un colpo generico.
    public AudioClip attackSound;

    // Suono della voce/sforzo quando il personaggio attacca.
    public AudioClip attackEffortSound;

    // Volume specifico della spadata.
    [Range(0f, 3f)]
    public float swordSwingVolume = 1f;

    // Volume specifico del suono d'attacco.
    [Range(0f, 3f)]
    public float attackVolume = 1f;

    // Volume specifico dello sforzo dell'attacco.
    [Range(0f, 3f)]
    public float attackEffortVolume = 1f;

    [Header("Damage")]
    // Suono riprodotto quando il personaggio subisce danno.
    public AudioClip hurtSound;

    // Suono riprodotto quando il personaggio muore.
    public AudioClip deathSound;

    // Volume specifico del danno.
    [Range(0f, 3f)]
    public float hurtVolume = 1f;

    // Volume specifico della morte.
    [Range(0f, 3f)]
    public float deathVolume = 1f;

    [Header("Shield")]
    // Suono della parata normale dell'HeroKnight.
    public AudioClip shieldSound;

    // Suono della parata perfetta dell'HeroKnight.
    public AudioClip perfectShieldSound;

    // Volume specifico della parata normale.
    [Range(0f, 3f)]
    public float shieldVolume = 1f;

    // Volume specifico della parata perfetta.
    [Range(0f, 3f)]
    public float perfectShieldVolume = 1f;

    // Ricorda il volume del loop attuale per aggiornarlo quando cambiano i settings.
    private float currentLoopVolume = 1f;

    // Volume globale condiviso da tutti i personaggi.
    public static float CharacterVolume
    {
        // Legge il volume salvato; se non esiste ancora usa 1.
        get { return PlayerPrefs.GetFloat(CharacterVolumeKey, 1f); }
    }

    private void Awake()
    {
        // Se non sono stati assegnati AudioSource dall'Inspector, li prepara automaticamente.
        if(loopAudioSource == null)
        {
            // Prova prima a usare un AudioSource gia' presente sul GameObject.
            loopAudioSource = GetComponent<AudioSource>();
        }

        if(loopAudioSource == null)
        {
            // Se non esiste, crea l'AudioSource per i loop.
            loopAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if(oneShotAudioSource == null)
        {
            // Crea un AudioSource separato per i suoni brevi.
            oneShotAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configura la source dei suoni continui.
        SetupAudioSource(loopAudioSource);

        // Configura la source dei suoni singoli.
        SetupAudioSource(oneShotAudioSource);
    }

    private void Update()
    {
        // Aggiorna anche i suoni in loop quando il volume viene cambiato dai settings.
        if(loopAudioSource != null && loopAudioSource.isPlaying)
        {
            // Il volume finale e' volume specifico del suono moltiplicato per volume globale.
            loopAudioSource.volume = currentLoopVolume * CharacterVolume;
        }
    }

    private void SetupAudioSource(AudioSource source)
    {
        // Gli audio dei personaggi sono 2D e non devono partire da soli.
        source.playOnAwake = false;

        // 0 significa audio 2D, quindi non cambia volume in base alla distanza.
        source.spatialBlend = 0f;
    }

    public static void SetCharacterVolume(float volume)
    {
        // Salva il volume globale, cosi' resta valido anche cambiando scena.
        PlayerPrefs.SetFloat(CharacterVolumeKey, Mathf.Clamp01(volume));

        // Scrive subito il valore su disco.
        PlayerPrefs.Save();
    }

    public void SetVolumeFromSettings(float volume)
    {
        // Metodo comodo da collegare direttamente a uno Slider di Unity.
        SetCharacterVolume(volume);
    }

    public void PlayRunSound()
    {
        // Avvia il suono di corsa in loop.
        PlayLoop(runSound, runVolume);
    }

    public void StopRunSound()
    {
        // Ferma il suono di corsa se e' quello attualmente in loop.
        StopLoop(runSound);
    }

    public void PlaySwordSwingSound()
    {
        // Riproduce il suono della spadata una volta.
        PlayOneShot(swordSwingSound, swordSwingVolume);
    }

    public void PlayAttackSound()
    {
        // Riproduce il suono d'attacco una volta.
        PlayOneShot(attackSound, attackVolume);
    }

    public void PlayAttackEffortSound()
    {
        // Riproduce il suono dello sforzo dell'attacco una volta.
        PlayOneShot(attackEffortSound, attackEffortVolume);
    }

    public void PlayHurtSound()
    {
        // Riproduce il suono di danno una volta.
        PlayOneShot(hurtSound, hurtVolume);
    }

    public void PlayDeathSound()
    {
        // Riproduce il suono di morte una volta.
        PlayOneShot(deathSound, deathVolume);
    }

    public void PlayShieldSound()
    {
        // Riproduce il suono della parata normale una volta.
        PlayOneShot(shieldSound, shieldVolume);
    }

    public void PlayPerfectShieldSound()
    {
        // Riproduce il suono della parata perfetta una volta.
        PlayOneShot(perfectShieldSound, perfectShieldVolume);
    }

    private void PlayLoop(AudioClip clip, float volume)
    {
        // I loop servono per suoni continui: se manca il clip, non succede nulla.
        if(loopAudioSource == null || clip == null)
            return;

        // Salva il volume del loop per poterlo aggiornare nei settings.
        currentLoopVolume = volume;

        // Se lo stesso loop e' gia' attivo, evita di riavviarlo ogni frame.
        if(loopAudioSource.clip == clip && loopAudioSource.isPlaying)
            return;

        // Assegna il clip continuo alla source dei loop.
        loopAudioSource.clip = clip;

        // Attiva il loop, cosi' il suono si ripete.
        loopAudioSource.loop = true;

        // Calcola il volume finale usando volume del suono e volume globale.
        loopAudioSource.volume = volume * CharacterVolume;

        // Avvia il loop.
        loopAudioSource.Play();
    }

    private void StopLoop(AudioClip clip)
    {
        // Ferma solo il loop richiesto, senza toccare eventuali altri audio.
        if(loopAudioSource == null)
            return;

        if(loopAudioSource.clip == clip && loopAudioSource.isPlaying)
        {
            // Ferma il loop attuale.
            loopAudioSource.Stop();

            // Disattiva il loop per sicurezza.
            loopAudioSource.loop = false;

            // Svuota il clip, cosi' il prossimo suono parte pulito.
            loopAudioSource.clip = null;
        }
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        // I one-shot sono suoni brevi che possono sovrapporsi senza bloccare il gameplay.
        if(oneShotAudioSource == null || clip == null)
            return;

        // Tiene la source a 1: il volume vero viene passato a PlayOneShot.
        oneShotAudioSource.volume = 1f;

        // Riproduce il clip una volta, applicando anche il volume globale.
        oneShotAudioSource.PlayOneShot(clip, volume * CharacterVolume);
    }
}
