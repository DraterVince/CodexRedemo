using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("Volume Sliders (Optional - Auto-found if not assigned)")]
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;

    [Header("Auto-find Settings")]
    [Tooltip("Automatically find sliders by name when scenes load")]
    public bool autoFindSliders = true;
    [Tooltip("Names of sliders to search for (default: MasterVolumeSlider, SFXVolumeSlider, MusicVolumeSlider)")]
    public string masterSliderName = "MasterVolumeSlider";
    public string sfxSliderName = "SFXVolumeSlider";
    public string musicSliderName = "MusicVolumeSlider";

    // Static volume values for easy access
    public static float MasterVolume { get; private set; } = 1f;
    public static float SFXVolume { get; private set; } = 1f;
    public static float MusicVolume { get; private set; } = 1f;

    // PlayerPrefs keys
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";

    // Singleton instance
    public static SettingsManager Instance { get; private set; }

    // Lists to track registered audio sources
    private static List<AudioSource> registeredSFXSources = new List<AudioSource>();
    private static List<AudioSource> registeredMusicSources = new List<AudioSource>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Don't destroy on load so settings persist across scenes
            DontDestroyOnLoad(gameObject);
            
            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Start()
    {
        // Load saved settings first
        LoadSettings();

        // Find and setup sliders
        FindAndSetupSliders();
        
        // MusicAudioSource components will register themselves automatically via their Start/OnEnable
        // MusicManager's AudioSource is registered in ApplyVolumeSettings()

        // Apply loaded settings
        ApplyVolumeSettings();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When a new scene loads, clear old slider references and find new ones
        // This ensures we connect to sliders in the new scene, not stale references
        ClearSliderReferences();
        FindAndSetupSliders();
        
        // MusicAudioSource components will register themselves automatically via their Start/OnEnable
        // MusicManager's AudioSource is registered in ApplyVolumeSettings()
        
        // Reapply volume settings to ensure they're applied to new scene
        ApplyVolumeSettings();
    }
    
    /// <summary>
    /// Find and register all AudioSources with MusicAudioSource component in the current scene
    /// Also explicitly register MusicManager's AudioSource if it exists
    /// NOTE: This method is kept for potential future use but is not currently called to avoid allocations
    /// </summary>
    private void RegisterMusicAudioSourcesInScene()
    {
        // This method uses FindObjectsOfType which allocates memory
        // MusicAudioSource components register themselves automatically via Start/OnEnable
        // So this method is not needed and should not be called
    }

    private void ClearSliderReferences()
    {
        // Clear slider references so they can be re-found in the new scene
        // Only clear if auto-find is enabled (if manually assigned, keep them)
        if (autoFindSliders)
        {
            masterVolumeSlider = null;
            sfxVolumeSlider = null;
            musicVolumeSlider = null;
        }
    }

    private void FindAndSetupSliders()
    {
        // If auto-find is enabled, find sliders by name (searches active and inactive)
        if (autoFindSliders)
        {
            if (masterVolumeSlider == null)
            {
                masterVolumeSlider = FindSliderByName(masterSliderName);
            }

            if (sfxVolumeSlider == null)
            {
                sfxVolumeSlider = FindSliderByName(sfxSliderName);
            }

            if (musicVolumeSlider == null)
            {
                musicVolumeSlider = FindSliderByName(musicSliderName);
            }
        }

        // Setup slider listeners
        SetupSliders();
    }

    private Slider FindSliderByName(string sliderName)
    {
        // Search through all root objects in all loaded scenes
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;
            
            // Get all root objects in this scene
            GameObject[] rootObjects = scene.GetRootGameObjects();
            
            // Search recursively through all objects (including inactive)
            foreach (GameObject rootObj in rootObjects)
            {
                Slider foundSlider = FindSliderInChildren(rootObj, sliderName);
                if (foundSlider != null)
                {
                    return foundSlider;
                }
            }
        }
        
        // Fallback: Try GameObject.Find (only finds active objects)
        GameObject sliderObj = GameObject.Find(sliderName);
        if (sliderObj != null)
        {
            Slider slider = sliderObj.GetComponent<Slider>();
            if (slider != null)
            {
                return slider;
            }
        }
        
        return null;
    }

    private Slider FindSliderInChildren(GameObject parent, string sliderName)
    {
        // Check if this object is the one we're looking for
        if (parent.name == sliderName)
        {
            Slider slider = parent.GetComponent<Slider>();
            if (slider != null)
            {
                return slider;
            }
        }
        
        // Recursively search children (even if inactive)
        foreach (Transform child in parent.transform)
        {
            Slider foundSlider = FindSliderInChildren(child.gameObject, sliderName);
            if (foundSlider != null)
            {
                return foundSlider;
            }
        }
        
        return null;
    }

    private void SetupSliders()
    {
        // Master Volume Slider
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = MasterVolume;
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        // SFX Volume Slider
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = SFXVolume;
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Music Volume Slider
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = MusicVolume;
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
    }

    private void LoadSettings()
    {
        // Load from PlayerPrefs (default to 1.0 if not found)
        MasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        SFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, MasterVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, SFXVolume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, MusicVolume);
        PlayerPrefs.Save();
    }

    public void ApplyVolumeSettings()
    {
        // Apply master volume to AudioListener (affects all audio globally)
        // This is applied to everything, so individual sources don't need to multiply by master
        AudioListener.volume = MasterVolume;

        // Ensure MusicManager's AudioSource is registered before updating
        // This is important because MusicManager uses DontDestroyOnLoad and might not be found by FindObjectsOfType
        if (MusicManager.Instance != null)
        {
            AudioSource musicManagerSource = MusicManager.Instance.GetMusicSource();
            if (musicManagerSource != null)
            {
                // Avoid Contains allocation - check manually
                bool alreadyRegistered = false;
                for (int i = 0; i < registeredMusicSources.Count; i++)
                {
                    if (registeredMusicSources[i] == musicManagerSource)
                    {
                        alreadyRegistered = true;
                        break;
                    }
                }
                
                if (!alreadyRegistered)
                {
                    RegisterMusicSource(musicManagerSource);
                }
            }
        }
        

        // Update registered SFX audio sources
        UpdateRegisteredAudioSources(registeredSFXSources, SFXVolume);

        // Update registered Music audio sources
        // This includes MusicManager's AudioSource if it's registered
        UpdateRegisteredAudioSources(registeredMusicSources, MusicVolume);
    }

    private void UpdateRegisteredAudioSources(List<AudioSource> sources, float volumeSetting)
    {
        // Remove null references - avoid lambda allocation
        for (int i = sources.Count - 1; i >= 0; i--)
        {
            if (sources[i] == null)
            {
                sources.RemoveAt(i);
            }
        }

        // Update volume for all registered sources
        foreach (AudioSource source in sources)
        {
            if (source != null)
            {
                // Store original volume if not already stored - cache GetComponent result
                AudioVolumeHelper volumeHelper = source.GetComponent<AudioVolumeHelper>();
                if (volumeHelper == null)
                {
                    volumeHelper = source.gameObject.AddComponent<AudioVolumeHelper>();
                    // Use current volume if > 0, otherwise default to 1.0
                    if (source.volume > 0)
                    {
                        volumeHelper.originalVolume = source.volume;
                    }
                    else
                    {
                        source.volume = 1f;
                        volumeHelper.originalVolume = 1f;
                    }
                }
                else if (volumeHelper.originalVolume <= 0)
                {
                    // Fix if original volume was incorrectly set to 0
                    source.volume = 1f;
                    volumeHelper.originalVolume = 1f;
                }

                // Apply volume setting: original volume * volume setting
                // Master volume is already applied via AudioListener.volume
                float finalVolume = volumeHelper.originalVolume * volumeSetting;
                source.volume = finalVolume;
            }
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        SaveSettings();
        ApplyVolumeSettings();
    }

    private void OnSFXVolumeChanged(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        SaveSettings();
        ApplyVolumeSettings();
    }

    private void OnMusicVolumeChanged(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        SaveSettings();
        ApplyVolumeSettings();
    }

    // Public methods to register audio sources
    public static void RegisterSFXSource(AudioSource source)
    {
        if (source != null)
        {
            // Avoid Contains allocation - check manually
            bool alreadyRegistered = false;
            for (int i = 0; i < registeredSFXSources.Count; i++)
            {
                if (registeredSFXSources[i] == source)
                {
                    alreadyRegistered = true;
                    break;
                }
            }
            
            if (!alreadyRegistered)
            {
                registeredSFXSources.Add(source);
                if (Instance != null)
                {
                    Instance.ApplyVolumeSettings();
                }
            }
        }
    }

    public static void RegisterMusicSource(AudioSource source)
    {
        if (source != null)
        {
            // Avoid Contains allocation - check manually
            bool alreadyRegistered = false;
            for (int i = 0; i < registeredMusicSources.Count; i++)
            {
                if (registeredMusicSources[i] == source)
                {
                    alreadyRegistered = true;
                    break;
                }
            }
            
            if (!alreadyRegistered)
            {
                registeredMusicSources.Add(source);
                
                if (Instance != null)
                {
                    Instance.ApplyVolumeSettings();
                }
            }
        }
    }

    public static void UnregisterSFXSource(AudioSource source)
    {
        if (source != null)
        {
            registeredSFXSources.Remove(source);
        }
    }

    public static void UnregisterMusicSource(AudioSource source)
    {
        if (source != null)
        {
            registeredMusicSources.Remove(source);
        }
    }

    // Method for AudioSources to get their effective volume (when playing one-shot sounds)
    // Master volume is handled by AudioListener, so we only return the category volume
    public static float GetEffectiveVolume(bool isMusic, float baseVolume = 1f)
    {
        if (isMusic)
        {
            return baseVolume * MusicVolume;
        }
        else
        {
            return baseVolume * SFXVolume;
        }
    }
}

// Helper component to store original volume
public class AudioVolumeHelper : MonoBehaviour
{
    public float originalVolume = 1f;
}
