using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Automatically adapts level layout when loaded in multiplayer mode
/// Attach this to a GameObject in each level scene
/// NOTE: Requires Photon PUN2 to be installed for multiplayer detection
/// </summary>
public class MultiplayerLevelAdapter : MonoBehaviour
{
    [Header("Layout Detection")]
    [Tooltip("Automatically detect mode based on scene name or manual setting")]
    [SerializeField] private bool autoDetectMode = true;
    
    [Header("Single Player Elements")]
    [Tooltip("GameObjects to show only in single player")]
    [SerializeField] private List<GameObject> singlePlayerOnly = new List<GameObject>();
    
    [Header("Multiplayer Elements")]
    [Tooltip("GameObjects to show only in multiplayer")]
    [SerializeField] private List<GameObject> multiplayerOnly = new List<GameObject>();
    
    [Header("UI Adjustments")]
    [Tooltip("Single player UI canvas")]
    [SerializeField] private Canvas singlePlayerUI;
    
    [Tooltip("Multiplayer UI canvas (with turn indicators, player list, etc.)")]
    [SerializeField] private Canvas multiplayerUI;
    
    [Header("Camera Adjustments")]
    [Tooltip("Single player camera position")]
    [SerializeField] private Vector3 singlePlayerCameraPosition = new Vector3(0, 5, -10);
    
    [Tooltip("Multiplayer camera position (wider view for multiple players)")]
    [SerializeField] private Vector3 multiplayerCameraPosition = new Vector3(0, 8, -15);
    
    [SerializeField] private bool adjustCameraPosition = true;
    
    [Header("Gameplay Adjustments")]
    [Tooltip("Enemy spawn positions for single player")]
    [SerializeField] private Transform[] singlePlayerEnemySpawns;
    
    [Tooltip("Enemy spawn positions for multiplayer (more spread out)")]
    [SerializeField] private Transform[] multiplayerEnemySpawns;
    
    [Header("Grid/Board Layout")]
    [Tooltip("Single player card grid (e.g., 3x3)")]
    [SerializeField] private GameObject singlePlayerCardGrid;
    
    [Tooltip("Multiplayer card grid (e.g., 4x4 or side-by-side)")]
    [SerializeField] private GameObject multiplayerCardGrid;
    
    private bool isMultiplayerMode = false;
    
    private void Awake()
    {
        // Detect mode
      if (autoDetectMode)
     {
            DetectMode();
   }
        
     Debug.Log($"[MultiplayerLevelAdapter] Mode detected: {(isMultiplayerMode ? "Multiplayer" : "Single Player")}");

        // Apply layout changes immediately
        ApplyLayout();
    }
    
    /// <summary>
    /// Detect if we're in multiplayer mode using reflection to avoid compile errors
    /// </summary>
    private void DetectMode()
    {
      try
        {
 // Try to find PhotonNetwork class using reflection
            System.Type photonNetworkType = System.Type.GetType("Photon.Pun.PhotonNetwork, PhotonUnityNetworking");
    
      if (photonNetworkType != null)
        {
                // Get IsConnected property
      var isConnectedProp = photonNetworkType.GetProperty("IsConnected");
  var inRoomProp = photonNetworkType.GetProperty("InRoom");
     
       if (isConnectedProp != null && inRoomProp != null)
          {
        bool isConnected = (bool)isConnectedProp.GetValue(null);
         bool inRoom = (bool)inRoomProp.GetValue(null);
          
    isMultiplayerMode = isConnected && inRoom;
           return;
   }
      }
        }
        catch (System.Exception ex)
   {
            Debug.LogWarning($"[MultiplayerLevelAdapter] Could not detect Photon: {ex.Message}");
        }
        
    // Fallback: not multiplayer
        isMultiplayerMode = false;
 }
    
    /// <summary>
    /// Apply the appropriate layout based on game mode
    /// </summary>
    private void ApplyLayout()
    {
        if (isMultiplayerMode)
        {
            ApplyMultiplayerLayout();
        }
        else
        {
      ApplySinglePlayerLayout();
        }
    }
    
