using UnityEngine;
using UnityEngine.Audio;
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

    [Header("Audio Mixer (Recommended)")]
    [Tooltip("Master mixer that contains 'Master', 'Music', and 'SFX' groups with exposed params.")]
    [SerializeField] private AudioMixer masterMixer;
    [Tooltip("Exposed parameter name for master volume in the mixer (dB)")]
    [SerializeField] private string masterParam = "MasterVol";
    [Tooltip("Exposed parameter name for music volume in the mixer (dB)")]
    [SerializeField] private string musicParam = "MusicVol";
    [Tooltip("Exposed parameter name for SFX volume in the mixer (dB)")]
    [SerializeField] private string sfxParam = "SFXVol";

    [Header("Mixer Group Paths (for routing)")]
    [SerializeField] private string masterGroupPath = "Master";
    [SerializeField] private string musicGroupPath  = "Master/Music";
    [SerializeField] private string sfxGroupPath    = "Master/SFX";

    private UnityEngine.Audio.AudioMixerGroup masterGroup, musicGroup, sfxGroup;

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
            // Ensure this object is at root so DontDestroyOnLoad works reliably
            if (transform.parent != null)
            {
                transform.SetParent(null, true);
            }
            // Don't destroy on load so settings persist across scenes
            DontDestroyOnLoad(gameObject);

            // Resolve and cache mixer groups for routing
            ResolveMixerGroups();

            // After resolving mixer groups, ensure the MusicManager source is routed
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.EnsureRouted();
            }
            
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

    private void ResolveMixerGroups()
    {
        if (masterMixer == null)
        {
            Debug.LogWarning("[SettingsManager] No AudioMixer assigned. Mixer routing disabled.");
            return;
        }

        var gMaster = masterMixer.FindMatchingGroups(masterGroupPath);
        var gMusic  = masterMixer.FindMatchingGroups(musicGroupPath);
        var gSFX    = masterMixer.FindMatchingGroups(sfxGroupPath);

        masterGroup = (gMaster != null && gMaster.Length > 0) ? gMaster[0] : null;
        musicGroup  = (gMusic  != null && gMusic.Length  > 0) ? gMusic[0]  : null;
        sfxGroup    = (gSFX    != null && gSFX.Length    > 0) ? gSFX[0]    : null;

        if (masterGroup == null) Debug.LogWarning("[SettingsManager] Mixer group not found: " + masterGroupPath);
        if (musicGroup == null)  Debug.LogWarning("[SettingsManager] Mixer group not found: " + musicGroupPath);
        if (sfxGroup == null)    Debug.LogWarning("[SettingsManager] Mixer group not found: " + sfxGroupPath);
    }

    public static void RouteMusic(AudioSource src)
    {
        if (src == null) return;
        if (Instance != null && Instance.musicGroup != null)
        {
            src.outputAudioMixerGroup = Instance.musicGroup;
        }
    }

    public static void RouteSFX(AudioSource src)
    {
        if (src == null) return;
        if (Instance != null && Instance.sfxGroup != null)
        {
            src.outputAudioMixerGroup = Instance.sfxGroup;
        }
    }

    private void Start()
    {
        // Load saved settings first
        LoadSettings();

        // Validate mixer configuration if assigned
        ValidateMixerConfiguration();

        // Find and setup sliders
        FindAndSetupSliders();
        
        // MusicAudioSource components will register themselves automatically via their Start/OnEnable
        // MusicManager's AudioSource is registered in ApplyVolumeSettings()

        // Apply loaded settings
        ApplyVolumeSettings();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Refresh sliders on scene load (UI may differ per scene)
        RefreshSliders();
    }

    private void ValidateMixerConfiguration()
    {
        if (masterMixer == null)
        {
            Debug.LogWarning("[SettingsManager] MasterMixer is not assigned. Volume will fall back to legacy per-source control.");
            return;
        }

        // Validate exposed parameter names
        float tmp;
        if (!masterMixer.GetFloat(masterParam, out tmp))
        {
            Debug.LogWarning($"[SettingsManager] Exposed parameter not found on mixer: '{masterParam}'. Check name in inspector and mixer.");
        }
        if (!masterMixer.GetFloat(musicParam, out tmp))
        {
            Debug.LogWarning($"[SettingsManager] Exposed parameter not found on mixer: '{musicParam}'. Check name in inspector and mixer.");
        }
        if (!masterMixer.GetFloat(sfxParam, out tmp))
        {
            Debug.LogWarning($"[SettingsManager] Exposed parameter not found on mixer: '{sfxParam}'. Check name in inspector and mixer.");
        }
    }

    /// <summary>
    /// Public method to re-scan and wire sliders. Call this when a settings/options panel
    /// becomes visible or is instantiated at runtime so sliders hook up correctly.
    /// </summary>
    public void RefreshSliders()
    {
        Debug.Log("[SettingsManager] RefreshSliders called");
        // Clear old slider refs if auto-find is enabled, then re-find and set up
        ClearSliderReferences();
        FindAndSetupSliders();
        // Do not change volumes here; sliders reflect current saved settings without pushing new values
        // If you need to force-apply current settings to the mixer, ensure ApplyVolumeSettings is called elsewhere on load
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
            // Remove listeners before setting value to avoid firing change events
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.SetValueWithoutNotify(MasterVolume);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        // SFX Volume Slider
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.SetValueWithoutNotify(SFXVolume);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Music Volume Slider
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.SetValueWithoutNotify(MusicVolume);
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
        Debug.Log($"[SettingsManager] Applying volume settings - Master: {MasterVolume:F2}, Music: {MusicVolume:F2}, SFX: {SFXVolume:F2}");

        // Preferred: drive AudioMixer exposed parameters (dB)
        if (masterMixer != null)
        {
            masterMixer.SetFloat(masterParam, ToDb(MasterVolume));
            masterMixer.SetFloat(musicParam,  ToDb(MusicVolume  * MasterVolume));
            masterMixer.SetFloat(sfxParam,    ToDb(SFXVolume    * MasterVolume));
            // Note: If parameters are misnamed, values won't apply. ValidateMixerConfiguration() logs that.
            return; // Mixer in control; no per-source updates needed
        }

        // Fallback (legacy path): per-source volume control
        // Ensure MusicManager's AudioSource is registered before updating
        if (MusicManager.Instance != null)
        {
            AudioSource musicManagerSource = MusicManager.Instance.GetMusicSource();
            if (musicManagerSource != null)
            {
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

        UpdateRegisteredAudioSources(registeredSFXSources, SFXVolume, true);
        UpdateRegisteredAudioSources(registeredMusicSources, MusicVolume, true);
    }

    private void UpdateRegisteredAudioSources(List<AudioSource> sources, float volumeSetting, bool applyMasterVolume = false)
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

                // Apply volume setting: original volume × category volume × master volume
                // This creates proper volume hierarchy: Master affects all, category affects specific type
                float finalVolume = volumeHelper.originalVolume * volumeSetting;
                
                if (applyMasterVolume)
                {
                    finalVolume *= MasterVolume;
                }
                
                source.volume = finalVolume;
                
                // Log for debugging
                Debug.Log($"[SettingsManager] Updated {source.gameObject.name} volume to {finalVolume:F3} (Original: {volumeHelper.originalVolume:F2}, Category: {volumeSetting:F2}, Master: {MasterVolume:F2})");
            }
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        Debug.Log($"[SettingsManager] Master Volume changed to: {MasterVolume:F2} ({MasterVolume * 100:F0}%)");
        SaveSettings();
        ApplyVolumeSettings();
    }

    private void OnSFXVolumeChanged(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        Debug.Log($"[SettingsManager] SFX Volume changed to: {SFXVolume:F2} ({SFXVolume * 100:F0}%)");
        SaveSettings();
        ApplyVolumeSettings();
    }

    private void OnMusicVolumeChanged(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        Debug.Log($"[SettingsManager] Music Volume changed to: {MusicVolume:F2} ({MusicVolume * 100:F0}%)");
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
    // Returns: baseVolume × category volume × master volume
    public static float GetEffectiveVolume(bool isMusic, float baseVolume = 1f)
    {
        // If an AudioMixer is assigned and we're routing sources, let the mixer control volume.
        if (Instance != null && Instance.masterMixer != null)
        {
            return baseVolume;
        }

        if (isMusic)
        {
            return baseVolume * MusicVolume * MasterVolume;
        }
        else
        {
            return baseVolume * SFXVolume * MasterVolume;
        }
    }
    private static float ToDb(float v)
    {
        return v <= 0f ? -80f : Mathf.Log10(v) * 20f;
    }
}

// Helper component to store original volume (used by legacy per-source path)
public class AudioVolumeHelper : MonoBehaviour
{
    public float originalVolume = 1f;
}
