using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

/// <summary>
/// Manages character selection flow between scenes
/// Place this in your MainMenu or LevelSelect scene
/// </summary>
public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("Name of the Character Selection scene (if separate)")]
    [SerializeField] private string characterSelectionSceneName = "CharacterSelection";
    
    [Tooltip("Show character selection before level starts")]
    [SerializeField] private bool showCharacterSelectionBeforeLevel = true;
    
    [Header("UI References")]
    [Tooltip("Character Selection UI Panel (if in same scene)")]
    [SerializeField] private GameObject characterSelectionPanel;
    
    [Tooltip("Main Menu Panel")]
    [SerializeField] private GameObject mainMenuPanel;
    
    [Header("Settings")]
    [Tooltip("Save last selected character")]
    // Reserved for future use
    #pragma warning disable CS0414
    [SerializeField] private bool _saveSelection = true;
    #pragma warning restore CS0414
    
    [Tooltip("Use DontDestroyOnLoad (keep manager across scenes)")]
    [SerializeField] private bool persistAcrossScenes = false;
    
    private static CharacterSelectionManager instance;
    private int selectedLevelIndex = -1;
    private string currentSceneName;
    
    public static CharacterSelectionManager Instance
    {
      get { return instance; }
    }

    private void Awake()
    {
        // Singleton pattern
        if (persistAcrossScenes)
        {
         if (instance == null)
        {
 instance = this;
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
        else
        {
     instance = this;
        }
        
        currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[CharacterSelectionManager] Initialized in scene: {currentSceneName}");
    }
    
    private void OnDestroy()
    {
        if (persistAcrossScenes && instance == this)
{
            // Unsubscribe from scene loaded event
   SceneManager.sceneLoaded -= OnSceneLoaded;
  instance = null;
        }
    }
    
    /// <summary>
 /// Called when a new scene is loaded (only if persistAcrossScenes is true)
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        Debug.Log($"[CharacterSelectionManager] Scene loaded: {currentSceneName}");
      
        // Clear references to old scene objects
        characterSelectionPanel = null;
        mainMenuPanel = null;
   
        // If we're back in LevelSelect, try to refresh character selection
        if (currentSceneName == "LevelSelect")
  {
            // Delay to let scene initialize
   Invoke(nameof(RefreshLevelSelectReferences), 0.2f);
 }
    }
    
  /// <summary>
    /// Refresh references when returning to level select
    /// </summary>
    private void RefreshLevelSelectReferences()
    {
  Debug.Log("[CharacterSelectionManager] Refreshing LevelSelect references...");
    
        // Try to find character selection UI dynamically
  var uiType = System.Type.GetType("CharacterSelectionUI");
        if (uiType != null)
        {
      var ui = FindObjectOfType(uiType);
            if (ui != null)
  {
            // Refresh the display
       var refreshMethod = uiType.GetMethod("RefreshDisplay");
        if (refreshMethod != null)
       {
        refreshMethod.Invoke(ui, null);
   Debug.Log("[CharacterSelectionManager] CharacterSelectionUI refreshed!");
      }
            }
 }
    }
    
    /// <summary>
    /// Called when player wants to start a level
    /// Shows character selection first if enabled
    /// </summary>
    /// <param name="levelSceneIndex">Scene index of the level to load</param>
  public void PrepareToStartLevel(int levelSceneIndex)
    {
        selectedLevelIndex = levelSceneIndex;
        
        if (showCharacterSelectionBeforeLevel)
        {
        OpenCharacterSelection();
        }
        else
        {
        StartLevel();
        }
  }
    
    /// <summary>
    /// Called when player wants to start a level by scene name
    /// </summary>
    /// <param name="levelSceneName">Name of the level scene</param>
    public void PrepareToStartLevel(string levelSceneName)
    {
        if (showCharacterSelectionBeforeLevel)
        {
   PlayerPrefs.SetString("NextLevelName", levelSceneName);
   OpenCharacterSelection();
        }
        else
        {
         LoadSceneMultiplayerAware(levelSceneName);
        }
    }
    
    /// <summary>
    /// Open the character selection UI
    /// </summary>
    public void OpenCharacterSelection()
    {
        if (characterSelectionPanel != null)
        {
       // In-scene character selection
            if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
      
     characterSelectionPanel.SetActive(true);
        }
   else
        {
    // Load separate character selection scene
            LoadSceneMultiplayerAware(characterSelectionSceneName);
     }
    }
    
    /// <summary>
    /// Close character selection and return to previous menu
    /// </summary>
    public void CloseCharacterSelection()
    {
  if (characterSelectionPanel != null)
        {
            characterSelectionPanel.SetActive(false);
            
     if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        }
 }
    
    /// <summary>
    /// Called when character is selected and confirmed
    /// Starts the level
    /// </summary>
    public void OnCharacterConfirmed()
    {
        StartLevel();
    }
    
    /// <summary>
    /// Actually load the level
 /// </summary>
    private void StartLevel()
    {
        // Check if we have a level name stored
        if (PlayerPrefs.HasKey("NextLevelName"))
        {
          string levelName = PlayerPrefs.GetString("NextLevelName");
    PlayerPrefs.DeleteKey("NextLevelName");
    LoadSceneMultiplayerAware(levelName);
            return;
        }
        
        // Otherwise use scene index
        if (selectedLevelIndex >= 0)
  {
    LoadSceneMultiplayerAware(selectedLevelIndex);
        }
      else
     {
    Debug.LogWarning("No level selected to start!");
        }
    }
    
    /// <summary>
    /// Quick access - open character selection from button
    /// </summary>
    public void OpenCharacterSelectionFromButton()
    {
        OpenCharacterSelection();
    }

    /// <summary>
    /// Load a scene by name, respecting Photon multiplayer sync
    /// </summary>
    private void LoadSceneMultiplayerAware(string sceneName)
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom && PhotonNetwork.AutomaticallySyncScene)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(sceneName);
            }
            else
            {
                Debug.LogWarning($"[CharacterSelectionManager] Only the Master Client can load scenes in multiplayer. Waiting for host to load '{sceneName}'.");
            }
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// Load a scene by build index, respecting Photon multiplayer sync
    /// </summary>
    private void LoadSceneMultiplayerAware(int sceneIndex)
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom && PhotonNetwork.AutomaticallySyncScene)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(sceneIndex);
            }
            else
            {
                Debug.LogWarning($"[CharacterSelectionManager] Only the Master Client can load scenes in multiplayer. Waiting for host to load scene index {sceneIndex}.");
            }
        }
        else
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }
}
