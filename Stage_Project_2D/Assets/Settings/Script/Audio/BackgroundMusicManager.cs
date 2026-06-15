using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicManager : MonoBehaviour
{
    [Range(0f, 1f)]
    public float musicVolume = 0.1f;

    private static BackgroundMusicManager instance;
    private AudioSource audioSource;

    public static BackgroundMusicManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject musicManager = new GameObject("Background Music Manager");
                instance = musicManager.AddComponent<BackgroundMusicManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = musicVolume;
    }

    public void PlayMusic(AudioClip music)
    {
        if (music == null)
            return;

        if (audioSource.clip == music && audioSource.isPlaying)
            return;

        StopOtherThemeMusic(music);

        audioSource.clip = music;
        audioSource.volume = musicVolume;
        audioSource.Play();
    }

    private void StopOtherThemeMusic(AudioClip music)
    {
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();

        foreach (AudioSource source in audioSources)
        {
            if (source == audioSource || source.clip == null)
                continue;

            if (source.isPlaying && source.loop && source.clip != music)
                source.Stop();
        }
    }
}
