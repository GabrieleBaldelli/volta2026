using UnityEngine;
using UnityEngine.Audio;

public static class AudioSettingsStore
{
    public const string MasterVolumeKey = "MasterVol";
    public const string MusicVolumeKey = "MusicVol";
    public const string SFXVolumeKey = "SFXVol";

    private const string SettingsVersionKey = "AudioSettingsVersion";
    private const int CurrentSettingsVersion = 5;
    private const float DefaultVolume = 0.5f;

    public static void EnsureInitialized()
    {
        if (PlayerPrefs.GetInt(SettingsVersionKey, 0) == CurrentSettingsVersion)
            return;

        PlayerPrefs.SetFloat(MasterVolumeKey, DefaultVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, DefaultVolume);
        PlayerPrefs.SetFloat(SFXVolumeKey, DefaultVolume);
        PlayerPrefs.SetInt(SettingsVersionKey, CurrentSettingsVersion);
        PlayerPrefs.Save();
    }

    public static float GetVolume(string parameterName)
    {
        EnsureInitialized();
        return Mathf.Clamp01(PlayerPrefs.GetFloat(parameterName, DefaultVolume));
    }

    public static void SetVolume(AudioMixer audioMixer, string parameterName, float volume)
    {
        EnsureInitialized();

        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(parameterName, volume);
        PlayerPrefs.SetInt(SettingsVersionKey, CurrentSettingsVersion);
        PlayerPrefs.Save();

        ApplyVolume(audioMixer, parameterName, volume);
    }

    public static void ApplySavedVolumes(AudioMixer audioMixer)
    {
        EnsureInitialized();

        ApplyVolume(audioMixer, MasterVolumeKey, GetVolume(MasterVolumeKey));
        ApplyVolume(audioMixer, MusicVolumeKey, GetVolume(MusicVolumeKey));
        ApplyVolume(audioMixer, SFXVolumeKey, GetVolume(SFXVolumeKey));
    }

    public static void ResetVolumes(AudioMixer audioMixer)
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, DefaultVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, DefaultVolume);
        PlayerPrefs.SetFloat(SFXVolumeKey, DefaultVolume);
        PlayerPrefs.SetInt(SettingsVersionKey, CurrentSettingsVersion);
        PlayerPrefs.Save();

        ApplySavedVolumes(audioMixer);
    }

    public static void ApplyVolume(AudioMixer audioMixer, string parameterName, float volume)
    {
        if (audioMixer == null || string.IsNullOrEmpty(parameterName))
            return;

        volume = Mathf.Clamp01(volume);
        float decibelVolume = volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20f;

        bool parameterFound = audioMixer.SetFloat(parameterName, decibelVolume);

        if (!parameterFound)
        {
            Debug.LogWarning("Audio mixer parameter not found: " + parameterName);
        }
    }
}
