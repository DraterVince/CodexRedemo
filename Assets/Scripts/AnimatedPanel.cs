using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class AnimatedPanel : MonoBehaviour
{
    public RectTransform panelRect;
    public Vector2 miniSize = new Vector2(640, 360);
    public Vector2 fullSize = new Vector2(1920, 1080);
    public Vector2 miniPos = new Vector2(684, 388);
    public Vector2 fullPos = new Vector2(0, 0);
    public float animationDuration = 0.3f;

    public bool isExpanded = true; // Start expanded (public for sync)
    
    /// <summary>
    /// Event fired when panel state changes (expanded/minimized)
    /// Parameters: (bool isExpanded, AnimatedPanel panel)
    /// </summary>
    public static event Action<bool, AnimatedPanel> OnPanelStateChanged;
    private float animTime = 0f;
    private Vector2 startSize, startPos, targetSize, targetPos;
    private bool hasStartedTimer = false; // Track if timer has been started
    private bool isMultiplayerMode = false;
    private bool isSyncingFromRPC = false; // Flag to prevent infinite sync loops
    private bool isAnimating = false; // Track if animation is in progress
    private Button button; // Cache button reference
    // Reserved for future use
    #pragma warning disable CS0414
    private bool shouldDelaySetActive = false; // Flag to delay SetActive calls
    #pragma warning restore CS0414

    void Start()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();
        
        // Cache button reference
        button = GetComponent<Button>();

        // Start large and centered
        panelRect.sizeDelta = fullSize;
        panelRect.anchoredPosition = fullPos;
        isExpanded = true;
        animTime = animationDuration; // No animation on start

        // Detect multiplayer mode
        DetectMultiplayerMode();
    }

    private void DetectMultiplayerMode()
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

                    if (isMultiplayerMode)
                    {
                        Debug.Log("[AnimatedPanel] Multiplayer mode detected - timer will sync");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[AnimatedPanel] Could not detect Photon: {ex.Message}");
            isMultiplayerMode = false;
        }
    }

    /// <summary>
    /// Called when panel is clicked from Unity Button (no parameters for Inspector assignment).
    /// </summary>
    public void OnPanelClicked()
    {
        OnPanelClicked(null);
    }
    
    /// <summary>
    /// Called when panel is clicked. Can be called with a specific state for syncing.
    /// </summary>
    public void OnPanelClicked(bool? forceState)
    {
        // Ensure panelRect is set
        if (panelRect == null)
        {
            panelRect = GetComponent<RectTransform>();
            if (panelRect == null)
            {
                Debug.LogError("[AnimatedPanel] panelRect is null and could not be found!");
                return;
            }
        }
        
        // If forceState is provided, use it; otherwise toggle
        bool previousState = isExpanded;
        if (forceState.HasValue)
        {
            isExpanded = forceState.Value;
        }
        else
        {
            isExpanded = !isExpanded;
        }
        
        // Fire event if state changed
        if (previousState != isExpanded)
        {
            OnPanelStateChanged?.Invoke(isExpanded, this);
        }
        
        // Note: When called directly from Button click, Unity's Button system already fired all onClick listeners
        // in order before calling OnPanelClicked(). So SetActive calls and Timer.StartTimer should have already fired.
        // When called from RPC sync, we need to manually trigger onClick listeners (done below).
        
        // Reset animation time to start animation
        animTime = 0f;
        startSize = panelRect.sizeDelta;
        startPos = panelRect.anchoredPosition;
        targetSize = isExpanded ? fullSize : miniSize;
        targetPos = isExpanded ? fullPos : miniPos;
        
        // Start animation IMMEDIATELY (don't wait for SetActive calls)
        // This ensures the panel minimizes first, then gameobjects activate
        isAnimating = true;
        animTime = 0f;
        
        // Force immediate visual update to start animation right away
        float t = Mathf.Clamp01(animTime / animationDuration);
        Vector2 immediateSize = Vector2.Lerp(startSize, targetSize, t);
        Vector2 immediatePos = Vector2.Lerp(startPos, targetPos, t);
        panelRect.sizeDelta = immediateSize;
        panelRect.anchoredPosition = immediatePos;
        
        // Verify the values were actually set
        if (Vector2.Distance(panelRect.sizeDelta, immediateSize) > 0.01f)
        {
            Debug.LogError($"[AnimatedPanel] WARNING: sizeDelta was changed immediately after setting! Expected: {immediateSize}, Got: {panelRect.sizeDelta}");
        }
        
        // Note: If called from Button click, SetActive calls already fired (they're before OnPanelClicked in onClick list)
        // The animation will now run and continuously override any layout changes in Update()
        // This creates the visual effect of panel minimizing first, even though gameobjects are activating

        // When minimizing panel for the first time, start timer
        // This happens BEFORE sync so the timer starts locally first
        if (!isExpanded && !hasStartedTimer)
        {
            hasStartedTimer = true;

            if (isMultiplayerMode)
            {
                // In multiplayer: Sync timer across all players
                StartSyncedTimer();
            }
            else
            {
                // In single player: Start timer locally
                StartLocalTimer();
            }
        }

        // Trigger Unity Button onClick events when syncing from RPC
        // When called directly from Button click, the onClick already fired all listeners
        // When syncing from RPC, we need to manually trigger all onClick listeners
        // However, we need to be careful: if OnPanelClicked is in the onClick list, invoking onClick
        // would call OnPanelClicked again, creating a loop. So we only invoke when syncing from RPC.
        if (isSyncingFromRPC || forceState.HasValue)
        {
            Button button = GetComponent<Button>();
            if (button != null && button.onClick != null)
            {
                // When syncing from RPC, we need to trigger all onClick listeners
                // But we need to prevent OnPanelClicked from being called again
                // So we temporarily set isSyncingFromRPC to prevent the sync call
                bool wasSyncing = isSyncingFromRPC;
                isSyncingFromRPC = true; // Prevent OnPanelClicked from syncing again
                button.onClick.Invoke();
                isSyncingFromRPC = wasSyncing; // Restore original state
            }
        }

        // In multiplayer: Sync panel state across all players (AFTER local state is set)
        // This ensures other players also get the click event logic (timer start, etc.)
        // Skip sync if this call came from an RPC to prevent infinite loops
        if (isMultiplayerMode && !isSyncingFromRPC)
        {
            SyncPanelState();
        }
    }
    
    /// <summary>
    /// Sync panel state with SharedMultiplayerGameManager
    /// </summary>
    private void SyncPanelState()
    {
        try
        {
            var manager = FindObjectOfType<SharedMultiplayerGameManager>();
            if (manager != null)
            {
                manager.SyncExpectedOutputPanel(isExpanded);
                Debug.Log($"[AnimatedPanel] Synced panel state: {(isExpanded ? "Expanded" : "Minimized")}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[AnimatedPanel] Could not sync panel state: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Force panel to a specific state (called by RPC)
    /// This triggers the full OnPanelClicked() logic to ensure all click events fire
    /// </summary>
    public void SetPanelState(bool expanded, bool triggerClickLogic = true)
    {
        if (isExpanded != expanded)
        {
            if (triggerClickLogic)
            {
                // If we need to trigger full click logic, call OnPanelClicked() with the desired state
                // This ensures all Unity Button onClick events fire and gameobjects are activated
                // OnPanelClicked will fire the event, so we don't need to fire it here
                isSyncingFromRPC = true;
                OnPanelClicked(expanded); // Pass the desired state directly
                isSyncingFromRPC = false;
            }
            else
            {
                // Just change visual state without triggering click logic
                bool previousState = isExpanded;
                isExpanded = expanded;
                animTime = 0f;
                startSize = panelRect.sizeDelta;
                startPos = panelRect.anchoredPosition;
                targetSize = isExpanded ? fullSize : miniSize;
                targetPos = isExpanded ? fullPos : miniPos;
                
                // Fire event for state change
                if (previousState != isExpanded)
                {
                    OnPanelStateChanged?.Invoke(isExpanded, this);
                }
            }
        }
    }

    private void StartSyncedTimer()
    {
        try
        {
            // Method 1: Direct find
            var manager = GameObject.FindObjectOfType<SharedMultiplayerGameManager>();
            if (manager != null)
            {
                manager.StartTimerForAllPlayers();
                Debug.Log("[AnimatedPanel] Started synced timer via direct reference");
                return;
            }

            // Method 2: Reflection fallback
            var managerType = System.Type.GetType("SharedMultiplayerGameManager");
            if (managerType != null)
            {
                var managerObj = GameObject.FindObjectOfType(managerType);
                if (managerObj != null)
                {
                    var method = managerType.GetMethod("StartTimerForAllPlayers");
                    if (method != null)
                    {
                        method.Invoke(managerObj, null);
                        Debug.Log("[AnimatedPanel] Started synced timer via reflection");
                        return;
                    }
                }
            }

            Debug.LogWarning("[AnimatedPanel] Could not start synced timer - SharedMultiplayerGameManager not found");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AnimatedPanel] Error starting synced timer: {ex.Message}");
        }
    }

    private void StartLocalTimer()
    {
        // Find Timer component and start it
        Timer timer = FindObjectOfType<Timer>();
        if (timer != null)
        {
            timer.StartTimer();
        }
        else
        {
            Debug.LogWarning("[AnimatedPanel] Timer component not found in scene!");
        }
    }
    

    void Update()
    {
        if (panelRect == null)
        {
            Debug.LogWarning("[AnimatedPanel] Update: panelRect is null!");
            return;
        }
        
        // Only animate if we're actively animating
        if (isAnimating && animTime < animationDuration)
        {
            animTime += Time.deltaTime;
            float t = Mathf.Clamp01(animTime / animationDuration);
            Vector2 newSize = Vector2.Lerp(startSize, targetSize, t);
            Vector2 newPos = Vector2.Lerp(startPos, targetPos, t);
            
            // Force set the values - don't let anything override them during animation
            Vector2 oldSize = panelRect.sizeDelta;
            Vector2 oldPos = panelRect.anchoredPosition;
            panelRect.sizeDelta = newSize;
            panelRect.anchoredPosition = newPos;
            
            // Scale children based on current size DURING animation
            // This ensures contents scale down/up smoothly with the panel
            float scale = panelRect.sizeDelta.y / fullSize.y;
            foreach (RectTransform child in panelRect)
            {
                child.localScale = new Vector3(scale, scale, 1f);
            }
            
            // Check if animation completed this frame
            if (animTime >= animationDuration)
            {
                // Ensure we're at the target
                panelRect.sizeDelta = targetSize;
                panelRect.anchoredPosition = targetPos;
                
                // Final scale update for children
                float finalScale = panelRect.sizeDelta.y / fullSize.y;
                foreach (RectTransform child in panelRect)
                {
                    child.localScale = new Vector3(finalScale, finalScale, 1f);
                }
                
                isAnimating = false;
            }
        }
        else if (isAnimating)
        {
            Debug.LogWarning($"[AnimatedPanel] isAnimating is true but animTime ({animTime}) >= animationDuration ({animationDuration})!");
            isAnimating = false;
        }
        else
        {
            // Scale children based on current size when not animating (for any manual size changes)
            float scale = panelRect.sizeDelta.y / fullSize.y;
            foreach (RectTransform child in panelRect)
            {
                child.localScale = new Vector3(scale, scale, 1f);
            }
        }
    }
}
