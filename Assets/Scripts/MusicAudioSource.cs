using UnityEngine;
using System.Collections;

/// <summary>
/// Helper component to automatically register an AudioSource as music with SettingsManager
/// Attach this to any AudioSource that plays background music
/// </summary>
public class MusicAudioSource : MonoBehaviour
{
    private AudioSource audioSource;
    private bool isRegistered = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning($"[MusicAudioSource] No AudioSource found on {gameObject.name}. This component requires an AudioSource.");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        // Try to register immediately
        RegisterAudioSource();
        
        // Also try after a short delay in case SettingsManager isn't ready yet
        StartCoroutine(DelayedRegistration());
    }

    private IEnumerator DelayedRegistration()
    {
        // Wait a frame to ensure SettingsManager is initialized
        yield return null;
        
        if (!isRegistered)
        {
            RegisterAudioSource();
        }
        
        // Try one more time after a short delay
        yield return new WaitForSeconds(0.1f);
        
        if (!isRegistered)
        {
            RegisterAudioSource();
        }
    }

    private void OnEnable()
    {
        // Re-register when enabled (in case SettingsManager wasn't ready before)
        if (!isRegistered && audioSource != null)
        {
            RegisterAudioSource();
        }
    }

    private void RegisterAudioSource()
    {
        if (audioSource != null && SettingsManager.Instance != null && !isRegistered)
        {
            SettingsManager.RegisterMusicSource(audioSource);
            isRegistered = true;
        }
    }

    private void OnDestroy()
    {
        if (audioSource != null && SettingsManager.Instance != null && isRegistered)
        {
            SettingsManager.UnregisterMusicSource(audioSource);
            isRegistered = false;
        }
    }
}

