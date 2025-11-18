using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
using Photon.Realtime;
#endif

/// <summary>
/// Manages shared multiplayer gameplay with:
/// - Shared HP pool for all players
/// - Character switching with slide animations
/// - Turn-based card playing
/// - Automatic turn progression
/// </summary>
public class SharedMultiplayerGameManager : MonoBehaviourPunCallbacks
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true; // Toggle in Inspector to enable/disable console logs

    [Header("Shared Health System")]
    [SerializeField] private float sharedMaxHealth = 100f;
    [SerializeField] private Image sharedHealthBar;
    [SerializeField] private TextMeshProUGUI sharedHealthText;

    [Header("Difficulty - Shared HP by level (editable)")]
    [Tooltip("Shared HP when room Difficulty = Easy")] [SerializeField]
    private float easySharedHP = 10f;
    [Tooltip("Shared HP when room Difficulty = Normal")] [SerializeField]
    private float normalSharedHP = 5f;
    [Tooltip("Shared HP when room Difficulty = Hard")] [SerializeField]
    private float hardSharedHP = 3f;
    
    [Header("Character Display")]
    [Tooltip("Position where multiplayer player characters are displayed when it's their turn. This is where characters appear after sliding in from the right.")]
    [SerializeField] private Transform characterDisplayPosition;
    // Reserved for future use
    #pragma warning disable CS0414
    [SerializeField] private float _characterSlideSpeed = 5f;
    #pragma warning restore CS0414
    [Tooltip("Initial spawn position for multiplayer characters (off-screen left). Characters are instantiated here, then slide to characterDisplayPosition when shown.")]
    [SerializeField] private Vector3 offScreenLeft = new Vector3(-15f, 0f, 0f);
    [Tooltip("Position where characters slide in from (off-screen right). Characters slide from here to characterDisplayPosition when switching turns.")]
    [SerializeField] private Vector3 offScreenRight = new Vector3(15f, 0f, 0f);
    [SerializeField] private string idleAnimationTrigger = "Idle";
    [SerializeField] private string idleAnimationStateName = "Idle"; // Fallback: use state name if trigger doesn't exist
    [SerializeField] private bool useIdleTrigger = false; // Set to true if using trigger, false if using state name
    [SerializeField] private string attackAnimationTrigger = "Attack";
  
    [Header("Turn Display")]
    [SerializeField] private TextMeshProUGUI currentTurnText;
    [SerializeField] private GameObject yourTurnIndicator;
    [SerializeField] private GameObject waitingIndicator;
    
    [Header("Player List UI")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerListItemPrefab;
    
    [Header("Game Over (Multiplayer)")]
    [SerializeField] private GameObject gameOverPanel; // Multiplayer game over panel
    [SerializeField] private GameObject victoryPanel; // Multiplayer victory panel
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button nextLevelButton; // Next Level button (host only, in victory panel)
    [SerializeField] private Button returnToLobbyButton; // Return to Lobby button (in both panels)
    
    [Header("Pause Panel (Multiplayer)")]
    [SerializeField] private GameObject pausePanel; // Multiplayer pause panel (no level select)
    [SerializeField] private Button resumeButton; // Resume button
    [SerializeField] private Button returnToLobbyFromPauseButton; // Return to lobby from pause
    
    [Header("Expected Output Panel Sync")]
    [SerializeField] private AnimatedPanel expectedOutputPanel; // Reference to expected output panel for sync
    
    [Header("Music")]
    [SerializeField] private bool playBattleMusicOnStart = true;
    
    [Header("References")]
 [SerializeField] private PlayCardButton playCardButton;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private EnemyManager enemyManager;
    
    // Game state
    private float currentSharedHealth;
    private bool isGameOver = false;
    private new PhotonView photonView;
private Dictionary<int, GameObject> playerCharacters = new Dictionary<int, GameObject>();
    private GameObject currentCharacterInstance;
    private bool isSwitchingCharacter = false;
    private CodexMultiplayerIntegration turnSystem;
    
    // Animation sync: delay health UI updates until attack animations complete
    private bool delayHealthUIUpdate = false;
    
    /// <summary>
    /// Conditional debug logging - only logs if enableDebugLogs is true
    /// </summary>
    private void Log(string message)
    {
        if (enableDebugLogs)
        {
 Debug.Log($"[SharedMultiplayerGameManager] {message}");
        }
    }
    
    private void LogWarning(string message)
    {
        if (enableDebugLogs)
        {
  Debug.LogWarning($"[SharedMultiplayerGameManager] {message}");
        }
    }
    
    private void LogError(string message)
    {
        // Always log errors (critical issues)
        Debug.LogError($"[SharedMultiplayerGameManager] {message}");
    }
    
  private void Awake()
{
        photonView = GetComponent<PhotonView>();
        
        if (photonView == null)
        {
            LogError("CRITICAL: PhotonView component is missing! Add a PhotonView component to this GameObject.");
            enabled = false;
            return;
        }
        
   turnSystem = GetComponent<CodexMultiplayerIntegration>();
        
        if (turnSystem == null)
        {
    LogWarning("CodexMultiplayerIntegration not found. Add it to the same GameObject!");
     }
    }
    
    private void Start()
    {
        // Validate PhotonView exists
        if (photonView == null)
        {
            LogError("CRITICAL: PhotonView is null! Cannot use multiplayer features.");
            HideMultiplayerUI();
            enabled = false;
            return;
        }
        
      if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            LogWarning("Not in Photon room. Disabling.");
 HideMultiplayerUI();
enabled = false;
         return;
        }
     
        // Setup multiplayer UI buttons
        SetupMultiplayerUI();
        
        // Setup pause system
        SetupPauseSystem();
        
        // Start battle music
        if (playBattleMusicOnStart)
        {
            StartBattleMusic();
        }
        
           if (turnSystem != null)
        {
            turnSystem.OnTurnChanged += OnTurnChanged;
        }
        else
        {
            LogWarning("Turn system is NULL!");
        }
        
        InitializeUI();
        UpdatePlayerListUI();
        
        // CRITICAL: In multiplayer, all players share the same character
        // No character loading/switching needed - just use PlayCardButton.playerCharacter
        if (PhotonNetwork.IsMasterClient)
        {
            // Apply difficulty-chosen shared HP before initializing
            ApplyDifficultySharedHP();
            InitializeSharedHealth();
            // Initialize turn system immediately - no character loading needed
            if (turnSystem != null)
            {
                turnSystem.InitializeTurnSystem();
            }
            else
            {
                LogError("Cannot initialize - turn system component is missing!");
            }
        }
    }
    
    private void HideMultiplayerUI()
    {
        if (currentTurnText != null) currentTurnText.gameObject.SetActive(false);
        if (yourTurnIndicator != null) yourTurnIndicator.SetActive(false);
        if (waitingIndicator != null) waitingIndicator.SetActive(false);
        if (playerListContainer != null) playerListContainer.gameObject.SetActive(false);
    }
    
    private void InitializeUI()
    {
        if (currentTurnText != null)
        {
            currentTurnText.gameObject.SetActive(true);
      }
 else
        {
  LogWarning("Current turn text not assigned!");
        }
        
        if (yourTurnIndicator != null)
        {
 yourTurnIndicator.SetActive(false);
        }
        else
     {
          LogWarning("Your turn indicator not assigned!");
        }
        
        if (waitingIndicator != null)
        {
      waitingIndicator.SetActive(false);
        }
        else
        {
            LogWarning("Waiting indicator not assigned!");
        }
        
        if (playerListContainer != null)
  {
 playerListContainer.gameObject.SetActive(true);
        }
    else
        {
    LogWarning("Player list container not assigned!");
      }
        
        if (playerListItemPrefab == null)
   {
   LogWarning("Player list item prefab not assigned!");
    }
    }
    
    
    #region Shared Health System

        private const string DIFFICULTY_KEY = "Difficulty";

        private int GetRoomDifficulty()
        {
            int diff = 1; // Normal
            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(DIFFICULTY_KEY, out object d))
            {
                if (d is int i) diff = Mathf.Clamp(i, 0, 2);
            }
            return diff;
        }

        private void ApplyDifficultySharedHP()
        {
            int diff = GetRoomDifficulty();
            float hp = normalSharedHP;
            if (diff == 0) hp = easySharedHP;
            else if (diff == 2) hp = hardSharedHP;

            sharedMaxHealth = Mathf.Max(1f, hp);
        }

        private float GetSharedMaxHealth()
        {
            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties != null)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("SharedMaxHealth", out object maxObj))
                {
                    if (maxObj is float f) return f;
                    if (maxObj is int i) return i;
                    if (maxObj is double d) return (float)d;
                }
            }
            return sharedMaxHealth;
        }
    
    private void InitializeSharedHealth()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        // Set shared max based on room or inspector
        float maxHP = sharedMaxHealth;
        // If host applied difficulty, sharedMaxHealth is already set; ensure room property reflects it
        maxHP = Mathf.Max(1f, maxHP);
        currentSharedHealth = maxHP;
  
        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable
        {
   { "SharedHealth", currentSharedHealth },
         { "SharedMaxHealth", currentSharedHealth }
 };
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }
    
    public void DamageSharedHealth(float damage)
    {
  if (!PhotonNetwork.IsMasterClient) return;

        currentSharedHealth -= damage;
    currentSharedHealth = Mathf.Max(0f, currentSharedHealth); // Fixed: Use Max to prevent negative health
        
        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable
        {
            { "SharedHealth", currentSharedHealth }
        };
   
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        
        if (currentSharedHealth <= 0f && !isGameOver)
 {
            photonView.RPC("RPC_GameOver", RpcTarget.All, false);
        }
    }
    
    private float GetSharedHealth()
    {
        object healthObj;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("SharedHealth", out healthObj))
        {
     return (float)healthObj;
        }
        return sharedMaxHealth;
    }
    
    private void UpdateHealthUI()
    {
      if (!PhotonNetwork.InRoom) return;
        
        // Only update UI if not waiting for animation to complete
  if (delayHealthUIUpdate) return;
     
  float health = GetSharedHealth();
   float maxHealth = GetSharedMaxHealth();
        
    if (sharedHealthBar != null)
        {
  sharedHealthBar.fillAmount = health / maxHealth;
   }
  
      if (sharedHealthText != null)
{
      sharedHealthText.text = $"{health:F0} / {maxHealth:F0}";
    }
    }
    
    /// <summary>
    /// Force update health UI after attack animation completes
    /// </summary>
    private void ForceUpdateHealthUI()
    {
        if (!PhotonNetwork.InRoom) return;
        
        float health = GetSharedHealth();
   float maxHealth = GetSharedMaxHealth();
      
        if (sharedHealthBar != null)
        {
         sharedHealthBar.fillAmount = health / maxHealth;
        }
        
   if (sharedHealthText != null)
        {
       sharedHealthText.text = $"{health:F0} / {maxHealth:F0}";
        }
        
        Log($"Forced health UI update: {health}/{maxHealth}");
    }
    
    #endregion
    
    #region Character Switching System
    
    private IEnumerator LoadPlayerCharacters()
    {
        // CRITICAL: Wait longer for player properties to sync across network
        // Read difficulty from room properties (default Normal)
        int diff = 1;
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Difficulty", out object d))
        {
            if (d is int i) diff = Mathf.Clamp(i, 0, 2);
        }
        Log($"[SharedMultiplayerGameManager] Difficulty: {(diff==0?"Easy":diff==1?"Normal":"Hard")}");

        // Properties set in OnJoinedRoom() might not be immediately available to all clients
        yield return new WaitForSeconds(1.0f);
        
        Log("===== LOADING CHARACTERS =====");
        Log($"Room: {PhotonNetwork.CurrentRoom.Name}, Players: {PhotonNetwork.PlayerList.Length}");
        
        // CRITICAL: Ensure all players have their cosmetic properties set BEFORE loading characters
        // This is especially important for players who joined after the game started
        EnsureAllPlayersHaveCosmeticProperties();
        
        // Wait a frame for properties to sync
        yield return null;
        
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Log($"Processing player: {player.NickName}, ActorNumber: {player.ActorNumber}");
         
            // Log all custom properties for debugging
            Log($"Player {player.NickName} custom properties:");
            foreach (var prop in player.CustomProperties)
            {
                Log($"  - {prop.Key}: {prop.Value}");
            }
            
            object cosmeticObj;
            string cosmetic = "Default";
            bool hasCosmeticProperty = false;
            
            if (player.CustomProperties.TryGetValue("multiplayer_cosmetic", out cosmeticObj))
            {
                cosmetic = cosmeticObj.ToString();
                hasCosmeticProperty = true;
                Log($"Found 'multiplayer_cosmetic' property: {cosmetic}");
            }
            else if (player.CustomProperties.TryGetValue("cosmetic", out cosmeticObj))
            {
                cosmetic = cosmeticObj.ToString();
                hasCosmeticProperty = true;
                Log($"Found 'cosmetic' property: {cosmetic}");
            }
            else if (player.CustomProperties.TryGetValue("current_cosmetic", out cosmeticObj))
            {
                cosmetic = cosmeticObj.ToString();
                hasCosmeticProperty = true;
                Log($"Found 'current_cosmetic' property: {cosmetic}");
            }
            else
            {
                LogWarning($"Player {player.NickName} has no cosmetic property! Will calculate based on position.");
            }
        
            // If no cosmetic property found, calculate based on player position
            if (!hasCosmeticProperty || string.IsNullOrWhiteSpace(cosmetic) || cosmetic.ToLower() == "default")
            {
                // Calculate player position (0 = first player, 1 = second player, etc.)
                Player[] sortedPlayers = PhotonNetwork.PlayerList;
                System.Array.Sort(sortedPlayers, (a, b) => a.ActorNumber.CompareTo(b.ActorNumber));
                
                int playerPosition = -1;
                for (int i = 0; i < sortedPlayers.Length; i++)
                {
                    if (sortedPlayers[i].ActorNumber == player.ActorNumber)
                    {
                        playerPosition = i;
                        break;
                    }
                }
                
                // Use default cosmetics array (matches LobbyManager's positionBasedCosmetics)
                string[] defaultCosmetics = { "Knight", "Ronin", "Daimyo", "King", "DemonGirl" };
                if (playerPosition >= 0 && playerPosition < defaultCosmetics.Length)
                {
                    cosmetic = defaultCosmetics[playerPosition];
                    LogWarning($"Player {player.NickName} has no cosmetic property - assigned '{cosmetic}' based on position {playerPosition}");
                    
                    // CRITICAL: Set the cosmetic property for this player so it's synced
                    // This ensures other clients also see the correct cosmetic
                    if (player.IsLocal)
                    {
                        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
                        {
                            { "multiplayer_cosmetic", cosmetic },
                            { "player_position", playerPosition }
                        };
                        player.SetCustomProperties(props);
                        Log($"Set cosmetic property for local player {player.NickName}: {cosmetic} (Position: {playerPosition})");
                    }
                }
                else
                {
                    cosmetic = "Default";
                    LogWarning($"Player {player.NickName} has no cosmetic property and invalid position {playerPosition} - using Default");
                }
            }
            
            Log($"Loading character for {player.NickName} with cosmetic: '{cosmetic}'");
            LoadPlayerCharacter(player.ActorNumber, cosmetic);
        }
        
        yield return null; // Wait one frame for instantiation to complete
        
        Log("===== CHARACTER LOADING COMPLETE =====");
        Log($"Loaded {playerCharacters.Count} characters");
        
        // Don't show character here - let the turn system handle it after initialization
        // This ensures characters are loaded before OnTurnChanged is called
    }
    
    /// <summary>
    /// Ensure all players have their cosmetic properties set based on their position
    /// This is called before loading characters to ensure properties are synced
    /// </summary>
    private void EnsureAllPlayersHaveCosmeticProperties()
    {
        if (!PhotonNetwork.InRoom) return;
        
        Log("===== Ensuring all players have cosmetic properties =====");
        
        // Sort players by actor number to maintain consistent order
        Player[] sortedPlayers = PhotonNetwork.PlayerList;
        System.Array.Sort(sortedPlayers, (a, b) => a.ActorNumber.CompareTo(b.ActorNumber));
        
        // Use default cosmetics array (matches LobbyManager's positionBasedCosmetics)
        string[] defaultCosmetics = { "Knight", "Ronin", "Daimyo", "King", "DemonGirl" };
        
        for (int i = 0; i < sortedPlayers.Length; i++)
        {
            Player player = sortedPlayers[i];
            string assignedCosmetic = defaultCosmetics.Length > i ? defaultCosmetics[i] : "Default";
            
            // Check if player already has the correct cosmetic property
            object existingCosmeticObj;
            bool needsUpdate = true;
            
            if (player.CustomProperties.TryGetValue("multiplayer_cosmetic", out existingCosmeticObj))
            {
                string existingCosmetic = existingCosmeticObj.ToString();
                if (existingCosmetic == assignedCosmetic && !string.IsNullOrWhiteSpace(existingCosmetic))
                {
                    needsUpdate = false;
                    Log($"Player {player.NickName} already has correct cosmetic: {assignedCosmetic}");
                }
            }
            
            // Set cosmetic property for local player if needed
            if (needsUpdate && player.IsLocal)
            {
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
                {
                    { "multiplayer_cosmetic", assignedCosmetic },
                    { "player_position", i }
                };
                player.SetCustomProperties(props);
                Log($"Set cosmetic property for local player {player.NickName}: {assignedCosmetic} (Position: {i})");
            }
            else if (needsUpdate)
            {
                LogWarning($"Remote player {player.NickName} (ActorNumber: {player.ActorNumber}) should have cosmetic '{assignedCosmetic}' (Position: {i}), but property is missing or incorrect. Remote players must set their own properties in OnJoinedRoom().");
            }
        }
    }
    
    private void LoadPlayerCharacter(int actorNumber, string cosmeticData)
    {
        Log("===== LoadPlayerCharacter =====");
    Log($"Actor: {actorNumber}, Cosmetic: '{cosmeticData}'");
  
        // CRITICAL: Check if character already exists for this actor to prevent duplicates
        if (playerCharacters.ContainsKey(actorNumber) && playerCharacters[actorNumber] != null)
        {
            LogWarning($"Character for actor {actorNumber} already exists! Skipping duplicate instantiation.");
            return;
        }
        
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        if (player == null)
        {
            LogError($"CRITICAL: Player with ActorNumber {actorNumber} not found in room!");
         return;
        }
        
 Log($"Found player: {player.NickName}");
        
        cosmeticData = cosmeticData?.Trim();
        if (string.IsNullOrEmpty(cosmeticData))
        {
            LogWarning("Empty cosmetic data, using 'Default'");
            cosmeticData = "Default";
        }
    
        GameObject characterPrefab = GetCharacterPrefab(cosmeticData);
        
      if (characterPrefab != null)
        {
            Log($"Creating character instance for {player.NickName} with cosmetic '{cosmeticData}'");
       
            try
   {
          // CRITICAL: Multiplayer characters spawn at offScreenLeft position initially
          // They are then moved to characterDisplayPosition when it's their turn
          // Spawn Position: offScreenLeft (default: Vector3(-15f, 0f, 0f))
          // Display Position: characterDisplayPosition (set in Unity Inspector)
          GameObject character = Instantiate(characterPrefab, offScreenLeft, Quaternion.identity);
 character.name = $"Character_{player.NickName}_Actor{actorNumber}";
                character.SetActive(false);
                Debug.Log($"[SharedMultiplayerGameManager] Character '{character.name}' spawned at initial position: {offScreenLeft}. Will be displayed at: {(characterDisplayPosition != null ? characterDisplayPosition.position.ToString() : "NULL - Set characterDisplayPosition in Inspector!")}");
       
          // Ensure character has Animator component
          Animator animator = character.GetComponent<Animator>();
          if (animator == null)
          {
              LogWarning($"Character prefab for {player.NickName} doesn't have an Animator component. Attack/Idle animations may not work.");
          }
          else
          {
              Log($"Character has Animator component: {animator.name}");
          }
       
          // CRITICAL: Store character before activating to prevent race conditions
          playerCharacters[actorNumber] = character;
       
           Log($"✓ Character created and stored for actor {actorNumber}: {character.name}");
        }
         catch (System.Exception ex)
         {
          LogError($"Exception creating character: {ex.Message}");
         }
        }
        else
        {
            LogError($"CRITICAL: Could not load character prefab for cosmetic: '{cosmeticData}'. Check if prefab exists in Resources/Characters/{cosmeticData}");
        }
    }
    
    private GameObject GetCharacterPrefab(string cosmeticData)
    {
 Log("===== GetCharacterPrefab =====");
   Log($"Searching for: '{cosmeticData}'");
        
   GameObject prefab = null;
    
        // Try exact match
        string exactPath = $"Characters/{cosmeticData}";
        Log($"Attempting to load: '{exactPath}'");
      prefab = Resources.Load<GameObject>(exactPath);
if (prefab != null)
        {
            Log($"✓ Found prefab with exact match: {exactPath}");
            return prefab;
        }
        else
        {
            LogWarning($"✗ Prefab not found at: {exactPath}");
        }
        
  // Try capitalized first letter
        if (!string.IsNullOrEmpty(cosmeticData) && cosmeticData.Length > 0)
        {
        string capitalizedPath = $"Characters/{char.ToUpper(cosmeticData[0])}{cosmeticData.Substring(1)}";
            if (capitalizedPath != exactPath) // Only try if different
            {
                Log($"Attempting to load: '{capitalizedPath}'");
                prefab = Resources.Load<GameObject>(capitalizedPath);
     
   if (prefab != null)
          {
      Log($"✓ Found prefab with capitalization: {capitalizedPath}");
                return prefab;
            }
                else
                {
                    LogWarning($"✗ Prefab not found at: {capitalizedPath}");
                }
            }
   }
        
      // Try all lowercase
        string lowercasePath = $"Characters/{cosmeticData.ToLower()}";
        if (lowercasePath != exactPath && lowercasePath != $"Characters/{char.ToUpper(cosmeticData[0])}{cosmeticData.Substring(1)}") // Only try if different
        {
            Log($"Attempting to load: '{lowercasePath}'");
            prefab = Resources.Load<GameObject>(lowercasePath);
      
        if (prefab != null)
    {
        Log($"✓ Found prefab with lowercase: {lowercasePath}");
         return prefab;
        }
            else
            {
                LogWarning($"✗ Prefab not found at: {lowercasePath}");
            }
        }
        
        // Try "Default" as ultimate fallback
     Log("Trying fallback: Characters/Default");
    prefab = Resources.Load<GameObject>("Characters/Default");
 
        if (prefab != null)
        {
            Log("✓ Using Default prefab as fallback");
    return prefab;
        }
        else
        {
   LogError("✗ CRITICAL: Even Default prefab not found!");
 LogError("Make sure you have a prefab at: Assets/Resources/Characters/Default.prefab");
  LogError("Resources.Load path format: 'Characters/Default' (no .prefab extension, no Assets/Resources/ prefix)");
  }
     
        return null;
    }
 
    private void ShowCharacter(int actorNumber, bool animated)
    {
        Log("===== ShowCharacter =====");
        Log($"Actor: {actorNumber}, Animated: {animated}");
      
        if (characterDisplayPosition == null)
     {
      LogError("Character Display Position is NULL! Assign it in Inspector.");
  return;
        }
   
        if (isSwitchingCharacter)
        {
        LogWarning($"Already switching character, ignoring request for actor {actorNumber}. Will retry in 0.5s...");
            // Wait a bit and retry if we're already switching
            StartCoroutine(RetryShowCharacterAfterDelay(actorNumber, animated, 0.5f));
       return;
   }
        
        if (!playerCharacters.ContainsKey(actorNumber))
  {
          LogError($"? No character found for actor {actorNumber}!");
            
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            if (player != null)
            {
                LogError($"Missing character for player: {player.NickName}");
            }
            
            // If characters are still loading, wait a bit and retry
            if (playerCharacters.Count == 0)
            {
                LogWarning("Characters not loaded yet - waiting and retrying...");
                StartCoroutine(RetryShowCharacterAfterDelay(actorNumber, animated, 0.5f));
                return;
            }
            
            LogWarning("Continuing gameplay without character visual for this player.");
         return;
        }
        
        Log($"Character found for actor {actorNumber}: {playerCharacters[actorNumber].name}");
        
   if (animated)
        {
        Log($"Starting animated switch to actor {actorNumber}");
            StartCoroutine(SwitchCharacterAnimated(actorNumber));
        }
        else
        {
            Log($"Showing character instantly for actor {actorNumber}");
          
            if (currentCharacterInstance != null)
     {
              currentCharacterInstance.SetActive(false);
      }
 
     currentCharacterInstance = playerCharacters[actorNumber];
            currentCharacterInstance.transform.position = characterDisplayPosition.position;
        currentCharacterInstance.SetActive(true);
       
        // Update PlayCardButton reference for attack animations
        if (playCardButton != null)
        {
            playCardButton.playerCharacter = currentCharacterInstance;
            // Try to get CharacterJumpAttack component
            CharacterJumpAttack jumpAttack = currentCharacterInstance.GetComponent<CharacterJumpAttack>();
            if (jumpAttack != null)
            {
                playCardButton.playerJumpAttack = jumpAttack;
            }
        }
       
        // Play idle animation
            // Play idle animation - will use CharacterJumpAttack if available, otherwise try trigger/state
            PlayCharacterAnimation(actorNumber, useIdleTrigger ? idleAnimationTrigger : idleAnimationStateName);
       
    Log($"? Character {actorNumber} shown at position {characterDisplayPosition.position}");
  }
    }
    
    /// <summary>
    /// Retry showing character after a delay if switching was in progress
    /// </summary>
    private IEnumerator RetryShowCharacterAfterDelay(int actorNumber, bool animated, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (!isSwitchingCharacter)
        {
            Log($"Retrying ShowCharacter for actor {actorNumber} after delay");
            ShowCharacter(actorNumber, animated);
        }
        else
        {
            LogWarning($"Still switching character after delay, giving up on actor {actorNumber}");
        }
    }
    
    /// <summary>
    /// Play animation on character by actor number
    /// </summary>
    private void PlayCharacterAnimation(int actorNumber, string triggerOrStateName)
    {
        if (!playerCharacters.ContainsKey(actorNumber))
        {
            LogWarning($"Cannot play animation - character for actor {actorNumber} not found");
            return;
        }
        
        GameObject character = playerCharacters[actorNumber];
        if (character == null)
        {
            LogWarning($"Cannot play animation - character GameObject for actor {actorNumber} is null");
            return;
        }
        
        // Try to get Animator component
        Animator animator = character.GetComponent<Animator>();
        if (animator == null)
        {
            animator = character.GetComponentInChildren<Animator>();
        }
        
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // First, try to use CharacterJumpAttack's PlayIdleAnimation if available and we're playing idle
            // CharacterJumpAttack uses state names (not triggers) for idle animations
            CharacterJumpAttack jumpAttack = character.GetComponent<CharacterJumpAttack>();
            if (jumpAttack != null && (triggerOrStateName == idleAnimationTrigger || triggerOrStateName == idleAnimationStateName))
            {
                // Use CharacterJumpAttack's built-in idle animation method (uses state names)
                jumpAttack.PlayIdleAnimation();
                Log($"Playing idle animation via CharacterJumpAttack on character {actorNumber}");
                return;
            }
            
            // Check if trigger exists in animator
            bool hasTrigger = false;
            
            // Check for trigger parameter
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == triggerOrStateName && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasTrigger = true;
                    break;
                }
            }
            
            // If trigger exists and we want to use it, use trigger
            if (hasTrigger && useIdleTrigger)
            {
                animator.SetTrigger(triggerOrStateName);
                Log($"Playing animation trigger '{triggerOrStateName}' on character {actorNumber}");
            }
            else
            {
                // Use state name instead of trigger (default for idle animation)
                try
                {
                    animator.Play(triggerOrStateName, 0, 0f);
                    Log($"Playing animation state '{triggerOrStateName}' on character {actorNumber}");
                }
                catch
                {
                    // State doesn't exist - log warning with available parameters
                    string availableParams = string.Join(", ", System.Array.ConvertAll(animator.parameters, p => p.name));
                    LogWarning($"Character {actorNumber} has Animator but cannot play state '{triggerOrStateName}'. Available parameters: {availableParams}");
                    
                    // Final fallback: try to use CharacterJumpAttack if available (even if name doesn't match exactly)
                    if (jumpAttack != null && triggerOrStateName.Contains("Idle"))
                    {
                        jumpAttack.PlayIdleAnimation();
                        Log($"Playing idle animation via CharacterJumpAttack fallback on character {actorNumber}");
                    }
                }
            }
        }
        else
        {
            LogWarning($"Character {actorNumber} has no Animator component or Animator Controller - animation '{triggerOrStateName}' cannot play");
        }
    }
    
    /// <summary>
    /// Play attack animation on current character
    /// </summary>
    public void PlayCurrentCharacterAttack()
    {
        if (currentCharacterInstance == null)
        {
            LogWarning("Cannot play attack animation - currentCharacterInstance is null");
            return;
        }
        
        // Try to get Animator component
        Animator animator = currentCharacterInstance.GetComponent<Animator>();
        if (animator == null)
        {
            animator = currentCharacterInstance.GetComponentInChildren<Animator>();
        }
        
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // Check if trigger exists
            bool hasTrigger = false;
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == attackAnimationTrigger && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasTrigger = true;
                    break;
                }
            }
            
            if (hasTrigger)
            {
                animator.SetTrigger(attackAnimationTrigger);
                Log($"Playing attack animation '{attackAnimationTrigger}' on current character");
            }
            else
            {
                LogWarning($"Current character has Animator but no '{attackAnimationTrigger}' trigger parameter. Available parameters: {string.Join(", ", System.Array.ConvertAll(animator.parameters, p => p.name))}");
            }
        }
        else
        {
            LogWarning($"Current character has no Animator component or Animator Controller - attack animation cannot play");
        }
    }
    
    private IEnumerator SwitchCharacterAnimated(int newActorNumber)
    {
        isSwitchingCharacter = true;
        Log($"===== SwitchCharacterAnimated STARTED for actor {newActorNumber} =====");
        
        // Validate character exists before starting animation
        if (!playerCharacters.ContainsKey(newActorNumber))
        {
            LogError($"CRITICAL: Character for actor {newActorNumber} not found in playerCharacters dictionary!");
            isSwitchingCharacter = false;
            yield break;
        }
        
        if (playerCharacters[newActorNumber] == null)
        {
            LogError($"Character instance for actor {newActorNumber} is null!");
            isSwitchingCharacter = false;
            yield break;
        }
        
        if (characterDisplayPosition == null)
        {
            LogError("CharacterDisplayPosition is null! Cannot switch character.");
            isSwitchingCharacter = false;
            yield break;
        }
        
        // Slide current character out to the left
        if (currentCharacterInstance != null)
        {
            currentCharacterInstance.SetActive(true);
            Vector3 startPos = currentCharacterInstance.transform.position;
            Vector3 targetPos = offScreenLeft;
            
            // Use fixed duration instead of speed-based to ensure smooth animation
            float duration = 0.5f; // 0.5 seconds for slide animation
            float elapsed = 0f;
            
            Log($"Sliding out current character from {startPos} to {targetPos} over {duration}s");
            while (elapsed < duration && currentCharacterInstance != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Use smooth step for better easing
                float smoothT = t * t * (3f - 2f * t);
                currentCharacterInstance.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
                yield return null;
            }
            
            if (currentCharacterInstance != null)
            {
                currentCharacterInstance.transform.position = targetPos;
                currentCharacterInstance.SetActive(false);
                Log("Current character slid out and deactivated");
            }
        }
        
        // Slide new character in from the right
        currentCharacterInstance = playerCharacters[newActorNumber];
        if (currentCharacterInstance == null)
        {
            LogError($"Character instance for actor {newActorNumber} became null!");
            isSwitchingCharacter = false;
            yield break;
        }
        
        currentCharacterInstance.SetActive(true);
        currentCharacterInstance.transform.position = offScreenRight;
        
        Log($"Sliding in new character for actor {newActorNumber} from {offScreenRight}");
        
        Vector3 slideInStartPos = offScreenRight;
        Vector3 slideInTargetPos = characterDisplayPosition.position;
        
        // Use fixed duration instead of speed-based to ensure smooth animation
        float slideInDuration = 0.5f; // 0.5 seconds for slide animation
        float slideInElapsed = 0f;
        
        while (slideInElapsed < slideInDuration && currentCharacterInstance != null)
        {
            slideInElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(slideInElapsed / slideInDuration);
            // Use smooth step for better easing
            float smoothT = t * t * (3f - 2f * t);
            currentCharacterInstance.transform.position = Vector3.Lerp(slideInStartPos, slideInTargetPos, smoothT);
            yield return null;
        }
        
        if (currentCharacterInstance != null)
        {
            currentCharacterInstance.transform.position = slideInTargetPos;
            Log($"New character slid in to {slideInTargetPos}");
            
            // CRITICAL: Update PlayCardButton reference AFTER character reaches final position
            // This ensures the enemy always attacks the correct character position
            if (playCardButton != null)
            {
                playCardButton.playerCharacter = currentCharacterInstance;
                // Try to get CharacterJumpAttack component
                CharacterJumpAttack jumpAttack = currentCharacterInstance.GetComponent<CharacterJumpAttack>();
                if (jumpAttack != null)
                {
                    playCardButton.playerJumpAttack = jumpAttack;
                    Log($"Updated PlayCardButton references for actor {newActorNumber} (character at final position)");
                }
                else
                {
                    LogWarning($"Character for actor {newActorNumber} has no CharacterJumpAttack component");
                }
            }
            
            // Play idle animation after sliding in
                // Play idle animation - will use CharacterJumpAttack if available, otherwise try trigger/state
                PlayCharacterAnimation(newActorNumber, useIdleTrigger ? idleAnimationTrigger : idleAnimationStateName);
            Log($"Idle animation triggered for actor {newActorNumber}");
        }
        
        // Always reset the flag, even if there was an error
        isSwitchingCharacter = false;
        Log($"===== SwitchCharacterAnimated COMPLETED for actor {newActorNumber} =====");
    }
    
    #endregion
    
    #region Turn Management
    
    private void OnTurnChanged(Player player)
    {
        Log($"========== TURN CHANGED TO: {player.NickName} ==========");
        
        // CRITICAL FIX: Check ActorNumber instead of relying on IsLocal
        // player.IsLocal can return false even for the local player in some cases
        bool isMyTurn = (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
        
        Log($"Player ActorNumber: {player.ActorNumber}");
        Log($"Local Player ActorNumber: {PhotonNetwork.LocalPlayer.ActorNumber}");
        Log($"Is my turn: {isMyTurn}");

        // CRITICAL: In multiplayer, all players share the same character
        // No character switching needed - just use PlayCardButton.playerCharacter
        // Ensure the character reference is updated for attacks
        if (playCardButton != null && playCardButton.playerCharacter != null)
        {
            // Update playerJumpAttack reference if needed
            if (playCardButton.playerJumpAttack == null)
            {
                playCardButton.playerJumpAttack = playCardButton.playerCharacter.GetComponent<CharacterJumpAttack>();
            }
        }
  
        // Update UI
        UpdateTurnUI();
  
        // **CRITICAL FIX: Force card reset via RPC FIRST to clear played cards for all players**
        if (PhotonNetwork.IsMasterClient)
        {
            Log("Master Client - forcing card reset via RPC for all players");
            photonView.RPC("RPC_ForceCardReset", RpcTarget.All);
        }
  
        // **CRITICAL: Master Client syncs ALL counters to ensure global consistency**
        // This also triggers card randomization for the active player (happens after RPC_ForceCardReset)
        if (PhotonNetwork.IsMasterClient)
        {
            Log("Master Client - syncing all counters globally before turn starts");
            SyncAllCountersGlobally();
        }
     
        // Enable/Disable controls based on whose turn it is
        if (isMyTurn)  // ? FIXED: Use ActorNumber comparison, not player.IsLocal
        {
            Log("LOCAL PLAYER - Enabling controls");
     EnablePlayerControls();
     
     if (cardManager != null && cardManager.grid != null)
    {
  cardManager.grid.SetActive(true);
              Log("Card grid activated");
              
              // Log current counter states (DO NOT MODIFY - they should already be synced via RPC)
              if (playCardButton != null && playCardButton.outputManager != null)
              {
                  Log($"=== COUNTER STATUS ===");
                  Log($"outputManager.counter (question): {playCardButton.outputManager.counter}");
                  Log($"cardManager.counter (card set): {cardManager.counter}");
                  Log($"playButton.counter (answer index): {playCardButton.counter}");
                  Log($"======================");
              }
              
              // NOTE: Card reset and randomization now happens in RPC_SyncAllCounters
              // after counters are synced to ensure correct counter values
              Log("Waiting for RPC_SyncAllCounters to reset and randomize cards...");
     }
            
            // Start timer only for the player whose turn it is
            StartTimerForCurrentPlayer();
        }
        else
        {
            Log($"REMOTE PLAYER ({player.NickName}) - Disabling controls");
         DisablePlayerControls();

            if (cardManager != null && cardManager.grid != null)
     {
   cardManager.grid.SetActive(false);
                Log("Card grid deactivated");
  }
            
            // **NOTE: Timer keeps running for all players**
            // Don't pause it - all players need to see the same countdown
            Log("Watching other player's turn - timer still running");
        }
    
        Log($"========== TURN CHANGE COMPLETE ==========");
    }
    
  /// <summary>
    /// Force reset cards and timer for all players (called via RPC)
    /// </summary>
    [PunRPC]
    void RPC_ForceCardReset()
    {
        Log("RPC_ForceCardReset - Clearing played cards for ALL players");
        Log($"Current counters - CardManager: {(cardManager != null ? cardManager.counter.ToString() : "null")}, PlayButton: {(playCardButton != null ? playCardButton.counter.ToString() : "null")}, Output: {(playCardButton?.outputManager != null ? playCardButton.outputManager.counter.ToString() : "null")}");
   
  if (cardManager != null)
        {
            // DO NOT modify counters here - they should already be synced via RPC_SyncCardState
            // Just reset card positions back to grid
            Log($"Resetting cards to grid for question {cardManager.counter}");
            cardManager.ResetCards();
            Log("Cards cleared from played area (counters unchanged, randomization happens on turn change)");
   }
  else
 {
      LogError("CardManager is NULL!");
        }
    }
    
    /// <summary>
    /// Sync ALL counters to all clients at once (called by Master Client)
    /// Use this to ensure global consistency at critical points
    /// </summary>
    public void SyncAllCountersGlobally()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        int cardCounter = cardManager != null ? cardManager.counter : 0;
        int playButtonCounter = playCardButton != null ? playCardButton.counter : 0;
        int outputCounter = (playCardButton != null && playCardButton.outputManager != null) ? playCardButton.outputManager.counter : 0;
        
        Log($"[Master] Syncing ALL counters globally: CardManager={cardCounter}, PlayButton={playButtonCounter}, Output={outputCounter}");
        photonView.RPC("RPC_SyncAllCounters", RpcTarget.All, cardCounter, playButtonCounter, outputCounter);
    }
    
    [PunRPC]
    void RPC_SyncAllCounters(int cardCounter, int playButtonCounter, int outputCounter)
    {
        Log($"===== RPC_SyncAllCounters RECEIVED =====");
        Log($"Setting ALL counters: CardManager={cardCounter}, PlayButton={playButtonCounter}, Output={outputCounter}");
        Log($"Local Player: {PhotonNetwork.LocalPlayer?.NickName} (ActorNumber: {PhotonNetwork.LocalPlayer?.ActorNumber})");
        
        if (cardManager != null)
        {
            cardManager.counter = cardCounter;
            Log($"✓ CardManager.counter = {cardCounter}");
        }
        else
        {
            LogError("CardManager is NULL!");
        }
        
        if (playCardButton != null)
        {
            playCardButton.counter = playButtonCounter;
            Log($"✓ PlayButton.counter = {playButtonCounter}");
        }
        else
        {
            LogError("PlayCardButton is NULL!");
        }
        
        if (playCardButton != null && playCardButton.outputManager != null)
        {
            playCardButton.outputManager.counter = outputCounter;
            Log($"✓ OutputManager.counter = {outputCounter}");
        }
        else
        {
            LogError("OutputManager is NULL!");
        }
        
        Log("===== ALL COUNTERS SYNCED =====");
        
        // CRITICAL FIX: After counters are synced, if it's my turn, reset and randomize cards
        // This ensures cards are randomized with the CORRECT counter values
        if (turnSystem == null)
        {
            LogError("CRITICAL: turnSystem is NULL! Cannot check whose turn it is. Re-fetching...");
            turnSystem = GetComponent<CodexMultiplayerIntegration>();
            if (turnSystem == null)
            {
                LogError("FATAL: CodexMultiplayerIntegration component not found!");
                return;
            }
        }
        
        // CRITICAL FIX: Get current turn player directly from PhotonNetwork.PlayerList
        // Don't use GetCurrentTurnPlayer() which reads from room properties (race condition!)
        Player currentTurnPlayer = null;
        int currentTurnIndex = turnSystem.GetCurrentPlayerTurnIndex();
        if (currentTurnIndex >= 0 && currentTurnIndex < PhotonNetwork.PlayerList.Length)
        {
            currentTurnPlayer = PhotonNetwork.PlayerList[currentTurnIndex];
        }
        
        Log($"Current turn player: {currentTurnPlayer?.NickName} (ActorNumber: {currentTurnPlayer?.ActorNumber})");
        
        bool isMyTurn = (currentTurnPlayer != null && currentTurnPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
        Log($"Checking if should randomize cards: isMyTurn={isMyTurn}");
        
        if (isMyTurn)
        {
            if (cardManager == null)
            {
                LogError("Cannot randomize cards - CardManager is NULL!");
                return;
            }
            
            if (cardManager.grid == null)
            {
                LogError("Cannot randomize cards - CardManager.grid is NULL!");
                return;
            }
            
            Log($"✓ It's my turn! Resetting and randomizing cards with counter={cardManager.counter}");
            
            // Cancel any ongoing randomization first
            cardManager.CancelRandomization();
            
            // Reset cards back to grid (using newly-synced cardManager.counter)
            cardManager.ResetCards();
            Log($"Cards reset for question {cardManager.counter}");
            
            // Randomize cards for the current question
            cardManager.StartRandomization();
            Log($"Started randomizing cards for question {cardManager.counter}");
        }
        else
        {
            Log($"Not my turn - skipping card randomization");
        }
    }
    
    /// <summary>
    /// Sync card manager counter across all clients (deprecated - use SyncAllCountersGlobally)
    /// </summary>
    [PunRPC]
    void RPC_SyncCardCounter(int newCounter)
  {
Log($"RPC_SyncCardCounter - Setting counter to {newCounter}");
      
        if (cardManager != null)
        {
          cardManager.counter = newCounter;
            Log($"Card manager counter synced to {newCounter}");
        }
      
        if (playCardButton != null)
        {
            playCardButton.counter = 0; // Reset play button counter for new question
 Log("Play button counter reset to 0");
        }
    }
    
    private void UpdateTurnUI()
    {
    if (!PhotonNetwork.InRoom) return;
        
        if (isGameOver)
        {
            HideMultiplayerUI();
        return;
        }
        
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
      if (currentTurnText != null)
            {
   currentTurnText.text = "Solo Testing Mode";
                currentTurnText.gameObject.SetActive(true);
            }
            
      if (yourTurnIndicator != null) yourTurnIndicator.SetActive(true);
          if (waitingIndicator != null) waitingIndicator.SetActive(false);
        return;
        }
        
        if (turnSystem == null)
        {
            if (currentTurnText != null)
    {
    currentTurnText.text = "ERROR: Turn System Missing!\nAdd CodexMultiplayerIntegration component";
        currentTurnText.gameObject.SetActive(true);
     currentTurnText.color = Color.red;
      }
            
          if (yourTurnIndicator != null) yourTurnIndicator.SetActive(false);
      if (waitingIndicator != null) waitingIndicator.SetActive(true);
  return;
 }
     
        Player currentPlayer = turnSystem.GetCurrentTurnPlayer();
   
        if (currentPlayer != null)
        {
      if (currentTurnText != null)
            {
                currentTurnText.text = $"{currentPlayer.NickName}'s Turn";
 currentTurnText.gameObject.SetActive(true);
       currentTurnText.color = Color.white;
}
        
          if (yourTurnIndicator != null) yourTurnIndicator.SetActive(currentPlayer.IsLocal);
            if (waitingIndicator != null) waitingIndicator.SetActive(!currentPlayer.IsLocal);
        }
        else
        {
            if (currentTurnText != null)
            {
  currentTurnText.text = "Initializing turn system...\nPlease wait";
       currentTurnText.gameObject.SetActive(true);
       currentTurnText.color = Color.yellow;
     }
            
          if (yourTurnIndicator != null) yourTurnIndicator.SetActive(false);
         if (waitingIndicator != null) waitingIndicator.SetActive(true);
        }
    }
    
    private void EnablePlayerControls()
    {
        if (playCardButton != null) playCardButton.enabled = true;
        if (cardManager != null) cardManager.enabled = true;
    }
    
    private void DisablePlayerControls()
    {
        if (playCardButton != null) playCardButton.enabled = false;
    }
    
  #endregion
    
    #region Timer Synchronization
    
    /// <summary>
    /// Start timer only for the player whose turn it is
    /// </summary>
    public void StartTimerForCurrentPlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Player currentPlayer = turnSystem?.GetCurrentTurnPlayer();
            if (currentPlayer != null)
            {
                Log($"Starting timer for {currentPlayer.NickName}'s turn");
                photonView.RPC("RPC_StartTimerForPlayer", RpcTarget.All, currentPlayer.ActorNumber);
            }
        }
    }
    
    /// <summary>
    /// Start timer for all players (used when starting new enemy)
    /// </summary>
    public void StartTimerForAllPlayers()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Log("Starting timer for all players (new enemy)");
            photonView.RPC("RPC_StartTimer", RpcTarget.All);
        }
    }
    
    /// <summary>
    /// Stop timer for all players
    /// </summary>
    public void StopTimerForAllPlayers()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Log("Stopping timer for all players");
            photonView.RPC("RPC_StopTimer", RpcTarget.All);
        }
    }
    
    [PunRPC]
    void RPC_StartTimer()
    {
        Timer timer = FindObjectOfType<Timer>();
        if (timer != null)
        {
          timer.ResetTimer();
  timer.StartTimer();
            Log("Timer started for all players");
        }
        else
 {
         LogWarning("Timer not found in scene!");
        }
    }
    
    [PunRPC]
    void RPC_StartTimerForPlayer(int playerActorNumber)
    {
        bool isMyTurn = (playerActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
        
        Timer timer = FindObjectOfType<Timer>();
        if (timer != null)
        {
            // **CRITICAL: Timer should run for ALL players, not just the active one**
            // This ensures perfect sync across all clients
            timer.ResetTimer();
            timer.StartTimer();
            
            if (isMyTurn)
            {
                Log($"Timer started for my turn (ActorNumber: {playerActorNumber})");
            }
            else
            {
                Log($"Timer running - watching {playerActorNumber}'s turn");
            }
        }
        else
        {
            LogWarning("Timer not found in scene!");
        }
    }
    
    [PunRPC]
    void RPC_StopTimer()
    {
        Timer timer = FindObjectOfType<Timer>();
        if (timer != null)
        {
            timer.PauseTimer();
            Log("Timer paused for all players");
        }
        else
        {
            LogWarning("Timer not found in scene!");
        }
    }
    
    #endregion
    
    #region Card Play Integration
    
    public void OnCardPlayed(bool wasCorrect, float damage)
    {
        Log($"OnCardPlayed called - Correct: {wasCorrect}, Damage: {damage}");
     
        if (!turnSystem.IsMyTurn())
        {
   LogWarning("Not my turn, ignoring");
            return;
        }
        
        Log("Sending RPC to all players");
        photonView.RPC("RPC_OnCardPlayed", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, wasCorrect, damage);
    }
    
    [PunRPC]
    void RPC_OnCardPlayed(int playerActorNumber, bool wasCorrect, float damage)
    {
        Log($"RPC_OnCardPlayed received - Player: {playerActorNumber}, Correct: {wasCorrect}");
        
        if (wasCorrect)
        {
            // CORRECT ANSWER - Trigger player attack animation on all clients
            
            bool hasAnimation = false;
            if (playCardButton != null && playCardButton.useJumpAttackAnimation && 
                playCardButton.enemyManager != null &&
                playCardButton.enemyManager.counter < playCardButton.enemyManager.enemies.Count)
            {
                GameObject currentEnemy = playCardButton.enemyManager.enemies[playCardButton.enemyManager.counter];
                
                if (currentEnemy != null && playCardButton.playerCharacter != null)
                {
                    // CRITICAL: Use playCardButton.playerCharacter directly (shared character for all players)
                    CharacterJumpAttack jumpAttack = playCardButton.playerJumpAttack;
                    if (jumpAttack == null)
                    {
                        jumpAttack = playCardButton.playerCharacter.GetComponent<CharacterJumpAttack>();
                        if (jumpAttack == null)
                        {
                            jumpAttack = playCardButton.playerCharacter.GetComponentInChildren<CharacterJumpAttack>();
                        }
                        playCardButton.playerJumpAttack = jumpAttack;
                    }
                    
                    if (jumpAttack != null)
                    {
                        hasAnimation = true;
                        
                        Log($"Triggering player jump attack on {playCardButton.playerCharacter.name} targeting enemy {currentEnemy.name}");
                        Log($"Character position: {playCardButton.playerCharacter.transform.position}, Enemy position: {currentEnemy.transform.position}");
                        
                        // Trigger player jump attack animation on ALL clients
                        // In multiplayer, all players share the same character - no character switching needed
                        jumpAttack.PerformJumpAttack(currentEnemy.transform, () => {
                            Log($"Attack animation callback invoked for character {playCardButton.playerCharacter?.name}");
                            
                            // Master Client applies damage
                            if (PhotonNetwork.IsMasterClient)
                            {
                                // Validate enemy index before accessing array
                                int enemyIndex = playCardButton.enemyManager.counter;
                                if (enemyIndex >= 0 && enemyIndex < playCardButton.enemyHealthAmount.Count)
                                {
                                    playCardButton.EnemyTakeDamage(damage);
                                    
                                    // Sync enemy health to all clients
                                    float newHealth = playCardButton.enemyHealthAmount[enemyIndex];
                                    photonView.RPC("RPC_SyncEnemyHealth", RpcTarget.All, enemyIndex, newHealth);
                                    
                                    Log($"Player attack animation complete - enemy damaged by {damage}");
                                }
                                else
                                {
                                    LogError($"Invalid enemy index {enemyIndex} when trying to apply damage! Health array count: {playCardButton.enemyHealthAmount.Count}");
                                }
                            }
                            else
                            {
                                Log("Non-master: Player attack animation complete (visual only)");
                            }
                            
                            // CRITICAL: After attack completes, advance turn (master client only)
                            // No character sliding needed since we're using shared character
                            if (PhotonNetwork.IsMasterClient)
                            {
                                // Advance turn after a brief delay to allow animation to complete visually
                                StartCoroutine(AdvanceTurnAfterAttackDelay(0.5f));
                            }
                        }, damage);
                    }
                    else
                    {
                        LogWarning($"Player character has no CharacterJumpAttack component - cannot perform jump attack");
                    }
                }
                else
                {
                    if (playCardButton.playerCharacter == null)
                    {
                        LogError("Cannot trigger player attack - playerCharacter is NULL!");
                    }
                    if (currentEnemy == null)
                    {
                        LogError("Cannot trigger player attack - currentEnemy is NULL!");
                    }
                }
            }
            
            if (!hasAnimation)
            {
                // No animation - apply damage immediately on Master Client
                if (PhotonNetwork.IsMasterClient)
                {
                    playCardButton.EnemyTakeDamage(damage);
                    
                    // Sync enemy health to all clients
                    int enemyIndex = playCardButton.enemyManager.counter;
                    float newHealth = playCardButton.enemyHealthAmount[enemyIndex];
                    photonView.RPC("RPC_SyncEnemyHealth", RpcTarget.All, enemyIndex, newHealth);
                    
                    Log($"No animation - enemy damaged immediately by {damage}");
                }
            }
            
            // Sync correct answer to all players
            // Get the answer index from the current counter before it was incremented
            if (playCardButton != null && playCardButton.outputManager != null)
            {
                int outputIndex = playCardButton.outputManager.counter;
                // The answer index should be passed from PlayCardButton, but we'll use counter as fallback
                // This will be synced via SyncCardState which is called from PlayCardButton
                Log($"Correct answer played - will sync answer list for output {outputIndex}");
            }
            
            Log($"===== CORRECT ANSWER: RPC_SyncEnemyHealth will handle turn advancement =====");
            // NOTE: Turn advancement for correct answers is handled in RPC_SyncEnemyHealth
            // This ensures the turn advances whether the enemy dies or survives
        }
        else
        {
            // WRONG ANSWER - Trigger enemy attack animation and damage shared health
            if (PhotonNetwork.IsMasterClient)
            {
                Log("Master Client damaging shared health");
                
                // Find the player character and enemy to trigger attack animation
                bool hasAnimation = false;
                if (playCardButton != null && playCardButton.playerCharacter != null && 
                    playCardButton.enemyManager.counter < playCardButton.enemyManager.enemies.Count)
                {
                    GameObject currentEnemy = playCardButton.enemyManager.enemies[playCardButton.enemyManager.counter];
                    EnemyJumpAttack enemyJumpAttack = currentEnemy.GetComponent<EnemyJumpAttack>();
                    
                    if (enemyJumpAttack != null)
                    {
                        hasAnimation = true;
                        delayHealthUIUpdate = true;
                        
                        // CRITICAL: Ensure player character is valid and in the correct position
                        if (playCardButton.playerCharacter == null)
                        {
                            LogError("Cannot trigger enemy attack - playerCharacter is NULL!");
                            hasAnimation = false;
                        }
                        else
                        {
                            // Update enemy's original position before attack to ensure correct return position
                            enemyJumpAttack.UpdateOriginalPosition();
                            
                            // CRITICAL: Use playCardButton.playerCharacter directly (shared character for all players)
                            if (playCardButton.playerCharacter == null)
                            {
                                LogError("Cannot trigger enemy attack - playerCharacter is NULL!");
                                hasAnimation = false;
                            }
                            else
                            {
                                // CRITICAL: Use Transform instead of position (same as singleplayer)
                                // This ensures the enemy attacks the actual player character position
                                Log($"Enemy attacking player character: {playCardButton.playerCharacter.name}");
                                Log($"Enemy position: {currentEnemy.transform.position}, Player position: {playCardButton.playerCharacter.transform.position}, Distance: {Vector3.Distance(currentEnemy.transform.position, playCardButton.playerCharacter.transform.position)}");
                                
                                // Trigger enemy attack animation (using Transform like singleplayer)
                                enemyJumpAttack.PerformJumpAttack(playCardButton.playerCharacter.transform, () => {
                                    // Damage applied when animation hits
                                    DamageSharedHealth(damage);
                                    
                                    // Force UI update immediately after damage
                                    ForceUpdateHealthUI();
                                    delayHealthUIUpdate = false;
                                }, damage);
                            }
                        }
                    }
                }
                
                if (!hasAnimation)
                {
                    // No animation - apply damage immediately
                    DamageSharedHealth(damage);
                }
                
                Log("Wrong answer - damaging shared health and advancing turn after delay");
                Log($"===== WRONG ANSWER: Starting turn advancement coroutine =====");
                
                // Force card reset for all players before advancing turn
                photonView.RPC("RPC_ForceCardReset", RpcTarget.All);
                
                // Advance turn after delay to allow animation to complete
                StartCoroutine(AdvanceTurnAfterDelay(1.0f));
                Log($"===== WRONG ANSWER: AdvanceTurnAfterDelay coroutine started =====");
            }
            else
            {
                // Non-master clients just play the animation visually
                if (playCardButton != null && playCardButton.playerCharacter != null && 
                    playCardButton.enemyManager.counter < playCardButton.enemyManager.enemies.Count)
                {
                    GameObject currentEnemy = playCardButton.enemyManager.enemies[playCardButton.enemyManager.counter];
                    EnemyJumpAttack enemyJumpAttack = currentEnemy.GetComponent<EnemyJumpAttack>();
                    
                    if (enemyJumpAttack != null)
                    {
                        // CRITICAL: Use playCardButton.playerCharacter directly (shared character for all players)
                        if (playCardButton.playerCharacter != null)
                        {
                            // Update enemy's original position before attack
                            enemyJumpAttack.UpdateOriginalPosition();
                            
                            // CRITICAL: Use Transform instead of position (same as singleplayer)
                            // This ensures the enemy attacks the actual player character position
                            Log($"Non-master: Enemy attacking player character: {playCardButton.playerCharacter.name}");
                            
                            enemyJumpAttack.PerformJumpAttack(playCardButton.playerCharacter.transform, () => {
                                Log("Non-master: Enemy attack animation complete (visual only)");
                            }, damage);
                        }
                        else
                        {
                            LogWarning("Cannot trigger enemy attack on non-master - playerCharacter is NULL!");
                        }
                    }
                }
                
                Log("Non-master client, waiting for sync");
            }
        }
    }
    
    /// <summary>
    /// Sync enemy health across all clients and update UI
    /// </summary>
    [PunRPC]
    void RPC_SyncEnemyHealth(int enemyIndex, float newHealth)
    {
        Log($"RPC_SyncEnemyHealth - Enemy {enemyIndex}, Health: {newHealth}");
     
        if (playCardButton == null || enemyIndex >= playCardButton.enemyHealthAmount.Count)
        {
            LogWarning($"Cannot sync health - invalid enemy index {enemyIndex}");
            return;
        }
      
        // Update enemy health amount
        playCardButton.enemyHealthAmount[enemyIndex] = newHealth;
        
        // Update enemy health UI
        playCardButton.UpdateEnemyHealthUI();
        
        // Master Client checks if enemy is defeated
        if (PhotonNetwork.IsMasterClient)
        {
            if (newHealth <= 0f)
            {
                Log($"Enemy {enemyIndex} defeated!");
                photonView.RPC("RPC_OnEnemyDefeated", RpcTarget.All, enemyIndex);
                StartCoroutine(AdvanceToNextEnemyOrEnd());
            }
            else
            {
                // Enemy still alive but player got correct answer - increment card set
                Log("Enemy still alive - incrementing card counter and advancing turn");
                
                // **CRITICAL: Increment cardManager.counter on EVERY correct answer**
                // This changes the card set for the next player
                // BUT: Check bounds to prevent index out of range errors
                if (cardManager != null)
                {
                    // Check bounds BEFORE incrementing
                    int maxCounter = Mathf.Min(cardManager.cardContainer.Count, cardManager.cardDisplayContainer.Count) - 1;
                    if (maxCounter < 0)
                    {
                        LogError($"CardManager has no card containers! Cannot increment counter.");
                    }
                    else
                    {
                        // Clamp current counter to valid range first
                        cardManager.counter = Mathf.Clamp(cardManager.counter, 0, maxCounter);
                        
                        // Check if we can safely increment
                        if (cardManager.counter < maxCounter)
                        {
                            cardManager.counter++;
                            Log($"CardManager counter incremented to {cardManager.counter} after correct answer (Max: {maxCounter})");
                        }
                        else
                        {
                            LogWarning($"Card counter already at max ({maxCounter}) - reusing last available card set");
                        }
                    }
                }
                
                // Sync ALL counters to all clients
                Log($"Syncing all counters after correct answer: CardManager={cardManager.counter}, PlayButton={playCardButton.counter}, Output={playCardButton.outputManager.counter}");
                SyncAllCountersGlobally();
                
                // **CRITICAL: Do NOT advance turn here - let the attack animation callback handle it**
                // The attack animation callback will slide the character off screen, then advance turn
                // This ensures proper visual flow: Attack -> Slide Out -> Turn Advance -> Next Character Slides In
            }
        }
    }
    
    /// <summary>
    /// Handle enemy defeat across all clients
    /// </summary>
    [PunRPC]
    void RPC_OnEnemyDefeated(int enemyIndex)
    {
        Log($"RPC_OnEnemyDefeated - Enemy {enemyIndex}");
        
        if (playCardButton != null)
        {
            playCardButton.DeactivateEnemy();
            playCardButton.DeactivateOutput();
            playCardButton.DeactivateAnswer();
        }
    }
    
    /// <summary>
    /// Advance to next enemy or end the game
    /// </summary>
    private IEnumerator AdvanceToNextEnemyOrEnd()
    {
        yield return new WaitForSeconds(1.5f);
     
        if (enemyManager == null || playCardButton == null) yield break;
        
        // Increment counters FOR NEXT QUESTION/ENEMY
        enemyManager.counter++;
        playCardButton.outputManager.counter++;
        playCardButton.counter = 0; // Reset answer index for new question
        
        // **CRITICAL: Also increment cardManager.counter to match new question**
        // BUT: Check bounds to prevent index out of range errors
        if (cardManager != null)
        {
            // Check bounds BEFORE incrementing
            int maxCounter = Mathf.Min(cardManager.cardContainer.Count, cardManager.cardDisplayContainer.Count) - 1;
            if (maxCounter < 0)
            {
                LogError($"CardManager has no card containers! Cannot increment counter after enemy defeat.");
            }
            else
            {
                // Clamp current counter to valid range first
                cardManager.counter = Mathf.Clamp(cardManager.counter, 0, maxCounter);
                
                // Check if we can safely increment
                if (cardManager.counter < maxCounter)
                {
                    cardManager.counter++;
                    Log($"CardManager counter incremented to {cardManager.counter} for next question (Max: {maxCounter})");
                }
                else
                {
                    LogWarning($"Card counter already at max ({maxCounter}) after enemy defeat - reusing last available card set");
                }
            }
        }
        
        // Sync ALL counters to all clients using global sync
        Log($"Syncing all counters after enemy defeat: CardManager={cardManager.counter}, PlayButton={playCardButton.counter}, Output={playCardButton.outputManager.counter}");
        SyncAllCountersGlobally();
    
        // Check if all enemies defeated
        if (enemyManager.counter >= enemyManager.enemies.Count)
        {
            Log("All enemies defeated - Game Over (Victory)");
            photonView.RPC("RPC_GameOver", RpcTarget.All, true);
        }
        else
        {
            // Activate next enemy
            Log($"Activating next enemy: {enemyManager.counter}");
            photonView.RPC("RPC_ActivateNextEnemy", RpcTarget.All);
            
            // Advance turn (timer will start automatically via OnTurnChanged)
            yield return new WaitForSeconds(0.5f);
            
            if (turnSystem != null)
            {
                Log("Advancing to next player's turn for new enemy");
                turnSystem.EndTurn();
            }
        }
    }
    
    /// <summary>
    /// Sync counters across all clients
    /// </summary>
    [PunRPC]
    void RPC_SyncCounters(int enemyCounter, int outputCounter, int playButtonCounter)
    {
        Log($"RPC_SyncCounters - Enemy: {enemyCounter}, Output: {outputCounter}, PlayButton: {playButtonCounter}");
    
 if (enemyManager != null) enemyManager.counter = enemyCounter;
        if (playCardButton != null && playCardButton.outputManager != null) playCardButton.outputManager.counter = outputCounter;
 if (playCardButton != null) playCardButton.counter = playButtonCounter;
    }
  
    /// <summary>
    /// Activate next enemy on all clients
    /// </summary>
    [PunRPC]
    void RPC_ActivateNextEnemy()
    {
        Log($"RPC_ActivateNextEnemy - Activating enemy {enemyManager.counter}");
     
        if (playCardButton != null)
        {
            playCardButton.ActivateEnemy();
            playCardButton.ActivateOutput();
            playCardButton.ActivateAnswer();
        }
    }
    
    private void SyncCorrectAnswerUI(int playerActorNumber, int answerIndex)
    {
        if (playCardButton != null && playCardButton.outputManager != null)
        {
            int currentOutput = playCardButton.outputManager.counter;
          
            if (currentOutput < playCardButton.outputManager.answerListContainer.Count &&
                answerIndex < playCardButton.outputManager.answerListContainer[currentOutput].answers.Count)
            {
                var answerObject = playCardButton.outputManager.answerListContainer[currentOutput].answers[answerIndex];
                answerObject.SetActive(true);
                Log($"Synced correct answer UI for output {currentOutput}, answer {answerIndex}");
            }
        }
    }
    
    /// <summary>
    /// Sync correct answer to all players when a player gets it right
    /// </summary>
    public void SyncCorrectAnswerToAll(int outputIndex, int answerIndex)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Log($"Syncing correct answer to all players - Output: {outputIndex}, Answer: {answerIndex}");
            photonView.RPC("RPC_SyncCorrectAnswer", RpcTarget.All, outputIndex, answerIndex);
        }
    }
    
    [PunRPC]
    void RPC_SyncCorrectAnswer(int outputIndex, int answerIndex)
    {
        Log($"RPC_SyncCorrectAnswer received - Output: {outputIndex}, Answer: {answerIndex}");
        
        if (playCardButton != null && playCardButton.outputManager != null)
        {
            // Ensure output index is valid
            if (outputIndex >= 0 && outputIndex < playCardButton.outputManager.answerListContainer.Count)
            {
                var answerContainer = playCardButton.outputManager.answerListContainer[outputIndex];
                
                // Ensure answer index is valid
                if (answerIndex >= 0 && answerIndex < answerContainer.answers.Count)
                {
                    var answerObject = answerContainer.answers[answerIndex];
                    if (answerObject != null)
                    {
                        answerObject.SetActive(true);
                        Log($"Correct answer activated for all players - Output: {outputIndex}, Answer: {answerIndex}");
                    }
                    else
                    {
                        LogWarning($"Answer object is null at Output: {outputIndex}, Answer: {answerIndex}");
                    }
                }
                else
                {
                    LogWarning($"Answer index {answerIndex} out of range for output {outputIndex} (max: {answerContainer.answers.Count - 1})");
                }
            }
            else
            {
                LogWarning($"Output index {outputIndex} out of range (max: {playCardButton.outputManager.answerListContainer.Count - 1})");
            }
        }
        else
        {
            LogError("PlayCardButton or OutputManager is null - cannot sync answer!");
        }
    }
    
    public void SyncCardState(int cardCounter, int playButtonCounter, int outputCounter, int answerIndex)
    {
        photonView.RPC("RPC_SyncCardState", RpcTarget.All, cardCounter, playButtonCounter, outputCounter, answerIndex);
    }
    
    [PunRPC]
    void RPC_SyncCardState(int cardCounter, int playButtonCounter, int outputCounter, int answerIndex)
    {
        Log($"===== RPC_SyncCardState =====");
        Log($"Received - CardManager: {cardCounter}, PlayButton: {playButtonCounter}, Output: {outputCounter}, Answer: {answerIndex}");
        Log($"Before sync - CardManager.counter: {(cardManager != null ? cardManager.counter.ToString() : "null")}, PlayButton.counter: {(playCardButton != null ? playCardButton.counter.ToString() : "null")}");
     
        if (cardManager != null) 
        {
            cardManager.counter = cardCounter;
            Log($"✓ CardManager.counter synced to: {cardCounter} (which question's card set to use)");
        }
   
        if (playCardButton != null) 
        {
            playCardButton.counter = playButtonCounter;
            Log($"✓ PlayButton.counter synced to: {playButtonCounter} (next answer index within question)");
        }
   
        if (playCardButton != null && playCardButton.outputManager != null) 
        {
            playCardButton.outputManager.counter = outputCounter;
            Log($"✓ OutputManager.counter synced to: {outputCounter} (which question/enemy)");
        }
   
        // Activate the correct answer in the UI
        if (answerIndex >= 0 && playCardButton != null && playCardButton.outputManager != null)
        {
            Log($"Attempting to activate answer - Output: {outputCounter}, Answer: {answerIndex}");
            Log($"Answer list container count: {playCardButton.outputManager.answerListContainer.Count}");
            
            if (outputCounter >= 0 && outputCounter < playCardButton.outputManager.answerListContainer.Count)
            {
                int answersCount = playCardButton.outputManager.answerListContainer[outputCounter].answers.Count;
                Log($"Answers available for output {outputCounter}: {answersCount}");
                
                if (answerIndex >= 0 && answerIndex < answersCount)
                {
                    var answerObject = playCardButton.outputManager.answerListContainer[outputCounter].answers[answerIndex];
                    if (answerObject != null)
                    {
                        answerObject.SetActive(true);
                        Log($"✓ Answer activated successfully! Output: {outputCounter}, Answer: {answerIndex}");
                    }
                    else
                    {
                        LogError($"Answer object is NULL at Output: {outputCounter}, Answer: {answerIndex}");
                    }
                }
                else
                {
                    LogError($"Answer index {answerIndex} out of range for {answersCount} answers");
                }
            }
            else
            {
                LogError($"Output counter {outputCounter} out of range for {playCardButton.outputManager.answerListContainer.Count} outputs");
            }
        }
    }
    
    public void OnEnemyDefeated()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(NextTurnAfterDelay(1.5f));
        }
    }
    
    public void AdvanceTurn()
    {
        if (PhotonNetwork.IsMasterClient && turnSystem != null)
        {
            Log("Advancing turn manually");
            turnSystem.EndTurn();
        }
    }
    
    private IEnumerator NextTurnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (AreAllEnemiesDead())
        {
            photonView.RPC("RPC_GameOver", RpcTarget.All, true);
        }
        else
        {
            if (turnSystem != null)
            {
                Log("Moving to next turn after delay");
                turnSystem.EndTurn();
            }
        }
    }
    
    /// <summary>
    /// Slide character off screen after attack, then advance turn
    /// This is called from the attack animation callback on ALL clients
    /// </summary>
    private IEnumerator SlideCharacterOffScreenThenAdvanceTurn(GameObject character, int actorNumber)
    {
        Log($"===== SlideCharacterOffScreenThenAdvanceTurn STARTED for actor {actorNumber} =====");
        Log($"Character: {character?.name}, Active: {character?.activeSelf}, Position: {character?.transform.position}");
        
        // Wait for attack animation to fully complete
        // CharacterJumpAttack moves the character to the enemy, attacks, then returns to original position
        // Wait long enough for the return jump animation to complete (jumpBackDuration is typically 0.5f)
        yield return new WaitForSeconds(0.6f);
        
        // CRITICAL: Ensure we're using the correct character reference
        // The character might have been changed by turn switching, so check if it's still the current character
        if (character == null)
        {
            LogError($"Character is null! Cannot slide off screen.");
            // Still advance turn if master client
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(AdvanceTurnAfterDelay(0.1f));
            }
            yield break;
        }
        
        // Ensure character is active before sliding
        if (!character.activeSelf)
        {
            character.SetActive(true);
            Log($"Character {character.name} was inactive - activated for slide animation");
        }
        
        // Get current position (character should have returned to original position after attack)
        Vector3 startPos = character.transform.position;
        Vector3 targetPos = offScreenLeft;
        
        // Log for debugging
        Log($"Sliding character {character.name} off screen from {startPos} to {targetPos}");
        Log($"Character active: {character.activeSelf}, Is current character: {character == currentCharacterInstance}");
        
        float duration = 0.5f; // 0.5 seconds for slide animation
        float elapsed = 0f;
        
        // Slide character off screen to the left
        while (elapsed < duration && character != null)
        {
            // Check if character is still valid
            if (character == null || !character.activeSelf)
            {
                LogWarning($"Character {character?.name} became null or inactive during slide - stopping animation");
                break;
            }
            
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Use smooth step for better easing
            float smoothT = t * t * (3f - 2f * t);
            character.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            yield return null;
        }
        
        // Finalize position and deactivate
        if (character != null)
        {
            character.transform.position = targetPos;
            character.SetActive(false);
            Log($"Character {character.name} slid off screen to {targetPos} and deactivated");
            
            // Clear current character instance if this was the current character
            if (character == currentCharacterInstance)
            {
                Log($"Clearing currentCharacterInstance (was {character.name})");
                currentCharacterInstance = null;
            }
        }
        
        // Wait a brief moment before advancing turn
        yield return new WaitForSeconds(0.2f);
        
        // Now advance turn - this will trigger character switching for the next player
        Log($"Advancing turn after character slide-out");
        if (PhotonNetwork.IsMasterClient)
        {
            if (turnSystem != null)
            {
                Log("Calling turnSystem.EndTurn()");
                turnSystem.EndTurn();
                Log("turnSystem.EndTurn() completed");
            }
            else
            {
                LogError("CRITICAL: turnSystem is NULL! Cannot advance turn!");
            }
        }
        else
        {
            Log("Non-master client - waiting for master to advance turn");
        }
        
        Log($"===== SlideCharacterOffScreenThenAdvanceTurn COMPLETED for actor {actorNumber} =====");
    }
    
    private IEnumerator AdvanceTurnAfterDelay(float delay)
    {
        Log($"===== ADVANCE TURN AFTER DELAY: {delay}s =====");
        Log($"Master Client: {PhotonNetwork.IsMasterClient}");
        Log($"Current turn player: {turnSystem?.GetCurrentTurnPlayer()?.NickName}");
  
        yield return new WaitForSeconds(delay);
     
        Log($"Delay complete - advancing turn now");
      
        if (turnSystem != null)
        {
            Log("Calling turnSystem.EndTurn()");
            turnSystem.EndTurn();
            Log("turnSystem.EndTurn() completed");
        }
        else
        {
            LogError("CRITICAL: turnSystem is NULL! Cannot advance turn!");
        }
    }
    
    /// <summary>
    /// Advance turn after player attack animation completes
    /// </summary>
    private IEnumerator AdvanceTurnAfterAttackDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (PhotonNetwork.IsMasterClient && turnSystem != null)
        {
            Log("Advancing turn after player attack");
            turnSystem.EndTurn();
        }
    }
  
    /// <summary>
    /// Handle turn timeout - treat like a wrong answer (damage shared health and advance turn)
    /// </summary>
    public void OnTurnTimedOut()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            LogWarning("OnTurnTimedOut called on non-Master Client - ignoring");
            return;
        }
        
        Log("===== TURN TIMED OUT =====");
      
        // Damage shared health (timeout is like a wrong answer)
        DamageSharedHealth(1f);
        Log("Shared health damaged due to timeout");
    
        // Force card reset for all players
        photonView.RPC("RPC_ForceCardReset", RpcTarget.All);
        Log("Cards reset due to timeout");
        
        // Advance turn after delay
        StartCoroutine(AdvanceTurnAfterDelay(1.0f));
    }
    
    #endregion
    
    #region Enemy Management
    
    private bool AreAllEnemiesDead()
    {
        if (enemyManager == null) return false;
        
        foreach (var enemy in enemyManager.enemies)
        {
            if (enemy != null && enemy.activeSelf)
            {
                return false;
            }
        }
   
        return true;
    }
    
    #endregion
  
    #region Multiplayer UI Setup
    
    /// <summary>
    /// Setup multiplayer-specific UI buttons and panels
    /// </summary>
    private void SetupMultiplayerUI()
    {
        // Setup Next Level button (host only)
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            // Hide button for non-hosts
            nextLevelButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }
        
        // Setup Return to Lobby buttons
        if (returnToLobbyButton != null)
        {
            returnToLobbyButton.onClick.RemoveAllListeners();
            returnToLobbyButton.onClick.AddListener(ReturnToLobby);
        }
        
        if (returnToLobbyFromPauseButton != null)
        {
            returnToLobbyFromPauseButton.onClick.RemoveAllListeners();
            returnToLobbyFromPauseButton.onClick.AddListener(ReturnToLobby);
        }
        
        // Ensure panels are hidden initially
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }
    
    /// <summary>
    /// Setup pause system with synchronization
    /// </summary>
    private void SetupPauseSystem()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }
        
        // Listen for ESC key to pause/unpause
        // This will be handled in Update()
    }
    
    /// <summary>
    /// Update loop: Handle ESC key for pause and update UI
    /// </summary>
    private void Update()
    {
        // Update UI (existing functionality)
        UpdateHealthUI();
        UpdateTurnUI();
        
        // Handle ESC key for pause (synchronized across all players)
        if (!isGameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel != null && pausePanel.activeSelf)
            {
                // Unpause
                ResumeGame();
            }
            else
            {
                // Pause (synchronized)
                PauseGame();
            }
        }
    }
    
    /// <summary>
    /// Pause game for all players (synchronized)
    /// </summary>
    public void PauseGame()
    {
        if (isGameOver) return;
        
        // Send RPC to pause for all players
        photonView.RPC("RPC_PauseGame", RpcTarget.All);
    }
    
    /// <summary>
    /// Resume game for all players (synchronized)
    /// </summary>
    public void ResumeGame()
    {
        // Send RPC to resume for all players
        photonView.RPC("RPC_ResumeGame", RpcTarget.All);
    }
    
    [PunRPC]
    void RPC_PauseGame()
    {
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        Log("Game paused (synchronized)");
    }
    
    [PunRPC]
    void RPC_ResumeGame()
    {
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        Log("Game resumed (synchronized)");
    }
    
    /// <summary>
    /// Synchronize expected output panel minimize/maximize across all players
    /// </summary>
    public void SyncExpectedOutputPanel(bool isExpanded)
    {
        if (photonView != null && PhotonNetwork.IsConnected)
        {
            photonView.RPC("RPC_SyncExpectedOutputPanel", RpcTarget.All, isExpanded);
        }
    }
    
    [PunRPC]
    void RPC_SyncExpectedOutputPanel(bool isExpanded)
    {
        if (expectedOutputPanel != null)
        {
            // Use SetPanelState with triggerClickLogic=true to also trigger timer start logic
            // This ensures that when one player clicks the panel, all players get the same behavior
            expectedOutputPanel.SetPanelState(isExpanded, triggerClickLogic: true);
            Log($"Synced expected output panel: {(isExpanded ? "Expanded" : "Minimized")}");
        }
    }
    
    #endregion
    
    #region Game Over
    
    [PunRPC]
    void RPC_GameOver(bool victory)
    {
        isGameOver = true;
        DisablePlayerControls();
        HideMultiplayerUI();
        
        // Resume game if paused
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        
        // Update leaderboard if victory
        if (victory)
        {
            // Increment levels beaten for ALL players in the room (shared victory)
            var leaderboardManager = FindObjectOfType<MultiplayerLeaderboardManager>();
            if (leaderboardManager != null && PhotonNetwork.IsMasterClient)
            {
                // Host increments for all players
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    if (player != null)
                    {
                        leaderboardManager.IncrementLevelsBeaten(player.ActorNumber);
                    }
                }
            }
            else if (leaderboardManager != null)
            {
                // Non-host: just increment locally (host will sync via RPC)
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    if (player != null)
                    {
                        leaderboardManager.IncrementLevelsBeatenLocalOnly(player.ActorNumber);
                    }
                }
            }
            
            // Update display for local player
            if (leaderboardManager != null)
            {
                leaderboardManager.UpdateLevelsBeatenDisplay();
            }
        }
        else
        {
            // Game Over (defeat) - Save session scores to leaderboard
            var leaderboardManager = FindObjectOfType<MultiplayerLeaderboardManager>();
            if (leaderboardManager != null)
            {
                // Save current session scores (only if they're new bests)
                leaderboardManager.SaveSessionScoresToLeaderboard();
                
                // Update display
                leaderboardManager.UpdateLevelsBeatenDisplay();
            }
        }
        
        if (victory)
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
            if (gameOverText != null) gameOverText.text = "VICTORY!\nAll enemies defeated!";
            
            // Show Next Level button only for host
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            }
            
            // Play victory music
            PlayVictoryMusic();
        }
        else
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (gameOverText != null) gameOverText.text = "GAME OVER\nShared health depleted!";
            
            // Update levels beaten display even on defeat (shows current progress)
            var leaderboardManager = FindObjectOfType<MultiplayerLeaderboardManager>();
            if (leaderboardManager != null)
            {
                leaderboardManager.UpdateLevelsBeatenDisplay();
            }
            
            // Play defeat music
            PlayDefeatMusic();
        }
    }
    
    /// <summary>
    /// Called when Next Level button is clicked (host only)
    /// </summary>
    private void OnNextLevelClicked()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            LogWarning("Non-host tried to click Next Level button!");
            return;
        }
        
        LoadNextRandomLevel();
    }
    
    /// <summary>
    /// Load next random level from available multiplayer levels
    /// </summary>
    private void LoadNextRandomLevel()
    {
        // Get available levels from LobbyManager
        var lobbyManager = FindObjectOfType<LobbyManager>();
        if (lobbyManager != null)
        {
            string[] availableLevels = lobbyManager.GetAvailableLevels();
            if (availableLevels != null && availableLevels.Length > 0)
            {
                // Pick random level
                string randomLevel = availableLevels[Random.Range(0, availableLevels.Length)];
                Log($"Loading next random level: {randomLevel}");
                PhotonNetwork.LoadLevel(randomLevel);
                return;
            }
        }
        
        // Fallback: use hardcoded levels if LobbyManager not found
        string[] fallbackLevels = { "Part1Level1", "Part1Level2", "Part1Level3" };
        string randomLevelFallback = fallbackLevels[Random.Range(0, fallbackLevels.Length)];
        LogWarning($"LobbyManager not found, using fallback level: {randomLevelFallback}");
        PhotonNetwork.LoadLevel(randomLevelFallback);
    }
    
    public void RestartLevel()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
    
    public void ReturnToLobby()
    {
        // Save session scores to leaderboard before leaving
        var leaderboardManager = FindObjectOfType<MultiplayerLeaderboardManager>();
        if (leaderboardManager != null && PhotonNetwork.IsMasterClient)
        {
            leaderboardManager.SaveSessionScoresToLeaderboard();
        }
        
        // Resume time scale before leaving
        Time.timeScale = 1f;
        PhotonNetwork.LoadLevel("MultiplayerLobby");
    }
    
    #endregion
    
    #region Player List UI
    
    private void UpdatePlayerListUI()
    {
        if (playerListContainer == null)
        {
            LogWarning("Player list container is NULL!");
            return;
        }
        
        if (playerListItemPrefab == null)
        {
            LogWarning("Player list item prefab is NULL!");
            return;
        }
        
        // Clear existing items
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }
        
        Log($"Creating player list for {PhotonNetwork.PlayerList.Length} players");
        
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject listItem = Instantiate(playerListItemPrefab, playerListContainer);
            listItem.SetActive(true);
            
            TextMeshProUGUI nameText = listItem.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = player.NickName;
                
                if (player.IsLocal)
                {
                    nameText.text += " (You)";
                    nameText.color = Color.green;
                }
                
                Log($"Added player: {nameText.text}");
            }
            else
            {
                LogWarning("No TextMeshProUGUI found in player list item prefab!");
            }
        }
        
        playerListContainer.gameObject.SetActive(true);
    }
    
    #endregion
    
    #region Photon Callbacks
    
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("SharedHealth"))
        {
            UpdateHealthUI();
        }
    }
 
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playerCharacters.ContainsKey(otherPlayer.ActorNumber))
        {
            Destroy(playerCharacters[otherPlayer.ActorNumber]);
            playerCharacters.Remove(otherPlayer.ActorNumber);
        }
  
        UpdatePlayerListUI();
        
        // NEW: If a player leaves during an active game, return all players to lobby
        if (!isGameOver && PhotonNetwork.InRoom)
        {
            Log($"Player {otherPlayer.NickName} left during game - returning all players to lobby");
     
            // Show message to remaining players
            if (gameOverText != null)
            {
                gameOverText.text = $"Player {otherPlayer.NickName} left the game!\nReturning to lobby...";
            }
        
            // Delay slightly to show message, then return to lobby
            StartCoroutine(ReturnToLobbyAfterDelay(2f));
        }
    }
  
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Log($"===== OnPlayerEnteredRoom: {newPlayer.NickName} (ActorNumber: {newPlayer.ActorNumber}) =====");
        
        // CRITICAL: If a new player joins after characters are loaded, we need to load their character
        // Wait a bit for their cosmetic property to be set
        if (playerCharacters != null && playerCharacters.Count > 0)
        {
            Log($"New player joined after characters were loaded - will load their character after property sync");
            StartCoroutine(LoadNewPlayerCharacterAfterDelay(newPlayer, 1.0f));
        }
        
        UpdatePlayerListUI();
    }
    
    /// <summary>
    /// Load character for a player who joined after the initial character loading
    /// </summary>
    private IEnumerator LoadNewPlayerCharacterAfterDelay(Player player, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (!PhotonNetwork.InRoom || player == null)
        {
            LogWarning("Cannot load new player character - not in room or player is null");
            yield break;
        }
        
        Log($"===== Loading character for new player: {player.NickName} =====");
        
        // Check if character already exists
        if (playerCharacters.ContainsKey(player.ActorNumber) && playerCharacters[player.ActorNumber] != null)
        {
            LogWarning($"Character for player {player.NickName} (ActorNumber: {player.ActorNumber}) already exists!");
            yield break;
        }
        
        // Get cosmetic property
        object cosmeticObj;
        string cosmetic = "Default";
        
        if (player.CustomProperties.TryGetValue("multiplayer_cosmetic", out cosmeticObj))
        {
            cosmetic = cosmeticObj.ToString();
            Log($"Found 'multiplayer_cosmetic' property: {cosmetic}");
        }
        else
        {
            // Calculate based on position
            Player[] sortedPlayers = PhotonNetwork.PlayerList;
            System.Array.Sort(sortedPlayers, (a, b) => a.ActorNumber.CompareTo(b.ActorNumber));
            
            int playerPosition = -1;
            for (int i = 0; i < sortedPlayers.Length; i++)
            {
                if (sortedPlayers[i].ActorNumber == player.ActorNumber)
                {
                    playerPosition = i;
                    break;
                }
            }
            
            string[] defaultCosmetics = { "Knight", "Ronin", "Daimyo", "King", "DemonGirl" };
            if (playerPosition >= 0 && playerPosition < defaultCosmetics.Length)
            {
                cosmetic = defaultCosmetics[playerPosition];
                LogWarning($"Player {player.NickName} has no cosmetic property - assigned '{cosmetic}' based on position {playerPosition}");
            }
        }
        
        LoadPlayerCharacter(player.ActorNumber, cosmetic);
        Log($"===== Character loading complete for new player: {player.NickName} =====");
    }
    
    /// <summary>
    /// Called when a player's custom properties are updated
    /// </summary>
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Log($"===== OnPlayerPropertiesUpdate: {targetPlayer.NickName} =====");
        
        // Check if cosmetic property was updated
        if (changedProps.ContainsKey("multiplayer_cosmetic"))
        {
            object cosmeticObj = changedProps["multiplayer_cosmetic"];
            string cosmetic = cosmeticObj != null ? cosmeticObj.ToString() : "Default";
            Log($"Player {targetPlayer.NickName} cosmetic property updated to: {cosmetic}");
            
            // If character already exists but with wrong cosmetic, reload it
            if (playerCharacters.ContainsKey(targetPlayer.ActorNumber))
            {
                GameObject existingCharacter = playerCharacters[targetPlayer.ActorNumber];
                if (existingCharacter != null)
                {
                    // Check if the character needs to be reloaded
                    // For now, just log - reloading could cause issues if character is currently displayed
                    Log($"Character for {targetPlayer.NickName} already exists - cosmetic property update noted (character not reloaded to avoid disruption)");
                }
            }
            else
            {
                // Character doesn't exist yet - load it
                Log($"Character for {targetPlayer.NickName} doesn't exist yet - loading with cosmetic: {cosmetic}");
                LoadPlayerCharacter(targetPlayer.ActorNumber, cosmetic);
            }
        }
        
        // Update player list UI if other properties changed
        UpdatePlayerListUI();
    }
  
    /// <summary>
    /// Return all players to lobby after a delay
    /// </summary>
    private IEnumerator ReturnToLobbyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (PhotonNetwork.InRoom)
        {
            Log("Leaving room and returning to lobby...");
    PhotonNetwork.LeaveRoom();
        }
    }
    
    /// <summary>
    /// Callback when we leave a room - return to lobby scene
  /// </summary>
    public override void OnLeftRoom()
    {
        Log("Successfully left room - loading lobby scene");
        PhotonNetwork.LoadLevel("MultiplayerLobby");
    }
    
    #endregion
    
    #region Music
    
    /// <summary>
    /// Start playing battle music
    /// </summary>
    private void StartBattleMusic()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayBattleMusic();
        }
    }
    
    /// <summary>
    /// Play victory music
    /// </summary>
    private void PlayVictoryMusic()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayVictoryMusic();
        }
    }
    
    /// <summary>
    /// Play defeat music
    /// </summary>
    private void PlayDefeatMusic()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayDefeatMusic();
        }
    }
    
    #endregion
}
