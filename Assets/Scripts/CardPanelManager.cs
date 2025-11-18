using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Manages card panels and allows setting card values programmatically
/// Supports multiple instances that coordinate selection across all managers
/// </summary>
public class CardPanelManager : MonoBehaviour
{
    // Static registry to track all CardPanelManager instances in the scene
    // This allows cross-manager deselection when cards are clicked
    private static List<CardPanelManager> allManagers = new List<CardPanelManager>();
    private static CardHoverDisplay globallySelectedCard = null;

    [System.Serializable]
    public class CardPanel
    {
        [Header("Panel Info")]
        public string panelName;
        public Transform cardListContainer; // Container for card list (Grid Layout Group)
        
        [Header("Card Prefab")]
        public GameObject cardPrefab; // Prefab for individual cards
        
        [Header("Card Data")]
        public List<CardData> cards = new List<CardData>();

        [HideInInspector]
        public List<CardHoverDisplay> instantiatedCards = new List<CardHoverDisplay>();
    }

    [Header("Card Panels")]
    public List<CardPanel> cardPanels = new List<CardPanel>();

    [Header("Detail Panel")]
    public CardSelectedDetailPanel selectedDetailPanel;

    [Header("Settings")]
    public bool autoSetupOnStart = true;
    public bool clearExistingCards = true;

    private CardHoverDisplay currentlySelectedCard;

    private void Awake()
    {
        // Register this manager instance with the static registry
        if (!allManagers.Contains(this))
        {
            allManagers.Add(this);
        }
    }

    private void OnDestroy()
    {
        // Unregister this manager when destroyed
        if (allManagers.Contains(this))
        {
            allManagers.Remove(this);
            Debug.Log($"[CardPanelManager] Unregistered manager '{gameObject.name}' (Remaining managers: {allManagers.Count})");
        }
        
        // If this manager had the globally selected card, clear it
        if (globallySelectedCard != null && currentlySelectedCard == globallySelectedCard)
        {
            globallySelectedCard = null;
        }
    }

    private void Start()
    {
        // IMPORTANT: Find detail panel BEFORE creating cards
        // This ensures cards can get their detail panel reference immediately
        if (selectedDetailPanel == null)
        {
            selectedDetailPanel = FindObjectOfType<CardSelectedDetailPanel>();
            if (selectedDetailPanel != null)
            {
                Debug.Log($"[CardPanelManager] Found detail panel '{selectedDetailPanel.gameObject.name}' automatically for manager '{gameObject.name}'");
            }
            else
            {
                Debug.LogWarning($"[CardPanelManager] No detail panel found for manager '{gameObject.name}'! " +
                    "Assign a CardSelectedDetailPanel to the 'Selected Detail Panel' field in the inspector. " +
                    "Cards will not be able to show details without this.");
            }
        }
        else
        {
            Debug.Log($"[CardPanelManager] Manager '{gameObject.name}' has detail panel '{selectedDetailPanel.gameObject.name}' assigned");
        }
        
        if (cardPanels.Count == 0)
        {
            Debug.LogWarning("[CardPanelManager] No card panels configured! Add a Card Panel entry in the inspector.");
        }
        
        if (autoSetupOnStart)
        {
            SetupAllPanels();
        }
        else
        {
            Debug.LogWarning("[CardPanelManager] Auto-setup is disabled! Cards will not be created automatically. Enable 'Auto Setup On Start' or call SetupAllPanels() manually.");
        }
    }

    /// <summary>
    /// Setup all card panels
    /// </summary>
    public void SetupAllPanels()
    {
        foreach (CardPanel panel in cardPanels)
        {
            SetupPanel(panel);
        }
    }