    /// <summary>
    /// Configure level for single player mode
    /// </summary>
    private void ApplySinglePlayerLayout()
  {
        // Show single player elements
        foreach (var obj in singlePlayerOnly)
        {
    if (obj != null) obj.SetActive(true);
        }
 
  // Hide multiplayer elements
 foreach (var obj in multiplayerOnly)
 {
    if (obj != null) obj.SetActive(false);
   }
      
        // UI
        if (singlePlayerUI != null) singlePlayerUI.enabled = true;
        if (multiplayerUI != null) multiplayerUI.enabled = false;
        
        // Camera
        if (adjustCameraPosition)
        {
            Camera mainCam = Camera.main;
        if (mainCam != null)
   {
      mainCam.transform.position = singlePlayerCameraPosition;
        }
        }
      
  // Card Grid
        if (singlePlayerCardGrid != null) singlePlayerCardGrid.SetActive(true);
      if (multiplayerCardGrid != null) multiplayerCardGrid.SetActive(false);
        
      Debug.Log("[MultiplayerLevelAdapter] Single player layout applied");
    }
    
    /// <summary>
    /// Configure level for multiplayer mode
    /// </summary>
    private void ApplyMultiplayerLayout()
    {
        // Hide single player elements
        foreach (var obj in singlePlayerOnly)
 {
            if (obj != null) obj.SetActive(false);
        }
        
        // Show multiplayer elements
 foreach (var obj in multiplayerOnly)
        {
            if (obj != null) obj.SetActive(true);
        }
        
        // UI
        if (singlePlayerUI != null) singlePlayerUI.enabled = false;
     if (multiplayerUI != null) multiplayerUI.enabled = true;
        
        // Camera
        if (adjustCameraPosition)
        {
        Camera mainCam = Camera.main;
  if (mainCam != null)
            {
        mainCam.transform.position = multiplayerCameraPosition;
            }
        }
        
        // Card Grid
        if (singlePlayerCardGrid != null) singlePlayerCardGrid.SetActive(false);
if (multiplayerCardGrid != null) multiplayerCardGrid.SetActive(true);
  
        // Adjust enemy spawns
        if (multiplayerEnemySpawns != null && multiplayerEnemySpawns.Length > 0)
     {
            AdjustEnemySpawns();
        }
        
      Debug.Log("[MultiplayerLevelAdapter] Multiplayer layout applied");
    }
    
    /// <summary>
 /// Adjust enemy spawn positions for multiplayer
 /// </summary>
    private void AdjustEnemySpawns()
    {
    // Use reflection to avoid compile errors if EnemyManager doesn't exist in this assembly
     var enemyManagerType = System.Type.GetType("EnemyManager");
if (enemyManagerType != null)
     {
            var enemyManager = FindObjectOfType(enemyManagerType);
    if (enemyManager != null && multiplayerEnemySpawns != null)
       {
     // Get enemies list using reflection
     var enemiesField = enemyManagerType.GetField("enemies");
     if (enemiesField != null)
       {
     var enemies = enemiesField.GetValue(enemyManager) as System.Collections.Generic.List<GameObject>;
       if (enemies != null)
           {
        // Reposition enemies to multiplayer spawn points
 for (int i = 0; i < enemies.Count && i < multiplayerEnemySpawns.Length; i++)
     {
        if (enemies[i] != null && multiplayerEnemySpawns[i] != null)
        {
     enemies[i].transform.position = multiplayerEnemySpawns[i].position;
         enemies[i].transform.rotation = multiplayerEnemySpawns[i].rotation;
             }
  }
    }
    }
      }
        }
    }

    /// <summary>
    /// Manually set mode (for testing)
    /// </summary>
    public void SetMode(bool multiplayer)
  {
        isMultiplayerMode = multiplayer;
     ApplyLayout();
    }
    
    /// <summary>
    /// Get current mode
    /// </summary>
    public bool IsMultiplayerMode()
    {
        return isMultiplayerMode;
    }
}
