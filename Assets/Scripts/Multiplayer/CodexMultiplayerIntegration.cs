using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Example integration with your existing Codex game systems
/// This shows how to make your existing game multiplayer-ready with turn-based gameplay
/// </summary>
public class CodexMultiplayerIntegration : MonoBehaviourPunCallbacks
{
    [Header("Integration Settings")]
    // Reserved for future use
    #pragma warning disable CS0414
    [SerializeField] private bool _syncLevelsUnlocked = true;
    [SerializeField] private bool _syncCosmetics = true;
    #pragma warning restore CS0414

    [Header("Turn System Settings")]
    [SerializeField] private float turnTimeLimit = 30f; // Time limit per turn in seconds
    [SerializeField] private bool enableTurnTimer = true;

    private new PhotonView photonView;
    
  // Reference to your existing managers
    private NewAndLoadGameManager saveManager;
    private PlayerDataManager playerDataManager;

    // Turn system
    private int currentPlayerTurn = 0; // Index in PhotonNetwork.PlayerList
    private float turnStartTime = 0f;
    
    // CRITICAL: Prevent multiple timeout calls
    private bool hasTimedOut = false;
    
    // Prevent log spam from GetCurrentTurnPlayer() called every frame
    // Reserved for future use
    #pragma warning disable CS0414
    private bool _hasLoggedMissingTurnProperty = false;
    #pragma warning restore CS0414

 // Events for turn changes
    public System.Action<Player> OnTurnChanged;
    public System.Action<Player, float> OnTurnTimeUpdate;
    public System.Action<Player> OnTurnTimeout;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        
        // Find your existing managers
        saveManager = FindObjectOfType<NewAndLoadGameManager>();
        playerDataManager = PlayerDataManager.Instance;
    }

    private void Update()
    {
        // Update turn timer if enabled
        if (enableTurnTimer && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            UpdateTurnTimer();
        }
    }

    #region Player Data Synchronization

    /// <summary>
    /// Load player data when joining multiplayer
    /// Combines your Supabase system with Photon
    /// </summary>
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
  
        // CRITICAL: No cosmetic assignment needed - all players share the same character
        // Character selection/assignment has been removed from multiplayer
        
        // Set player properties from your existing PlayerData (without cosmetic)
     if (playerDataManager != null && playerDataManager.GetCurrentPlayerData() != null)
 {
   var playerData = playerDataManager.GetCurrentPlayerData();
     
   ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable
     {
         { "username", playerData.username },
      { "levels_unlocked", playerData.levels_unlocked },
              { "IsReady", false }
    };
       
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
     Debug.Log($"[CodexMultiplayerIntegration] Player data synced to Photon network");
        }
        else
        {
            // Even if player data manager is not available, set basic properties
            ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable
            {
                { "IsReady", false }
            };
            
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
            Debug.Log($"[CodexMultiplayerIntegration] Basic player properties set");
        }

        // Initialize turn system if Master Client
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 2)
  {
       InitializeTurnSystem();
        }
    }

    #endregion

    #region Turn-Based System

    /// <summary>
    /// Initialize the turn system when game starts
    /// </summary>
    public void InitializeTurnSystem()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
        Debug.LogWarning("[CodexMultiplayerIntegration] Only Master Client can initialize turn system");
     return;
        }

        currentPlayerTurn = 0;
        turnStartTime = (float)PhotonNetwork.Time;

        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable
        {
  { "CurrentTurn", currentPlayerTurn },
            { "TurnStartTime", turnStartTime },
        { "GameState", "Playing" }
        };

 PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    
        Debug.Log($"[CodexMultiplayerIntegration] Turn system initialized! Starting with player index: {currentPlayerTurn}");
        
        photonView.RPC("RPC_TurnChanged", RpcTarget.All, currentPlayerTurn);
    }

    /// <summary>
    /// Get the current player turn index (safe to call every frame)
    /// </summary>
    public int GetCurrentPlayerTurnIndex()
    {
        return currentPlayerTurn;
    }
    
    /// <summary>
    /// Get the current player whose turn it is
    /// </summary>
    public Player GetCurrentTurnPlayer()
    {
        if (!PhotonNetwork.InRoom)
        {
            // Don't log warning every frame - this is called in Update()
            return null;
        }

        // Use the cached currentPlayerTurn instead of reading from room properties
        if (currentPlayerTurn >= 0 && currentPlayerTurn < PhotonNetwork.PlayerList.Length)
        {
            return PhotonNetwork.PlayerList[currentPlayerTurn];
        }
        else
        {
            Debug.LogWarning($"[CodexMultiplayerIntegration] Turn index {currentPlayerTurn} out of range for {PhotonNetwork.PlayerList.Length} players");
        }

        return null;
    }

    /// <summary>
    /// Check if it's the local player's turn
    /// </summary>
    public bool IsMyTurn()
    {
        Player currentPlayer = GetCurrentTurnPlayer();
        return currentPlayer != null && currentPlayer.IsLocal;
    }

    /// <summary>
    /// End current player's turn and move to next player
    /// Can be called by Master Client to advance turn even if it's not their turn
    /// (e.g., after a player's action completes)
    /// </summary>
    public void EndTurn()
    {
        Debug.Log($"[CodexMultiplayerIntegration] ===== EndTurn() CALLED =====");
  Debug.Log($"[CodexMultiplayerIntegration] IsMyTurn: {IsMyTurn()}");
        Debug.Log($"[CodexMultiplayerIntegration] IsMasterClient: {PhotonNetwork.IsMasterClient}");
    
        // Master Client can always advance turns (for automatic progression after card plays)
        // Non-Master Clients can only end their own turn
        if (!PhotonNetwork.IsMasterClient && !IsMyTurn())
 {
        Debug.LogWarning("[CodexMultiplayerIntegration] Cannot end turn - it's not your turn and you're not Master Client!");
          return;
      }

      if (PhotonNetwork.IsMasterClient)
        {
     Debug.Log("[CodexMultiplayerIntegration] Master Client - calling NextTurn() directly");
    NextTurn();
    }
        else
 {
     Debug.Log("[CodexMultiplayerIntegration] Non-Master Client - sending RPC_RequestNextTurn to Master");
      photonView.RPC("RPC_RequestNextTurn", RpcTarget.MasterClient);
        }
 }

    [PunRPC]
    void RPC_RequestNextTurn()
    {
        Debug.Log("[CodexMultiplayerIntegration] RPC_RequestNextTurn received");
      
        if (PhotonNetwork.IsMasterClient)
   {
            Debug.Log("[CodexMultiplayerIntegration] Master Client - calling NextTurn()");
     NextTurn();
     }
        else
        {
  Debug.LogWarning("[CodexMultiplayerIntegration] RPC_RequestNextTurn received by non-Master Client!");
        }
    }

    /// <summary>
    /// Move to the next player's turn (Master Client only)
    /// </summary>
    private void NextTurn()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[CodexMultiplayerIntegration] NextTurn called but not Master Client!");
        return;
        }

        int previousTurn = currentPlayerTurn;
        Player previousPlayer = GetCurrentTurnPlayer();
        
