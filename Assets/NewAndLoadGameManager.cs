using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewAndLoadGameManager : MonoBehaviour
{
    public static NewAndLoadGameManager Instance;
    
    private const int SlotCount = 3;
    private const string SaveKeyPrefix = "SaveSlot_";
    private const string CurrentSlotKey = "CurrentSlot";
    
    // Level progress tracking
    private const int FirstLevelIndex = 5; // Tutorial level (index 5)
    private const int LastLevelIndex = 25; // Part2Level10 (index 25) - Updated to include Part 2 levels
    
    public int CurrentSlot { get; private set; }
    private string currentUserId = "";
  
    [Header("Auto-Save Settings")]
    [Tooltip("Automatically save when application quits")]
    public bool autoSaveOnQuit = true;
    
    [Tooltip("Automatically save when application pauses (mobile/WebGL)")]
  public bool autoSaveOnPause = true;
    
    // Track save status
    private bool isSaveInProgress = false;
    // Reserved for future use
    #pragma warning disable CS0414
    private Coroutine currentSaveCoroutine = null;
    #pragma warning restore CS0414
    
    /// <summary>
    /// Returns true if a save operation is currently in progress
    /// </summary>
    public bool IsSaveInProgress => isSaveInProgress;

    [Serializable]
    public class GameData
    {
        public string username;
        public int levelsUnlocked;
        public int currentMoney;
        public string unlockedCosmetics;
        public string lastPlayed;
        public bool isEmpty;
        
        public GameData()
        {
       username = "";
       levelsUnlocked = 6; // FIXED: Start with Level 1 unlocked (scene index 6)
         currentMoney = 0;
            unlockedCosmetics = "[]";
     lastPlayed = "";
            isEmpty = true;
        }
    }

    [Serializable]
    public class SupabaseSaveData
    {
        public string user_id;
        public int slot_number;
        public string username;
        public int levels_unlocked;
        public int current_money;
        public string unlocked_cosmetics;
        public string last_played;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[NewAndLoadGameManager] Instance created and set to DontDestroyOnLoad");
        }
        else if (Instance != this)
        {
            Debug.LogWarning("[NewAndLoadGameManager] Duplicate instance detected, destroying duplicate");
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        // Prevent accidental destruction - only destroy if this is not the instance
        if (Instance == this)
        {
            Debug.LogWarning("[NewAndLoadGameManager] Instance is being destroyed! This should not happen with DontDestroyOnLoad.");
        }
    }
    
    private void OnEnable()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
  // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reload saves from Supabase whenever we return to the save menu
 if (scene.name == "SaveLoadMenu" || scene.name == "MainMenu")
  {
 Debug.Log($"[NewAndLoadGameManager] Scene {scene.name} loaded - reloading saves from Supabase");
            
#if UNITY_WEBGL && !UNITY_EDITOR
   LoadAllSavesFromSupabase();
            StartCoroutine(WaitForSupabaseLoad());
#endif
    }
    }

    private void Start()
    {
      LoadCurrentSlot();
     
#if UNITY_WEBGL && !UNITY_EDITOR
        // FOR DEPLOYMENT: Supabase enabled
        Debug.Log("[NewAndLoadGameManager] WebGL: Loading saves from Supabase (PlayerPrefs unreliable)...");
        LoadAllSavesFromSupabase();
        StartCoroutine(WaitForSupabaseLoad());
        
  // FOR LOCALHOST: Uncomment this line to disable Supabase
        // Debug.Log("[NewAndLoadGameManager] WebGL: LOCALHOST MODE - Using PlayerPrefs only (Supabase disabled)");
#else
      // Desktop uses PlayerPrefs only
   LoadAllSavesFromSupabase();
#endif
    }
    
    private void OnApplicationQuit()
    {
     if (autoSaveOnQuit && CurrentSlot > 0)
        {
      Debug.Log("[NewAndLoadGameManager] Application quitting - auto-saving...");
         SaveToCurrentSlot();
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
  {
     // When app is paused (going to background on mobile/WebGL)
        if (pauseStatus && autoSaveOnPause && CurrentSlot > 0)
 {
     Debug.Log("[NewAndLoadGameManager] Application paused - auto-saving...");
  SaveToCurrentSlot();
 }
    }

    public void SetUserId(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("[NewAndLoadGameManager] SetUserId called with empty userId!");
            return;
        }
        
        if (currentUserId != userId)
        {
            Debug.Log($"[NewAndLoadGameManager] User ID changed from '{currentUserId}' to '{userId}'");
            currentUserId = userId;
            LoadCurrentSlot();
   
            // Log all existing save slots for this user
            Debug.Log($"[NewAndLoadGameManager] Loading save slots for user: {userId}");
            for (int i = 1; i <= SlotCount; i++)
            {
                String key = GetSlotKey(i);
                if (PlayerPrefs.HasKey(key))
                {
                    Debug.Log($"[NewAndLoadGameManager] ? Found save in slot {i} with key: {key}");
                }
            }
        }
        else
        {
            Debug.Log($"[NewAndLoadGameManager] User ID unchanged: {userId}");
        }
    }

    private void LoadCurrentSlot()
    {
        string key = GetUserSpecificKey(CurrentSlotKey);
        int loadedSlot = PlayerPrefs.GetInt(key, 0);
        CurrentSlot = loadedSlot;
        Debug.Log($"[NewAndLoadGameManager] Loaded current slot: {CurrentSlot} (key: {key})");
    }

    private string GetUserSpecificKey(string baseKey)
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            return baseKey;
        }
        return $"{currentUserId}_{baseKey}";
    }

    private string GetSlotKey(int slot)
    {
        return GetUserSpecificKey(SaveKeyPrefix + slot);
    }

    private string GetRewardLockKey(int slot, int levelIndex)
    {
        return GetUserSpecificKey($"rewardLock_Slot{slot}_Level_{levelIndex}");
    }

    public void NewGame(int slot)
    {
        if (!IsValidSlot(slot))
        {
            return;
        }

        string username = GetCurrentUsername();

        GameData data = new GameData
     {
      username = username,
            levelsUnlocked = 6, // FIXED: Start with Level 1 unlocked (scene index 6)
            currentMoney = 0,
            unlockedCosmetics = "[]",
        lastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
    isEmpty = false
        };

        ClearSlotRewardLocks(slot);
        
        SaveToSlot(slot, data);
        CurrentSlot = slot;
        string currentSlotKey = GetUserSpecificKey(CurrentSlotKey);
      PlayerPrefs.SetInt(currentSlotKey, slot);
        
        ApplyDataToPlayerPrefs(data);
        
        SaveSlotToSupabase(slot, data);
        
     Debug.Log($"[NewAndLoadGameManager] NewGame created for slot {slot} with Level 1 unlocked (levelAt=6)");
    }

    public void LoadGame(int slot)
    {
   if (!IsValidSlot(slot))
      {
      return;
        }

      GameData data = LoadFromSlot(slot);

     if (data != null && !data.isEmpty)
        {
            CurrentSlot = slot;
         string currentSlotKey = GetUserSpecificKey(CurrentSlotKey);
    PlayerPrefs.SetInt(currentSlotKey, slot);
  
         ApplyDataToPlayerPrefs(data);
  
            // Also update PlayerDataManager if available
     if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
   {
     var playerData = PlayerDataManager.Instance.GetCurrentPlayerData();
      playerData.levels_unlocked = data.levelsUnlocked;
          playerData.current_money = data.currentMoney;
         playerData.unlocked_cosmetics = data.unlockedCosmetics;
  
         Debug.Log($"[NewAndLoadGameManager] Updated PlayerDataManager with save data: Level={data.levelsUnlocked}, Cosmetics={data.unlockedCosmetics}");
    }
  
       // CRITICAL FIX: Wait for PlayerPrefs to save before loading scene
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("[NewAndLoadGameManager] WebGL: Waiting for PlayerPrefs to save before loading scene...");
      StartCoroutine(LoadGameSceneAfterDelay());
#else
        // Show loading screen if available
        if (LoadingScreenManager.Instance != null)
        {
            StartCoroutine(LoadGameSceneWithLoading());
        }
        else
        {
            LoadGameScene();
        }
#endif
      }
    }
    
