using UnityEngine;

/// <summary>
/// Manages background music for different game states (battle, victory, defeat)
/// </summary>
public class MusicManager : MonoBehaviour
{
    [Header("Music Clips")]
    [SerializeField] private AudioClip battleMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip defeatMusic;
    [SerializeField] private AudioClip menuMusic; // Optional: separate music for main menu

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;

    private static MusicManager instance;
    public static MusicManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MusicManager>();
            }
            return instance;
        }
    }

    private AudioClip currentMusicClip;
    private bool isPlaying = false;

    private void Awake()
    {
        // Singleton pattern - keep only one instance
        if (instance == null)
        {
            instance = this;
            if (transform.parent != null)
            {
                transform.SetParent(null, true);
            }
            DontDestroyOnLoad(gameObject);
            
            // Create AudioSource if not assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = 1f; // Set default volume to 1.0
            }

            // Route to Music mixer group if available (after SettingsManager is likely initialized)
            EnsureRouted();
           
           // Ensure AudioVolumeHelper stores original volume (1.0) - cache GetComponent
            AudioVolumeHelper volumeHelper = musicSource.GetComponent<AudioVolumeHelper>();
            if (volumeHelper == null)
            {
                volumeHelper = musicSource.gameObject.AddComponent<AudioVolumeHelper>();
                volumeHelper.originalVolume = 1f;
            }
            
            // Add MusicAudioSource component to ensure proper registration - cache GetComponent
            MusicAudioSource musicAudioSource = musicSource.GetComponent<MusicAudioSource>();
            if (musicAudioSource == null)
            {
                musicAudioSource = musicSource.gameObject.AddComponent<MusicAudioSource>();
            }
            
            // Register with SettingsManager for volume control
            if (SettingsManager.Instance != null)
            {
                SettingsManager.RegisterMusicSource(musicSource);
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Second-chance routing in case SettingsManager initialized after Awake
        EnsureRouted();
        // Ensure AudioVolumeHelper is set up with original volume
        if (musicSource != null)
        {
            AudioVolumeHelper volumeHelper = musicSource.GetComponent<AudioVolumeHelper>();
            if (volumeHelper == null)
            {
                volumeHelper = musicSource.gameObject.AddComponent<AudioVolumeHelper>();
                volumeHelper.originalVolume = 1f;
            }
            else if (volumeHelper.originalVolume <= 0)
            {
                volumeHelper.originalVolume = 1f;
            }
        }
        
        // Register with SettingsManager if available
        if (SettingsManager.Instance != null && musicSource != null)
        {
            SettingsManager.RegisterMusicSource(musicSource);
            // Force immediate volume update
            SettingsManager.Instance.ApplyVolumeSettings();
        }
        
        // Subscribe to scene loaded to reapply volume when scenes change
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Ensure the music source is routed to the correct mixer group after a scene change
        EnsureRouted();

        // Re-register with SettingsManager when a new scene loads
        // This ensures the AudioSource is registered and volume is applied
        if (SettingsManager.Instance != null && musicSource != null)
        {
            SettingsManager.RegisterMusicSource(musicSource);
            // Force immediate volume update
            SettingsManager.Instance.ApplyVolumeSettings();
        }
    }
    
    /// <summary>
    /// Apply current volume settings to the music source
    /// DEPRECATED: Volume is now managed entirely by SettingsManager
    /// This method is kept for backwards compatibility but does nothing
    /// </summary>
    private void ApplyVolumeSettings()
    {
        // Volume is now managed entirely by SettingsManager.UpdateRegisteredAudioSources()
        // No need to apply volume here - it will be set automatically when the slider changes
    }

    /// <summary>
    /// Play battle music (looping)
    /// </summary>
    public void PlayBattleMusic()
    {
        if (battleMusic == null)
        {
            return;
        }

        PlayMusic(battleMusic, true);
    }
    
    /// <summary>
    /// Play menu music (looping) - uses menuMusic if assigned, otherwise uses battleMusic
    /// </summary>
    public void PlayMenuMusic()
    {
        AudioClip musicToPlay = menuMusic != null ? menuMusic : battleMusic;
        if (musicToPlay == null)
        {
            return;
        }

        PlayMusic(musicToPlay, true);
    }

    /// <summary>
    /// Ensure menu music is playing, but do not restart if the same menu track is already playing.
    /// Use this in scenes where you want continuous background music across transitions.
    /// </summary>
    public void PlayMenuMusicIfNotPlaying()
    {
        AudioClip musicToPlay = menuMusic != null ? menuMusic : battleMusic;
        if (musicToPlay == null)
        {
            return;
        }

        if (currentMusicClip == musicToPlay && isPlaying && musicSource != null && musicSource.isPlaying)
        {
            // Already playing the same track; do nothing to avoid a restart
            return;
        }

        PlayMusic(musicToPlay, true);
    }

    /// <summary>
    /// Play victory music (non-looping - plays once)
    /// </summary>
    public void PlayVictoryMusic()
    {
        if (victoryMusic == null)
        {
            return;
        }

        PlayMusic(victoryMusic, false);
    }

    /// <summary>
    /// Play defeat music (non-looping - plays once)
    /// </summary>
    public void PlayDefeatMusic()
    {
        if (defeatMusic == null)
        {
            return;
        }

        PlayMusic(defeatMusic, false);
    }

    /// <summary>
    /// Stop current music
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            isPlaying = false;
            currentMusicClip = null;
        }
    }

    /// <summary>
    /// Play a specific music clip
    /// </summary>
    private void PlayMusic(AudioClip clip, bool loop)
    {
        if (musicSource == null)
        {
            return;
        }

        if (clip == null)
        {
            return;
        }

        // Only change music if it's different from what's currently playing
        if (currentMusicClip != clip || !isPlaying)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            
            // Ensure we're registered with SettingsManager
            if (SettingsManager.Instance != null)
            {
                SettingsManager.RegisterMusicSource(musicSource);
            }
            
            // Force SettingsManager to update volume immediately
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ApplyVolumeSettings();
            }
            
            // Check current volume after SettingsManager applies it
            float currentMusicVolume = SettingsManager.Instance != null ? SettingsManager.MusicVolume : 1f;
            
            // Always set the clip and play state
            // Volume is controlled by SettingsManager, so we don't need to check volume here
            // Music will play at whatever volume SettingsManager has set (even if 0)
            musicSource.Play();
            currentMusicClip = clip;
            isPlaying = true;
        }
    }

    /// <summary>
    /// Check if music is currently playing
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying && musicSource != null && musicSource.isPlaying;
    }

    /// <summary>
    /// Set the music source volume (base volume, before settings multiplier)
    /// </summary>
    public void SetVolume(float volume)
    {
        if (musicSource != null)
        {
            // Store as original volume
            AudioVolumeHelper volumeHelper = musicSource.GetComponent<AudioVolumeHelper>();
            if (volumeHelper == null)
            {
                volumeHelper = musicSource.gameObject.AddComponent<AudioVolumeHelper>();
            }
            volumeHelper.originalVolume = Mathf.Clamp01(volume);
            
            // Volume is managed by SettingsManager, no need to reapply
        }
    }
    
    /// <summary>
    /// Public method to force volume update (called by SettingsManager when volume changes)
    /// DEPRECATED: Volume is now managed entirely by SettingsManager
    /// </summary>
    public void UpdateVolume()
    {
        // Volume is now managed entirely by SettingsManager.UpdateRegisteredAudioSources()
        // No action needed here
    }
    
    /// <summary>
    /// Get the AudioSource used for music playback
    /// </summary>
    public AudioSource GetMusicSource()
    {
        return musicSource;
    }

    /// <summary>
    /// Ensure the music AudioSource is routed to the Music mixer group if available.
    /// Safe to call multiple times.
    /// </summary>
    public void EnsureRouted()
    {
        if (musicSource == null) return;
        SettingsManager.RouteMusic(musicSource);
    }
}

