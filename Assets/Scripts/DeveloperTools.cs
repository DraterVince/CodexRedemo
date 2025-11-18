using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Developer tools for testing - provides quick access to clear saves, reset data, etc.
/// REMOVE THIS FROM PRODUCTION BUILDS!
/// </summary>
public class DeveloperTools : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject devToolsPanel;
    [SerializeField] private Button clearLocalSavesButton;
    [SerializeField] private Button clearSupabaseSavesButton;
    [SerializeField] private Button clearAllDataButton;
    [SerializeField] private Button togglePanelButton;
    [SerializeField] private Text statusText; // Using UnityEngine.UI.Text instead of TMPro
    
    [Header("Confirmation")]
    [SerializeField] private GameObject confirmationDialog;
    [SerializeField] private Text confirmationText; // Using UnityEngine.UI.Text instead of TMPro
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    private System.Action pendingAction;
    private bool isPanelVisible = false;
    
    private void Start()
    {
        // Setup buttons
        if (clearLocalSavesButton != null)
            clearLocalSavesButton.onClick.AddListener(() => ShowConfirmation("Clear all LOCAL saves?", ClearLocalSaves));
        
        if (clearSupabaseSavesButton != null)
            clearSupabaseSavesButton.onClick.AddListener(() => ShowConfirmation("Clear all SUPABASE saves?", ClearSupabaseSaves));
        
        if (clearAllDataButton != null)
            clearAllDataButton.onClick.AddListener(() => ShowConfirmation("Clear ALL data (local + Supabase)?", ClearAllData));
   
        if (togglePanelButton != null)
            togglePanelButton.onClick.AddListener(TogglePanel);
   
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(OnConfirmYes);
     
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(OnConfirmNo);
        
        // Hide panel by default
        if (devToolsPanel != null)
            devToolsPanel.SetActive(false);
        
        if (confirmationDialog != null)
         confirmationDialog.SetActive(false);
    }
    
    private void Update()
    {
        // Toggle dev tools with Ctrl+Shift+D
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D))
        {
            TogglePanel();
        }
    }
    
    private void TogglePanel()
    {
        isPanelVisible = !isPanelVisible;
        if (devToolsPanel != null)
            devToolsPanel.SetActive(isPanelVisible);
        
        Debug.Log($"[DeveloperTools] Panel {(isPanelVisible ? "shown" : "hidden")}");
 }
 
    private void ShowConfirmation(string message, System.Action action)
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(true);
            if (confirmationText != null)
                confirmationText.text = message;
            
            pendingAction = action;
        }
    }
    
    private void OnConfirmYes()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
        
        pendingAction?.Invoke();
        pendingAction = null;
    }
    
    private void OnConfirmNo()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
        
        pendingAction = null;
        UpdateStatus("Cancelled");
    }

    /// <summary>
    /// Clear all local PlayerPrefs saves
    /// </summary>
  private void ClearLocalSaves()
    {
    Debug.Log("[DeveloperTools] Clearing local saves...");
     
        // Use reflection to avoid assembly issues
        var saveManagerType = System.Type.GetType("NewAndLoadGameManager");
        if (saveManagerType != null)
        {
  var saveManager = FindObjectOfType(saveManagerType);
   if (saveManager != null)
      {
    var clearMethod = saveManagerType.GetMethod("ClearAllSlots");
   if (clearMethod != null)
   {
        clearMethod.Invoke(saveManager, null);
           UpdateStatus("? Local saves cleared!");
       return;
    }
  }
   }
   
   UpdateStatus("? NewAndLoadGameManager not found!");
    }
  
    /// <summary>
    /// Clear all Supabase saves (WebGL only)
    /// </summary>
  private void ClearSupabaseSaves()
    {
        Debug.Log("[DeveloperTools] Clearing Supabase saves...");
  
#if UNITY_WEBGL && !UNITY_EDITOR
        // Call JavaScript to delete all saves for current user
      Application.ExternalCall("DeleteAllSupabaseSaves");
        UpdateStatus("? Supabase saves deletion requested!");
#else
      UpdateStatus("?? Supabase clear only works in WebGL build!");
#endif
    }
    
    /// <summary>
    /// Clear everything - local saves, PlayerPrefs, and Supabase
    /// </summary>
    private void ClearAllData()
    {
        Debug.Log("[DeveloperTools] Clearing ALL data...");
        
// Clear local saves using reflection
        var saveManagerType = System.Type.GetType("NewAndLoadGameManager");
if (saveManagerType != null)
        {
 var saveManager = FindObjectOfType(saveManagerType);
if (saveManager != null)
   {
   var clearMethod = saveManagerType.GetMethod("ClearAllSlots");
  if (clearMethod != null)
      {
     clearMethod.Invoke(saveManager, null);
      }
   }
  }
        
        // Clear all PlayerPrefs
   PlayerPrefs.DeleteAll();
 PlayerPrefs.Save();
   
        // Clear PlayerDataManager cache using reflection
    var playerDataManagerType = System.Type.GetType("PlayerDataManager");
        if (playerDataManagerType != null)
        {
 var playerDataManager = FindObjectOfType(playerDataManagerType);
if (playerDataManager != null)
    {
       var clearMethod = playerDataManagerType.GetMethod("ClearPlayerDataCache");
       if (clearMethod != null)
   {
         clearMethod.Invoke(playerDataManager, null);
    }
            }
      }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        // Clear Supabase
Application.ExternalCall("DeleteAllSupabaseSaves");
UpdateStatus("? All data cleared (local + Supabase)!");
#else
     UpdateStatus("? All local data cleared! (Supabase requires WebGL build)");
#endif
  
     Debug.Log("[DeveloperTools] All data cleared!");
    }
    
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            Debug.Log($"[DeveloperTools] {message}");
        }
     
// Auto-hide status after 3 seconds
        Invoke(nameof(ClearStatus), 3f);
  }
    
    private void ClearStatus()
 {
    if (statusText != null)
    statusText.text = "";
 }
}
