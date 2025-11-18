using UnityEngine;

public static class AudioUtility
{
    private static GameObject sfxRoot;
    private static AudioSource tempSfxSource;

    // Centralized SFX play that is properly routed to the SFX mixer group
    public static void PlaySFX(AudioClip clip, Vector3 position, float baseVolume = 1f)
    {
        if (!clip) return;
        EnsureSfxSource();
        sfxRoot.transform.position = position;
        tempSfxSource.volume = Mathf.Clamp01(baseVolume);
        tempSfxSource.PlayOneShot(clip, 1f);
    }

    private static void EnsureSfxSource()
    {
        if (sfxRoot != null && tempSfxSource != null) return;
        sfxRoot = new GameObject("SFX_AudioSource_Temp");
        Object.DontDestroyOnLoad(sfxRoot);
        tempSfxSource = sfxRoot.AddComponent<AudioSource>();
        tempSfxSource.playOnAwake = false;
        tempSfxSource.spatialBlend = 0f; // UI SFX default to 2D
        // Route to SFX group via SettingsManager helper
        SettingsManager.RouteSFX(tempSfxSource);
    }
}