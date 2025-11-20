using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI Panels")]
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject roomPanel;

    [Header("Connection Panel")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button connectButton;
    [SerializeField] private TMP_Text statusText;

    [Header("Lobby Panel")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private TMP_Text lobbyInfoText;

    private const string DIFFICULTY_KEY = "Difficulty"; // Room custom property key

    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }


    [Header("Room Panel")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerListText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button leaveRoomButton;

    [Header("Room Difficulty (Host selects)")]
    [SerializeField] private TMP_Dropdown difficultyDropdown; // Optional: Dropdown for Easy/Normal/Hard
    [SerializeField] private TMP_Text difficultyLabel;        // Optional: Read-only label for clients

    [SerializeField] private TMP_Text readyStatusText;
    [SerializeField] private TMP_Text selectedLevelText;
    [SerializeField] private Button readyButton; // NEW: Ready button for players
    [SerializeField] private TMP_Text readyButtonText; // NEW: Text on ready button

    [Header("Player List UI (Optional - for individual kick buttons)")]
    [SerializeField] private Transform playerListContainer; // Container for player entries
    [SerializeField] private GameObject playerEntryPrefab; // Prefab with player name, ready status, and kick button
    
    [Header("Simple Kick System (Alternative)")]
  [SerializeField] private Button[] playerKickButtons; // Array of 5 buttons (one per player slot)
    [SerializeField] private TMP_Text[] playerNameTexts; // Text showing player names on each button
    
    [Header("Kick Confirmation (Optional)")]
 [SerializeField] private GameObject kickConfirmationPanel; // Panel asking "Kick [PlayerName]?"
    [SerializeField] private TMP_Text kickConfirmationText; // Text showing who will be kicked
    [SerializeField] private Button confirmKickButton; // Confirm kick
    [SerializeField] private Button cancelKickButton; // Cancel kick
  
    [Header("Leaderboard")]
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private LeaderboardPanel leaderboardPanel;
    
    // Public property to access leaderboard button
    public Button LeaderboardButton => leaderboardButton;
  
    [Header("Game Settings")]
    [SerializeField] private int minPlayers = 2;
    [SerializeField] private int maxPlayers = 5;

    [Header("Difficulty Level Sets")]
    [Tooltip("Levels available when Difficulty=Easy")] [SerializeField]
    private string[] easyLevels = { "Level_E1", "Level_E2" };
    [Tooltip("Levels available when Difficulty=Normal")] [SerializeField]
    private string[] normalLevels = { "Level_1", "Level_2", "Level_3" };
    [Tooltip("Levels available when Difficulty=Hard")] [SerializeField]
    private string[] hardLevels = { "Level_H1", "Level_H2" };

    // Deprecated: used only as a global fallback if difficulty arrays are empty
    [SerializeField] private string[] availableLevels = { "Level_1", "Level_2", "Level_3" };
    
    /// <summary>
    /// Get available multiplayer levels (for use by SharedMultiplayerGameManager)
    /// </summary>
    public string[] GetAvailableLevels()
    {
        return availableLevels;
    }

    private string selectedLevel = "";
    private bool isLocalPlayerReady = false;
    private Player playerPendingKick = null; // Track who is pending kick
    private bool hasManuallyConnected = false; // Track if user clicked connect button
    
    // NEW: Step 2 - List to track dynamic player entries
    private List<PlayerListEntry> playerEntries = new List<PlayerListEntry>();

    // Player custom property keys
    private const string READY_STATE_KEY = "IsReady";
    private const string COSMETIC_KEY = "multiplayer_cosmetic";
    private const string POSITION_KEY = "player_position";

    private void Start()
    {
        ShowConnectionPanel();
SetupButtons();
      
   if (PlayerPrefs.HasKey("PlayerNickname"))
  {
          if (nicknameInput != null)
             nicknameInput.text = PlayerPrefs.GetString("PlayerNickname");
        }
    }

    private void Update()
    {
     if (statusText != null && NetworkManager.Instance != null)
        {
            string status = NetworkManager.Instance.GetConnectionStatus();
            statusText.text = status;
            
            // Update status text color based on connection state
            if (PhotonNetwork.InLobby)
            {
                statusText.color = Color.green;
            }
            else if (PhotonNetwork.IsConnected)
            {
                statusText.color = Color.yellow;
            }
            else
            {
                statusText.color = Color.white;
            }
        }

   if (PhotonNetwork.InRoom)
    {
         UpdateRoomUI();
        }
    
    if (lobbyInfoText != null && PhotonNetwork.InLobby)
        {
        lobbyInfoText.text = $"In Lobby - {PhotonNetwork.CountOfPlayers} players online";
     }
     
     // Ensure UI panels match connection state - but only if user manually connected
     if (hasManuallyConnected && PhotonNetwork.InLobby && lobbyPanel != null && !lobbyPanel.activeSelf && !PhotonNetwork.InRoom)
     {
         ShowLobbyPanel();
     }
     else if (!hasManuallyConnected && connectionPanel != null && !connectionPanel.activeSelf && !PhotonNetwork.InRoom)
     {
         // If user hasn't manually connected, always show connection panel
         ShowConnectionPanel();
     }
     else if (!PhotonNetwork.InLobby && !PhotonNetwork.IsConnected && connectionPanel != null && !connectionPanel.activeSelf && !PhotonNetwork.InRoom)
     {
         ShowConnectionPanel();
         hasManuallyConnected = false; // Reset flag when disconnected
     }
    }

    #region UI Panel Management

    private void ShowConnectionPanel()
    {
        if (connectionPanel != null) connectionPanel.SetActive(true);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (roomPanel != null) roomPanel.SetActive(false);
    }

    private void ShowLobbyPanel()
    {
        if (connectionPanel != null) connectionPanel.SetActive(false);
 if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (roomPanel != null) roomPanel.SetActive(false);
    }

    private void ShowRoomPanel()
    {
        SetupDifficultyUI();
        if (connectionPanel != null) connectionPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
     if (roomPanel != null) roomPanel.SetActive(true);
    }

  #endregion

    #region Button Setup

    private void SetupButtons()
    {
        // Clear all existing listeners first to prevent duplicates
        if (connectButton != null)
        {
            connectButton.onClick.RemoveAllListeners();
            connectButton.onClick.AddListener(OnConnectButtonClicked);
        }

        if (createRoomButton != null)
        {
            createRoomButton.onClick.RemoveAllListeners();
            createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        }

        if (joinRoomButton != null)
        {
            joinRoomButton.onClick.RemoveAllListeners();
            joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        }

        if (leaveRoomButton != null)
        {
            leaveRoomButton.onClick.RemoveAllListeners();
            leaveRoomButton.onClick.AddListener(OnLeaveRoomButtonClicked);
        }
        
        // Ready button
        if (readyButton != null)
        {
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(OnReadyButtonClicked);
        }
        
        // Kick confirmation buttons
        if (confirmKickButton != null)
        {
            confirmKickButton.onClick.RemoveAllListeners();
            confirmKickButton.onClick.AddListener(OnConfirmKickClicked);
        }
  
        if (cancelKickButton != null)
        {
            cancelKickButton.onClick.RemoveAllListeners();
            cancelKickButton.onClick.AddListener(OnCancelKickClicked);
        }
        
        // Setup kick buttons for each player slot
        if (playerKickButtons != null)
        {
            for (int i = 0; i < playerKickButtons.Length; i++)
            {
                int index = i; // Capture for lambda
                if (playerKickButtons[i] != null)
                {
                    playerKickButtons[i].onClick.RemoveAllListeners();
                    playerKickButtons[i].onClick.AddListener(() => OnKickButtonClicked(index));
                }
            }
        }
        
        // Hide kick confirmation panel initially
        if (kickConfirmationPanel != null)
            kickConfirmationPanel.SetActive(false);
        
        // Leaderboard button
        if (leaderboardButton != null && leaderboardPanel != null)
        {
            leaderboardButton.onClick.RemoveAllListeners();
            leaderboardButton.onClick.AddListener(() => {
                // Pass button reference so LeaderboardPanel can hide it after verifying panel is visible
                leaderboardPanel.OpenLeaderboard(leaderboardButton);
            });
        }
    }

    #endregion

    #region Button Callbacks

    private void OnConnectButtonClicked()
    {
        string nickname = nicknameInput != null ? nicknameInput.text : "Player";

   if (string.IsNullOrEmpty(nickname) || string.IsNullOrWhiteSpace(nickname))
    {
            Debug.LogWarning("[LobbyManager] Nickname is empty! Please enter a name.");
            if (statusText != null)
            {
                statusText.text = "Please enter a nickname!";
                statusText.color = Color.red;
            }
            return;
  }

  PlayerPrefs.SetString("PlayerNickname", nickname);
        PlayerPrefs.Save();

        hasManuallyConnected = true; // Mark that user manually connected

        if (NetworkManager.Instance != null)
    {
  NetworkManager.Instance.SetPlayerNickname(nickname);
            NetworkManager.Instance.ConnectToPhoton();
        }

        // Don't show lobby panel immediately - wait for actual connection
        // The panel will be shown in OnJoinedLobby callback
    }

    private void OnCreateRoomButtonClicked()
    {
        // Validate connection state before creating room
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("[LobbyManager] Cannot create room - not connected and ready!");
            if (statusText != null)
            {
                statusText.text = "Not connected! Please wait...";
                statusText.color = Color.red;
            }
            return;
        }
        
        // Note: We don't strictly require InLobby here because:
        // 1. Photon allows creating rooms even when not explicitly in a lobby (uses default lobby)
        // 2. After a failed operation, player might temporarily not be in lobby but can still create
        // 3. Photon will handle the lobby state automatically
        
        if (PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[LobbyManager] Already in a room!");
            return;
        }
        
string roomName = roomNameInput != null && !string.IsNullOrEmpty(roomNameInput.text) 
            ? roomNameInput.text.Trim() 
            : "Room_" + Random.Range(1000, 9999);
 
        if (string.IsNullOrEmpty(roomName) || string.IsNullOrWhiteSpace(roomName))
        {
            Debug.LogWarning("[LobbyManager] Room name is empty! Generating random name.");
            roomName = "Room_" + Random.Range(1000, 9999);
        }
 
     RoomOptions roomOptions = new RoomOptions
        {
         MaxPlayers = (byte)maxPlayers,
 IsVisible = true,
            IsOpen = true,
            PublishUserId = true // Make sure player IDs are published for joining
        };

        Debug.Log($"[LobbyManager] Creating room: '{roomName}' (Max: {maxPlayers} players, Visible: {roomOptions.IsVisible}, Open: {roomOptions.IsOpen})");
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    private void OnJoinRoomButtonClicked()
    {
        // Validate connection state before joining room
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("[LobbyManager] Cannot join room - not connected and ready!");
            if (statusText != null)
            {
                statusText.text = "Not connected! Please wait...";
                statusText.color = Color.red;
            }
            return;
        }
        
        // Note: We don't strictly require InLobby here because:
        // 1. Photon allows joining rooms even when not explicitly in a lobby (uses default lobby)
        // 2. After a failed join, player might temporarily not be in lobby but can still join
        // 3. Photon will handle the lobby state automatically
        
        if (PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[LobbyManager] Already in a room!");
            return;
        }
        
        string roomName = roomNameInput != null ? roomNameInput.text.Trim() : "";
      
        if (!string.IsNullOrEmpty(roomName) && !string.IsNullOrWhiteSpace(roomName))
        {
            Debug.Log($"[LobbyManager] Attempting to join room: '{roomName}'");
            if (NetworkManager.Instance != null)
   {
         NetworkManager.Instance.JoinRoom(roomName);
      }
            else
            {
                Debug.LogError("[LobbyManager] NetworkManager.Instance is null!");
            }
        }
        else
        {
            Debug.LogWarning("[LobbyManager] Please enter a room name!");
            if (statusText != null)
            {
                statusText.text = "Please enter a room name!";
                statusText.color = Color.yellow;
            }
        }
    }

    // NEW: Ready button callback
    private void OnReadyButtonClicked()
    {
        if (!PhotonNetwork.InRoom) return;

        if (PhotonNetwork.IsMasterClient)
        {
            // Host is always ready; enforce true
            SetLocalReady(true);
            return;
        }

        // Toggle ready state for non-host
        SetLocalReady(!isLocalPlayerReady);
    }

    private void OnStartGameButtonClicked()
    {
        // Ensure difficulty is set by host before starting
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                // If not set yet, default to Normal
                if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(DIFFICULTY_KEY))
                {
                    var props = new ExitGames.Client.Photon.Hashtable { { DIFFICULTY_KEY, (int)Difficulty.Normal } };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                }
            }
        }
      if (!PhotonNetwork.IsMasterClient)
     {
            Debug.LogWarning("Only the host can start the game!");
 if (readyStatusText != null)
        readyStatusText.text = "Only host can start!";
  return;
        }

     if (PhotonNetwork.CurrentRoom.PlayerCount < minPlayers)
        {
        Debug.LogWarning($"Need at least {minPlayers} players to start!");
       if (readyStatusText != null)
         readyStatusText.text = $"Need {minPlayers} players minimum!";
            return;
        }

        // NEW: Check if all players are ready
        if (!AreAllPlayersReady())
     {
            Debug.LogWarning("All players must be ready before starting!");
   if (readyStatusText != null)
       readyStatusText.text = "All players must be ready!";
         return;
        }

        // Pick a random level from the set based on room difficulty
        int diff = (int)Difficulty.Normal;
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(DIFFICULTY_KEY, out object d))
        {
            if (d is int i) diff = Mathf.Clamp(i, 0, 2);
        }

        string[] pool = normalLevels;
        if (diff == (int)Difficulty.Easy && easyLevels != null && easyLevels.Length > 0) pool = easyLevels;
        else if (diff == (int)Difficulty.Hard && hardLevels != null && hardLevels.Length > 0) pool = hardLevels;
        else if ((pool == null || pool.Length == 0) && availableLevels != null && availableLevels.Length > 0) pool = availableLevels; // fallback

        selectedLevel = pool[Random.Range(0, pool.Length)];
        Debug.Log($"Starting game on level: {selectedLevel} (Difficulty: {(diff==0?"Easy":diff==1?"Normal":"Hard")})");

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        
    // Mark that we are transitioning to a multiplayer gameplay level to prevent auto-disconnect
    if (NetworkManager.Instance != null)
    {
        var nmType = typeof(NetworkManager);
        var field = nmType.GetField("transitioningToMultiplayerLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(NetworkManager.Instance, true);
        }
    }
    PhotonNetwork.LoadLevel(selectedLevel);
    }

    private void OnLeaveRoomButtonClicked()
    {
        if (NetworkManager.Instance != null)
      {
NetworkManager.Instance.LeaveRoom();
      }
  
   // Reset ready state
   isLocalPlayerReady = false;
      
// Clear dynamic player list
        ClearDynamicPlayerList();
    
     ShowLobbyPanel();
    }

    #endregion

    #region Ready System

    /// <summary>
    /// Helper to set the local player's ready state and update UI + Photon property.
    /// </summary>
    private void SetLocalReady(bool ready)
    {
        isLocalPlayerReady = ready;
        var props = new ExitGames.Client.Photon.Hashtable { { READY_STATE_KEY, isLocalPlayerReady } };
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
        UpdateReadyButton();
    }

    /// <summary>
    /// Check if all players in the room are ready
    /// </summary>
    private bool AreAllPlayersReady()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // Host (Master Client) is always considered ready
 if (player.IsMasterClient)
 continue;

         // Check if player has ready property and it's true
            if (!player.CustomProperties.ContainsKey(READY_STATE_KEY))
          return false;

         bool playerReady = (bool)player.CustomProperties[READY_STATE_KEY];
         if (!playerReady)
      return false;
        }

        return true;
    }

    /// <summary>
    /// Get the number of ready players
    /// </summary>
    private int GetReadyPlayerCount()
    {
        int readyCount = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
    // Host is always considered ready
      if (player.IsMasterClient)
        {
     readyCount++;
           continue;
      }

  if (player.CustomProperties.ContainsKey(READY_STATE_KEY))
     {
        bool playerReady = (bool)player.CustomProperties[READY_STATE_KEY];
         if (playerReady)
     readyCount++;
  }
    }

        return readyCount;
    }

  /// <summary>
    /// Check if a specific player is ready
    /// </summary>
    private bool IsPlayerReady(Player player)
    {
        // Host is always ready
        if (player != null && player.IsMasterClient)
            return true;

        if (player != null && player.CustomProperties != null && player.CustomProperties.ContainsKey(READY_STATE_KEY))
        {
            object readyObj = player.CustomProperties[READY_STATE_KEY];
            if (readyObj is bool b) return b;
        }

        return false;
    }

    /// <summary>
    /// Update the ready button appearance
    /// </summary>
    private void UpdateReadyButton()
    {
        if (readyButton == null) return;

        // Hide ready button for host
 if (PhotonNetwork.IsMasterClient)
    {
            readyButton.gameObject.SetActive(false);
      return;
     }

 readyButton.gameObject.SetActive(true);

        if (readyButtonText != null)
  {
            readyButtonText.text = isLocalPlayerReady ? "UNREADY" : "READY";
        }

      // Change button color based on ready state
        ColorBlock colors = readyButton.colors;
        colors.normalColor = isLocalPlayerReady ? new Color(1f, 0.5f, 0.5f) : new Color(0.5f, 1f, 0.5f);
        readyButton.colors = colors;
    }

    #endregion

    #region Kick System

    /// <summary>
    /// Kick a player from the room (Host only)
    /// </summary>
    public void KickPlayer(Player playerToKick)
    {
        if (!PhotonNetwork.IsMasterClient)
     {
       Debug.LogWarning("Only the host can kick players!");
        return;
        }

        if (playerToKick == null || playerToKick.IsLocal)
        {
   Debug.LogWarning("Cannot kick yourself!");
    return;
        }

        Debug.Log($"[LobbyManager] Host kicking player: {playerToKick.NickName}");

        // Close the room for the kicked player using RPC
        PhotonView photonView = GetComponent<PhotonView>();
        if (photonView != null)
        {
            photonView.RPC("RPC_KickPlayer", playerToKick);
        }
      else
        {
 Debug.LogError("PhotonView component missing! Add PhotonView to LobbyManager.");
        }
}

    /// <summary>
    /// RPC called on the player being kicked
    /// </summary>
    [PunRPC]
    private void RPC_KickPlayer()
    {
      Debug.Log("[LobbyManager] You have been kicked from the room!");
  
        if (readyStatusText != null)
      {
        readyStatusText.text = "You have been kicked by the host!";
  readyStatusText.color = Color.red;
        }

    // Leave the room
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region Dynamic Player List

    /// <summary>
    /// Step 3: Update the dynamic player list (spawn/despawn entries)
    /// </summary>
    private void UpdateDynamicPlayerList()
    {
        if (playerListContainer == null || playerEntryPrefab == null)
        {
            // Dynamic player list not set up - skip
            if (playerListContainer == null)
            {
                Debug.LogWarning("[LobbyManager] playerListContainer is null! Assign it in the Inspector.");
            }
            if (playerEntryPrefab == null)
            {
                Debug.LogWarning("[LobbyManager] playerEntryPrefab is null! Assign it in the Inspector.");
            }
            return;
        }

        if (!PhotonNetwork.InRoom) return;
        
        // Ensure container is active
        if (!playerListContainer.gameObject.activeSelf)
        {
            playerListContainer.gameObject.SetActive(true);
            Debug.Log("[LobbyManager] Activated playerListContainer");
        }

        // Get sorted players
        Player[] sortedPlayers = PhotonNetwork.PlayerList;
        System.Array.Sort(sortedPlayers, (a, b) => a.ActorNumber.CompareTo(b.ActorNumber));

        bool isHost = PhotonNetwork.IsMasterClient;

        // Remove entries for players who left
        for (int i = playerEntries.Count - 1; i >= 0; i--)
        {
            if (playerEntries[i] == null)
            {
                playerEntries.RemoveAt(i);
                continue;
            }

            Player entryPlayer = playerEntries[i].GetPlayer();
            if (entryPlayer == null || !System.Array.Exists(sortedPlayers, p => p.ActorNumber == entryPlayer.ActorNumber))
            {
                // Player left - destroy entry
                Destroy(playerEntries[i].gameObject);
                playerEntries.RemoveAt(i);
                Debug.Log($"[LobbyManager] Removed player entry for departed player");
            }
        }

        // Add/update entries for current players
        for (int i = 0; i < sortedPlayers.Length; i++)
        {
            Player player = sortedPlayers[i];

            // Check if entry already exists
            PlayerListEntry existingEntry = playerEntries.Find(e =>
                   e != null && e.GetPlayer() != null && e.GetPlayer().ActorNumber == player.ActorNumber);

            if (existingEntry != null)
            {
                // Update existing entry
                bool playerIsHost = player.IsMasterClient;
                bool canKick = isHost && !player.IsLocal && !player.IsMasterClient;
                existingEntry.UpdateDisplay(playerIsHost, canKick);
            }
            else
            {
                // Create new entry
                GameObject entryObj = Instantiate(playerEntryPrefab, playerListContainer);
                
                // CRITICAL FIX: Ensure the instantiated object is active and properly scaled
                entryObj.SetActive(true);
                entryObj.transform.localScale = Vector3.one;
                
                // CRITICAL FIX: Fix any child objects with scale 0 (common prefab issue)
                // The prefab has a Canvas child with scale (0,0,0) which makes it invisible
                foreach (Transform child in entryObj.GetComponentsInChildren<Transform>(true))
                {
                    if (child.localScale == Vector3.zero)
                    {
                        Debug.LogWarning($"[LobbyManager] Found child '{child.name}' with scale 0 in prefab, fixing it");
                        child.localScale = Vector3.one;
                    }
                }

                // IMPORTANT: Get OUR custom PlayerListEntry (not the Photon demo one!)
                // The Photon demo one is in namespace Photon.Pun.Demo.Asteroids
                // Ours is in the global namespace
                PlayerListEntry entry = entryObj.GetComponent<PlayerListEntry>();
                
                // If not on root, try to find it in children
                if (entry == null)
                {
                    entry = entryObj.GetComponentInChildren<PlayerListEntry>();
                }

                // Double-check it's not the Photon demo version
                if (entry != null && entry.GetType().Namespace == "Photon.Pun.Demo.Asteroids")
                {
                    Debug.LogError("[LobbyManager] Prefab is using Photon Demo PlayerListEntry! Remove it and add the custom PlayerListEntry script instead.");
                    entry = null;
                }

                if (entry != null)
                {
                    bool playerIsHost = player.IsMasterClient;
                    bool canKick = isHost && !player.IsLocal && !player.IsMasterClient;

                    entry.Initialize(player, playerIsHost, canKick, OnKickButtonClicked);
                    playerEntries.Add(entry);

                    Debug.Log($"[LobbyManager] Added player entry for: {player.NickName} (Host: {playerIsHost}, CanKick: {canKick})");
                }
                else
                {
                    Debug.LogError($"[LobbyManager] PlayerListEntry component missing on prefab! GameObject: {entryObj.name}, Active: {entryObj.activeSelf}, Scale: {entryObj.transform.localScale}");
                    Destroy(entryObj);
                }
            }
        }
    }
    
    /// <summary>
    /// Clear all player entries
    /// </summary>
    private void ClearDynamicPlayerList()
    {
    foreach (var entry in playerEntries)
        {
     if (entry != null)
     {
           Destroy(entry.gameObject);
         }
        }
        
        playerEntries.Clear();
    }
    
    #endregion

    #region Room UI Updates

    private void UpdateRoomUI()
    {
        SetupDifficultyUI();
        if (!PhotonNetwork.InRoom) return;

        ShowRoomPanel();

        if (roomNameText != null)
    {
     roomNameText.text = $"Room: {PhotonNetwork.CurrentRoom.Name}";
 }

    // Update player list with ready status and kick buttons
 if (playerListText != null)
        {
            // Ensure the text component and its GameObject are active
            if (!playerListText.gameObject.activeSelf)
            {
                playerListText.gameObject.SetActive(true);
            }
            
            string playerList = $"Players ({PhotonNetwork.CurrentRoom.PlayerCount}/{maxPlayers}):\n\n";
 
// Sort players by actor number to maintain consistent order
        Player[] sortedPlayers = PhotonNetwork.PlayerList;
     System.Array.Sort(sortedPlayers, (a, b) => a.ActorNumber.CompareTo(b.ActorNumber));
     
            if (sortedPlayers.Length == 0)
            {
                playerList += "No players in room";
            }
            else
            {
                for (int i = 0; i < sortedPlayers.Length; i++)
                {
                    Player player = sortedPlayers[i];
                    
                    if (player == null) continue;
                    
     
                             // Use a simple bullet point character that works in all fonts
                    playerList += $"•� {player.NickName}";
 
                    if (player.IsMasterClient)
                        playerList += " [HOST]";
         
       
                    // Show ready status
                    bool playerReady = IsPlayerReady(player);
                    playerList += playerReady ? " ✓ READY" : " ✗ Not Ready";
       
                    playerList += "\n";
                }
            }
  
            playerListText.text = playerList;
            
            // Ensure text is visible (check color and enable raycast target if needed)
            if (playerListText.color.a < 0.1f)
            {
                Color textColor = playerListText.color;
                textColor.a = 1f;
                playerListText.color = textColor;
            }
        }
    
 // NEW: Update dynamic player list (if set up)
        UpdateDynamicPlayerList();
        
      // Update kick button array (if using static buttons instead)
        UpdateKickButtons();

        // Update start button - only enabled if host and all players ready
   if (startGameButton != null)
        {
 bool canStart = PhotonNetwork.IsMasterClient 
   && PhotonNetwork.CurrentRoom.PlayerCount >= minPlayers
  && AreAllPlayersReady();
    startGameButton.interactable = canStart;
     }

  // Update ready button
    UpdateReadyButton();

 // Update ready status text
        if (readyStatusText != null)
   {
int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
   int readyPlayers = GetReadyPlayerCount();

if (currentPlayers < minPlayers)
         {
     readyStatusText.text = $"Waiting for players... ({currentPlayers}/{minPlayers} minimum)";
        readyStatusText.color = Color.yellow;
   }
 else if (readyPlayers < currentPlayers)
  {
    readyStatusText.text = $"Players ready: {readyPlayers}/{currentPlayers}";
     readyStatusText.color = Color.yellow;
       }
      else
  {
   readyStatusText.text = $"All players ready! ({currentPlayers}/{maxPlayers})";
  readyStatusText.color = Color.green;
 }
  }

        if (selectedLevelText != null)
        {
        selectedLevelText.text = "Random level will be selected";
        }
 }

    #endregion
    
    // Difficulty helpers
    private void SetupDifficultyUI()
    {
        bool isHost = PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom;

        if (difficultyDropdown != null)
        {
            // Always overwrite options to avoid seeing TMP default A/B/C
            difficultyDropdown.onValueChanged.RemoveListener(OnDifficultyDropdownChanged);
            difficultyDropdown.gameObject.SetActive(isHost);
            difficultyDropdown.interactable = isHost;

            difficultyDropdown.ClearOptions();
            var opts = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("Easy"),
                new TMP_Dropdown.OptionData("Normal"),
                new TMP_Dropdown.OptionData("Hard")
            };
            difficultyDropdown.AddOptions(opts);

            int diff = (int)Difficulty.Normal;
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(DIFFICULTY_KEY, out object val))
            {
                if (val is int i) diff = Mathf.Clamp(i, 0, 2);
            }

            difficultyDropdown.SetValueWithoutNotify(diff);

            // Resize the dropdown RectTransform to fit the longest option text
            ResizeDropdownToFit(difficultyDropdown, minWidth: 180f, sidePadding: 48f);

            // Place the dropdown just under the difficulty label within the room panel
            PositionDropdownUnderLabel(difficultyDropdown, difficultyLabel, verticalSpacing: 8f, leftPadding: 0f);

            if (isHost)
            {
                difficultyDropdown.onValueChanged.AddListener(OnDifficultyDropdownChanged);
            }
        }

        if (difficultyLabel != null)
        {
            int diff = (int)Difficulty.Normal;
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(DIFFICULTY_KEY, out object val))
            {
                if (val is int i) diff = Mathf.Clamp(i, 0, 2);
            }
            string text = diff == 0 ? "Easy" : diff == 1 ? "Normal" : "Hard";
            difficultyLabel.text = $"Difficulty: {text}";
            difficultyLabel.gameObject.SetActive(true);
        }
    }

    private static void PositionDropdownUnderLabel(TMP_Dropdown dropdown, TMP_Text label, float verticalSpacing, float leftPadding)
    {
        if (dropdown == null || label == null) return;

        var dropRt = dropdown.GetComponent<RectTransform>();
        var labelRt = label.GetComponent<RectTransform>();
        if (dropRt == null || labelRt == null) return;

        // Ensure same parent; if not, reparent into label's parent (room panel hierarchy)
        if (dropRt.parent != labelRt.parent)
        {
            dropRt.SetParent(labelRt.parent, worldPositionStays: false);
        }

        // Copy label anchors to keep consistent local space
        dropRt.anchorMin = labelRt.anchorMin;
        dropRt.anchorMax = labelRt.anchorMax;

        // Use top-left pivot for dropdown so we can align to label's left edge
        dropRt.pivot = new Vector2(0f, 1f);

        // Compute label's left X in its anchored space
        // anchoredPosition is at label's pivot; left = centerX - width * pivotX
        float labelLeftX = labelRt.anchoredPosition.x - (labelRt.sizeDelta.x * labelRt.pivot.x);

        // Compute dropdown top Y just below label: start from label's top edge (centerY + height*(1 - pivotY)) then subtract spacing
        float labelTopY = labelRt.anchoredPosition.y + (labelRt.sizeDelta.y * (1f - labelRt.pivot.y));
        float dropTopY = labelTopY - labelRt.sizeDelta.y - verticalSpacing;

        float x = labelLeftX + leftPadding;
        float y = dropTopY;

        dropRt.anchoredPosition = new Vector2(x, y);
    }

    private static void ResizeDropdownToFit(TMP_Dropdown dropdown, float minWidth, float sidePadding)
    {
        if (dropdown == null) return;

        // Determine the longest option text
        float maxPreferred = 0f;
        TMP_Text measure = dropdown.captionText != null ? dropdown.captionText : dropdown.itemText;
        if (measure == null) return;

        // Use current font settings to measure preferred width for each option
        foreach (var opt in dropdown.options)
        {
            if (string.IsNullOrEmpty(opt.text)) continue;
            var size = measure.GetPreferredValues(opt.text);
            maxPreferred = Mathf.Max(maxPreferred, size.x);
        }

        // Also ensure current caption text fits (in case it has prefix/icon etc.)
        if (dropdown.captionText != null && !string.IsNullOrEmpty(dropdown.captionText.text))
        {
            var size = dropdown.captionText.GetPreferredValues(dropdown.captionText.text);
            maxPreferred = Mathf.Max(maxPreferred, size.x);
        }

        // Apply width with padding and a minimum
        float targetWidth = Mathf.Max(minWidth, maxPreferred + sidePadding);

        // Resize the collapsed dropdown control (keep existing anchors; we'll position it relative to the label)
        var rt = dropdown.GetComponent<RectTransform>();
        if (rt != null)
        {
            var sz = rt.sizeDelta;
            sz.x = targetWidth;
            rt.sizeDelta = sz;
        }

        // Ensure the caption label stretches to the dropdown width
        if (dropdown.captionText != null)
        {
            var labelRt = dropdown.captionText.GetComponent<RectTransform>();
            if (labelRt != null)
            {
                labelRt.anchorMin = new Vector2(0, 0);
                labelRt.anchorMax = new Vector2(1, 1);
                labelRt.offsetMin = new Vector2(10f, 0f); // small left padding
                labelRt.offsetMax = new Vector2(-30f, 0f); // leave room for arrow
            }
        }

        // Resize the dropdown template (the expanded list) to match width
        var template = dropdown.template; // RectTransform
        if (template != null)
        {
            var tSz = template.sizeDelta;
            tSz.x = targetWidth;
            template.sizeDelta = tSz;

            // Try to also resize the viewport/content rects if present
            // Common hierarchy: Template (RectTransform) -> Viewport -> Content -> Item
            // We don't rely on names; sizes generally stretch from template, but ensure item label stretches.
        }

        // Ensure the item label (each option) also stretches properly and add space for checkmark
        if (dropdown.itemText != null)
        {
            var itemLabelRt = dropdown.itemText.GetComponent<RectTransform>();
            if (itemLabelRt != null)
            {
                itemLabelRt.anchorMin = new Vector2(0, 0);
                itemLabelRt.anchorMax = new Vector2(1, 1);
                itemLabelRt.offsetMin = new Vector2(24f, 0f); // more left padding for checkmark icon
                itemLabelRt.offsetMax = new Vector2(-40f, 0f); // more right padding for scrollbar/arrow
            }
        }

        // Try to pad the item checkmark if present
        var item = dropdown.itemText != null ? dropdown.itemText.transform.parent as RectTransform : null;
        if (item != null)
        {
            // Expected children: Item Background, Item Checkmark, Item Label
            for (int i = 0; i < item.childCount; i++)
            {
                var child = item.GetChild(i) as RectTransform;
                if (child == null) continue;
                if (child.name.ToLower().Contains("checkmark"))
                {
                    // Nudge checkmark a bit to the left
                    var pos = child.anchoredPosition;
                    pos.x = Mathf.Min(pos.x, 10f);
                    child.anchoredPosition = pos;
                }
            }
        }
    }

    private void OnDifficultyDropdownChanged(int index)
    {
        if (!PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom) return;
        int diff = Mathf.Clamp(index, 0, 2);
        var props = new ExitGames.Client.Photon.Hashtable { { DIFFICULTY_KEY, diff } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        if (difficultyLabel != null)
        {
            string text = diff == 0 ? "Easy" : diff == 1 ? "Normal" : "Hard";
            difficultyLabel.text = $"Difficulty: {text}";
        }
    }

    #region Photon Callbacks
    
    /// <summary>
    /// Called when successfully joined the lobby
    /// </summary>
    public override void OnJoinedLobby()
    {
        Debug.Log("[LobbyManager] Successfully joined lobby - showing lobby panel");
        ShowLobbyPanel();
        
        // Update status text if available
        if (statusText != null)
        {
            statusText.text = "Connected to Lobby";
            statusText.color = Color.green;
        }
    }
    
    /// <summary>
    /// Called when disconnected from Photon
    /// </summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"[LobbyManager] Disconnected: {cause}");
        ShowConnectionPanel();
        
        // Reset ready state
        isLocalPlayerReady = false;
        
        // Reset manual connection flag
        hasManuallyConnected = false;
        
        // Clear dynamic player list
        ClearDynamicPlayerList();
    }
    
    /// <summary>
    /// Called when a player joins the room
    /// </summary>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[LobbyManager] Player joined: {newPlayer.NickName}");
      
        // CRITICAL: No cosmetic assignment needed - all players share the same character
        // Character selection/assignment has been removed from multiplayer
      
   // Update UI
   if (PhotonNetwork.InRoom)
     {
    UpdateRoomUI();
        }
    }
    
    /// <summary>
    /// Called when a player leaves the room
    /// </summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[LobbyManager] Player left: {otherPlayer.NickName}");
   
  // Update UI
   if (PhotonNetwork.InRoom)
   {
            UpdateRoomUI();
        }
    }
    
    /// <summary>
    /// Called when player properties are updated (e.g., ready status)
    /// </summary>
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
  // Check if ready status changed
        if (changedProps.ContainsKey("IsReady"))
        {
            Debug.Log($"[LobbyManager] {targetPlayer.NickName} ready status changed");
            
    // Update UI
            if (PhotonNetwork.InRoom)
  {
              UpdateRoomUI();
  }
        }
    }
    
    /// <summary>
    /// Called when master client changes (host leaves)
    /// </summary>
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"[LobbyManager] New host: {newMasterClient.NickName}");

        // If we became the host, ensure we are marked ready
        if (newMasterClient == PhotonNetwork.LocalPlayer)
        {
            SetLocalReady(true);
        }
    
        // Update UI (kick buttons visibility changes)
        if (PhotonNetwork.InRoom)
        {
            UpdateRoomUI();
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        // If we are the host upon join, mark ready immediately
        if (PhotonNetwork.IsMasterClient)
        {
            SetLocalReady(true);
        }
        // Ensure room UI is shown (Dropdown placed under label)
        ShowRoomPanel();
        UpdateRoomUI();
    }
    
    /// <summary>
    /// Called when room creation fails
    /// </summary>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[LobbyManager] Failed to create room: {message} (ReturnCode: {returnCode})");
        
        if (statusText != null)
        {
            statusText.text = $"Failed to create room: {message}";
            statusText.color = Color.red;
        }
        
        // After a failed create, Photon may have disconnected us from the lobby
        // Rejoin the lobby if we're connected but not in lobby
        if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InLobby && !PhotonNetwork.InRoom)
        {
            Debug.Log("[LobbyManager] Rejoining lobby after failed room creation...");
            StartCoroutine(RejoinLobbyAfterDelay(0.5f));
        }
        
        // Stay in lobby panel
        ShowLobbyPanel();
    }
    
    /// <summary>
    /// Called when room join fails
    /// </summary>
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[LobbyManager] Failed to join room: {message} (ReturnCode: {returnCode})");
        
        string errorMessage = message;
        
        // Provide user-friendly error messages
        switch (returnCode)
        {
            case 32760: // GameDoesNotExist
                errorMessage = "Room does not exist! Check the room name and try again.";
                break;
            case 32758: // GameFull
                errorMessage = "Room is full! Maximum players reached.";
                break;
            case 32757: // GameClosed
                errorMessage = "Room is closed! The host may have started the game.";
                break;
            default:
                errorMessage = $"Failed to join: {message}";
                break;
        }
        
        if (statusText != null)
        {
            statusText.text = errorMessage;
            statusText.color = Color.red;
        }
        
        // After a failed join, Photon may have disconnected us from the lobby
        // Rejoin the lobby if we're connected but not in lobby
        if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InLobby && !PhotonNetwork.InRoom)
        {
            Debug.Log("[LobbyManager] Rejoining lobby after failed room join...");
            if (NetworkManager.Instance != null)
            {
                // NetworkManager will handle rejoining the lobby
                StartCoroutine(RejoinLobbyAfterDelay(0.5f));
            }
        }
        
        // Stay in lobby panel
        ShowLobbyPanel();
    }
    
    /// <summary>
    /// Rejoin lobby after a short delay (used after failed room operations)
    /// </summary>
    private IEnumerator RejoinLobbyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InLobby && !PhotonNetwork.InRoom)
        {
            Debug.Log("[LobbyManager] Attempting to rejoin lobby...");
            PhotonNetwork.JoinLobby();
        }
    }
    
    #endregion

    #region Kick Button Callbacks
    
    /// <summary>
    /// Called when kick button is clicked from the button array
    /// </summary>
    private void OnKickButtonClicked(int playerIndex)
    {
        if (!PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom) return;
        
        if (playerIndex < 0 || playerIndex >= PhotonNetwork.PlayerList.Length) return;
 
        Player targetPlayer = PhotonNetwork.PlayerList[playerIndex];
        
        // Don't allow kicking yourself
     if (targetPlayer.IsLocal) return;
  
        // Show confirmation panel
   if (kickConfirmationPanel != null)
     {
        playerPendingKick = targetPlayer;
            
            if (kickConfirmationText != null)
   {
    kickConfirmationText.text = $"Kick {targetPlayer.NickName}?";
  }
       
          kickConfirmationPanel.SetActive(true);
        }
        else
        {
    // No confirmation panel - kick immediately
            KickPlayer(targetPlayer);
        }
    }
    
 /// <summary>
    /// Called when dynamic player entry kick button is clicked
    /// </summary>
    private void OnKickButtonClicked(Player playerToKick)
    {
        if (!PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom) return;
  
        if (playerToKick == null || playerToKick.IsLocal) return;
   
   // Show confirmation panel
        if (kickConfirmationPanel != null)
      {
        playerPendingKick = playerToKick;
            
            if (kickConfirmationText != null)
      {
      kickConfirmationText.text = $"Kick {playerToKick.NickName}?";
            }
      
            kickConfirmationPanel.SetActive(true);
}
        else
   {
            // No confirmation panel - kick immediately
         KickPlayer(playerToKick);
      }
  }
    
    /// <summary>
    /// Confirm kick button clicked
    /// </summary>
    private void OnConfirmKickClicked()
    {
        if (playerPendingKick != null)
        {
    KickPlayer(playerPendingKick);
    playerPendingKick = null;
     }
        
      if (kickConfirmationPanel != null)
        {
          kickConfirmationPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Cancel kick button clicked
    /// </summary>
    private void OnCancelKickClicked()
    {
playerPendingKick = null;
        
        if (kickConfirmationPanel != null)
        {
       kickConfirmationPanel.SetActive(false);
        }
    }
  
    /// <summary>
    /// Update the kick button array (static buttons)
    /// Only shows kick buttons to the host
    /// </summary>
    private void UpdateKickButtons()
    {
        if (playerKickButtons == null || playerKickButtons.Length == 0) return;
      
        bool isHost = PhotonNetwork.IsMasterClient;
    Player[] sortedPlayers = PhotonNetwork.PlayerList;
        System.Array.Sort(sortedPlayers, (a, b) => a.ActorNumber.CompareTo(b.ActorNumber));
        
        for (int i = 0; i < playerKickButtons.Length; i++)
        {
            if (playerKickButtons[i] == null) continue;
          
 if (i < sortedPlayers.Length)
     {
                Player player = sortedPlayers[i];
      
            // Only show kick button if host can kick this player
        bool canKick = isHost && !player.IsLocal && !player.IsMasterClient;
            
            // Hide button completely for non-hosts, or if this player can't be kicked
            playerKickButtons[i].gameObject.SetActive(canKick);
      
  // Update name text (only if button is visible)
         if (canKick && playerNameTexts != null && i < playerNameTexts.Length && playerNameTexts[i] != null)
    {
          playerNameTexts[i].text = player.NickName;
                }
            
             // Enable button (should always be true if visible, but set it anyway)
      if (canKick)
            {
                playerKickButtons[i].interactable = true;
            }
 }
            else
          {
      // Hide unused buttons
 playerKickButtons[i].gameObject.SetActive(false);
            }
   }
 }
    
    #endregion
}