    /// <summary>
    /// Setup a specific panel
    /// </summary>
    public void SetupPanel(CardPanel panel)
    {
        if (panel == null || panel.cardListContainer == null)
        {
            Debug.LogWarning($"[CardPanelManager] Panel or container is null for panel: {panel?.panelName}");
            return;
        }

        // Clear existing cards if requested
        if (clearExistingCards)
        {
            ClearPanel(panel);
        }

        // Setup grid layout if not present
        SetupGridLayout(panel.cardListContainer);

        // Instantiate cards
        if (panel.cardPrefab != null)
        {
            if (panel.cards.Count == 0)
            {
                Debug.LogWarning($"[CardPanelManager] Panel '{panel.panelName}' has no CardData assigned! Add CardData ScriptableObjects to the Cards list in the inspector.");
            }
            
            foreach (CardData cardData in panel.cards)
            {
                if (cardData != null)
                {
                    CreateCard(panel, cardData);
                }
                else
                {
                    Debug.LogWarning($"[CardPanelManager] Null CardData found in cards list for panel: {panel.panelName}");
                }
            }
        }
        else
        {
            Debug.LogError($"[CardPanelManager] Card prefab is null for panel: {panel.panelName}. Assign a card prefab in the inspector.");
        }
    }

    /// <summary>
    /// Setup grid layout on container
    /// </summary>
    private void SetupGridLayout(Transform container)
    {
        GridLayoutGroup gridLayout = container.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = container.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(200, 280);
            gridLayout.spacing = new Vector2(10, 10);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;
        }

        // Ensure content size fitter for scrolling
        ContentSizeFitter sizeFitter = container.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = container.gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    /// <summary>
    /// Create a card in the panel
    /// </summary>
    private void CreateCard(CardPanel panel, CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogError($"[CardPanelManager] Cannot create card: CardData is null!");
            return;
        }

        if (panel.cardPrefab == null)
        {
            Debug.LogError($"[CardPanelManager] Cannot create card: Card prefab is null for panel: {panel.panelName}");
            return;
        }

        GameObject cardObj = Instantiate(panel.cardPrefab, panel.cardListContainer);
        CardHoverDisplay cardDisplay = cardObj.GetComponent<CardHoverDisplay>();

        if (cardDisplay == null)
        {
            Debug.LogWarning($"[CardPanelManager] Card prefab '{panel.cardPrefab.name}' doesn't have CardHoverDisplay component. Adding it automatically.");
            cardDisplay = cardObj.AddComponent<CardHoverDisplay>();
            // Auto-setup UI references if possible
            SetupCardUIReferences(cardObj, cardDisplay);
        }

