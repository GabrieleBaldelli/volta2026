using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// Richiede automaticamente un AudioSource sull'oggetto che usa questo script.
[RequireComponent(typeof(AudioSource))]

public class BackgroundMusicManager : MonoBehaviour
{
    // Volume generale della musica di gioco, regolabile dall'Inspector.
    [Range(0f, 1f)]
    public float musicVolume = 0.4f;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    // Riferimento statico all'unico BackgroundMusicManager attivo.
    private static BackgroundMusicManager instance;

    // AudioSource usato per riprodurre la musica di sottofondo.
    private AudioSource audioSource;

    // Crea o recupera l'unico manager della musica di sottofondo.
    public static BackgroundMusicManager Instance
    {
        get
        {
            // Se non esiste ancora un manager, crea un nuovo GameObject.
            if (instance == null)
            {
                // Crea l'oggetto che conterra' il manager della musica.
                GameObject musicManager = new GameObject("Background Music Manager");

                // Aggiunge questo script al nuovo oggetto appena creato.
                instance = musicManager.AddComponent<BackgroundMusicManager>();
            }

            // Restituisce il manager esistente o appena creato.
            return instance;
        }
    }

    private void Awake()
    {
        // Evita di avere piu' music manager contemporaneamente tra una scena e l'altra.
        if (instance != null && instance != this)
        {
            // Se c'e' gia' un altro manager, questo oggetto viene eliminato.
            Destroy(gameObject);
            return;
        }

        // Salva questo oggetto come manager principale.
        instance = this;

        // DontDestroyOnLoad funziona solo su oggetti root della scena.
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        // Mantiene la musica anche quando si cambia scena.
        DontDestroyOnLoad(gameObject);

        // L'AudioSource collegato a questo oggetto riproduce la musica in loop.
        audioSource = GetComponent<AudioSource>();

        // La musica di sottofondo deve ripetersi finche' non viene cambiata.
        audioSource.loop = true;

        // La musica non parte da sola: viene avviata da PlayMusic.
        audioSource.playOnAwake = false;

        // Imposta il volume iniziale scelto dall'Inspector.
        audioSource.volume = musicVolume;

        // Applica i volumi salvati anche se si avvia direttamente una scena di gioco.
        ApplySavedMixerVolumes();
    }

    private void OnEnable()
    {
        // Si iscrive all'evento di Unity che avvisa quando una scena viene caricata.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Si disiscrive dall'evento per evitare chiamate a oggetti distrutti.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Quando si torna al menu principale, la musica di gioco deve fermarsi.
        if (scene.name == "Main Menu")
        {
            // Distruggendo il manager si ferma anche la sua musica.
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip music)
    {
        // Se non c'e' nessun clip, non prova a cambiare musica.
        if (music == null)
            return;

        // Se la musica richiesta e' gia' in riproduzione, non la fa ripartire da capo.
        if (audioSource.clip == music && audioSource.isPlaying)
            return;

        // Ferma altre musiche in loop che potrebbero essere rimaste nella scena.
        StopOtherThemeMusic(music);

        // Assegna il nuovo clip musicale all'AudioSource.
        audioSource.clip = music;

        // Applica il volume scelto per la musica di gioco.
        audioSource.volume = musicVolume;

        // Avvia la musica.
        audioSource.Play();
    }

    private void StopOtherThemeMusic(AudioClip music)
    {
        // Ferma eventuali altre musiche in loop rimaste nella scena.
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();

        // Controlla tutti gli AudioSource presenti nella scena.
        foreach (AudioSource source in audioSources)
        {
            // Salta il suo AudioSource e salta gli AudioSource senza clip.
            if (source == audioSource || source.clip == null)
                continue;

            // Se un altro AudioSource sta riproducendo in loop una musica diversa, la ferma.
            if (source.isPlaying && source.loop && source.clip != music)
                source.Stop();
        }
    }

    private void ApplySavedMixerVolumes()
    {
        AudioSettingsStore.ApplySavedVolumes(audioMixer);
    }
}
