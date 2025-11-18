using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Handles individual card display with click/select functionality
/// </summary>
public class CardHoverDisplay : MonoBehaviour, IPointerClickHandler
{
    [Header("Card UI Elements")]
    public Image cardBackground;
    public TextMeshProUGUI cardNameText;
    // Note: Description and Example are only shown in the detail panel, not on the card itself

    [Header("Card Data")]
    [Tooltip("Assign CardData ScriptableObject here. If assigned, it will be used automatically in Start().")]
    public CardData cardDataAsset; // Public field for Inspector assignment

    [Header("Selection Settings")]
    public Color selectedColor = new Color(1f, 1f, 0.5f, 1f); // Highlight color when selected
    public Color normalColor = Color.white;
    public Image selectionIndicator; // Optional: visual indicator for selected card

    private CardData cardData;
    private CardSelectedDetailPanel detailPanel;
    private CardPanelManager panelManager;
    private bool isSelected = false;
    private Button button;
    
    // Public property to allow CardPanelManager to set the manager reference
    public CardPanelManager PanelManager
    {
        get { return panelManager; }
        set { panelManager = value; }
    }
    
    /// <summary>
    /// Set the detail panel reference explicitly (called by CardPanelManager)
    /// This ensures each card uses its manager's own detail panel
    /// </summary>
    public void SetDetailPanel(CardSelectedDetailPanel panel)
    {
        detailPanel = panel;
        if (panel != null)
        {
            Debug.Log($"[CardHoverDisplay] Detail panel set for card '{gameObject.name}': '{panel.gameObject.name}'");
        }
    }