        if (cardDisplay != null)
        {
            cardDisplay.SetupCard(cardData);
            panel.instantiatedCards.Add(cardDisplay);
            
            // Explicitly set the panel manager reference to ensure all cards use the same manager
            // This is critical for cross-panel deselection to work
            if (cardDisplay.PanelManager == null)
            {
                cardDisplay.PanelManager = this;
            }
            else if (cardDisplay.PanelManager != this)
            {
                Debug.LogWarning($"[CardPanelManager] Card '{cardObj.name}' in panel '{panel.panelName}' already has a different CardPanelManager reference! " +
                    "This may cause deselection issues across panels. Reassigning to this manager.");
                cardDisplay.PanelManager = this;
            }
            
            // CRITICAL: Explicitly set the detail panel reference from this manager
            // This ensures each card uses its manager's own detail panel
            if (selectedDetailPanel != null)
            {
                cardDisplay.SetDetailPanel(selectedDetailPanel);
                
                // Verify the detail panel has its GameObject assigned
                if (selectedDetailPanel.detailPanel == null)
                {
                    Debug.LogError($"[CardPanelManager] Manager '{gameObject.name}' has a CardSelectedDetailPanel component assigned, " +
                        $"but the component's 'Detail Panel' GameObject field is not set! " +
                        $"Component: '{selectedDetailPanel.gameObject.name}'. " +
                        $"Cards in panel '{panel.panelName}' will not be able to show details. " +
                        $"Assign the detail panel GameObject to the CardSelectedDetailPanel component's 'Detail Panel' field in the inspector.");
                }
                else
                {
                    Debug.Log($"[CardPanelManager] Card '{cardObj.name}' in panel '{panel.panelName}' is using detail panel '{selectedDetailPanel.detailPanel.name}' " +
                        $"(Component: '{selectedDetailPanel.gameObject.name}')");
                }
            }
            else
            {
                Debug.LogWarning($"[CardPanelManager] Manager '{gameObject.name}' has no detail panel assigned! " +
                    "Cards in panel '{panel.panelName}' will not be able to show details. Assign a CardSelectedDetailPanel to the 'Selected Detail Panel' field.");
            }
        }
        else
        {
            Debug.LogError($"[CardPanelManager] Failed to get or add CardHoverDisplay component to card GameObject: {cardObj.name}");
        }
    }

    /// <summary>
    /// Setup UI references for card display
    /// Only sets up name text and background (description/example are in detail panel)
    /// </summary>
    private void SetupCardUIReferences(GameObject cardObj, CardHoverDisplay cardDisplay)
    {
        // Try to find UI elements by name or type
        Image[] images = cardObj.GetComponentsInChildren<Image>();
        TextMeshProUGUI[] texts = cardObj.GetComponentsInChildren<TextMeshProUGUI>();

        // Find background (usually the first Image that's not a child of another Image)
        foreach (Image img in images)
        {
            if (img.transform.parent == cardObj.transform && cardDisplay.cardBackground == null)
            {
                cardDisplay.cardBackground = img;
                break;
            }
        }

        // Find card name text (only text component needed on the card itself)
        // Try to find by name first
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.name.ToLower().Contains("name") && cardDisplay.cardNameText == null)
            {
                cardDisplay.cardNameText = text;
                break;
            }
        }

        // If not found by name, use the first text component
        if (cardDisplay.cardNameText == null && texts.Length > 0)
        {
            cardDisplay.cardNameText = texts[0];
        }

        // Note: Description and Example text components are NOT needed on the card itself
        // They are only used in the CardSelectedDetailPanel
    }

    /// <summary>
    /// Clear all cards in a panel
    /// </summary>
    public void ClearPanel(CardPanel panel)
    {
        if (panel == null)
        {
            return;
        }

        foreach (CardHoverDisplay card in panel.instantiatedCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }

        panel.instantiatedCards.Clear();

        // Also clear direct children
        if (panel.cardListContainer != null)
        {
            for (int i = panel.cardListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(panel.cardListContainer.GetChild(i).gameObject);
            }
        }
    }

    /// <summary>
    /// Add a card to a panel programmatically
    /// </summary>
    public void AddCardToPanel(string panelName, CardData cardData)
    {
        CardPanel panel = cardPanels.Find(p => p.panelName == panelName);
        if (panel != null)
        {
            panel.cards.Add(cardData);
            CreateCard(panel, cardData);
        }
        else
        {
            Debug.LogWarning($"[CardPanelManager] Panel not found: {panelName}");
        }
    }

    /// <summary>
    /// Set card data for a specific card in a panel
    /// </summary>
    public void SetCardData(string panelName, int cardIndex, string name, string description, string example, Sprite backgroundSprite = null, Color? backgroundColor = null)
    {
        CardPanel panel = cardPanels.Find(p => p.panelName == panelName);
        if (panel != null && cardIndex >= 0 && cardIndex < panel.instantiatedCards.Count)
        {
            CardHoverDisplay cardDisplay = panel.instantiatedCards[cardIndex];
            if (cardDisplay != null)
            {
                cardDisplay.SetCardData(name, description, example, backgroundSprite, backgroundColor);
            }
        }
        else
        {
            Debug.LogWarning($"[CardPanelManager] Panel or card index not found: {panelName}, index: {cardIndex}");
        }
    }

    /// <summary>
    /// Get panel by name
    /// </summary>
    public CardPanel GetPanel(string panelName)
    {
        return cardPanels.Find(p => p.panelName == panelName);
    }

    /// <summary>
    /// Get all cards in a panel
    /// </summary>
    public List<CardHoverDisplay> GetPanelCards(string panelName)
    {
        CardPanel panel = GetPanel(panelName);
        return panel != null ? panel.instantiatedCards : new List<CardHoverDisplay>();
    }

    /// <summary>
    /// Deselect all cards in all panels managed by THIS manager instance
    /// </summary>
    public void DeselectAllCards()
    {
        // Store reference to currently selected card before clearing
        CardHoverDisplay previousSelected = currentlySelectedCard;
        
        // Clear the reference first to avoid confusion
        currentlySelectedCard = null;
        
        // Deselect all cards in all panels managed by this instance
        int totalCards = 0;
        int deselectedCards = 0;
        
        foreach (CardPanel panel in cardPanels)
        {
            if (panel == null)
            {
                continue;
            }
            
            foreach (CardHoverDisplay card in panel.instantiatedCards)
            {
                totalCards++;
                if (card != null)
                {
                    // Force deselection and visual update
                    card.SetSelected(false);
                    deselectedCards++;
                }
            }
        }
        
        // Debug log to verify deselection is working
        if (totalCards > 0)
        {
            Debug.Log($"[CardPanelManager] Deselected {deselectedCards} out of {totalCards} cards across {cardPanels.Count} panel(s) in manager '{gameObject.name}'");
        }
        
        // Force canvas update to ensure visual changes are applied immediately
        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// Static method to deselect all cards across ALL CardPanelManager instances
    /// This is called when a card is clicked to ensure cross-manager deselection
    /// </summary>
    public static void DeselectAllCardsGlobally()
    {
        int totalManagers = allManagers.Count;
        int totalCards = 0;
        int deselectedCards = 0;
        
        // Clear the globally selected card reference
        globallySelectedCard = null;
        
        // Deselect cards in all registered managers
        foreach (CardPanelManager manager in allManagers)
        {
            if (manager == null)
            {
                continue;
            }
            
            // Clear the manager's local selected card reference
            manager.currentlySelectedCard = null;
            
            // Deselect all cards in all panels managed by this manager
            foreach (CardPanel panel in manager.cardPanels)
            {
                if (panel == null)
                {
                    continue;
                }
                
                foreach (CardHoverDisplay card in panel.instantiatedCards)
                {
                    totalCards++;
                    if (card != null)
                    {
                        // Force deselection and visual update
                        card.SetSelected(false);
                        deselectedCards++;
                    }
                }
            }
        }
        
        // Debug log to verify global deselection is working
        if (totalCards > 0)
        {
            Debug.Log($"[CardPanelManager] Globally deselected {deselectedCards} out of {totalCards} cards across {totalManagers} manager(s)");
        }
        
        // Force canvas update to ensure visual changes are applied immediately
        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// Get currently selected card
    /// </summary>
    public CardHoverDisplay GetSelectedCard()
    {
        return currentlySelectedCard;
    }

    /// <summary>
    /// Set currently selected card (called internally by cards)
    /// </summary>
    public void SetSelectedCard(CardHoverDisplay card)
    {
        // Store reference to currently selected card in this manager
        currentlySelectedCard = card;
        
        // Also update the global selected card reference
        globallySelectedCard = card;
    }

    /// <summary>
    /// Get the globally selected card (across all managers)
    /// </summary>
    public static CardHoverDisplay GetGloballySelectedCard()
    {
        return globallySelectedCard;
    }

    /// <summary>
    /// Clear selected card and hide detail panel
    /// </summary>
    public void ClearSelection()
    {
        DeselectAllCards();
        if (selectedDetailPanel != null)
        {
            selectedDetailPanel.HideCardDetails();
        }
    }
}
