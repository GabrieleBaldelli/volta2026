using UnityEngine;

public class SceneMusicStarter : MonoBehaviour
{
    public AudioClip musicToPlay;

    private void Start()
    {
        if (musicToPlay != null)
            BackgroundMusicManager.Instance.PlayMusic(musicToPlay);
    }
}