    private void Awake()
    {
        // Try to get button component
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnCardClicked);
            Debug.Log($"[CardHoverDisplay] Button component found on {gameObject.name}. Click detection via Button.");
        }
        else
        {
            // No Button component, need Image with Raycast Target for IPointerClickHandler
            if (cardBackground == null)
            {
                // Try to find Image component
                Image image = GetComponent<Image>();
                if (image == null)
                {
                    image = GetComponentInChildren<Image>();
                }
                cardBackground = image;
            }
            
            if (cardBackground != null)
            {
                if (!cardBackground.raycastTarget)
                {
                    Debug.LogWarning($"[CardHoverDisplay] Card background Image on {gameObject.name} does not have Raycast Target enabled. Enabling it for click detection.");
                    cardBackground.raycastTarget = true; // Auto-enable if not set
                }
                Debug.Log($"[CardHoverDisplay] Using Image component '{cardBackground.name}' with Raycast Target for click detection on {gameObject.name}.");
            }
            else
            {
                Debug.LogError($"[CardHoverDisplay] ⚠️ No Button or Image component found on {gameObject.name}! " +
                    $"Click detection will not work. Add either:\n" +
                    $"1. Button component (recommended)\n" +
                    $"2. Image component with 'Raycast Target' enabled");
            }
        }
        
        // Set normal color
        if (cardBackground != null)
        {
            normalColor = cardBackground.color;
        }

        // Check for EventSystem
        if (EventSystem.current == null)
        {
            Debug.LogError($"[CardHoverDisplay] ⚠️ No EventSystem found in scene! Click detection will not work. Create one: GameObject → UI → Event System");
        }
    }

    private void Start()
    {
        // Find components in Start (after all objects are initialized)
        FindDetailPanel();
        FindPanelManager();
        
        // If cardDataAsset is assigned in Inspector, use it automatically
        if (cardData == null && cardDataAsset != null)
        {
            Debug.Log($"[CardHoverDisplay] Using CardData asset '{cardDataAsset.cardName}' assigned in Inspector for card '{gameObject.name}'");
            SetupCard(cardDataAsset);
        }
        
        // If we found a panel manager, make sure we're registered with it
        // This is important for manually placed cards that might not be in the instantiatedCards list
        if (panelManager != null)
        {
            // Check if this card is already in any panel's instantiatedCards list
            bool isRegistered = false;
            foreach (var panel in panelManager.cardPanels)
            {
                if (panel.instantiatedCards.Contains(this))
                {
                    isRegistered = true;
                    break;
                }
            }
            
            // If not registered, try to add it to the first panel (for manually placed cards)
            // Note: This is a fallback - ideally all cards should be created via CardPanelManager
            if (!isRegistered)
            {
                Debug.LogWarning($"[CardHoverDisplay] Card '{gameObject.name}' is not registered with CardPanelManager. " +
                    "This may cause deselection issues. Make sure cards are created via CardPanelManager or manually added to a panel's instantiatedCards list.");
            }
        }
        
        // Check if card data is missing
        if (cardData == null)
        {
            Debug.LogWarning($"[CardHoverDisplay] Card '{gameObject.name}' has no CardData assigned. " +
                "If using CardPanelManager, make sure CardData ScriptableObjects are in the Cards list. " +
                "If manually placed, assign CardData to 'Card Data Asset' field in Inspector.");
        }
    }

    /// <summary>
    /// Find the detail panel in the scene
    /// Priority: 1. Manager's detail panel, 2. Find in scene
    /// </summary>
    private void FindDetailPanel()
    {
        if (detailPanel == null)
        {
            // FIRST: Try to get detail panel from the manager (most reliable)
            if (panelManager != null && panelManager.selectedDetailPanel != null)
            {
                detailPanel = panelManager.selectedDetailPanel;
                Debug.Log($"[CardHoverDisplay] Found detail panel from manager '{panelManager.gameObject.name}' for card '{gameObject.name}'");
                return;
            }
            
            // SECOND: Try to find in scene (fallback for manually placed cards)
            detailPanel = FindObjectOfType<CardSelectedDetailPanel>();
            
            // If not found, try to find in all objects (including inactive)
            if (detailPanel == null)
            {
                CardSelectedDetailPanel[] allPanels = Resources.FindObjectsOfTypeAll<CardSelectedDetailPanel>();
                foreach (CardSelectedDetailPanel panel in allPanels)
                {
                    // Check if it's in a loaded scene (not a prefab)
                    if (panel.gameObject.scene.isLoaded)
                    {
                        detailPanel = panel;
                        Debug.Log($"[CardHoverDisplay] Found detail panel '{panel.gameObject.name}' in scene for card '{gameObject.name}'");
                        break;
                    }
                }
            }
            
            if (detailPanel == null)
            {
                Debug.LogWarning($"[CardHoverDisplay] CardSelectedDetailPanel not found for card '{gameObject.name}'. " +
                    "Make sure:\n" +
                    "1. CardPanelManager has a 'Selected Detail Panel' assigned, OR\n" +
                    "2. A CardSelectedDetailPanel component exists in the scene");
            }
        }
    }

    /// <summary>
    /// Find the panel manager in the scene
    /// </summary>
    private void FindPanelManager()
    {
        if (panelManager == null)
        {
            // Try to find CardPanelManager
            CardPanelManager[] managers = FindObjectsOfType<CardPanelManager>();
            if (managers.Length == 0)
            {
                Debug.LogWarning($"[CardHoverDisplay] CardPanelManager not found in scene on {gameObject.name}.");
            }
            else if (managers.Length > 1)
            {
                Debug.LogWarning($"[CardHoverDisplay] Multiple CardPanelManager instances found ({managers.Length})! Using the first one. This may cause deselection issues across panels.");
                panelManager = managers[0];
            }
            else
            {
                panelManager = managers[0];
            }
            
            if (panelManager != null)
            {
                // Get detail panel from manager if available
                if (panelManager.selectedDetailPanel != null && detailPanel == null)
                {
                    detailPanel = panelManager.selectedDetailPanel;
                }
            }
        }
    }

    /// <summary>
    /// Setup card with data
    /// </summary>
    public void SetupCard(CardData data)
    {
        cardData = data;
        
        // Also set the cardDataAsset field so it shows in Inspector (for debugging)
        if (cardDataAsset != data)
        {
            cardDataAsset = data;
        }
        
        UpdateCardDisplay();
    }

    /// <summary>
    /// Update card display with current data
    /// Only shows card name on the card itself
    /// Description and example are shown in the detail panel when selected
    /// </summary>
    private void UpdateCardDisplay()
    {
        if (cardData == null)
        {
            Debug.LogWarning($"[CardHoverDisplay] Card data is null on {gameObject.name}");
            return;
        }

        // Update card name (only thing displayed on the card)
        if (cardNameText != null)
        {
            cardNameText.text = cardData.cardName;
        }

        // Update card background image
        if (cardBackground != null && cardData.cardImage != null)
        {
            cardBackground.sprite = cardData.cardImage;
        }

        // Update background color (use selected color if selected, otherwise use card color or normal color)
        if (cardBackground != null)
        {
            if (isSelected)
            {
                // Highlight selected card
                cardBackground.color = selectedColor;
            }
            else
            {
                // Reset to normal color (use card color if available, otherwise use stored normal color)
                if (cardData != null && cardData.cardColor != Color.clear)
                {
                    cardBackground.color = cardData.cardColor;
                    normalColor = cardData.cardColor;
                }
                else
                {
                    cardBackground.color = normalColor;
                }
            }
        }

        // Update selection indicator
        if (selectionIndicator != null)
        {
            selectionIndicator.gameObject.SetActive(isSelected);
        }

        // Note: Description and Example are NOT displayed on the card itself
        // They are only shown in the detail panel when the card is selected/clicked
    }

    /// <summary>
    /// Set card data programmatically
    /// Note: Description and example are stored but only displayed in the detail panel when selected
    /// </summary>
    public void SetCardData(string name, string description, string example, Sprite backgroundSprite = null, Color? backgroundColor = null)
    {
        // Create a temporary card data or update existing
        if (cardData == null)
        {
            cardData = ScriptableObject.CreateInstance<CardData>();
        }

        cardData.cardName = name;
        cardData.cardDescription = description;
        cardData.cardExample = example;

        if (backgroundSprite != null)
        {
            cardData.cardImage = backgroundSprite;
        }

        if (backgroundColor.HasValue)
        {
            cardData.cardColor = backgroundColor.Value;
            normalColor = backgroundColor.Value;
        }

        UpdateCardDisplay();
    }

    /// <summary>
    /// Handle card click via IPointerClickHandler
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClicked();
    }

    /// <summary>
    /// Handle card click (called from button or pointer click)
    /// </summary>
    private void OnCardClicked()
    {
        if (cardData == null)
        {
            Debug.LogWarning($"[CardHoverDisplay] Card data is null on {gameObject.name}. Cannot show details.");
            return;
        }

        // Try to find detail panel if not found yet
        if (detailPanel == null)
        {
            FindDetailPanel();
            FindPanelManager();
            
            if (detailPanel == null)
            {
                Debug.LogError($"[CardHoverDisplay] CardSelectedDetailPanel not found! Make sure:\n" +
                    "1. A CardSelectedDetailPanel component exists in the scene\n" +
                    "2. The Detail Panel GameObject is assigned in the CardSelectedDetailPanel inspector\n" +
                    "3. The panel is active (not disabled)");
                return;
            }
        }

        // Toggle selection (if already selected, deselect it)
        if (isSelected)
        {
            // Deselect this card
            SetSelected(false);
            if (panelManager != null)
            {
                panelManager.ClearSelection();
            }
        }
        else
        {
            // Deselect all other cards across ALL CardPanelManager instances FIRST
            // This ensures cross-manager deselection works even with multiple managers
            CardPanelManager.DeselectAllCardsGlobally();
            
            // Then set this card as selected in its manager (if it has one)
            if (panelManager != null)
            {
                panelManager.SetSelectedCard(this);
            }
            else
            {
                // If no manager, manually deselect this card's current state
                SetSelected(false);
            }

            // Now select this card (after all others are deselected)
            SetSelected(true);

            // Show card details in the detail panel
            // CRITICAL: Always use the detail panel from THIS card's manager to avoid cross-contamination
            // Don't fall back to FindObjectOfType which might find the wrong panel
            if (panelManager != null && panelManager.selectedDetailPanel != null)
            {
                // Use the manager's assigned detail panel (most reliable)
                detailPanel = panelManager.selectedDetailPanel;
                Debug.Log($"[CardHoverDisplay] Using detail panel from manager '{panelManager.gameObject.name}': '{detailPanel.gameObject.name}'");
            }
            else
            {
                // If manager doesn't have a detail panel, try to find one
                // But log a warning because this shouldn't happen
                Debug.LogWarning($"[CardHoverDisplay] Card '{cardData.cardName}' has no manager or manager has no detail panel assigned! " +
                    $"Manager: {(panelManager != null ? panelManager.gameObject.name : "null")}. " +
                    "Trying to find detail panel in scene (may find wrong panel if multiple exist)...");
                FindDetailPanel();
            }
            
            if (detailPanel != null)
            {
                // Verify the detail panel component and its GameObject
                if (detailPanel.gameObject == null)
                {
                    Debug.LogError($"[CardHoverDisplay] Detail panel GameObject is null! Cannot show card details for '{cardData.cardName}'");
                    return;
                }
                
                // Log which manager and detail panel we're using
                string managerName = panelManager != null ? panelManager.gameObject.name : "null";
                Debug.Log($"[CardHoverDisplay] Showing card details for card '{cardData.cardName}' from manager '{managerName}' " +
                    $"in detail panel '{detailPanel.gameObject.name}' (Component: '{detailPanel.gameObject.name}'). " +
                    $"Panel active: {detailPanel.gameObject.activeSelf}, Panel active in hierarchy: {detailPanel.gameObject.activeInHierarchy}");
                
                detailPanel.ShowCardDetails(cardData);
                
                // Log additional info about the panel state
                System.Reflection.FieldInfo detailPanelField = typeof(CardSelectedDetailPanel).GetField("detailPanel", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (detailPanelField != null)
                {
                    GameObject panelObj = detailPanelField.GetValue(detailPanel) as GameObject;
                    if (panelObj != null)
                    {
                        Debug.Log($"[CardHoverDisplay] Panel GameObject '{panelObj.name}' state after ShowCardDetails: " +
                            $"active={panelObj.activeSelf}, activeInHierarchy={panelObj.activeInHierarchy}, " +
                            $"parent={panelObj.transform.parent?.name ?? "null"}");
                    }
                    else
                    {
                        Debug.LogError($"[CardHoverDisplay] Panel GameObject is NULL in CardSelectedDetailPanel '{detailPanel.gameObject.name}'! " +
                            "The 'Detail Panel' GameObject field is not assigned in the inspector.");
                    }
                }
            }
            else
            {
                Debug.LogError($"[CardHoverDisplay] Detail panel is null! Cannot show card details for '{cardData.cardName}'. " +
                    $"Manager: {(panelManager != null ? panelManager.gameObject.name : "null")}, " +
                    $"Manager's detail panel: {(panelManager != null && panelManager.selectedDetailPanel != null ? panelManager.selectedDetailPanel.gameObject.name : "null")}");
            }
        }
    }

    /// <summary>
    /// Set card selection state
    /// </summary>
    public void SetSelected(bool selected)
    {
        // Only update if state is actually changing
        if (isSelected != selected)
        {
            isSelected = selected;
            UpdateCardDisplay();
            
            // Force immediate visual update
            if (cardBackground != null)
            {
                // Ensure the color update is applied immediately
                Canvas.ForceUpdateCanvases();
            }
        }
    }

    /// <summary>
    /// Check if card is selected
    /// </summary>
    public bool IsSelected()
    {
        return isSelected;
    }

    /// <summary>
    /// Get the current card data
    /// </summary>
    public CardData GetCardData()
    {
        return cardData;
    }
}
