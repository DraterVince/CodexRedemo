using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
using Photon.Realtime;
#endif

/// <summary>
/// UI Panel for displaying multiplayer leaderboard in multiplayer lobby
/// </summary>
public class LeaderboardPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform leaderboardContainer;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button leaderboardButton; // Reference to the button that opens the leaderboard
    
    [Header("Leaderboard Entry UI (if using prefab)")]
    [SerializeField] private int rankTextIndex = 0; // Index of rank text in prefab
    [SerializeField] private int nameTextIndex = 1; // Index of name text in prefab
    [SerializeField] private int scoreTextIndex = 2; // Index of score text in prefab
    
    private MultiplayerLeaderboardManager leaderboardManager;
    
    private void Start()
    {
        // Find leaderboard manager (it persists across scenes with DontDestroyOnLoad)
        leaderboardManager = FindObjectOfType<MultiplayerLeaderboardManager>();
        
        // If not found, try to find it in the scene or create a reference
        if (leaderboardManager == null)
        {
            // Try to find it by name or tag
            GameObject managerObj = GameObject.Find("MultiplayerLeaderboardManager");
            if (managerObj != null)
            {
                leaderboardManager = managerObj.GetComponent<MultiplayerLeaderboardManager>();
            }
        }
        
        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseLeaderboard);
        }
        
        // Hide panel initially
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        // Refresh leaderboard manager reference when panel is enabled
        if (leaderboardManager == null)
        {
            leaderboardManager = FindObjectOfType<MultiplayerLeaderboardManager>();
        }
    }
    
    /// <summary>
    /// Open and display the leaderboard
    /// </summary>
    /// <param name="buttonToHide">Optional: Button to hide after panel is confirmed visible</param>
    public void OpenLeaderboard(Button buttonToHide = null)
    {
        Debug.Log("[LeaderboardPanel] OpenLeaderboard called");
        
        // Use provided button or fall back to assigned reference
        if (buttonToHide != null)
        {
            leaderboardButton = buttonToHide;
        }
        
        // CRITICAL: Check if panel reference exists FIRST - don't hide button if panel can't be shown
        if (leaderboardPanel == null)
        {
            // Try to find the panel by name as a fallback
            GameObject foundPanel = GameObject.Find("LeaderboardPanel");
            if (foundPanel == null)
            {
                // Try other common names
                foundPanel = GameObject.Find("Leaderboard");
            }
            
            if (foundPanel != null)
            {
                leaderboardPanel = foundPanel;
                Debug.LogWarning($"[LeaderboardPanel] leaderboardPanel was not assigned in Inspector, but found by name: {foundPanel.name}. Please assign it in the Inspector for better performance.");
            }
            else
            {
                Debug.LogError("[LeaderboardPanel] leaderboardPanel GameObject is NULL and could not be found by name! Cannot open leaderboard. Please assign it in the Inspector.");
                return; // Don't hide button if we can't show panel
            }
        }
        
        // Verify the panel GameObject is valid and not destroyed
        if (leaderboardPanel == null)
        {
            Debug.LogError("[LeaderboardPanel] leaderboardPanel is NULL after assignment! This should not happen.");
            return;
        }
        
        // Check if the GameObject was destroyed
        if (leaderboardPanel.GetInstanceID() == 0)
        {
            Debug.LogError("[LeaderboardPanel] leaderboardPanel GameObject appears to be destroyed or invalid!");
            return;
        }
        
        Debug.Log($"[LeaderboardPanel] Activating leaderboard panel: {leaderboardPanel.name} (InstanceID: {leaderboardPanel.GetInstanceID()})");
        
        // Ensure parent objects are active
        Transform parent = leaderboardPanel.transform.parent;
        while (parent != null)
        {
            if (!parent.gameObject.activeSelf)
            {
                Debug.LogWarning($"[LeaderboardPanel] Parent {parent.name} is inactive! Activating it.");
                parent.gameObject.SetActive(true);
            }
            parent = parent.parent;
        }
        
        // Ensure Canvas is active
        Canvas canvas = leaderboardPanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (!canvas.gameObject.activeSelf)
            {
                Debug.LogWarning("[LeaderboardPanel] Canvas is inactive! Activating it.");
                canvas.gameObject.SetActive(true);
            }
            
            // Check for other canvases that might be in front
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            int maxSortingOrder = 0;
            foreach (Canvas c in allCanvases)
            {
                if (c.overrideSorting && c.sortingOrder > maxSortingOrder)
                {
                    maxSortingOrder = c.sortingOrder;
                }
            }
            
            // Set sorting order higher than any other canvas
            int targetSortingOrder = Mathf.Max(100, maxSortingOrder + 10);
            if (canvas.overrideSorting == false || canvas.sortingOrder < targetSortingOrder)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = targetSortingOrder;
                Debug.Log($"[LeaderboardPanel] Canvas sorting order set to {canvas.sortingOrder} (max found: {maxSortingOrder})");
            }
            
            // Ensure canvas has GraphicRaycaster for interaction
            if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                Debug.Log("[LeaderboardPanel] Added GraphicRaycaster to canvas");
            }
        }
        else
        {
            Debug.LogError("[LeaderboardPanel] No Canvas found! Panel cannot be displayed. Adding Canvas component...");
            // Try to add a canvas if none exists
            canvas = leaderboardPanel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1000; // Very high to ensure it's on top
            leaderboardPanel.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            leaderboardPanel.AddComponent<UnityEngine.UI.CanvasScaler>();
            Debug.LogWarning("[LeaderboardPanel] Added Canvas component to panel - this is unusual, check your scene setup!");
        }
        
        // Bring panel to front in hierarchy (renders on top)
        // Keep panel in its original parent but bring it to front
        leaderboardPanel.transform.SetAsLastSibling();
        
        // Also ensure the parent is at the front of its siblings
        Transform panelParent = leaderboardPanel.transform.parent;
        if (panelParent != null && panelParent.parent != null)
        {
            panelParent.SetAsLastSibling();
            Debug.Log($"[LeaderboardPanel] Panel and parent brought to front in hierarchy");
        }
        
        // Ensure CanvasGroup alpha is 1 (fully visible)
        CanvasGroup canvasGroup = leaderboardPanel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            Debug.Log("[LeaderboardPanel] CanvasGroup alpha set to 1");
        }
        
        // Ensure panel has a visible background (Image component)
        Image panelImage = leaderboardPanel.GetComponent<Image>();
        if (panelImage != null)
        {
            // Make sure image is enabled and visible
            panelImage.enabled = true;
            Color imageColor = panelImage.color;
            // Force alpha to be visible (at least 0.9)
            if (imageColor.a < 0.9f)
            {
                imageColor.a = 0.9f;
                panelImage.color = imageColor;
                Debug.Log($"[LeaderboardPanel] Panel Image alpha was {imageColor.a}, set to 0.9 for visibility");
            }
            Debug.Log($"[LeaderboardPanel] Panel Image color: {imageColor}, enabled: {panelImage.enabled}, raycastTarget: {panelImage.raycastTarget}");
        }
        else
        {
            Debug.LogWarning("[LeaderboardPanel] Panel has no Image component! Adding one for visibility...");
            // Add an Image component for visibility
            panelImage = leaderboardPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.9f); // Semi-transparent black background
            panelImage.raycastTarget = true; // Allow raycasting
            Debug.Log("[LeaderboardPanel] Added Image component with semi-transparent black background");
        }
        
        // CRITICAL: Ensure the panel's RectTransform is properly set up
        RectTransform panelRect = leaderboardPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            // Force the panel to fill the entire screen
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            Debug.Log("[LeaderboardPanel] Panel RectTransform set to fill screen");
        }
        
        // CRITICAL: Activate the panel and verify it stays active
        Debug.Log($"[LeaderboardPanel] BEFORE activation - Panel activeSelf: {leaderboardPanel.activeSelf}, activeInHierarchy: {leaderboardPanel.activeInHierarchy}");
        
        // Activate the panel
        leaderboardPanel.SetActive(true);
        UnityEngine.Canvas.ForceUpdateCanvases();
        
        // Verify activation immediately
        if (!leaderboardPanel.activeSelf)
        {
            Debug.LogError("[LeaderboardPanel] Panel SetActive(true) FAILED! Panel reference might be wrong or destroyed.");
            Debug.LogError($"[LeaderboardPanel] Panel GameObject: {(leaderboardPanel == null ? "NULL" : leaderboardPanel.name)}, InstanceID: {(leaderboardPanel == null ? "N/A" : leaderboardPanel.GetInstanceID().ToString())}");
            return; // Don't hide button if panel can't be activated
        }
        
        Debug.Log($"[LeaderboardPanel] AFTER activation - Panel activeSelf: {leaderboardPanel.activeSelf}, activeInHierarchy: {leaderboardPanel.activeInHierarchy}");
        
        // CRITICAL: Ensure all children are active (they might be inactive)
        for (int i = 0; i < leaderboardPanel.transform.childCount; i++)
        {
            Transform child = leaderboardPanel.transform.GetChild(i);
            if (!child.gameObject.activeSelf)
            {
                Debug.LogWarning($"[LeaderboardPanel] Child {child.name} is inactive! Activating it.");
                child.gameObject.SetActive(true);
            }
        }
        
        // Double-check panel is still active after activating children
        if (!leaderboardPanel.activeSelf)
        {
            Debug.LogError("[LeaderboardPanel] Panel became inactive after activating children! Reactivating...");
            leaderboardPanel.SetActive(true);
        }
        
        // Check panel's RectTransform to ensure it's visible
        RectTransform rectTransform = leaderboardPanel.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Debug.Log($"[LeaderboardPanel] Panel position: {rectTransform.position}, sizeDelta: {rectTransform.sizeDelta}, anchoredPosition: {rectTransform.anchoredPosition}");
            Debug.Log($"[LeaderboardPanel] Panel rect: {rectTransform.rect}, anchorMin: {rectTransform.anchorMin}, anchorMax: {rectTransform.anchorMax}");
            
            // Check if panel has proper size (either from sizeDelta or from anchors)
            Rect rect = rectTransform.rect;
            bool hasSize = rect.width > 0 && rect.height > 0;
            
            if (!hasSize)
            {
                Debug.LogWarning("[LeaderboardPanel] Panel has no size! Attempting to fix...");
                
                // Check if it's using stretched anchors
                Vector2 anchorMin = rectTransform.anchorMin;
                Vector2 anchorMax = rectTransform.anchorMax;
                bool isStretched = (anchorMin.x != anchorMax.x) || (anchorMin.y != anchorMax.y);
                
                if (isStretched)
                {
                    // Panel is using stretched anchors - set offset to fill parent
                    rectTransform.offsetMin = Vector2.zero; // Left/Bottom
                    rectTransform.offsetMax = Vector2.zero; // Right/Top
                    Debug.Log("[LeaderboardPanel] Set stretched anchors with zero offsets");
                }
                else
                {
                    // Panel is not stretched - set a default size
                    if (rectTransform.sizeDelta.x == 0 && rectTransform.sizeDelta.y == 0)
                    {
                        // Set default size (e.g., 800x600 or full screen)
                        Canvas parentCanvas = leaderboardPanel.GetComponentInParent<Canvas>();
                        if (parentCanvas != null)
                        {
                            RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                            if (canvasRect != null)
                            {
                                // Set to full screen size
                                rectTransform.sizeDelta = canvasRect.sizeDelta;
                                Debug.Log($"[LeaderboardPanel] Set panel size to canvas size: {rectTransform.sizeDelta}");
                            }
                            else
                            {
                                // Fallback: set a reasonable default size
                                rectTransform.sizeDelta = new Vector2(800, 600);
                                Debug.Log($"[LeaderboardPanel] Set panel size to default: {rectTransform.sizeDelta}");
                            }
                        }
                        else
                        {
                            // Fallback: set a reasonable default size
                            rectTransform.sizeDelta = new Vector2(800, 600);
                            Debug.Log($"[LeaderboardPanel] Set panel size to default (no canvas): {rectTransform.sizeDelta}");
                        }
                    }
                }
                
                // Verify size after fix
                rect = rectTransform.rect;
                Debug.Log($"[LeaderboardPanel] Panel rect after fix: {rect}, sizeDelta: {rectTransform.sizeDelta}");
            }
            else
            {
                Debug.Log($"[LeaderboardPanel] Panel has valid size: {rect.width}x{rect.height}");
            }
        }
        
        // Final visibility check before hiding button
        bool panelIsVisible = leaderboardPanel.activeSelf && leaderboardPanel.activeInHierarchy;
        
        // Check all parent objects are active
        Transform checkParent = leaderboardPanel.transform.parent;
        while (checkParent != null)
        {
            if (!checkParent.gameObject.activeSelf)
            {
                Debug.LogError($"[LeaderboardPanel] Parent {checkParent.name} is INACTIVE! Panel cannot be visible.");
                panelIsVisible = false;
                break;
            }
            checkParent = checkParent.parent;
        }
        
        // Check canvas is active and visible
        Canvas checkCanvas = leaderboardPanel.GetComponentInParent<Canvas>();
        if (checkCanvas != null)
        {
            if (!checkCanvas.gameObject.activeSelf)
            {
                Debug.LogError("[LeaderboardPanel] Canvas is INACTIVE! Panel cannot be visible.");
                panelIsVisible = false;
            }
            else
            {
                Debug.Log($"[LeaderboardPanel] Canvas is active. RenderMode: {checkCanvas.renderMode}, SortingOrder: {checkCanvas.sortingOrder}");
            }
        }
        else
        {
            Debug.LogError("[LeaderboardPanel] No Canvas found! Panel cannot be visible.");
            panelIsVisible = false;
        }
        
        if (rectTransform != null)
        {
            Rect finalRect = rectTransform.rect;
            bool hasSize = (finalRect.width > 0 && finalRect.height > 0);
            panelIsVisible = panelIsVisible && hasSize;
            
            if (!hasSize)
            {
                Debug.LogError($"[LeaderboardPanel] Panel has NO SIZE! Rect: {finalRect}");
            }
        }
        
        // Check if panel has any visible content
        if (leaderboardContainer != null)
        {
            Debug.Log($"[LeaderboardPanel] Leaderboard container has {leaderboardContainer.childCount} children");
        }
        else
        {
            Debug.LogWarning("[LeaderboardPanel] leaderboardContainer is NULL!");
        }
        
        Debug.Log($"[LeaderboardPanel] Final visibility check - Panel visible: {panelIsVisible}");
        Debug.Log($"[LeaderboardPanel] Panel activeSelf: {leaderboardPanel.activeSelf}, activeInHierarchy: {leaderboardPanel.activeInHierarchy}");
        
        // Refresh the leaderboard content
        RefreshLeaderboard();
        
        // Force a frame update to ensure panel is rendered
        Canvas.ForceUpdateCanvases();
        
        // Double-check visibility after canvas update
        if (leaderboardPanel != null)
        {
            panelIsVisible = leaderboardPanel.activeSelf && leaderboardPanel.activeInHierarchy;
            Debug.Log($"[LeaderboardPanel] After canvas update - Panel visible: {panelIsVisible}, activeSelf: {leaderboardPanel.activeSelf}, activeInHierarchy: {leaderboardPanel.activeInHierarchy}");
        }
        
        // Only hide the button AFTER we've verified the panel is actually visible
        if (panelIsVisible)
        {
            if (leaderboardButton != null)
            {
                Debug.Log("[LeaderboardPanel] Panel is visible - hiding leaderboard button (direct reference)");
                leaderboardButton.gameObject.SetActive(false);
            }
            else
            {
                // Try to find and hide button from MainMenu or LobbyManager
                MainMenu mainMenu = FindObjectOfType<MainMenu>();
                if (mainMenu != null && mainMenu.leaderboardButton != null)
                {
                    Debug.Log("[LeaderboardPanel] Panel is visible - hiding leaderboard button from MainMenu");
                    mainMenu.leaderboardButton.gameObject.SetActive(false);
                }
                else
                {
                    LobbyManager lobbyManager = FindObjectOfType<LobbyManager>();
                    if (lobbyManager != null && lobbyManager.LeaderboardButton != null)
                    {
                        Debug.Log("[LeaderboardPanel] Panel is visible - hiding leaderboard button from LobbyManager");
                        lobbyManager.LeaderboardButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        Debug.LogWarning("[LeaderboardPanel] Could not find leaderboard button to hide");
                    }
                }
            }
        }
        else
        {
            Debug.LogError("[LeaderboardPanel] Panel is NOT visible! Not hiding button. Check panel setup in Inspector.");
            Debug.LogError("[LeaderboardPanel] Panel activeSelf: " + leaderboardPanel.activeSelf + ", activeInHierarchy: " + leaderboardPanel.activeInHierarchy);
            if (rectTransform != null)
            {
                Debug.LogError($"[LeaderboardPanel] Panel rect: {rectTransform.rect}");
            }
        }
    }
    
    /// <summary>
    /// Close the leaderboard panel
    /// </summary>
    public void CloseLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
        
        // Show the leaderboard button again when closed
        if (leaderboardButton != null)
        {
            leaderboardButton.gameObject.SetActive(true);
        }
        else
        {
            // Try to find MainMenu and call its ShowLeaderboardButton method
            MainMenu mainMenu = FindObjectOfType<MainMenu>();
            if (mainMenu != null)
            {
                mainMenu.ShowLeaderboardButton();
            }
            else
            {
                // Try to find LobbyManager and show its leaderboard button
                LobbyManager lobbyManager = FindObjectOfType<LobbyManager>();
                if (lobbyManager != null && lobbyManager.LeaderboardButton != null)
                {
                    lobbyManager.LeaderboardButton.gameObject.SetActive(true);
                }
            }
        }
    }
    
    /// <summary>
    /// Refresh the leaderboard display
    /// Shows top 10 best single-run scores from all players
    /// </summary>
    public void RefreshLeaderboard()
    {
        if (leaderboardContainer == null) return;
        
        // Clear existing entries
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }
        
        if (titleText != null)
        {
            titleText.text = "Multiplayer Leaderboard\n(Top 10 Best Single-Run Scores)";
        }
        
        // Get leaderboard from manager (shows top 10 best scores)
        if (leaderboardManager != null)
        {
            DisplayTopScoresLeaderboard();
        }
        else
        {
            // Leaderboard manager not found
            GameObject messageObj = new GameObject("NoManagerMessage");
            messageObj.transform.SetParent(leaderboardContainer);
            TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
            messageText.text = "Leaderboard data not available.";
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.fontSize = 24;
        }
    }
    
    /// <summary>
    /// Display top 10 best single-run scores from all players
    /// </summary>
    private void DisplayTopScoresLeaderboard()
    {
        if (leaderboardManager == null || leaderboardContainer == null) return;
        
        // Get top 10 best scores (from Supabase)
        var leaderboard = leaderboardManager.GetLeaderboardData();
        
        if (leaderboard.Count == 0)
        {
            // No data yet
            GameObject messageObj = new GameObject("NoDataMessage");
            messageObj.transform.SetParent(leaderboardContainer);
            TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
            messageText.text = "No leaderboard entries yet.\nPlay multiplayer and beat levels to appear on the leaderboard!";
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.fontSize = 20;
            return;
        }
        
        // Already sorted by GetLeaderboardData(), just display
        int rank = 1;
        foreach (var entry in leaderboard)
        {
            if (leaderboardEntryPrefab != null)
            {
                GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
                
                // Try to find text components
                TextMeshProUGUI[] texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
                
                if (texts.Length > rankTextIndex && texts[rankTextIndex] != null)
                {
                    texts[rankTextIndex].text = $"#{rank}";
                }
                
                if (texts.Length > nameTextIndex && texts[nameTextIndex] != null)
                {
                    texts[nameTextIndex].text = entry.Key;
                }
                
                if (texts.Length > scoreTextIndex && texts[scoreTextIndex] != null)
                {
                    texts[scoreTextIndex].text = $"{entry.Value} levels";
                }
            }
            else
            {
                // Fallback: create simple text entry
                GameObject entryObj = new GameObject($"LeaderboardEntry_{rank}");
                entryObj.transform.SetParent(leaderboardContainer);
                
                RectTransform rect = entryObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(400, 50);
                
                TextMeshProUGUI text = entryObj.AddComponent<TextMeshProUGUI>();
                text.text = $"#{rank}. {entry.Key}: {entry.Value} levels";
                text.fontSize = 20;
                text.alignment = TextAlignmentOptions.Left;
            }
            
            rank++;
        }
    }
    
    /// <summary>
    /// Toggle leaderboard panel visibility
    /// </summary>
    public void ToggleLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            if (leaderboardPanel.activeSelf)
            {
                CloseLeaderboard();
            }
            else
            {
                OpenLeaderboard();
            }
        }
    }
}

