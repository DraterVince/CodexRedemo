using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private byte maxPlayersPerRoom = 4;

    [Header("Multiplayer Scene Detection")]
    [Tooltip("Scenes that should use multiplayer (lobby, multiplayer level select, etc.)")]
    [SerializeField]
    private string[] multiplayerScenes = new string[]
    {
        "MultiplayerLobby",
        "MultiplayerLevelSelect",
        "MultiplayerGameplay",
        "Multiplayer" // Catch-all for any scene with "Multiplayer" in name
    };

    public bool IsConnectedToMaster = false;
    public bool IsInLobby = false;
    public string CurrentRoomName = "";

    private bool shouldBeConnected = false;

    // When loading a multiplayer gameplay level via PhotonNetwork.LoadLevel from a multiplayer scene,
    // set this to true to avoid auto-disconnect on the next scene load even if the scene name doesn't look like a multiplayer scene.
    private bool transitioningToMultiplayerLevel = false;
    
    [Header("Connection Retry Settings")]
    [SerializeField] private int maxLobbyJoinRetries = 3;
    [SerializeField] private float lobbyJoinRetryDelay = 2f;
    [SerializeField] private int maxConnectionRetries = 3;
    [SerializeField] private float connectionRetryDelay = 3f;
    
    private int lobbyJoinRetryCount = 0;
    private int connectionRetryCount = 0;
    private Coroutine lobbyJoinRetryCoroutine = null;
    private Coroutine connectionRetryCoroutine = null;

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

        PhotonNetwork.AutomaticallySyncScene = true;

        // Subscribe to scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Don't auto-connect on scene load - user must manually click connect
        // This prevents auto-connecting when entering multiplayer scenes
        // CheckAndConnectIfNeeded(); // Disabled - require manual connection
    }

    /// <summary>
    /// Called when a new scene is loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Don't auto-connect on scene load - user must manually click connect
        // Only check connection if already in a room (for scene syncing)
        if (PhotonNetwork.InRoom)
        {
            Debug.Log($"[NetworkManager] Scene loaded while in room - staying connected for scene sync");
        }

        // Ensure scene stays in sync with Master Client when already in a Photon room
        if (PhotonNetwork.InRoom && PhotonNetwork.AutomaticallySyncScene && !PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom != null &&
                PhotonNetwork.CurrentRoom.CustomProperties != null &&
                PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("curScn", out object sceneId))
            {
                bool isMatchingScene = false;

                if (sceneId is string sceneName)
                {
                    isMatchingScene = string.Equals(sceneName, scene.name);
                }
                else if (sceneId is int sceneIndex)
                {
                    isMatchingScene = sceneIndex == scene.buildIndex;
                }

                if (!isMatchingScene)
                {
                    Debug.LogWarning($"[NetworkManager] Scene mismatch detected (local: {scene.name}). Resyncing to master scene: {sceneId}");

                    if (sceneId is string targetSceneName)
                    {
                        SceneManager.LoadScene(targetSceneName);
                    }
                    else if (sceneId is int targetSceneIndex)
                    {
                        SceneManager.LoadScene(targetSceneIndex);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if current scene needs multiplayer and connect/disconnect accordingly
    /// </summary>
    private void CheckAndConnectIfNeeded()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool isMultiplayerScene = IsMultiplayerScene(currentScene) || transitioningToMultiplayerLevel;

        Debug.Log($"[NetworkManager] Scene: {currentScene}, Is Multiplayer: {isMultiplayerScene}");

        // If we're already in a Photon room, STAY CONNECTED regardless of scene name
        // The Master Client is managing the scene loading via PhotonNetwork.LoadLevel
        if (PhotonNetwork.InRoom)
        {
            // If we're transitioning to a multiplayer level, keep connection
            Debug.Log($"[NetworkManager] Already in room '{PhotonNetwork.CurrentRoom.Name}' - staying connected");
            transitioningToMultiplayerLevel = false; // reset after first check
            return;
        }

        if (isMultiplayerScene && !PhotonNetwork.IsConnected)
        {
            // We're in a multiplayer scene and not connected - connect now
            shouldBeConnected = true;
            ConnectToPhoton();
        }
        else if (!isMultiplayerScene && PhotonNetwork.IsConnected)
        {
            // We're in a single-player scene but still connected (and NOT in a room) - disconnect
            shouldBeConnected = false;
            Disconnect();
        }
    }

    /// <summary>
    /// Check if a scene name is a multiplayer scene
    /// </summary>
    private bool IsMultiplayerScene(string sceneName)
    {
        foreach (string multiplayerScene in multiplayerScenes)
        {
            if (sceneName.Equals(multiplayerScene, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Also check if scene name contains "Multiplayer" or "Lobby"
        if (sceneName.Contains("Multiplayer") || sceneName.Contains("Lobby") || sceneName.Contains("PvP"))
        {
            return true;
        }

        return false;
    }

    public void ConnectToPhoton()
    {
        // Show loading screen when connecting to lobby
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.ShowLoadingScreen("Connecting to lobby...");
        }
        
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("[NetworkManager] Already connected to Photon");
            // If connected but not in lobby, try joining lobby
            if (!PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady)
            {
                JoinLobbyWithRetry();
            }
            return;
        }
        
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("[NetworkManager] Already connected and ready");
            // If connected but not in lobby, try joining lobby
            if (!PhotonNetwork.InLobby)
            {
                JoinLobbyWithRetry();
            }
            return;
        }
        
        // Reset retry counter when manually connecting
        connectionRetryCount = 0;
        
        Debug.Log("[NetworkManager] Connecting to Photon...");
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }
    
    /// <summary>
    /// Retry connection with exponential backoff
    /// </summary>
    private void RetryConnection()
    {
        if (connectionRetryCount >= maxConnectionRetries)
        {
            Debug.LogError($"[NetworkManager] Max connection retries ({maxConnectionRetries}) exceeded. Please check your internet connection.");
            shouldBeConnected = false;
            return;
        }
        
        connectionRetryCount++;
        float delay = connectionRetryDelay * connectionRetryCount; // Exponential backoff
        
        Debug.Log($"[NetworkManager] Retrying connection ({connectionRetryCount}/{maxConnectionRetries}) in {delay} seconds...");
        
        if (connectionRetryCoroutine != null)
        {
            StopCoroutine(connectionRetryCoroutine);
        }
        connectionRetryCoroutine = StartCoroutine(RetryConnectionCoroutine(delay));
    }
    
    private IEnumerator RetryConnectionCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (!PhotonNetwork.IsConnected && shouldBeConnected)
        {
            Debug.Log("[NetworkManager] Retrying connection...");
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void Disconnect()
    {
        if (PhotonNetwork.IsConnected)
        {
            // Log stack trace to see what's calling this
            Debug.LogWarning($"[NetworkManager] Disconnect() called! In room: {PhotonNetwork.InRoom}. Stack trace:\n{System.Environment.StackTrace}");
            PhotonNetwork.Disconnect();
            Debug.Log("[NetworkManager] Disconnecting from Photon...");
        }
    }

    public void CreateRoom(string roomName)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("[NetworkManager] Cannot create room - not connected to Photon! Call ConnectToPhoton() first.");
            return;
        }
        
        if (PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[NetworkManager] Already in a room. Leave current room first.");
            return;
        }
        
        if (string.IsNullOrEmpty(roomName) || string.IsNullOrWhiteSpace(roomName))
        {
            roomName = "Room_" + Random.Range(1000, 9999);
            Debug.LogWarning($"[NetworkManager] Room name was empty, generated: {roomName}");
        }

        RoomOptions roomOptions = new RoomOptions
        {
            // You can extend CustomRoomPropertiesForLobby to include Difficulty for lobby filtering later
            CustomRoomPropertiesForLobby = new string[] { "Difficulty" },
            MaxPlayers = maxPlayersPerRoom,
            IsVisible = true, // Make room visible in lobby
            IsOpen = true, // Allow players to join
            PublishUserId = true // Publish user IDs for better matching
        };

        Debug.Log($"[NetworkManager] Creating room: '{roomName}' (Max: {maxPlayersPerRoom}, Visible: {roomOptions.IsVisible}, Open: {roomOptions.IsOpen})");
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinRoom(string roomName)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("[NetworkManager] Cannot join room - not connected to Photon! Call ConnectToPhoton() first.");
            return;
        }
        
        if (PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[NetworkManager] Already in a room. Leave current room first.");
            return;
        }
        
        if (!string.IsNullOrEmpty(roomName) && !string.IsNullOrWhiteSpace(roomName))
        {
            Debug.Log($"[NetworkManager] Attempting to join room: '{roomName}'");
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Debug.LogWarning("[NetworkManager] Room name is empty or whitespace!");
        }
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            // Log stack trace to see what's calling this
            Debug.LogWarning($"[NetworkManager] LeaveRoom() called! Stack trace:\n{System.Environment.StackTrace}");
            PhotonNetwork.LeaveRoom();
            Debug.Log("Leaving room...");
        }
    }

    public void StartGame(string gameSceneName = "GameScene")
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"Starting game, loading scene: {gameSceneName}");
            PhotonNetwork.LoadLevel(gameSceneName);
        }
        else
        {
            Debug.LogWarning("Only the Master Client can start the game!");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("? Connected to Photon Master Server");
        IsConnectedToMaster = true;
        
        // Reset retry counters on successful connection
        connectionRetryCount = 0;
        if (connectionRetryCoroutine != null)
        {
            StopCoroutine(connectionRetryCoroutine);
            connectionRetryCoroutine = null;
        }

        // Update loading message
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.ShowLoadingScreen("Joining lobby...");
        }

        // Join lobby - this will be retried if it fails
        JoinLobbyWithRetry();
    }
    
    /// <summary>
    /// Join lobby with retry logic
    /// </summary>
    private void JoinLobbyWithRetry()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("[NetworkManager] Cannot join lobby - not connected and ready");
            return;
        }
        
        Debug.Log("[NetworkManager] Attempting to join lobby...");
        bool success = PhotonNetwork.JoinLobby();
        
        if (!success)
        {
            Debug.LogWarning("[NetworkManager] JoinLobby() returned false, will check timeout");
        }
        
        // Start timeout check - if we don't get OnJoinedLobby callback within the delay, retry
        if (lobbyJoinRetryCoroutine != null)
        {
            StopCoroutine(lobbyJoinRetryCoroutine);
        }
        lobbyJoinRetryCoroutine = StartCoroutine(CheckLobbyJoinTimeout());
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"? Disconnected from Photon: {cause}");
        
        // Hide loading screen on disconnect
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.HideLoadingScreen();
        }
        IsConnectedToMaster = false;
        IsInLobby = false;
        
        // Stop any retry coroutines
        if (lobbyJoinRetryCoroutine != null)
        {
            StopCoroutine(lobbyJoinRetryCoroutine);
            lobbyJoinRetryCoroutine = null;
        }
        if (connectionRetryCoroutine != null)
        {
            StopCoroutine(connectionRetryCoroutine);
            connectionRetryCoroutine = null;
        }
        
        // If we should be connected and it wasn't a manual disconnect, retry connection
        if (shouldBeConnected && cause != DisconnectCause.DisconnectByClientLogic)
        {
            Debug.Log($"[NetworkManager] Unexpected disconnect ({cause}), will retry connection...");
            RetryConnection();
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("? Joined Photon Lobby");
        IsInLobby = true;
        
        // Reset retry counter on successful join
        lobbyJoinRetryCount = 0;
        if (lobbyJoinRetryCoroutine != null)
        {
            StopCoroutine(lobbyJoinRetryCoroutine);
            lobbyJoinRetryCoroutine = null;
        }
        
        // Hide loading screen when successfully joined lobby
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.HideLoadingScreen();
        }
    }
    
    /// <summary>
    /// Check if lobby join failed by checking if we're still not in lobby after a timeout
    /// </summary>
    private IEnumerator CheckLobbyJoinTimeout()
    {
        yield return new WaitForSeconds(lobbyJoinRetryDelay);
        
        // If we're connected but not in lobby, the join likely failed
        if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InLobby)
        {
            Debug.LogWarning("[NetworkManager] Lobby join appears to have failed - not in lobby after timeout");
            
            // Retry joining lobby if we haven't exceeded max retries
            if (lobbyJoinRetryCount < maxLobbyJoinRetries)
            {
                lobbyJoinRetryCount++;
                Debug.Log($"[NetworkManager] Retrying lobby join ({lobbyJoinRetryCount}/{maxLobbyJoinRetries})...");
                JoinLobbyWithRetry();
            }
            else
            {
                Debug.LogError($"[NetworkManager] Max lobby join retries ({maxLobbyJoinRetries}) exceeded. Connection may be unstable.");
                IsInLobby = false;
            }
        }
        else if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("[NetworkManager] Cannot retry lobby join - not connected and ready. Retrying connection...");
            RetryConnection();
        }
    }

    public override void OnLeftLobby()
    {
        Debug.Log("Left Photon Lobby");
        IsInLobby = false;
        
        // Stop retry coroutine if leaving lobby
        if (lobbyJoinRetryCoroutine != null)
        {
            StopCoroutine(lobbyJoinRetryCoroutine);
            lobbyJoinRetryCoroutine = null;
        }
    }

    public override void OnCreatedRoom()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            Debug.Log($"? Room Created: {PhotonNetwork.CurrentRoom.Name}");
            Debug.Log($"[NetworkManager] Room Properties - Visible: {PhotonNetwork.CurrentRoom.IsVisible}, Open: {PhotonNetwork.CurrentRoom.IsOpen}, MaxPlayers: {PhotonNetwork.CurrentRoom.MaxPlayers}");
            CurrentRoomName = PhotonNetwork.CurrentRoom.Name;
        }
        else
        {
            Debug.LogError("[NetworkManager] OnCreatedRoom called but CurrentRoom is null!");
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"? Failed to create room: {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"? Failed to join room: {message}");
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left the room");
        CurrentRoomName = "";
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"New Master Client: {newMasterClient.NickName}");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        if (PhotonNetwork.CurrentRoom != null)
        {
            CurrentRoomName = PhotonNetwork.CurrentRoom.Name;
            Debug.Log($"? Joined Room: {CurrentRoomName}");
            Debug.Log($"Players in room: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
        }

        // If Master Client, ensure scene is synced
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.AutomaticallySyncScene)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log($"[NetworkManager] Master Client - syncing scene: {currentScene}");
            // Scene will be synced automatically via AutomaticallySyncScene
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log($"Player joined: {newPlayer.NickName} (Total: {PhotonNetwork.CurrentRoom.PlayerCount})");

        // If we're Master Client and new player joins, ensure they get the current scene
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.AutomaticallySyncScene)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log($"[NetworkManager] New player joined - current scene is: {currentScene}");
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log($"Player left: {otherPlayer.NickName} (Total: {PhotonNetwork.CurrentRoom?.PlayerCount ?? 0})");
    }

    public void SetPlayerNickname(string nickname)
    {
        if (!string.IsNullOrEmpty(nickname))
        {
            PhotonNetwork.NickName = nickname;
            Debug.Log($"Nickname set to: {nickname}");
        }
    }

    public string GetConnectionStatus()
    {
        if (!PhotonNetwork.IsConnected)
            return "Disconnected";

        if (PhotonNetwork.InRoom)
            return $"In Room: {PhotonNetwork.CurrentRoom.Name}";

        if (PhotonNetwork.InLobby)
            return "In Lobby";

        if (PhotonNetwork.IsConnectedAndReady)
            return "Connected";

        return "Connecting...";
    }
}