#if UNITY_WEBGL && !UNITY_EDITOR
 /// <summary>
    /// Wait for PlayerPrefs to save before loading the game scene (WebGL fix)
 /// </summary>
    private IEnumerator LoadGameSceneAfterDelay()
    {
        // CRITICAL FIX: Wait for any in-progress saves to complete first!
        if (isSaveInProgress)
        {
            Debug.LogWarning("[NewAndLoadGameManager] Save in progress detected! Waiting for completion before loading scene...");
   yield return StartCoroutine(WaitForSaveCompletion());
Debug.Log("[NewAndLoadGameManager] Save completed, proceeding with scene load");
        }
        
 // Force multiple save cycles
        for (int i = 0; i < 3; i++)
  {
 PlayerPrefs.Save();
          yield return new WaitForSeconds(0.2f);
     }
    
        // Verify data is actually saved
        int levelAt = PlayerPrefs.GetInt("levelAt", 5);
     Debug.Log($"[NewAndLoadGameManager] PlayerPrefs verification - levelAt: {levelAt}");
        
    yield return new WaitForSeconds(0.5f);
   
        Debug.Log("[NewAndLoadGameManager] Loading LevelSelect scene now");
        
        // Show loading screen if available
        if (LoadingScreenManager.Instance != null)
        {
            StartCoroutine(LoadGameSceneWithLoading());
        }
        else
        {
            LoadGameScene();
        }
    }
