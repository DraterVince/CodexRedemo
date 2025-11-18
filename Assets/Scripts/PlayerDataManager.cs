using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

[System.Serializable]
public class PlayerDataSimple
{
    public int id;
    public string user_id;
    public string email;
    public string username;
    public int levels_unlocked;
    public int current_money;
    public string unlocked_cosmetics;
    public string created_at;
    public string updated_at;
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;
    
    private PlayerData currentPlayerData;
    
    private Supabase.Client GetSupabaseClient()
    {
        var authManagerObj = GameObject.Find("AuthManager");
        if (authManagerObj != null)
        {
            var authManager = authManagerObj.GetComponent(System.Type.GetType("AuthManager"));
            if (authManager != null)
            {
                var supabaseProperty = authManager.GetType().GetProperty("Supabase");
                if (supabaseProperty != null)
                {
                    return supabaseProperty.GetValue(authManager) as Supabase.Client;
                }
            }
        }
        return null;
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Ensure GameObject is root before calling DontDestroyOnLoad
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // If not root, move to root first
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (Instance != this)
        {
            Debug.LogWarning("[PlayerDataManager] Duplicate instance detected, destroying duplicate");
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        // Only warn if this is actually the instance being destroyed
        // (not just a duplicate that was destroyed)
        if (Instance == this)
        {
            Debug.LogWarning("[PlayerDataManager] Instance is being destroyed! This should not happen with DontDestroyOnLoad. This may indicate the GameObject was destroyed manually or the scene was unloaded incorrectly.");
            Instance = null; // Clear instance reference
        }
    }

    public async Task<bool> CreatePlayerData(string userId, string email, string username)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        try
        {
        var playerData = new PlayerData
     {
   user_id = userId,
         email = email,
      username = username,
  levels_unlocked = 6, // FIXED: Start with Level 1 unlocked (scene index 6)
         current_money = 0,
        unlocked_cosmetics = "[]"
            };

      var supabaseClient = GetSupabaseClient();
            if (supabaseClient != null)
     {
         var response = await supabaseClient
 .From<PlayerData>()
    .Insert(playerData);
            }

    currentPlayerData = playerData;
            SaveToPlayerPrefs(playerData);
    
  if (NewAndLoadGameManager.Instance != null)
        {
           NewAndLoadGameManager.Instance.SetUserId(userId);
   }
        
return true;
        }
        catch (Exception)
        {
     return false;
  }
#else
        createPlayerData(userId, email, username);
        
        var playerData = new PlayerData
        {
  user_id = userId,
            email = email,
            username = username,
          levels_unlocked = 6, // FIXED: Start with Level 1 unlocked (scene index 6)
current_money = 0,
         unlocked_cosmetics = "[]"
        };
    currentPlayerData = playerData;
        SaveToPlayerPrefs(playerData);
 
      if (NewAndLoadGameManager.Instance != null)
        {
          NewAndLoadGameManager.Instance.SetUserId(userId);
 }
        
        return true;
#endif
 }

