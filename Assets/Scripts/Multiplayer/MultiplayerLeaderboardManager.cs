using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using System;

#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
using Photon.Realtime;
#endif

/// <summary>
/// Manages multiplayer leaderboard tracking levels beaten per player in the current lobby session
/// </summary>
public class MultiplayerLeaderboardManager : MonoBehaviourPunCallbacks
{
    public static MultiplayerLeaderboardManager Instance;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI levelsBeatenText; // Text to show levels beaten in victory/defeat screen (optional - will find at runtime if not assigned)
    [SerializeField] private Transform leaderboardContainer; // Container for leaderboard entries in main menu
    [SerializeField] private GameObject leaderboardEntryPrefab; // Prefab for each leaderboard entry
    [SerializeField] private string levelsBeatenTextGameObjectName = "LevelsBeatenText"; // Name to search for if not assigned
    
    // Track levels beaten per player in current lobby session
    private Dictionary<int, int> playerLevelsBeaten = new Dictionary<int, int>();
    
    // Leaderboard data for main menu (persistent across sessions)
    private Dictionary<string, int> persistentLeaderboard = new Dictionary<string, int>();
    
    private new PhotonView photonView;
    
    // Supabase client access
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        photonView = GetComponent<PhotonView>();
        if (photonView == null)
        {
            photonView = gameObject.AddComponent<PhotonView>();
            photonView.ViewID = 999; // Use a high view ID to avoid conflicts
        }
    }
    
    private async void Start()
    {
        // Always load persistent leaderboard from Supabase (for main menu display)
        await LoadLeaderboardFromSupabase();
        
        // Only initialize session tracking if in a room
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // Initialize leaderboard for all players in room
            InitializeLeaderboard();
        }
    }
    
    /// <summary>
    /// Initialize leaderboard for all players in the room
    /// </summary>
    private void InitializeLeaderboard()
    {
        playerLevelsBeaten.Clear();
        
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player != null)
            {
                playerLevelsBeaten[player.ActorNumber] = 0;
            }
        }
        
        // Sync initial state
        if (PhotonNetwork.IsMasterClient)
        {
            SyncLeaderboardToAll();
        }
    }
    
    /// <summary>
    /// Increment levels beaten for a player (called when level is completed)
    /// Only tracks current session - doesn't save until game over
    /// </summary>
    public void IncrementLevelsBeaten(int actorNumber)
    {
        if (!playerLevelsBeaten.ContainsKey(actorNumber))
        {
            playerLevelsBeaten[actorNumber] = 0;
        }
        
        playerLevelsBeaten[actorNumber]++;
        
        // Sync to all players (just for display, not saved yet)
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_UpdateLevelsBeaten", RpcTarget.All, actorNumber, playerLevelsBeaten[actorNumber]);
        }
    }
    
    /// <summary>
    /// Increment levels beaten locally only (for non-host players)
    /// </summary>
    public void IncrementLevelsBeatenLocalOnly(int actorNumber)
    {
        if (!playerLevelsBeaten.ContainsKey(actorNumber))
        {
            playerLevelsBeaten[actorNumber] = 0;
        }
        
        playerLevelsBeaten[actorNumber]++;
    }
    
    /// <summary>
    /// Get levels beaten for a specific player
    /// </summary>
    public int GetLevelsBeaten(int actorNumber)
    {
        if (playerLevelsBeaten.ContainsKey(actorNumber))
        {
            return playerLevelsBeaten[actorNumber];
        }
        return 0;
    }
    
    /// <summary>
    /// Get levels beaten for local player
    /// </summary>
    public int GetLocalPlayerLevelsBeaten()
    {
        if (PhotonNetwork.LocalPlayer != null)
        {
            return GetLevelsBeaten(PhotonNetwork.LocalPlayer.ActorNumber);
        }
        return 0;
    }
    
    /// <summary>
    /// Get all players' levels beaten for leaderboard display
    /// Shows top 10 best single-run scores from Supabase
    /// </summary>
    public Dictionary<string, int> GetLeaderboardData()
    {
        // Return top 10 best scores from Supabase
        // Sort by score descending and take top 10
        var sorted = persistentLeaderboard.OrderByDescending(x => x.Value).Take(10);
        return sorted.ToDictionary(x => x.Key, x => x.Value);
    }
    
    /// <summary>
    /// Find the levels beaten text component at runtime (if not assigned)
    /// </summary>
    private TextMeshProUGUI FindLevelsBeatenText()
    {
        // If already assigned, use it
        if (levelsBeatenText != null)
        {
            return levelsBeatenText;
        }
        
        // Try to find by GameObject name
        GameObject textObj = GameObject.Find(levelsBeatenTextGameObjectName);
        if (textObj != null)
        {
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                levelsBeatenText = text; // Cache it for next time
                return text;
            }
        }
        
        // Try to find in victory/game over panels (search by common names)
        string[] panelNames = { "VictoryPanel", "GameOverPanel", "Victory Panel", "Game Over Panel", "victoryPanel", "gameOverPanel" };
        foreach (string panelName in panelNames)
        {
            GameObject panel = GameObject.Find(panelName);
            if (panel != null)
            {
                // Search for TextMeshProUGUI components in children
                TextMeshProUGUI[] texts = panel.GetComponentsInChildren<TextMeshProUGUI>(true);
                if (texts != null && texts.Length > 0)
                {
                    // Prefer a text that's not the main game over/victory message
                    // (usually the first one is the title, we want a secondary text)
                    TextMeshProUGUI text = texts.Length > 1 ? texts[1] : texts[0];
                    levelsBeatenText = text;
                    return text;
                }
            }
        }
        
        // Last resort: search all TextMeshProUGUI in scene for one with a specific name
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text.gameObject.name.Contains("LevelsBeaten") || 
                text.gameObject.name.Contains("Levels Beaten") ||
                text.gameObject.name.Contains("Leaderboard"))
            {
                levelsBeatenText = text;
                return text;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Update UI to show levels beaten in victory/defeat screen
    /// </summary>
    public void UpdateLevelsBeatenDisplay()
    {
        TextMeshProUGUI text = FindLevelsBeatenText();
        if (text != null)
        {
            int localLevels = GetLocalPlayerLevelsBeaten();
            text.text = $"Levels Beaten in This Lobby: {localLevels}";
        }
    }
    
    /// <summary>
    /// Display leaderboard in main menu panel
    /// </summary>
    public void DisplayLeaderboard()
    {
        if (leaderboardContainer == null) return;
        
        // Clear existing entries
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get leaderboard data
        var leaderboard = GetLeaderboardData();
        
        // Sort by levels beaten (descending)
        var sortedLeaderboard = leaderboard.OrderByDescending(x => x.Value).ToList();
        
        // Create entries
        int rank = 1;
        foreach (var entry in sortedLeaderboard)
        {
            if (leaderboardEntryPrefab != null)
            {
                GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
                
                // Set rank, name, and levels beaten
                TextMeshProUGUI[] texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 3)
                {
                    texts[0].text = $"#{rank}";
                    texts[1].text = entry.Key;
                    texts[2].text = entry.Value.ToString();
                }
                else if (texts.Length >= 2)
                {
                    texts[0].text = $"{entry.Key}";
                    texts[1].text = $"{entry.Value} levels";
                }
            }
            else
            {
                // Fallback: create simple text entry
                GameObject entryObj = new GameObject($"LeaderboardEntry_{rank}");
                entryObj.transform.SetParent(leaderboardContainer);
                TextMeshProUGUI text = entryObj.AddComponent<TextMeshProUGUI>();
                text.text = $"#{rank}. {entry.Key}: {entry.Value} levels";
            }
            
            rank++;
        }
    }
    
    /// <summary>
    /// Sync leaderboard to all players
    /// </summary>
    private void SyncLeaderboardToAll()
    {
        foreach (var kvp in playerLevelsBeaten)
        {
            photonView.RPC("RPC_UpdateLevelsBeaten", RpcTarget.All, kvp.Key, kvp.Value);
        }
    }
    
    [PunRPC]
    void RPC_UpdateLevelsBeaten(int actorNumber, int levelsBeaten)
    {
        playerLevelsBeaten[actorNumber] = levelsBeaten;
        Debug.Log($"[MultiplayerLeaderboardManager] Player {actorNumber} has beaten {levelsBeaten} levels");
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Initialize new player's levels beaten to 0
        if (!playerLevelsBeaten.ContainsKey(newPlayer.ActorNumber))
        {
            playerLevelsBeaten[newPlayer.ActorNumber] = 0;
        }
        
        // Sync current leaderboard to new player
        if (PhotonNetwork.IsMasterClient)
        {
            SyncLeaderboardToAll();
        }
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // If local player is leaving, save their session score before leaving
        if (otherPlayer.IsLocal && PhotonNetwork.IsMasterClient)
        {
            // Save session scores for all players before leaving
            SaveSessionScoresToLeaderboard();
        }
        
        // Remove player from leaderboard
        if (playerLevelsBeaten.ContainsKey(otherPlayer.ActorNumber))
        {
            playerLevelsBeaten.Remove(otherPlayer.ActorNumber);
        }
    }
    
    public override void OnLeftRoom()
    {
        // Save session scores when leaving room (if master client)
        if (PhotonNetwork.IsMasterClient)
        {
            SaveSessionScoresToLeaderboard();
        }
        
        // Clear session tracking
        playerLevelsBeaten.Clear();
    }
    
    /// <summary>
    /// Save leaderboard entry to Supabase (only saves if it's a new best score)
    /// </summary>
    private async Task SaveLeaderboardToSupabase(string username, int levelsBeaten)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        try
        {
            var supabaseClient = GetSupabaseClient();
            if (supabaseClient == null)
            {
                Debug.LogWarning("[MultiplayerLeaderboardManager] Supabase client not available");
                return;
            }
            
            // Get user_id if available
            string userId = "";
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
            {
                userId = PlayerDataManager.Instance.GetCurrentPlayerData().user_id;
            }
            
            // Check if entry exists and get current best
            try
            {
                var existingResponse = await supabaseClient
                    .From<MultiplayerLeaderboardEntry>()
                    .Where(x => x.username == username)
                    .Get();
                
                MultiplayerLeaderboardEntry existing = null;
                if (existingResponse != null && existingResponse.Models != null && existingResponse.Models.Count > 0)
                {
                    existing = existingResponse.Models[0];
                }
                
                if (existing != null)
                {
                    // Only update if new score is higher than existing best
                    if (levelsBeaten > existing.levels_beaten)
                    {
                        existing.levels_beaten = levelsBeaten;
                        existing.updated_at = DateTime.UtcNow.ToString("o");
                        if (!string.IsNullOrEmpty(userId))
                        {
                            existing.user_id = userId;
                        }
                        await supabaseClient
                            .From<MultiplayerLeaderboardEntry>()
                            .Where(x => x.username == username)
                            .Update(existing);
                        Debug.Log($"[MultiplayerLeaderboardManager] Updated best score for {username}: {levelsBeaten} levels (was {existing.levels_beaten})");
                    }
                    else
                    {
                        Debug.Log($"[MultiplayerLeaderboardManager] Score {levelsBeaten} for {username} is not higher than best {existing.levels_beaten}, not saving");
                    }
                }
                else
                {
                    // Insert new entry (first time on leaderboard)
                    var entry = new MultiplayerLeaderboardEntry
                    {
                        username = username,
                        user_id = userId,
                        levels_beaten = levelsBeaten,
                        updated_at = DateTime.UtcNow.ToString("o")
                    };
                    await supabaseClient.From<MultiplayerLeaderboardEntry>().Insert(entry);
                    Debug.Log($"[MultiplayerLeaderboardManager] Created new leaderboard entry for {username}: {levelsBeaten} levels");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MultiplayerLeaderboardManager] Error checking/updating leaderboard: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[MultiplayerLeaderboardManager] Failed to save leaderboard to Supabase: {ex.Message}");
        }
#else
        // WebGL: Use JavaScript bridge
        try
        {
            string userId = "";
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
            {
                userId = PlayerDataManager.Instance.GetCurrentPlayerData().user_id;
            }
            
            // Call JavaScript function to save leaderboard
            Application.ExternalCall("saveLeaderboardEntry", username, levelsBeaten, userId);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[MultiplayerLeaderboardManager] Failed to save leaderboard (WebGL): {ex.Message}");
        }
#endif
    }
    
    /// <summary>
    /// Load leaderboard from Supabase (top 10 best single-run scores)
    /// </summary>
    private async Task LoadLeaderboardFromSupabase()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        try
        {
            var supabaseClient = GetSupabaseClient();
            if (supabaseClient == null)
            {
                Debug.LogWarning("[MultiplayerLeaderboardManager] Supabase client not available");
                return;
            }
            
            // Get top 10 leaderboard entries, ordered by levels_beaten descending
            var response = await supabaseClient
                .From<MultiplayerLeaderboardEntry>()
                .Order("levels_beaten", Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(10)
                .Get();
            
            persistentLeaderboard.Clear();
            
            if (response != null && response.Models != null)
            {
                foreach (var entry in response.Models)
                {
                    persistentLeaderboard[entry.username] = entry.levels_beaten;
                }
                
                Debug.Log($"[MultiplayerLeaderboardManager] Loaded top {persistentLeaderboard.Count} leaderboard entries from Supabase");
                
                // Sync to all players in room (if in a room)
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                {
                    SyncPersistentLeaderboardToAll();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[MultiplayerLeaderboardManager] Failed to load leaderboard from Supabase: {ex.Message}");
        }
#else
        // WebGL: Use JavaScript bridge
        try
        {
            Application.ExternalCall("loadLeaderboardEntries");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[MultiplayerLeaderboardManager] Failed to load leaderboard (WebGL): {ex.Message}");
        }
#endif
    }
    
    /// <summary>
    /// Get persistent leaderboard data (from Supabase)
    /// </summary>
    public Dictionary<string, int> GetPersistentLeaderboard()
    {
        return new Dictionary<string, int>(persistentLeaderboard);
    }
    
    /// <summary>
    /// Sync persistent leaderboard to all players
    /// </summary>
    private void SyncPersistentLeaderboardToAll()
    {
        // Send leaderboard data via RPC
        foreach (var kvp in persistentLeaderboard)
        {
            photonView.RPC("RPC_UpdatePersistentLeaderboard", RpcTarget.All, kvp.Key, kvp.Value);
        }
    }
    
    [PunRPC]
    void RPC_UpdatePersistentLeaderboard(string username, int levelsBeaten)
    {
        persistentLeaderboard[username] = levelsBeaten;
    }
    
    /// <summary>
    /// Save current session scores to leaderboard (called on game over)
    /// Only saves if the session score is higher than the player's best
    /// </summary>
    public async void SaveSessionScoresToLeaderboard()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player != null)
            {
                int sessionScore = GetLevelsBeaten(player.ActorNumber);
                string playerName = player.NickName;
                
                // Check if this is a new best score
                int currentBest = 0;
                if (persistentLeaderboard.ContainsKey(playerName))
                {
                    currentBest = persistentLeaderboard[playerName];
                }
                
                // Only save if session score is higher than best
                if (sessionScore > currentBest)
                {
                    await SaveLeaderboardToSupabase(playerName, sessionScore);
                    persistentLeaderboard[playerName] = sessionScore;
                    
                    // Sync updated leaderboard to all players
                    photonView.RPC("RPC_UpdatePersistentLeaderboard", RpcTarget.All, playerName, sessionScore);
                }
            }
        }
    }
    
    /// <summary>
    /// Get leaderboard data (shows best single-run scores from Supabase)
    /// </summary>
    public Dictionary<string, int> GetCombinedLeaderboard()
    {
        Dictionary<string, int> combined = new Dictionary<string, int>(persistentLeaderboard);
        
        // Add current session scores for display (shows current run progress)
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player != null)
            {
                string playerName = player.NickName;
                int sessionLevels = GetLevelsBeaten(player.ActorNumber);
                
                // Show current session score if higher than stored best
                if (!combined.ContainsKey(playerName) || sessionLevels > combined[playerName])
                {
                    combined[playerName] = sessionLevels;
                }
            }
        }
        
        return combined;
    }
}