#endif

  private GameData LoadFromSlot(int slot)
 {
        string key = GetSlotKey(slot);
  if (PlayerPrefs.HasKey(key))
        {
string json = PlayerPrefs.GetString(key);
       try
          {
return JsonUtility.FromJson<GameData>(json);
  }
      catch (Exception ex)
       {
    Debug.LogError($"Error loading slot {slot}: {ex.Message}");
      return null;
       }
     }
  
 // No data found in PlayerPrefs
        Debug.Log($"[NewAndLoadGameManager] No data found for slot {slot} in PlayerPrefs");
        return null;
    }

    private void LoadGameScene()
    {
        // Show loading screen if available
        if (LoadingScreenManager.Instance != null)
        {
            StartCoroutine(LoadGameSceneWithLoading());
        }
        else
        {
            SceneManager.LoadScene("LevelSelect");
        }
    }
    
    private IEnumerator LoadGameSceneWithLoading()
    {
        // Check if LoadingScreenManager still exists
        if (LoadingScreenManager.Instance == null)
        {
            SceneManager.LoadScene("LevelSelect");
            yield break;
        }
        
        // Show loading screen
        LoadingScreenManager.Instance.ShowLoadingScreen("Loading...");
        
        // Wait for minimum loading duration
        float minDuration = LoadingScreenManager.Instance != null ? LoadingScreenManager.Instance.minLoadingDuration : 1.5f;
        yield return new WaitForSeconds(minDuration);
        
        // Load scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LevelSelect");
        asyncLoad.allowSceneActivation = false;
        
        // Wait until scene is loaded
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        // Wait a bit more for smooth transition
        yield return new WaitForSeconds(0.2f);
        
        // Activate scene
        asyncLoad.allowSceneActivation = true;
        
        // Wait for scene to fully activate
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Wait a moment for scene to initialize
        yield return new WaitForSeconds(0.1f);
        
        // Hide loading screen after scene is loaded (fade out from black)
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.HideLoadingScreen();
        }
    }

    public void AutoSave()
    {
        if (CurrentSlot > 0)
    {
   // Show saving indicator if available
   if (SaveStatusIndicator.Instance != null)
{
       SaveStatusIndicator.Instance.ShowSaving("Saving game...");
      }
            
  SaveToCurrentSlot();
 }
 else
  {
  Debug.LogWarning("[NewAndLoadGameManager] AutoSave called but no active slot!");
  
   // Show error if available
    if (SaveStatusIndicator.Instance != null)
   {
      SaveStatusIndicator.Instance.ShowError("No active save slot!");
   }
  }
 }
    
    /// <summary>
    /// Wait until the current save operation completes (useful for WebGL before scene transitions)
    /// </summary>
    public IEnumerator WaitForSaveCompletion()
    {
        if (!isSaveInProgress)
   {
            Debug.Log("[NewAndLoadGameManager] No save in progress, continuing immediately");
yield break;
  }
        
  Debug.Log("[NewAndLoadGameManager] Waiting for save to complete...");
        float timeout = 30f; // Increased timeout for WebGL save verification (was 15f)
   float elapsed = 0f;
   float logInterval = 1f; // Log every second
    float lastLogTime = 0f;
    
     while (isSaveInProgress && elapsed < timeout)
   {
    yield return null;
      elapsed += Time.unscaledDeltaTime;
 
    // Log progress every second
         if (elapsed - lastLogTime >= logInterval)
   {
      Debug.Log($"[NewAndLoadGameManager] Still waiting for save... ({elapsed:F1}s / {timeout:F0}s)");
        lastLogTime = elapsed;
         }
     }
        
  if (isSaveInProgress)
   {
     Debug.LogWarning($"[NewAndLoadGameManager] Save operation timeout after {timeout}s! Proceeding anyway...");
    Debug.LogWarning("[NewAndLoadGameManager] Data may not have fully persisted to IndexedDB!");
        }
        else
        {
  Debug.Log($"[NewAndLoadGameManager] Save completed successfully after {elapsed:F2}s");
   }
    }

    public void ClearAllSlots()
    {
    for (int i = 1; i <= SlotCount; i++)
  {
         string key = GetSlotKey(i);
          if (PlayerPrefs.HasKey(key))
   {
      PlayerPrefs.DeleteKey(key);
       }
       ClearSlotRewardLocks(i);
        }
        
  string currentSlotKey = GetUserSpecificKey(CurrentSlotKey);
   PlayerPrefs.DeleteKey(currentSlotKey);
        CurrentSlot = 0;
      PlayerPrefs.Save();
    }

    public void SaveToCurrentSlot()
  {
        if (CurrentSlot == 0)
        {
            Debug.LogWarning("[NewAndLoadGameManager] SaveToCurrentSlot called but CurrentSlot is 0! No active slot to save to.");
  return;
   }

      Debug.Log($"[NewAndLoadGameManager] ??? SAVE START ???");
      Debug.Log($"[NewAndLoadGameManager] SaveToCurrentSlot - Slot: {CurrentSlot}, User: {currentUserId}");
      Debug.Log($"[NewAndLoadGameManager] isSaveInProgress: {isSaveInProgress}");

  // Get current unlocked cosmetics from PlayerDataManager if available
        string unlockedCosmetics = "[]";
   if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
        {
unlockedCosmetics = PlayerDataManager.Instance.GetCurrentPlayerData().unlocked_cosmetics;
 Debug.Log($"[NewAndLoadGameManager] ? Using unlocked cosmetics from PlayerDataManager: {unlockedCosmetics}");
  }
   else
  {
  // Fallback to PlayerPrefs
   unlockedCosmetics = PlayerPrefs.GetString("unlockedCosmetics", "[]");
  Debug.LogWarning($"[NewAndLoadGameManager] ?? PlayerDataManager unavailable! Using PlayerPrefs fallback: {unlockedCosmetics}");
       Debug.LogWarning("[NewAndLoadGameManager] ?? This may not reflect latest unlocked cosmetics!");
 }

        GameData data = new GameData
     {
     username = PlayerPrefs.GetString("username", "Player"),
levelsUnlocked = PlayerPrefs.GetInt("levelAt", 5),
       currentMoney = PlayerPrefs.GetInt("moneyCount", 0),
unlockedCosmetics = unlockedCosmetics,
  lastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
   isEmpty = false
};

        Debug.Log($"[NewAndLoadGameManager] Saving: Level={data.levelsUnlocked}, Money={data.currentMoney}, Cosmetics={data.unlockedCosmetics}");
      string key = GetSlotKey(CurrentSlot);
Debug.Log($"[NewAndLoadGameManager] Save key: {key}");

SaveToSlot(CurrentSlot, data);
        SaveSlotToSupabase(CurrentSlot, data);
        
        Debug.Log($"[NewAndLoadGameManager] ??? SAVE END ???");
  }

    public void DeleteSlot(int slot)
    {
        if (!IsValidSlot(slot))
        {
            return;
        }

        string key = GetSlotKey(slot);
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            ClearSlotRewardLocks(slot);
            PlayerPrefs.Save();
            DeleteSlotFromSupabase(slot);
        }
    }

    /// <summary>
    /// Manually retry saving to current slot (useful if save failed)
    /// </summary>
    public void RetryCurrentSlotSave()
    {
        if (CurrentSlot == 0)
    {
        Debug.LogWarning("[NewAndLoadGameManager] No active slot to retry!");
     return;
        }
     
        Debug.Log($"[NewAndLoadGameManager] Manually retrying save for slot {CurrentSlot}...");
        SaveToCurrentSlot();
    }
    
    /// <summary>
    /// Force reload saves from Supabase (useful if local saves are corrupted)
  /// </summary>
    public void ForceReloadFromSupabase()
    {
        Debug.Log("[NewAndLoadGameManager] Force reloading saves from Supabase...");
        LoadAllSavesFromSupabase();
    
#if UNITY_WEBGL && !UNITY_EDITOR
        StartCoroutine(WaitForSupabaseLoad());
#endif
    }

    private void ClearSlotRewardLocks(int slot)
    {
        List<string> keysToDelete = new List<string>();
        
        for (int levelIndex = 0; levelIndex < 100; levelIndex++)
        {
            string rewardLockKey = GetRewardLockKey(slot, levelIndex);
            if (PlayerPrefs.HasKey(rewardLockKey))
            {
                keysToDelete.Add(rewardLockKey);
            }
        }
        
        foreach (string keyToDelete in keysToDelete)
        {
            PlayerPrefs.DeleteKey(keyToDelete);
        }
    }

    public List<GameData> GetAllSlots()
    {
        Debug.Log($"[NewAndLoadGameManager] GetAllSlots called for user: {currentUserId}");
        List<GameData> slots = new List<GameData>();
        
        for (int i = 1; i <= SlotCount; i++)
        {
            GameData data = GetSlotData(i);
            string status = data != null && !data.isEmpty ? $"HAS DATA (Level: {data.levelsUnlocked}, Money: {data.currentMoney})" : "EMPTY";
            Debug.Log($"[NewAndLoadGameManager] Slot {i}: {status}");
            slots.Add(data);
        }
        
        return slots;
    }

    public GameData GetSlotData(int slot)
    {
        if (!IsValidSlot(slot))
        {
            return null;
        }

        GameData data = LoadFromSlot(slot);
        GameData result = data ?? new GameData { isEmpty = true };
        
        string key = GetSlotKey(slot);
        Debug.Log($"[NewAndLoadGameManager] GetSlotData({slot}) - Key: {key}, Has Data: {data != null}");
        
        return result;
    }

    public bool IsSlotEmpty(int slot)
    {
        if (!IsValidSlot(slot))
        {
            return true;
        }

        GameData data = LoadFromSlot(slot);
        return data == null || data.isEmpty;
    }
    
    /// <summary>
    /// Calculate progress percentage based on levels unlocked
    /// Returns a value from 0 to 100 representing completion percentage
    /// Progress is calculated from first level (index 5 - Tutorial) to last level (index 25 - Part2Level10)
    /// Includes: Tutorial (5), Part1Level1-10 (6-15), Part2Level1-10 (16-25) = 21 total levels
    /// Note: Having the first level unlocked still counts as 0% (no levels completed yet)
    /// </summary>
    public float CalculateProgressPercentage(int levelsUnlocked)
    {
        // Total number of levels (from index 5 to 25, inclusive)
        // Tutorial (5) + Part1Level1-10 (6-15) + Part2Level1-10 (16-25) = 21 levels total
        int totalLevels = LastLevelIndex - FirstLevelIndex + 1;
        
        // Calculate how many levels have been completed
        // levelsUnlocked represents the highest unlocked level index
        // If levelsUnlocked = 5, first level is unlocked but not completed → 0%
        // If levelsUnlocked = 6, level 1 is unlocked, meaning first level is completed → 1 completed
        // If levelsUnlocked = 7, level 2 is unlocked, meaning 2 levels completed
        // So completed levels = levelsUnlocked - FirstLevelIndex - 1
        // We subtract 1 because unlocking a level means you completed the previous one
        
        int completedLevels = Mathf.Max(0, levelsUnlocked - FirstLevelIndex - 1);
        
        // Clamp to valid range (0 to totalLevels)
        completedLevels = Mathf.Clamp(completedLevels, 0, totalLevels);
        
        // Calculate percentage
        float percentage = (float)completedLevels / totalLevels * 100f;
        
        // Clamp between 0 and 100
        return Mathf.Clamp(percentage, 0f, 100f);
    }
    
    /// <summary>
    /// Get progress percentage for a specific slot
    /// </summary>
    public float GetSlotProgressPercentage(int slot)
    {
        if (IsSlotEmpty(slot))
        {
            return 0f;
        }
        
        GameData data = GetSlotData(slot);
        if (data == null || data.isEmpty)
        {
            return 0f;
        }
        
        return CalculateProgressPercentage(data.levelsUnlocked);
    }

    private void LoadAllSavesFromSupabase()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
      // FOR DEPLOYMENT: Supabase enabled
