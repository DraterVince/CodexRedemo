using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component to add button click sound effects that respect volume settings
/// Attach this to any Button GameObject to add click sounds
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Sound effect to play when button is clicked")]
    public AudioClip clickSound;
    
    [Tooltip("AudioSource to play sound (auto-created if not assigned)")]
    public AudioSource audioSource;
    
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        
        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D sound
            }
       }
       
       // Route to SFX mixer group if available
       SettingsManager.RouteSFX(audioSource);
       
       // Register audio source with SettingsManager for volume control
        if (audioSource != null && SettingsManager.Instance != null)
        {
            SettingsManager.RegisterSFXSource(audioSource);
        }
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }

    /// <summary>
    /// Play the click sound effect
    /// </summary>
    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            float volume = 1f;
            if (SettingsManager.Instance != null)
            {
                volume = SettingsManager.GetEffectiveVolume(false, 1f);
            }
            audioSource.PlayOneShot(clickSound, volume);
        }
    }
}

