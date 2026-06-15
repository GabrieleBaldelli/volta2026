using UnityEngine;
using UnityEngine.SceneManagement;

// Gestisce gli audio del menu principale, soprattutto il click dei pulsanti.
public class MainMenuAudio : MonoBehaviour
{
    // Suono usato dai pulsanti del menu principale.
    public AudioClip buttonClickSound;

    // Volume del click dei pulsanti.
    [Range(0f, 1f)]
    public float buttonClickVolume = 1f;

    // Riferimento statico all'unico audio manager del menu.
    private static MainMenuAudio instance;

    // AudioSource usato solo per i click dei pulsanti.
    private AudioSource buttonAudioSource;

    private void Awake()
    {
        // Mantiene un solo gestore audio del menu tra una scena e l'altra.
        if (instance != null && instance != this)
        {
            // Se si rientra nel menu principale, preferisce il nuovo oggetto della scena.
            if (SceneManager.GetActiveScene().name == "Main Menu")
            {
                // Elimina la vecchia istanza rimasta da prima.
                Destroy(instance.gameObject);
            }
            else
            {
                // Se non siamo nel menu, elimina questo duplicato.
                Destroy(gameObject);
                return;
            }
        }

        // Salva questo oggetto come istanza principale.
        instance = this;

        // Mantiene l'oggetto anche quando si cambia scena.
        DontDestroyOnLoad(gameObject);

        // Source separata per i click, cosi' il suono puo' finire anche durante cambi scena.
        buttonAudioSource = gameObject.AddComponent<AudioSource>();

        // Il click non deve partire da solo.
        buttonAudioSource.playOnAwake = false;

        // Il click e' un suono breve, quindi non deve andare in loop.
        buttonAudioSource.loop = false;

        // Il click e' 2D, quindi non dipende dalla posizione nello spazio.
        buttonAudioSource.spatialBlend = 0f;

        // Imposta il volume iniziale del click.
        buttonAudioSource.volume = buttonClickVolume;
    }

    private void OnEnable()
    {
        // Si iscrive all'evento di caricamento scena.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Si disiscrive dall'evento quando l'oggetto viene disattivato.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Quando parte il gioco, il gestore del menu non serve piu'.
        if (scene.name == "Room 1")
        {
            // Elimina il manager del menu quando si entra nella partita.
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Libera il riferimento statico quando questo oggetto viene distrutto.
        if (instance == this)
        {
            instance = null;
        }
    }

    public void PlayButtonClick()
    {
        // Riproduce un click senza interrompere eventuali altri suoni.
        if (buttonAudioSource != null && buttonClickSound != null)
        {
            // Aggiorna il volume nel caso sia stato cambiato dall'Inspector.
            buttonAudioSource.volume = buttonClickVolume;

            // Riproduce il click una volta.
            buttonAudioSource.PlayOneShot(buttonClickSound);
        }
    }
}
