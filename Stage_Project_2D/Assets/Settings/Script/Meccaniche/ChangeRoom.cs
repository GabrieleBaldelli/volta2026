using UnityEngine;

public class ChangeRoom : MonoBehaviour
{
    [Header("Camere")]
    public GameObject cameraDaSpegnere;
    public GameObject cameraDaAccendere;

    public GameObject player;

    public GameObject spawnPoint;

    public AudioClip musicToPlay;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (musicToPlay != null)
                BackgroundMusicManager.Instance.PlayMusic(musicToPlay);

            SetAudioListener(cameraDaSpegnere, false);
            SetAudioListener(cameraDaAccendere, true);

            cameraDaSpegnere.SetActive(false);
            cameraDaAccendere.SetActive(true);

            player.transform.position = spawnPoint.transform.position;
        }
    }

    private void SetAudioListener(GameObject cameraObject, bool active)
    {
        if (cameraObject == null)
            return;

        AudioListener audioListener = cameraObject.GetComponent<AudioListener>();

        if (audioListener != null)
            audioListener.enabled = active;
    }
}