    public async Task<PlayerData> LoadPlayerData(string userId)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        try
        {
            var supabaseClient = GetSupabaseClient();
            if (supabaseClient != null)
            {
                var response = await supabaseClient
                    .From<PlayerData>()
                    .Where(x => x.user_id == userId)
                    .Single();

                if (response != null)
                {
                    currentPlayerData = response;
                    SaveToPlayerPrefs(response);
                    
                    if (NewAndLoadGameManager.Instance != null)
                    {
                        NewAndLoadGameManager.Instance.SetUserId(userId);
                    }
                
                    return response;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
          catch (Exception)
        {
            return null;
        }
#else
        loadPlayerData(userId);
        
        if (NewAndLoadGameManager.Instance != null)
        {
            NewAndLoadGameManager.Instance.SetUserId(userId);
        }
        
        return null;
#endif
    }

    public async Task<bool> UpdatePlayerData(PlayerData data)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        try
        {
            data.updated_at = DateTime.UtcNow.ToString("o");
            
            var supabaseClient = GetSupabaseClient();
            if (supabaseClient != null)
            {
                await supabaseClient
                    .From<PlayerData>()
                    .Where(x => x.user_id == data.user_id)
                    .Update(data);
            }

            currentPlayerData = data;
            SaveToPlayerPrefs(data);
            
            return true;
        }
          catch (Exception)
        {
            return false;
        }
#else
        data.updated_at = DateTime.UtcNow.ToString("o");
        updatePlayerData(data.user_id, data.levels_unlocked, data.current_money, data.unlocked_cosmetics);
        
        currentPlayerData = data;
        SaveToPlayerPrefs(data);
        
        return true;
#endif
    }

    public async Task<bool> SyncGameData()
    {
        if (currentPlayerData == null)
  {
   return false;
 }

        currentPlayerData.levels_unlocked = PlayerPrefs.GetInt("levelAt", 6); // FIXED: Default to Level 1 unlocked
        currentPlayerData.current_money = PlayerPrefs.GetInt("moneyCount", 0);
      
   return await UpdatePlayerData(currentPlayerData);
    }

    private void SaveToPlayerPrefs(PlayerData data)
    {
        PlayerPrefs.SetString("username", data.username);
        PlayerPrefs.SetString("email", data.email);
        PlayerPrefs.SetInt("levelAt", data.levels_unlocked);
        PlayerPrefs.SetInt("moneyCount", data.current_money);
        PlayerPrefs.SetString("unlockedCosmetics", data.unlocked_cosmetics);
        PlayerPrefs.Save();
    }

    public PlayerData GetCurrentPlayerData()
    {
        return currentPlayerData;
    }

    public string GetUsername()
    {
        return PlayerPrefs.GetString("username", "Player");
    }

    public async Task UpdateLevelsUnlocked(int levelIndex)
    {
        if (currentPlayerData != null)
        {
            currentPlayerData.levels_unlocked = levelIndex;
            await UpdatePlayerData(currentPlayerData);
        }
    }

    public async Task UpdateMoney(int amount)
    {
        if (currentPlayerData != null)
        {
            currentPlayerData.current_money = amount;
            await UpdatePlayerData(currentPlayerData);
        }
    }

    public async Task AddUnlockedCosmetic(string cosmeticId)
    {
        if (currentPlayerData != null)
        {
            try
            {
                string cosmetics = currentPlayerData.unlocked_cosmetics.Trim('[', ']', ' ');
                List<string> cosmeticList = new List<string>();
 
                if (!string.IsNullOrEmpty(cosmetics))
                {
                    string[] items = cosmetics.Split(',');
                    foreach (var item in items)
                    {
                        cosmeticList.Add(item.Trim('"', ' '));
                    }
                }
      
                if (!cosmeticList.Contains(cosmeticId))
                {
                    cosmeticList.Add(cosmeticId);
                    currentPlayerData.unlocked_cosmetics = "[\"" + string.Join("\",\"", cosmeticList) + "\"]";
                    await UpdatePlayerData(currentPlayerData);
                }
            }
              catch (Exception)
            {
            }
        }
    }

    public List<string> GetUnlockedCosmetics()
    {
        if (currentPlayerData != null)
        {
            try
            {
                string cosmetics = currentPlayerData.unlocked_cosmetics.Trim('[', ']', ' ');
                List<string> cosmeticList = new List<string>();
        
                if (!string.IsNullOrEmpty(cosmetics))
                {
                    string[] items = cosmetics.Split(',');
                    foreach (var item in items)
                    {
                        cosmeticList.Add(item.Trim('"', ' '));
                    }
                }
                
                return cosmeticList;
            }
            catch
            {
                return new List<string>();
            }
        }
        return new List<string>();
    }

    public void ClearPlayerDataCache()
    {
        currentPlayerData = null;
        
        if (NewAndLoadGameManager.Instance != null)
        {
            NewAndLoadGameManager.Instance.SetUserId("");
            NewAndLoadGameManager.Instance.ClearAllSlots();
        }
    }
    
    /// <summary>
    /// NEW: Create guest player data (local only, no Supabase sync)
    /// </summary>
    public Task<bool> CreateGuestPlayerData(string guestId, string guestUsername)
    {
        try
        {
            var playerData = new PlayerData
            {
                user_id = guestId,
                email = "guest@local",
                username = guestUsername,
                levels_unlocked = 6, // Start at level 1 (tutorial is index 5)
                current_money = 0,
                unlocked_cosmetics = "[]"
            };

            currentPlayerData = playerData;
            SaveToPlayerPrefs(playerData);
            
            if (NewAndLoadGameManager.Instance != null)
            {
                NewAndLoadGameManager.Instance.SetUserId(guestId);
            }
            
            Debug.Log($"[PlayerDataManager] Guest account created: {guestUsername}");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PlayerDataManager] Guest creation failed: {ex.Message}");
            return Task.FromResult(false);
        }
    }
    
  /// <summary>
    /// NEW: Load existing player data or create if doesn't exist (for Google Sign-In)
    /// </summary>
    public async Task<PlayerData> LoadOrCreatePlayerData(string userId, string email, string username)
    {
        // Try to load existing data first
        var existingData = await LoadPlayerData(userId);
  
        if (existingData != null)
    {
    Debug.Log($"[PlayerDataManager] Loaded existing player data for {username}");
            return existingData;
        }
        
        // Data doesn't exist, create new
        Debug.Log($"[PlayerDataManager] Creating new player data for Google user: {username}");
  bool created = await CreatePlayerData(userId, email, username);
        
     if (created)
        {
            return currentPlayerData;
        }
        
 return null;
    }

    private void ClearUserData()
    {
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.DeleteKey("email");
        PlayerPrefs.DeleteKey("levelAt");
        PlayerPrefs.DeleteKey("moneyCount");
        PlayerPrefs.DeleteKey("unlockedCosmetics");
        
        if (NewAndLoadGameManager.Instance != null)
        {
            NewAndLoadGameManager.Instance.ClearAllSlots();
        }

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.ClearPlayerDataCache();
        }
        