LoadAllSaveSlotsJS();
        
        // FOR LOCALHOST: Uncomment this line to disable
 // Debug.Log("[NewAndLoadGameManager] WebGL: LoadAllSavesFromSupabase() disabled for localhost testing");
#endif
    }
    
#if UNITY_WEBGL && !UNITY_EDITOR
    private System.Collections.IEnumerator WaitForSupabaseLoad()
    {
Debug.Log("[NewAndLoadGameManager] WebGL: Waiting for Supabase to load saves...");
        
 // Wait for Supabase to fetch data (it stores it in JavaScript)
 yield return new WaitForSeconds(2.0f);
    
        // Now try to retrieve the stored data
        int maxAttempts = 50;
     int attempt = 0;
  bool dataRetrieved = false;
 
  while (attempt < maxAttempts && !dataRetrieved)
 {
  attempt++;
   
      // Check if data is available
      if (HasPendingSaveData())
    {
    Debug.Log($"[NewAndLoadGameManager] WebGL: Save data available, retrieving... (attempt {attempt})");
      
    // Get the data
       string jsonData = GetPendingSaveData();
    
     if (!string.IsNullOrEmpty(jsonData) && jsonData != "[]")
       {
    Debug.Log($"[NewAndLoadGameManager] WebGL: Retrieved {jsonData.Length} chars of save data");
         OnAllSaveSlotsLoaded(jsonData);
    dataRetrieved = true;
     } else {
    Debug.Log("[NewAndLoadGameManager] WebGL: No saves found from Supabase (empty array)");
      dataRetrieved = true;
  }
          } else {
 Debug.Log($"[NewAndLoadGameManager] WebGL: No save data available yet (attempt {attempt}/{maxAttempts})");
yield return new WaitForSeconds(0.1f);
    }
    }
    
        if (!dataRetrieved)
    {
   Debug.LogWarning("[NewAndLoadGameManager] WebGL: Timed out waiting for save data");
        }
  }