// Move to next player
    currentPlayerTurn = (currentPlayerTurn + 1) % PhotonNetwork.PlayerList.Length;
        turnStartTime = (float)PhotonNetwork.Time;

   Debug.Log($"[CodexMultiplayerIntegration] ===== ADVANCING TURN =====");
      Debug.Log($"[CodexMultiplayerIntegration] Previous turn: {previousTurn} ({previousPlayer?.NickName})");
        Debug.Log($"[CodexMultiplayerIntegration] New turn: {currentPlayerTurn} (Player count: {PhotonNetwork.PlayerList.Length})");
        
        if (currentPlayerTurn < PhotonNetwork.PlayerList.Length)
        {
            Player nextPlayer = PhotonNetwork.PlayerList[currentPlayerTurn];
        Debug.Log($"[CodexMultiplayerIntegration] Next player: {nextPlayer.NickName} (ActorNumber: {nextPlayer.ActorNumber})");
     }

        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable
        {
    { "CurrentTurn", currentPlayerTurn },
            { "TurnStartTime", turnStartTime }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        Debug.Log($"[CodexMultiplayerIntegration] Room properties updated with CurrentTurn={currentPlayerTurn}" );
  
  photonView.RPC("RPC_TurnChanged", RpcTarget.All, currentPlayerTurn);
      Debug.Log($"[CodexMultiplayerIntegration] RPC_TurnChanged sent to all clients");
    }

    [PunRPC]
    void RPC_TurnChanged(int turnIndex)
    {
   Debug.Log($"[CodexMultiplayerIntegration] ===== RPC_TurnChanged RECEIVED =====");
        Debug.Log($"[CodexMultiplayerIntegration] Turn index: {turnIndex}");
        Debug.Log($"[CodexMultiplayerIntegration] Player list length: {PhotonNetwork.PlayerList.Length}");
        Debug.Log($"[CodexMultiplayerIntegration] Players in room:");
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Debug.Log($"  [{i}] {PhotonNetwork.PlayerList[i].NickName} (ActorNumber: {PhotonNetwork.PlayerList[i].ActorNumber})");
        }
    
        currentPlayerTurn = turnIndex;
        
 // CRITICAL FIX: Reset timeout flag when turn changes
        hasTimedOut = false;
        
    // CRITICAL FIX: Get player directly from array using turnIndex parameter
        // Don't use GetCurrentTurnPlayer() which reads from room properties (race condition!)
        Player currentPlayer = null;
        if (turnIndex >= 0 && turnIndex < PhotonNetwork.PlayerList.Length)
        {
            currentPlayer = PhotonNetwork.PlayerList[turnIndex];
        }
        
