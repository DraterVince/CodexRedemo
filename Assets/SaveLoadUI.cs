using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUI : MonoBehaviour
{
    [Header("Slot Prefab")]
    public GameObject slotPrefab;
    public Transform slotContainer;

    [Header("Buttons")]
    public Button backButton;

    [Header("Confirmation Dialog")]
    public GameObject confirmationDialog;
    public TextMeshProUGUI confirmationText;
    public Button confirmYesButton;
    public Button confirmNoButton;

    private int pendingSlot = 0;
    private bool isNewGame = false;
    private bool isCreatingSave = false;  // NEW: Debounce flag

    private void Start()
    {
        Debug.Log("[SaveLoadUI] Starting SaveLoadUI...");

#if UNITY_WEBGL && !UNITY_EDITOR
        // FOR DEPLOYMENT: Supabase enabled
        Debug.Log("[SaveLoadUI] WebGL: Waiting for Supabase to load saves...");
        StartCoroutine(WaitForSupabaseAndInit());
      
        // FOR LOCALHOST: Uncomment these lines to disable Supabase
        // Debug.Log("[SaveLoadUI] WebGL: LOCALHOST MODE - Initializing immediately (Supabase disabled)");
        // InitializeUI();
#else
        InitializeUI();
#endif
    }
 
#if UNITY_WEBGL && !UNITY_EDITOR
    private System.Collections.IEnumerator WaitForSupabaseAndInit()
    {
     // Wait for NewAndLoadGameManager to load from Supabase
 Debug.Log("[SaveLoadUI] WebGL: Waiting 2 seconds for Supabase load...");
  yield return new WaitForSeconds(2.0f);
        
 Debug.Log("[SaveLoadUI] WebGL: Supabase load should be complete, initializing UI...");
        InitializeUI();
    }
#endif
    
    private void InitializeUI()
    {
        if (NewAndLoadGameManager.Instance == null)
        {
            Debug.LogError("NewAndLoadGameManager not found! Add it to your scene.");
            return;
  }

        EnsureUserIdSet();
        GenerateSlots();

    if (backButton != null)
{
            backButton.onClick.AddListener(OnBackClicked);
    }

        if (confirmationDialog != null)
 {
         confirmationDialog.SetActive(false);
        }
        else
        {
    Debug.LogError("Confirmation dialog is NULL! Assign it to Inspector.");
        }

        if (confirmYesButton != null)
        {
            confirmYesButton.onClick.AddListener(OnConfirmYes);
        }
   else
        {
          Debug.LogWarning("Confirm Yes Button is NULL!");
        }

        if (confirmNoButton != null)
        {
            confirmNoButton.onClick.AddListener(OnConfirmNo);
      }
        else
        {
  Debug.LogWarning("Confirm No Button is NULL!");
        }
    }

    private void EnsureUserIdSet()
    {
        Debug.Log("[SaveLoadUI] EnsureUserIdSet called...");
     
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
        {
            string userId = PlayerDataManager.Instance.GetCurrentPlayerData().user_id;
            Debug.Log($"[SaveLoadUI] Got user ID from PlayerDataManager: {userId}");
            
            if (NewAndLoadGameManager.Instance != null && !string.IsNullOrEmpty(userId))
            {
                NewAndLoadGameManager.Instance.SetUserId(userId);
            }
            else if (string.IsNullOrEmpty(userId))
            {
                Debug.LogError("[SaveLoadUI] User ID is empty! This will cause save slots to disappear!");
            }
        }
        else
        {
            Debug.LogError("[SaveLoadUI] PlayerDataManager or PlayerData is NULL! Saves may not load correctly!");
        }
    }

    private void GenerateSlots()
    {
        Debug.Log("[SaveLoadUI] GenerateSlots called...");
        
        if (slotPrefab == null || slotContainer == null)
        {
            Debug.LogError("Slot prefab or container not assigned!");
            return;
        }

        // Clear existing slots
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        // Get all save data
        List<NewAndLoadGameManager.GameData> slots = NewAndLoadGameManager.Instance.GetAllSlots();
        Debug.Log($"[SaveLoadUI] Retrieved {slots.Count} slots from NewAndLoadGameManager");

        // Create slot UI for each
        for (int i = 0; i < slots.Count; i++)
        {
            int slotNumber = i + 1; // Slots are 1-indexed
            NewAndLoadGameManager.GameData data = slots[i];
            
            string slotStatus = data.isEmpty ? "EMPTY" : $"OCCUPIED (Level: {data.levelsUnlocked}, Money: {data.currentMoney})";
            Debug.Log($"[SaveLoadUI] Slot {slotNumber}: {slotStatus}");

            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            slotObj.name = "SaveSlot_" + slotNumber;

            SaveLoadSlot slotScript = slotObj.GetComponent<SaveLoadSlot>();

            if (slotScript != null)
            {
                slotScript.SetupSlot(slotNumber, data, OnSlotClicked, OnDeleteClicked);
            }
        }
    }

    private void OnSlotClicked(int slot, bool isEmpty)
    {
        pendingSlot = slot;

        if (isEmpty)
        {
            // Start new game
            isNewGame = true;
            ShowConfirmation("Start a new game in Slot " + slot + "?");
        }
        else
        {
            // Load existing game
            isNewGame = false;
            ShowConfirmation("Load game from Slot " + slot + "?");
        }
    }

    private void OnDeleteClicked(int slot)
    {
        pendingSlot = slot;
        isNewGame = false;
        ShowConfirmation("Delete save data in Slot " + slot + "?");
    }

    private void ShowConfirmation(string message)
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(true);
            
            if (confirmationText != null)
            {
                confirmationText.text = message;
            }
        }
    }

    private void OnConfirmYes()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }

        if (pendingSlot == 0) return;

        if (isNewGame)
        {
            // NEW: Prevent rapid save creation
 if (isCreatingSave)
            {
                Debug.LogWarning("[SaveLoadUI] Save creation already in progress! Please wait...");
          return;
    }
            
            // Start new game with debounce
   StartCoroutine(CreateSaveWithDelay(pendingSlot));
        }
    else if (confirmationText != null && confirmationText.text.Contains("Delete"))
      {
            // Delete slot
    NewAndLoadGameManager.Instance.DeleteSlot(pendingSlot);
            GenerateSlots(); // Refresh UI
        }
