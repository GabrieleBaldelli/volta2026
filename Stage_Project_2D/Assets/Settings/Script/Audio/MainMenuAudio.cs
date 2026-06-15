using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuAudio : MonoBehaviour
{
    public AudioClip buttonClickSound;

    [Range(0f, 1f)]
    public float buttonClickVolume = 1f;

    private static MainMenuAudio instance;
    private AudioSource buttonAudioSource;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            if (SceneManager.GetActiveScene().name == "Main Menu")
            {
                Destroy(instance.gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        buttonAudioSource = gameObject.AddComponent<AudioSource>();
        buttonAudioSource.playOnAwake = false;
        buttonAudioSource.loop = false;
        buttonAudioSource.spatialBlend = 0f;
        buttonAudioSource.volume = buttonClickVolume;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Room 1")
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void PlayButtonClick()
    {
        if (buttonAudioSource != null && buttonClickSound != null)
        {
            buttonAudioSource.volume = buttonClickVolume;
            buttonAudioSource.PlayOneShot(buttonClickSound);
        }
    }
}