if (currentPlayer != null)
        {
     Debug.Log($"[CodexMultiplayerIntegration] Turn changed to: {currentPlayer.NickName} (IsLocal: {currentPlayer.IsLocal}, ActorNumber: {currentPlayer.ActorNumber})");
            Debug.Log($"[CodexMultiplayerIntegration] Invoking OnTurnChanged event...");
   OnTurnChanged?.Invoke(currentPlayer);
       Debug.Log($"[CodexMultiplayerIntegration] OnTurnChanged event invoked successfully");
        }
 else
        {
      Debug.LogError($"[CodexMultiplayerIntegration] ERROR: Could not get current turn player for index {turnIndex}!");
        }
    }

    /// <summary>
    /// Update turn timer (Master Client only)
    /// </summary>
    private void UpdateTurnTimer()
    {
      if (!PhotonNetwork.InRoom) return;

     object turnStartObj;
      if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("TurnStartTime", out turnStartObj))
        {
float turnStart = (float)turnStartObj;
            float elapsed = (float)PhotonNetwork.Time - turnStart;
      float remaining = turnTimeLimit - elapsed;

          Player currentPlayer = GetCurrentTurnPlayer();
if (currentPlayer != null)
    {
      OnTurnTimeUpdate?.Invoke(currentPlayer, remaining);
            }

  // CRITICAL FIX: Only call timeout ONCE per turn using hasTimedOut flag
            if (remaining <= 0f && !hasTimedOut)
        {
hasTimedOut = true; // Set flag immediately to prevent multiple calls
      
   Debug.Log($"[CodexMultiplayerIntegration] Turn timeout for {currentPlayer?.NickName}");
    OnTurnTimeout?.Invoke(currentPlayer);
     
     // Handle timeout like a wrong answer
       // Call the SharedMultiplayerGameManager to handle it properly
    var sharedManager = FindObjectOfType<SharedMultiplayerGameManager>();
    if (sharedManager != null)
      {
 Debug.Log("[CodexMultiplayerIntegration] Notifying SharedMultiplayerGameManager of timeout");
            sharedManager.OnTurnTimedOut();
 }
         else
     {
             Debug.LogWarning("[CodexMultiplayerIntegration] SharedMultiplayerGameManager not found - just advancing turn");
    NextTurn();
 }
   }
        }
  }

    /// <summary>
    /// Get remaining time for current turn
    /// </summary>
    public float GetTurnTimeRemaining()
    {
        if (!PhotonNetwork.InRoom) return 0f;

        object turnStartObj;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("TurnStartTime", out turnStartObj))
        {
            float turnStart = (float)turnStartObj;
            float elapsed = (float)PhotonNetwork.Time - turnStart;
            return Mathf.Max(0f, turnTimeLimit - elapsed);
        }

        return 0f;
    }

    #endregion

    #region Level Synchronization

    /// <summary>
    /// When a player completes a level in multiplayer
    /// </summary>
    public void OnLevelCompleted(int levelNumber)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_LevelCompleted", RpcTarget.All, PhotonNetwork.NickName, levelNumber);
        }
    }

    [PunRPC]
    void RPC_LevelCompleted(string playerName, int levelNumber)
    {
        Debug.Log($"{playerName} completed level {levelNumber}!");
        
        // You can show notification to all players
        // or update multiplayer objectives
    }

    #endregion

    #region Cosmetics/Items Synchronization

    /// <summary>
    /// Apply cosmetics to player visible to all
    /// </summary>
    public void ApplyCosmetic(string cosmeticId)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_ApplyCosmetic", RpcTarget.AllBuffered, cosmeticId);
        }
    }

    [PunRPC]
    void RPC_ApplyCosmetic(string cosmeticId)
    {
        Debug.Log($"Applying cosmetic: {cosmeticId}");
        
        // Apply visual changes to this player
        // Your existing cosmetic system can be called here
        
        // Example:
        // var characterSwitcher = GetComponent<CharacterSwitcher>();
        // if (characterSwitcher != null)
        // {
        //     characterSwitcher.ApplyCosmetic(cosmeticId);
        // }
    }

    #endregion

    #region Game State Synchronization

    /// <summary>
    /// Share game state with other players (Master Client only)
    /// </summary>
    public void SetGameState(string state)
    {
        if (PhotonNetwork.IsMasterClient)
        {
     ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable
      {
{ "GameState", state },
    { "GameStartTime", PhotonNetwork.Time }
       };
          
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }
    }

    /// <summary>
    /// Called when room properties change
    /// </summary>
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("GameState"))
        {
            string gameState = (string)propertiesThatChanged["GameState"];
            Debug.Log($"Game state changed to: {gameState}");
            
            // Handle game state changes
            // Example: "Waiting", "Playing", "GameOver"
        }
    }

    /// <summary>
    /// Called when a player leaves - handle turn system
    /// </summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        // CRITICAL: Validate turn index after player leaves
        // The player list has changed, so we need to check if currentPlayerTurn is still valid
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
        {
            // Check if current turn index is still valid
            if (currentPlayerTurn >= PhotonNetwork.PlayerList.Length)
            {
                // Turn index is out of range - reset to first player
                Debug.LogWarning($"[CodexMultiplayerIntegration] Turn index {currentPlayerTurn} out of range after player left. Resetting to 0.");
                currentPlayerTurn = 0;
                
                // Update room properties
                ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable
                {
                    { "CurrentTurn", currentPlayerTurn },
                    { "TurnStartTime", (float)PhotonNetwork.Time }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
                
                // Notify all clients of turn change
                photonView.RPC("RPC_TurnChanged", RpcTarget.All, currentPlayerTurn);
            }
            else
            {
                // Check if it was the current player's turn
                Player currentTurnPlayer = GetCurrentTurnPlayer();
                if (currentTurnPlayer != null && currentTurnPlayer.ActorNumber == otherPlayer.ActorNumber)
                {
                    // The player whose turn it was just left - move to next
                    Debug.Log($"[CodexMultiplayerIntegration] Player {otherPlayer.NickName} left during their turn - advancing to next player");
                    NextTurn();
                }
                else
                {
                    // A different player left - just update the turn index if needed
                    // Recalculate turn index based on remaining players
                    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                    {
                        if (PhotonNetwork.PlayerList[i].ActorNumber == otherPlayer.ActorNumber)
                        {
                            // This shouldn't happen - player should be removed from list
                            Debug.LogWarning($"[CodexMultiplayerIntegration] Player {otherPlayer.NickName} still in player list after leaving!");
                        }
                    }
                    
                    // Adjust turn index if needed (if a player before current turn left)
                    // For simplicity, just validate the current turn index is still valid
                    if (currentPlayerTurn >= 0 && currentPlayerTurn < PhotonNetwork.PlayerList.Length)
                    {
                        // Turn index is still valid - no change needed
                        Debug.Log($"[CodexMultiplayerIntegration] Player {otherPlayer.NickName} left, but turn index {currentPlayerTurn} is still valid");
                    }
                }
            }
        }
    }

    #endregion

    #region Card System Integration (if you want multiplayer card battles)

    /// <summary>
    /// Example: Play a card in multiplayer during your turn
    /// </summary>
    public void PlayCard(string cardId, int targetPlayerId)
    {
        if (!IsMyTurn())
        {
            Debug.LogWarning("Cannot play card - it's not your turn!");
            return;
        }

        if (photonView.IsMine)
        {
            photonView.RPC("RPC_PlayCard", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, cardId, targetPlayerId);
        }
    }

    [PunRPC]
    void RPC_PlayCard(int playerActorNumber, string cardId, int targetPlayerId)
    {
        Debug.Log($"Player {playerActorNumber} played card {cardId} on player {targetPlayerId}");
        
        // Integrate with your CardManager
        // var cardManager = FindObjectOfType<CardManager>();
        // if (cardManager != null)
        // {
        //     cardManager.ExecuteCard(cardId, targetPlayerId);
        // }
    }

    #endregion

    #region Save/Load Integration

    /// <summary>
    /// Example: Sync save slots in multiplayer lobby
    /// Players can see each other's progress
    /// </summary>
    public void ShareSaveSlotInfo(int slotNumber)
    {
        if (saveManager != null)
        {
            // Get slot data
            // var slotData = saveManager.GetSlotData(slotNumber);
            
            // Broadcast to other players
            photonView.RPC("RPC_ReceiveSaveSlotInfo", RpcTarget.Others, 
                PhotonNetwork.NickName, 
                slotNumber, 
                "username", // from slot data
                5 // levels unlocked
            );
        }
    }

    [PunRPC]
    void RPC_ReceiveSaveSlotInfo(string playerName, int slotNumber, string username, int levelsUnlocked)
    {
        Debug.Log($"{playerName}'s Save Slot {slotNumber}: {username} - Level {levelsUnlocked}");
        
        // You can display this in a multiplayer lobby UI
    }

    #endregion

    #region Multiplayer Game Modes Examples

    /// <summary>
    /// Example: Co-op mode - players must work together (turn-based)
    /// </summary>
    public void StartCoopMode()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SetGameState("Coop");
            
            // Load co-op scene for all players
            PhotonNetwork.LoadLevel("CoopGameScene");
        }
    }

    /// <summary>
    /// Example: Competitive mode - players compete (turn-based)
    /// </summary>
    public void StartCompetitiveMode()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SetGameState("Competitive");
            
            // Load competitive scene
            PhotonNetwork.LoadLevel("CompetitiveGameScene");
        }
    }

    /// <summary>
    /// Example: Track player score in competitive mode
    /// </summary>
    public void AddScore(int points)
    {
        if (photonView.IsMine)
        {
            // Get current score
            object currentScoreObj;
            int currentScore = PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Score", out currentScoreObj) 
                ? (int)currentScoreObj 
                : 0;
            
            // Add points
            int newScore = currentScore + points;
            
            // Update
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { "Score", newScore }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            
            Debug.Log($"Score: {newScore}");
        }
    }

    /// <summary>
    /// Get leaderboard of all players
    /// </summary>
    public void ShowLeaderboard()
    {
        Debug.Log("=== LEADERBOARD ===");
        
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object scoreObj;
            int score = player.CustomProperties.TryGetValue("Score", out scoreObj) ? (int)scoreObj : 0;
            
            Debug.Log($"{player.NickName}: {score} points");
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Check if all players are ready
    /// </summary>
    public bool AreAllPlayersReady()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object isReadyObj;
            bool isReady = player.CustomProperties.TryGetValue("IsReady", out isReadyObj) && (bool)isReadyObj;
            
            if (!isReady)
                return false;
        }
        
        return true;
    }

    /// <summary>
    /// Set local player as ready
    /// </summary>
    public void SetReady(bool ready)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "IsReady", ready }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // Start game when all players are ready (Master Client)
        if (PhotonNetwork.IsMasterClient && ready && AreAllPlayersReady())
        {
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playerCount >= 2 && playerCount <= 5)
            {
                Debug.Log("All players ready! Starting game...");
                InitializeTurnSystem();
            }
        }
    }

    /// <summary>
    /// Get player count in room
    /// </summary>
    public int GetPlayerCount()
    {
        return PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
    }

    /// <summary>
    /// Check if room is full (5 players max)
    /// </summary>
    public bool IsRoomFull()
    {
        return GetPlayerCount() >= 5;
    }

    #endregion
}

