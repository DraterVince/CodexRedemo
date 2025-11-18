using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Shows a tutorial prompt dialog when a new save is created
/// Attach this to a Canvas in the SaveLoadMenu scene
/// </summary>
public class TutorialPromptDialog : MonoBehaviour
{
    public static TutorialPromptDialog Instance;
    
    [Header("UI References")]
    [Tooltip("The tutorial prompt panel")]
    public GameObject tutorialPromptPanel;
    
    [Tooltip("Yes button - plays tutorial")]
    public Button yesButton;
    
    [Tooltip("No button - skips to level select")]
    public Button noButton;
    
    [Header("Optional Text")]
    [Tooltip("Message text (optional)")]
    public TextMeshProUGUI messageText;
    
    private const string TUTORIAL_SCENE = "Tutorial"; // Scene name or use index 5
    private const string LEVEL_SELECT_SCENE = "LevelSelect";
    
    private void Awake()
    {
      if (Instance == null)
        {
   Instance = this;
       DontDestroyOnLoad(gameObject);
  }
        else
    {
            Destroy(gameObject);
            return;
        }
    
        // Hide panel initially
   if (tutorialPromptPanel != null)
        {
            tutorialPromptPanel.SetActive(false);
        }
    
   // Setup button listeners
    if (yesButton != null)
  {
            yesButton.onClick.AddListener(OnYesClicked);
        }
      
  if (noButton != null)
   {
      noButton.onClick.AddListener(OnNoClicked);
        }
    }
    
  /// <summary>
    /// Show the tutorial prompt dialog
    /// Call this when a new save is created
    /// </summary>
    public void ShowTutorialPrompt()
    {
  Debug.Log("[TutorialPromptDialog] Showing tutorial prompt");
        
if (tutorialPromptPanel != null)
        {
     tutorialPromptPanel.SetActive(true);
      
 // Update message if text component exists
            if (messageText != null)
     {
  messageText.text = "Would you like to play the tutorial?\n\n" +
      "The tutorial will teach you the basics of the game.";
   }
    }
        else
        {
            Debug.LogWarning("[TutorialPromptDialog] Tutorial prompt panel is not assigned!");
        }
    }
    
    /// <summary>
    /// Hide the tutorial prompt without loading any scene
    /// </summary>
  public void HidePrompt()
    {
     if (tutorialPromptPanel != null)
        {
  tutorialPromptPanel.SetActive(false);
     }
    }
    
    /// <summary>
    /// User clicked "Yes" - Load tutorial level
    /// </summary>
    private void OnYesClicked()
    {
      Debug.Log("[TutorialPromptDialog] User chose to play tutorial");
     
        HidePrompt();
      
    // Mark that tutorial has been seen
   PlayerPrefs.SetInt("HasSeenTutorial", 1);
        PlayerPrefs.Save();

        // Ensure managers persist before loading scene
        EnsureManagersPersist();

  // Load tutorial level (scene index 5)
        // The lore dialogue will play automatically when the scene loads
 Debug.Log("[TutorialPromptDialog] Loading tutorial level with lore intro...");
        SceneManager.LoadScene(5); // Tutorial scene index
    }
    
    /// <summary>
    /// User clicked "No" - Skip to level select
 /// </summary>
  private void OnNoClicked()
    {
   Debug.Log("[TutorialPromptDialog] User chose to skip tutorial");
        
      HidePrompt();
        
        // Mark that tutorial has been seen (even if skipped)
        PlayerPrefs.SetInt("HasSeenTutorial", 1);
        PlayerPrefs.Save();
        
        // Ensure managers persist before loading scene
        EnsureManagersPersist();
        
      // Load level select
     Debug.Log("[TutorialPromptDialog] Loading level select...");
  SceneManager.LoadScene(LEVEL_SELECT_SCENE);
 }
    
    /// <summary>
    /// Check if player has already seen tutorial prompt
    /// </summary>
    public static bool HasSeenTutorial()
    {
        return PlayerPrefs.GetInt("HasSeenTutorial", 0) == 1;
    }
    