#endif

    private void SaveSlotToSupabase(int slot, GameData data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // FOR DEPLOYMENT: Supabase enabled
        Debug.Log($"[NewAndLoadGameManager] WebGL: Attempting Supabase save for slot {slot}...");
  SaveSlotToSupabaseJS(slot, data.username, data.levelsUnlocked, data.currentMoney, data.unlockedCosmetics);
        Debug.Log($"[NewAndLoadGameManager] WebGL: Supabase save call completed for slot {slot}");
        
   // FOR LOCALHOST: Uncomment this line to disable
        // Debug.Log($"[NewAndLoadGameManager] WebGL: SaveSlotToSupabase({slot}) disabled for localhost testing");
#else
        Debug.Log($"[NewAndLoadGameManager] Desktop: Skipping Supabase save (not WebGL build)");
#endif
    }

    private void DeleteSlotFromSupabase(int slot)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
  // FOR DEPLOYMENT: Supabase enabled
        DeleteSlotFromSupabaseJS(slot);
        
  // FOR LOCALHOST: Uncomment this line to disable
        // Debug.Log($"[NewAndLoadGameManager] WebGL: DeleteSlotFromSupabaseJS({slot}) disabled for localhost testing");
#endif
    }

    public void OnAllSaveSlotsLoaded(string jsonArray)
    {
        try
        {
          string dataInfo = string.IsNullOrEmpty(jsonArray) ? "empty" : jsonArray.Length + " chars";
     Debug.Log($"[NewAndLoadGameManager] OnAllSaveSlotsLoaded called with data: {dataInfo}");
 
     if (string.IsNullOrEmpty(jsonArray) || jsonArray == "[]")
            {
   Debug.Log("[NewAndLoadGameManager] No cloud saves found");
   return;
    }

SupabaseSaveData[] cloudSaves = JsonHelper.FromJson<SupabaseSaveData>(jsonArray);

        if (cloudSaves == null || cloudSaves.Length == 0)
      {
   Debug.Log("[NewAndLoadGameManager] Cloud saves array is empty");
      return;
      }

            Debug.Log($"[NewAndLoadGameManager] Processing {cloudSaves.Length} cloud saves...");

     foreach (var cloudSave in cloudSaves)
      {
        GameData localData = LoadFromSlot(cloudSave.slot_number);
   
#if UNITY_WEBGL && !UNITY_EDITOR
  // In WebGL, ALWAYS overwrite local with cloud data (PlayerPrefs unreliable)
    Debug.Log($"[NewAndLoadGameManager] WebGL: Overwriting slot {cloudSave.slot_number} with Supabase data");
#else
         // On desktop, only overwrite if local is empty
      if (localData != null && !localData.isEmpty)
 {
        Debug.Log($"[NewAndLoadGameManager] Slot {cloudSave.slot_number} already has local data, skipping");
            continue;
       }
#endif
   
      GameData data = new GameData
      {
        username = cloudSave.username,
         levelsUnlocked = cloudSave.levels_unlocked,
    currentMoney = cloudSave.current_money,
unlockedCosmetics = cloudSave.unlocked_cosmetics,
      lastPlayed = cloudSave.last_played,
   isEmpty = false
    };
     
            SaveToSlot(cloudSave.slot_number, data);
     Debug.Log($"[NewAndLoadGameManager] ? Loaded slot {cloudSave.slot_number} from Supabase: Level={data.levelsUnlocked}, Money={data.currentMoney}, Cosmetics={data.unlockedCosmetics}");
  
      // Update PlayerDataManager if this is the current slot
   if (cloudSave.slot_number == CurrentSlot && PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
      {
    var playerData = PlayerDataManager.Instance.GetCurrentPlayerData();
        playerData.unlocked_cosmetics = data.unlockedCosmetics;
    Debug.Log($"[NewAndLoadGameManager] Updated PlayerDataManager cosmetics from slot {cloudSave.slot_number}");
    }
 }
   
     var saveLoadUI = FindObjectOfType<SaveLoadUI>();
   if (saveLoadUI != null)
      {
     Debug.Log("[NewAndLoadGameManager] Refreshing SaveLoadUI after Supabase load");
     saveLoadUI.RefreshSlots();
 }
  }
    catch (Exception ex)
     {
 Debug.LogError($"[NewAndLoadGameManager] Error in OnAllSaveSlotsLoaded: {ex.Message}");
   }
    }

    public void OnSaveSlotSaved(string jsonData)
{
     Debug.Log($"[NewAndLoadGameManager] ? Supabase save successful: {jsonData}");
}

    public void OnSaveSlotDeleted(string slotNumber)
    {
        Debug.Log($"[NewAndLoadGameManager] ? Supabase delete successful for slot: {slotNumber}");
    }

    public void OnSaveSlotError(string errorMessage)
    {
  Debug.LogError($"[NewAndLoadGameManager] ? Supabase error: {errorMessage}");
    }
    
    /// <summary>
  /// Called when all Supabase saves are deleted (developer tools)
    /// </summary>
    public void OnAllSavesDeleted(string deletedCount)
  {
        Debug.Log($"[NewAndLoadGameManager] ? All Supabase saves deleted: {deletedCount} save(s)");
 
 // Refresh UI if it exists
 var saveLoadUI = FindObjectOfType<SaveLoadUI>();
        if (saveLoadUI != null)
        {
Debug.Log("[NewAndLoadGameManager] Refreshing SaveLoadUI after delete all");
    saveLoadUI.RefreshSlots();
 }
    }

    private string GetCurrentUsername()
    {
        string username = "Player";
        if (PlayerDataManager.Instance != null)
        {
            username = PlayerDataManager.Instance.GetUsername();
            if (string.IsNullOrEmpty(username))
            {
                username = "Player";
            }
        }
        return username;
    }

    private void ApplyDataToPlayerPrefs(GameData data)
    {
        Debug.Log($"[NewAndLoadGameManager] Applying save data to PlayerPrefs...");
        Debug.Log($"[NewAndLoadGameManager] Level={data.levelsUnlocked}, Money={data.currentMoney}, Cosmetics={data.unlockedCosmetics}");
        
        PlayerPrefs.SetString("username", data.username);
        PlayerPrefs.SetInt("levelAt", data.levelsUnlocked);
        PlayerPrefs.SetInt("moneyCount", data.currentMoney);
        PlayerPrefs.SetString("unlockedCosmetics", data.unlockedCosmetics);
        
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: Save multiple times to ensure IndexedDB persistence
        for (int i = 0; i < 3; i++)
        {
            PlayerPrefs.Save();
        }
        Debug.Log("[NewAndLoadGameManager] WebGL: Saved PlayerPrefs 3 times for persistence");
#else
        PlayerPrefs.Save();
#endif
    
      // Verify the data was saved
        int savedLevel = PlayerPrefs.GetInt("levelAt", 0);
        int savedMoney = PlayerPrefs.GetInt("moneyCount", 0);
        Debug.Log($"[NewAndLoadGameManager] Verification - Saved levelAt: {savedLevel}, moneyCount: {savedMoney}");
 
        if (savedLevel != data.levelsUnlocked)
        {
 Debug.LogWarning($"[NewAndLoadGameManager] ?? levelAt mismatch! Expected: {data.levelsUnlocked}, Got: {savedLevel}");
      }
    }

    private bool IsValidSlot(int slot)
    {
        return slot >= 1 && slot <= SlotCount;
    }

    private void SaveToSlot(int slot, GameData data)
    {
        string json = JsonUtility.ToJson(data);
        string key = GetSlotKey(slot);
        
#if UNITY_WEBGL && !UNITY_EDITOR
   // WebGL: Aggressive multi-save approach to ensure IndexedDB persistence
  Debug.Log($"[NewAndLoadGameManager] WebGL: Saving to slot {slot} with key: {key}");
  
     // Set save in progress flag
isSaveInProgress = true;
        
        // Write data multiple times immediately
        for (int i = 0; i < 3; i++)
     {
      PlayerPrefs.SetString(key, json);
       PlayerPrefs.Save();
   }
        
   Debug.Log($"[NewAndLoadGameManager] WebGL: Initial save complete, starting verification...");
        
   // Stop any existing save coroutine
   if (currentSaveCoroutine != null)
   {
            StopCoroutine(currentSaveCoroutine);
        }
        
        currentSaveCoroutine = StartCoroutine(VerifySaveAfterDelay(slot, key, json));
#else
      // Desktop: Single save is reliable
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
        Debug.Log($"[NewAndLoadGameManager] ? Saved to slot {slot} with key: {key}");
#endif
    }
    
#if UNITY_WEBGL && !UNITY_EDITOR
    private IEnumerator VerifySaveAfterDelay(int slot, string key, string expectedJson)
    {
   // WebGL requires multiple save cycles to ensure IndexedDB persistence
        Debug.Log($"[NewAndLoadGameManager] WebGL: Verifying save for slot {slot}...");
        
        // Force multiple save cycles with longer delays
        for (int i = 0; i < 5; i++)
    {
            PlayerPrefs.Save();
            yield return new WaitForSeconds(0.3f);
        }

        // Wait a bit more for IndexedDB to flush
        yield return new WaitForSeconds(0.5f);

    // Verify the save persisted
        bool saveSuccessful = false;
    int retryCount = 0;
    int maxRetries = 3;
        
        while (!saveSuccessful && retryCount < maxRetries)
    {
  if (PlayerPrefs.HasKey(key))
       {
          string savedJson = PlayerPrefs.GetString(key);
     if (savedJson == expectedJson)
      {
   Debug.Log($"[NewAndLoadGameManager] ? WebGL save verified for slot {slot} (attempt {retryCount + 1})");
   saveSuccessful = true;
        }
     else
          {
        Debug.LogWarning($"[NewAndLoadGameManager] ? WebGL save mismatch for slot {slot} (attempt {retryCount + 1})! Retrying...");
      PlayerPrefs.SetString(key, expectedJson);
        
       // Multiple aggressive save attempts
   for (int i = 0; i < 3; i++)
 {
        PlayerPrefs.Save();
              yield return new WaitForSeconds(0.2f);
  }
       
        retryCount++;
      yield return new WaitForSeconds(0.5f);
           }
         }
   else
 {
       Debug.LogError($"[NewAndLoadGameManager] ? WebGL save FAILED for slot {slot} (attempt {retryCount + 1})! Key not found. Retrying...");
                
  // Re-write the data
     PlayerPrefs.SetString(key, expectedJson);
      
     // Multiple aggressive save attempts
   for (int i = 0; i < 3; i++)
  {
     PlayerPrefs.Save();
    yield return new WaitForSeconds(0.2f);
}
   
       retryCount++;
                yield return new WaitForSeconds(0.5f);
            }
        }
        
      // Final verification
        if (!saveSuccessful)
     {
       Debug.LogError($"[NewAndLoadGameManager] ??? CRITICAL: WebGL save FAILED for slot {slot} after {maxRetries} retries!");
            Debug.LogError($"[NewAndLoadGameManager] Data may not persist! Consider re-saving or using Supabase.");
            
        // Try one last time with maximum persistence
    PlayerPrefs.SetString(key, expectedJson);
for (int i = 0; i < 5; i++)
       {
         PlayerPrefs.Save();
        yield return new WaitForSeconds(0.3f);
}
            
 // Show error if available
 if (SaveStatusIndicator.Instance != null)
{
   SaveStatusIndicator.Instance.ShowError("Save may have failed!");
   }
      }
   else
        {
 // Show success if available
   if (SaveStatusIndicator.Instance != null)
   {
       SaveStatusIndicator.Instance.ShowSuccess("Game saved!");
   }
     }
   
        // Clear save in progress flag
   isSaveInProgress = false;
        currentSaveCoroutine = null;
        Debug.Log($"[NewAndLoadGameManager] Save operation completed for slot {slot}");
    }
     private System.Collections.IEnumerator SyncPlayerPrefsFromIndexedDB()
    {
      // Force Unity to READ PlayerPrefs FROM IndexedDB
        // In WebGL, PlayerPrefs cache can get out of sync with IndexedDB
 
    Debug.Log("[NewAndLoadGameManager] WebGL: Calling ForcePlayerPrefsSyncJS()...");
      ForcePlayerPrefsSyncJS();
    
        Debug.Log("[NewAndLoadGameManager] WebGL: Forcing PlayerPrefs.Save() to sync cache...");
        // Force multiple save/loading cycles to ensure IndexedDB is synced
        for (int i = 0; i < 3; i++)
 {
       PlayerPrefs.Save();
       yield return new WaitForSeconds(0.3f);
        }
        
    Debug.Log($"[NewAndLoadGameManager] WebGL: Checking saves for user: {currentUserId}");
        int foundCount = 0;
 for (int i = 1; i <= SlotCount; i++)
        {
       string key = GetSlotKey(i);
       if (PlayerPrefs.HasKey(key))
            {
    string json = PlayerPrefs.GetString(key);
         Debug.Log($"[NewAndLoadGameManager] WebGL: ? Found save in slot {i}, data length: {json.Length}");
      foundCount++;
            }
   }
     
if (foundCount == 0)
        {
            Debug.LogWarning("[NewAndLoadGameManager] WebGL: No saves found after IndexedDB sync!");
     Debug.LogWarning("[NewAndLoadGameManager] WebGL: Attempting fallback - loading from Supabase...");
}
        else
        {
  Debug.Log($"[NewAndLoadGameManager] WebGL: ? Successfully loaded {foundCount} saves from IndexedDB");
    }
        
  LoadAllSavesFromSupabase();
    }

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void LoadAllSaveSlotsJS();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SaveSlotToSupabaseJS(int slotNumber, string username, int levelsUnlocked, int currentMoney, string unlockedCosmetics);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void DeleteSlotFromSupabaseJS(int slotNumber);
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void ForcePlayerPrefsSyncJS();
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern bool HasPendingSaveData();
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern string GetPendingSaveData();
#endif
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
     string wrappedJson = "{\"items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
      return wrapper.items;
    }

 [Serializable]
    private class Wrapper<T>
  {
        public T[] items;
  }
}