/*
 * INTEGRATION NOTES:
 * 
 * 1. Attach this script to a GameObject in your multiplayer scene
 * 2. Add PhotonView component to the same GameObject
 * 3. This implements a TURN-BASED system for 2-5 players
 * 
 * 4. Turn System Features:
 *    - Automatic turn rotation among all players
 *    - Optional turn time limit (default: 30 seconds)
 *    - Turn skips automatically if player times out
 *    - Handles player disconnection during their turn
 * 
 * 5. Example Integration Flow:
 *    - Players join room (2-5 players)
 *    - All players set themselves as ready
 *    - Master Client initializes turn system
 *    - Players take turns in succession
 *    - Use IsMyTurn() to check if player can act
 *    - Call EndTurn() when player finishes their turn
 * 
 * 6. Events you can subscribe to:
 *    - OnTurnChanged: Called when turn changes to a new player
 *    - OnTurnTimeUpdate: Called every frame with remaining time
 *    - OnTurnTimeout: Called when a player's turn times out
 * 
 * 7. Example Usage in Your Game:
 *    void Start() {
 *        var integration = FindObjectOfType<CodexMultiplayerIntegration>();
 *        integration.OnTurnChanged += OnPlayerTurnChanged;
 *    }
 *    
 *    void OnPlayerTurnChanged(Player player) {
 *        if (player.IsLocal) {
 *            // Enable player controls
 *        } else {
 *            // Disable controls, show "Waiting for [player name]"
 *        }
 *    }
 */