    /// <summary>
    /// Reset tutorial flag (for testing)
    /// </summary>
    public static void ResetTutorialFlag()
    {
        PlayerPrefs.DeleteKey("HasSeenTutorial");
PlayerPrefs.Save();
   Debug.Log("[TutorialPromptDialog] Tutorial flag reset");
    }
    
    /// <summary>
    /// Ensure critical managers persist before loading a new scene
    /// </summary>
    private void EnsureManagersPersist()
    {
        Debug.Log("[TutorialPromptDialog] Ensuring managers persist before scene load...");
        
        // Ensure NewAndLoadGameManager persists
        if (NewAndLoadGameManager.Instance != null)
        {
            if (NewAndLoadGameManager.Instance.gameObject != null)
            {
                // Move to root if it's a child (children get destroyed with parent)
                if (NewAndLoadGameManager.Instance.transform.parent != null)
                {
                    Debug.LogWarning("[TutorialPromptDialog] NewAndLoadGameManager is a child object! Moving to root to prevent destruction.");
                    NewAndLoadGameManager.Instance.transform.SetParent(null);
                }
                DontDestroyOnLoad(NewAndLoadGameManager.Instance.gameObject);
                Debug.Log("[TutorialPromptDialog] NewAndLoadGameManager set to persist");
            }
            else
            {
                Debug.LogError("[TutorialPromptDialog] NewAndLoadGameManager.Instance.gameObject is null!");
            }
        }
        else
        {
            Debug.LogWarning("[TutorialPromptDialog] NewAndLoadGameManager.Instance is null!");
        }
        
        // Ensure PlayerDataManager persists
        if (PlayerDataManager.Instance != null)
        {
            if (PlayerDataManager.Instance.gameObject != null)
            {
                // Move to root if it's a child (children get destroyed with parent)
                if (PlayerDataManager.Instance.transform.parent != null)
                {
                    Debug.LogWarning("[TutorialPromptDialog] PlayerDataManager is a child object! Moving to root to prevent destruction.");
                    PlayerDataManager.Instance.transform.SetParent(null);
                }
                DontDestroyOnLoad(PlayerDataManager.Instance.gameObject);
                Debug.Log("[TutorialPromptDialog] PlayerDataManager set to persist");
            }
            else
            {
                Debug.LogError("[TutorialPromptDialog] PlayerDataManager.Instance.gameObject is null!");
            }
        }
        else
        {
            Debug.LogWarning("[TutorialPromptDialog] PlayerDataManager.Instance is null!");
        }
        
        // Ensure LoadingScreenManager persists
        if (LoadingScreenManager.Instance != null)
        {
            if (LoadingScreenManager.Instance.gameObject != null)
            {
                // Move to root if it's a child (children get destroyed with parent)
                if (LoadingScreenManager.Instance.transform.parent != null)
                {
                    Debug.LogWarning("[TutorialPromptDialog] LoadingScreenManager is a child object! Moving to root to prevent destruction.");
                    LoadingScreenManager.Instance.transform.SetParent(null);
                }
                DontDestroyOnLoad(LoadingScreenManager.Instance.gameObject);
                Debug.Log("[TutorialPromptDialog] LoadingScreenManager set to persist");
            }
            else
            {
                Debug.LogError("[TutorialPromptDialog] LoadingScreenManager.Instance.gameObject is null!");
            }
        }
        else
        {
            Debug.LogWarning("[TutorialPromptDialog] LoadingScreenManager.Instance is null!");
        }
        
        // Ensure other critical managers persist
        if (SupabaseReadyManager.Instance != null && SupabaseReadyManager.Instance.gameObject != null)
        {
            DontDestroyOnLoad(SupabaseReadyManager.Instance.gameObject);
            Debug.Log("[TutorialPromptDialog] SupabaseReadyManager set to persist");
        }
        
        if (AuthManager.Instance != null && AuthManager.Instance.gameObject != null)
        {
            DontDestroyOnLoad(AuthManager.Instance.gameObject);
            Debug.Log("[TutorialPromptDialog] AuthManager set to persist");
        }
    }
}