else
  {
            // Load game - this will load the game scene
        NewAndLoadGameManager.Instance.LoadGame(pendingSlot);
     }

        pendingSlot = 0;
    }
    
    /// <summary>
  /// Create save with delay to ensure WebGL IndexedDB can keep up
    /// </summary>
    private IEnumerator CreateSaveWithDelay(int slot)
    {
        isCreatingSave = true;
        
        Debug.Log($"[SaveLoadUI] Creating save for slot {slot}...");
 
      // Create the save
   NewAndLoadGameManager.Instance.NewGame(slot);
        
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: Wait for save to complete before allowing another save
   Debug.Log("[SaveLoadUI] WebGL: Waiting for save operation to complete...");
   
     // Wait for the save verification to complete
    float timeout = 10f;
        float elapsed = 0f;
     
     while (NewAndLoadGameManager.Instance.IsSaveInProgress && elapsed < timeout)
     {
          yield return new WaitForSeconds(0.1f);
   elapsed += 0.1f;
    }
    
        if (elapsed >= timeout)
    {
            Debug.LogWarning($"[SaveLoadUI] Save operation timeout! ({elapsed:F1}s)");
        }
        else
        {
  Debug.Log($"[SaveLoadUI] Save completed after {elapsed:F1}s");
        }
   
   // Additional buffer for Supabase sync
        Debug.Log("[SaveLoadUI] WebGL: Adding 1s buffer for Supabase sync...");
   yield return new WaitForSeconds(1f);
#else
        // Desktop: Small delay just for UI smoothness
  yield return new WaitForSeconds(0.5f);
#endif
      
  // Refresh UI
      Debug.Log("[SaveLoadUI] Refreshing UI...");
        GenerateSlots();
        
    isCreatingSave = false;
        Debug.Log($"[SaveLoadUI] Save creation complete for slot {slot}");
        
        // NEW: Show tutorial prompt for first-time players
        if (!TutorialPromptDialog.HasSeenTutorial())
     {
            Debug.Log("[SaveLoadUI] First save created - showing tutorial prompt");
            
            // CRITICAL: Ensure managers persist before showing tutorial prompt
            EnsureManagersPersist();
            
            // Wait a frame to ensure all Awake methods have been called
            yield return null;
            
            // Try to find TutorialPromptDialog if Instance is null or destroyed
            bool instanceValid = TutorialPromptDialog.Instance != null && TutorialPromptDialog.Instance.gameObject != null;
            
            if (!instanceValid)
            {
                Debug.Log("[SaveLoadUI] TutorialPromptDialog.Instance is null or destroyed, searching in scene...");
                TutorialPromptDialog dialog = FindObjectOfType<TutorialPromptDialog>();
                if (dialog != null)
                {
                    Debug.Log("[SaveLoadUI] Found TutorialPromptDialog in scene, waiting for initialization...");
                    // Wait one more frame to ensure Awake has been called
                    yield return null;
                    // Re-check if Instance is now valid
                    instanceValid = TutorialPromptDialog.Instance != null && TutorialPromptDialog.Instance.gameObject != null;
                }
                else
                {
                    Debug.LogError("[SaveLoadUI] TutorialPromptDialog component not found in scene! Make sure it's attached to a GameObject in the SaveLoadMenu scene.");
                }
            }
            
            if (instanceValid && TutorialPromptDialog.Instance != null)
         {
          Debug.Log("[SaveLoadUI] Showing tutorial prompt dialog");
          
          // Verify that the panel reference is assigned
          if (TutorialPromptDialog.Instance.tutorialPromptPanel == null)
          {
              Debug.LogError("[SaveLoadUI] TutorialPromptDialog.tutorialPromptPanel is not assigned in the Inspector! The tutorial prompt will not be visible.");
          }
          
          TutorialPromptDialog.Instance.ShowTutorialPrompt();
            }
 else
     {
 Debug.LogWarning("[SaveLoadUI] TutorialPromptDialog not available! Skipping to level select...");
       // Fallback: Go directly to level select
            NewAndLoadGameManager.Instance.LoadGame(slot);
          }
        }
        else
        {
       // Player has seen tutorial before, load game normally
   Debug.Log("[SaveLoadUI] Player has seen tutorial - loading game");
        NewAndLoadGameManager.Instance.LoadGame(slot);
        }
  }
    private void OnConfirmNo()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
        pendingSlot = 0;
    }

    private void OnBackClicked()
    {
        // Go back to main menu
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void RefreshSlots()
    {
        GenerateSlots();
    }
    
    /// <summary>
    /// Ensure critical managers persist before showing tutorial prompt
    /// </summary>
    private void EnsureManagersPersist()
    {
        Debug.Log("[SaveLoadUI] Ensuring managers persist before showing tutorial prompt...");
        
        // Ensure NewAndLoadGameManager persists
        if (NewAndLoadGameManager.Instance != null)
        {
            if (NewAndLoadGameManager.Instance.gameObject != null)
            {
                // Move to root if it's a child (children get destroyed with parent)
                if (NewAndLoadGameManager.Instance.transform.parent != null)
                {
                    Debug.LogWarning("[SaveLoadUI] NewAndLoadGameManager is a child object! Moving to root to prevent destruction.");
                    NewAndLoadGameManager.Instance.transform.SetParent(null);
                }
                DontDestroyOnLoad(NewAndLoadGameManager.Instance.gameObject);
                Debug.Log("[SaveLoadUI] NewAndLoadGameManager set to persist");
            }
            else
            {
                Debug.LogError("[SaveLoadUI] NewAndLoadGameManager.Instance.gameObject is null!");
            }
        }
        else
        {
            Debug.LogError("[SaveLoadUI] NewAndLoadGameManager.Instance is null! This will cause issues!");
        }
        
        // Ensure LoadingScreenManager persists
        if (LoadingScreenManager.Instance != null)
        {
            if (LoadingScreenManager.Instance.gameObject != null)
            {
                // Move to root if it's a child (children get destroyed with parent)
                if (LoadingScreenManager.Instance.transform.parent != null)
                {
                    Debug.LogWarning("[SaveLoadUI] LoadingScreenManager is a child object! Moving to root to prevent destruction.");
                    LoadingScreenManager.Instance.transform.SetParent(null);
                }
                DontDestroyOnLoad(LoadingScreenManager.Instance.gameObject);
                Debug.Log("[SaveLoadUI] LoadingScreenManager set to persist");
            }
            else
            {
                Debug.LogError("[SaveLoadUI] LoadingScreenManager.Instance.gameObject is null!");
            }
        }
        else
        {
            Debug.LogWarning("[SaveLoadUI] LoadingScreenManager.Instance is null!");
        }
        
        // Ensure PlayerDataManager persists
        if (PlayerDataManager.Instance != null)
        {
            if (PlayerDataManager.Instance.gameObject != null)
            {
                // Move to root if it's a child (children get destroyed with parent)
                if (PlayerDataManager.Instance.transform.parent != null)
                {
                    Debug.LogWarning("[SaveLoadUI] PlayerDataManager is a child object! Moving to root to prevent destruction.");
                    PlayerDataManager.Instance.transform.SetParent(null);
                }
                DontDestroyOnLoad(PlayerDataManager.Instance.gameObject);
                Debug.Log("[SaveLoadUI] PlayerDataManager set to persist");
            }
        }
    }
}