        PlayerPrefs.Save();
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void createPlayerData(string userId, string email, string username);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void loadPlayerData(string userId);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void updatePlayerData(string userId, int levelsUnlocked, int currentMoney, string unlockedCosmetics);
#endif

    public void OnPlayerDataCreated(string jsonData)
    {
        Debug.Log($"[PlayerDataManager] ===== OnPlayerDataCreated CALLED =====");
        Debug.Log($"[PlayerDataManager] JSON received: {jsonData}");
     
        try
        {
  var simpleData = JsonUtility.FromJson<PlayerDataSimple>(jsonData);
      if (simpleData != null)
{
         Debug.Log($"[PlayerDataManager] JSON parsed successfully");
                Debug.Log($"[PlayerDataManager] User ID: {simpleData.user_id}");
          Debug.Log($"[PlayerDataManager] Username: {simpleData.username}");
         Debug.Log($"[PlayerDataManager] Email: {simpleData.email}");
      
     currentPlayerData = new PlayerData
          {
        id = simpleData.id.ToString(),
     user_id = simpleData.user_id,
              email = simpleData.email,
         username = simpleData.username,
       levels_unlocked = simpleData.levels_unlocked,
  current_money = simpleData.current_money,
        unlocked_cosmetics = simpleData.unlocked_cosmetics,
         created_at = simpleData.created_at,
              updated_at = simpleData.updated_at
             };
  
       Debug.Log($"[PlayerDataManager] ? currentPlayerData SET!");
      Debug.Log($"[PlayerDataManager] Verifying: GetCurrentPlayerData() is {(GetCurrentPlayerData() != null ? "NOT NULL" : "NULL")}");
  
                SaveToPlayerPrefs(currentPlayerData);
        Debug.Log($"[PlayerDataManager] PlayerPrefs saved");
            }
    else
       {
                Debug.LogError("[PlayerDataManager] ? Failed to parse JSON - simpleData is NULL");
     }
        }
        catch (Exception ex)
      {
            Debug.LogError($"[PlayerDataManager] ? Exception in OnPlayerDataCreated: {ex.Message}\n{ex.StackTrace}");
        }

        Debug.Log($"[PlayerDataManager] ===== OnPlayerDataCreated COMPLETE =====");
    }

    public void OnPlayerDataLoaded(string jsonData)
    {
  Debug.Log($"[PlayerDataManager] ===== OnPlayerDataLoaded CALLED =====");
      Debug.Log($"[PlayerDataManager] JSON received: {jsonData}");
        
        try
        {
     var simpleData = JsonUtility.FromJson<PlayerDataSimple>(jsonData);
            if (simpleData != null)
            {
   Debug.Log($"[PlayerDataManager] JSON parsed successfully");
          Debug.Log($"[PlayerDataManager] User ID: {simpleData.user_id}");
           Debug.Log($"[PlayerDataManager] Username: {simpleData.username}");
      Debug.Log($"[PlayerDataManager] Email: {simpleData.email}");
          
          currentPlayerData = new PlayerData
                {
       id = simpleData.id.ToString(),
    user_id = simpleData.user_id,
           email = simpleData.email,
     username = simpleData.username,
 levels_unlocked = simpleData.levels_unlocked,
         current_money = simpleData.current_money,
  unlocked_cosmetics = simpleData.unlocked_cosmetics,
             created_at = simpleData.created_at,
        updated_at = simpleData.updated_at
      };
      
      Debug.Log($"[PlayerDataManager] ? currentPlayerData SET!");
         Debug.Log($"[PlayerDataManager] Verifying: GetCurrentPlayerData() is {(GetCurrentPlayerData() != null ? "NOT NULL" : "NULL")}");
  
             SaveToPlayerPrefs(currentPlayerData);
       Debug.Log($"[PlayerDataManager] PlayerPrefs saved");
   
                if (NewAndLoadGameManager.Instance != null)
        {
     NewAndLoadGameManager.Instance.SetUserId(simpleData.user_id);
         Debug.Log($"[PlayerDataManager] User ID set in NewAndLoadGameManager: {simpleData.user_id}");
  }
                else
     {
               Debug.LogWarning("[PlayerDataManager] NewAndLoadGameManager.Instance is NULL!");
           }

                string displayName = string.IsNullOrEmpty(simpleData.username) ? simpleData.email : simpleData.username;
       Debug.Log($"[PlayerDataManager] Player data fully loaded for: {displayName}");
 }
         else
  {
  Debug.LogError("[PlayerDataManager] ? Failed to parse JSON - simpleData is NULL");
         }
      }
      catch (Exception ex)
     {
      Debug.LogError($"[PlayerDataManager] ? Exception in OnPlayerDataLoaded: {ex.Message}\n{ex.StackTrace}");
        }
   
        Debug.Log($"[PlayerDataManager] ===== OnPlayerDataLoaded COMPLETE =====");
    }

    public void OnPlayerDataUpdated(string jsonData)
    {
        try
     {
   }
          catch (Exception)
        {
        }
    }

    public void OnPlayerDataError(string errorMessage)
    {
    }
}
