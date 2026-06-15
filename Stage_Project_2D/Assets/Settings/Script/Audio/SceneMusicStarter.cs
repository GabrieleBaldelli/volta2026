using UnityEngine;

// Script semplice da mettere in una scena per far partire la musica corretta.
public class SceneMusicStarter : MonoBehaviour
{
    // Musica da avviare quando questa scena viene caricata.
    public AudioClip musicToPlay;

    private void Start()
    {
        // Chiede al manager globale di riprodurre la musica assegnata.
        if (musicToPlay != null)
        {
            // Usa il BackgroundMusicManager, cosi' la musica resta tra le scene.
            BackgroundMusicManager.Instance.PlayMusic(musicToPlay);
        }
    }
}
