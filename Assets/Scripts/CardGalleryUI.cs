using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardGalleryUI : MonoBehaviour
{
    [Header("Grid Settings")]
    public Transform gridContainer;
    public GameObject cardPrefab;
    public int columns = 3;
    public float spacing = 10f;
    
    [Header("Filters")]
    public TMP_Dropdown levelFilter;
    public Toggle showLockedToggle;
    
    [Header("UI Elements")]
    public TextMeshProUGUI unlockedCountText;
    public Button backButton;
    
    private List<CardDisplayUI> spawnedCards = new List<CardDisplayUI>();
    private int currentFilterLevel = -1; // -1 = all levels
    private bool showLocked = true;

    private void Start()
    {
        SetupFilters();
        GenerateCardGallery();
        UpdateUnlockedCount();
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    /// <summary>
    /// Setup filter UI
    /// </summary>
    private void SetupFilters()
    {
        // Setup level filter dropdown
        if (levelFilter != null)
        {
            levelFilter.ClearOptions();
            
            List<string> options = new List<string> { "All Levels" };
            int maxLevel = PlayerPrefs.GetInt("levelAt", 5);
            
            for (int i = 1; i <= maxLevel; i++)
            {
                options.Add("Level " + i);
            }
            
            levelFilter.AddOptions(options);
            levelFilter.onValueChanged.AddListener(OnLevelFilterChanged);
        }
        
        // Setup show locked toggle
        if (showLockedToggle != null)
        {
            showLockedToggle.isOn = true;
            showLockedToggle.onValueChanged.AddListener(OnShowLockedChanged);
        }
    }

    /// <summary>
    /// Generate the card gallery
    /// </summary>
    private void GenerateCardGallery()
    {
        if (gridContainer == null || cardPrefab == null)
        {
            Debug.LogError("Grid container or card prefab not assigned!");
            return;
        }
        
        if (CardCollectionManager.Instance == null)
        {
            Debug.LogError("CardCollectionManager not found!");
            return;
        }
        
        // Clear existing cards
        ClearGallery();
        
        // Setup Grid Layout Group
        GridLayoutGroup grid = gridContainer.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
        }
        
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.spacing = new Vector2(spacing, spacing);
        grid.cellSize = new Vector2(200, 280); // Adjust as needed
        
        // Get cards based on filters
        List<CardData> cardsToShow = GetFilteredCards();
        
        // Spawn card UI elements
        foreach (CardData cardData in cardsToShow)
        {
            GameObject cardObj = Instantiate(cardPrefab, gridContainer);
            CardDisplayUI cardUI = cardObj.GetComponent<CardDisplayUI>();
            
            if (cardUI != null)
            {
                cardUI.SetupCard(cardData);
                spawnedCards.Add(cardUI);
            }
        }
        
        Debug.Log($"Generated {spawnedCards.Count} cards in gallery");
    }

    /// <summary>
    /// Get filtered cards based on current filter settings
    /// </summary>
    private List<CardData> GetFilteredCards()
    {
        List<CardData> allCards = CardCollectionManager.Instance.allCards;
        List<CardData> filtered = new List<CardData>();
        
        foreach (CardData card in allCards)
        {
            // Filter by level
            if (currentFilterLevel != -1 && card.unlockLevel != currentFilterLevel)
            {
                continue;
            }
            
            // Filter by locked status
            if (!showLocked && !card.isUnlocked)
            {
                continue;
            }
            
            filtered.Add(card);
        }
        
        return filtered;
    }

    /// <summary>
    /// Clear all spawned cards
    /// </summary>
    private void ClearGallery()
    {
        foreach (CardDisplayUI card in spawnedCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        
        spawnedCards.Clear();
    }

    /// <summary>
    /// Update unlocked count display
    /// </summary>
    private void UpdateUnlockedCount()
    {
        if (CardCollectionManager.Instance != null && unlockedCountText != null)
        {
            int unlocked = CardCollectionManager.Instance.GetUnlockedCards().Count;
            int total = CardCollectionManager.Instance.allCards.Count;
            
            unlockedCountText.text = $"Unlocked: {unlocked}/{total}";
        }
    }

    /// <summary>
    /// Refresh the gallery
    /// </summary>
    public void RefreshGallery()
    {
        GenerateCardGallery();
        UpdateUnlockedCount();
    }

    /// <summary>
    /// Handle level filter change
    /// </summary>
    private void OnLevelFilterChanged(int index)
    {
        currentFilterLevel = index == 0 ? -1 : index; // 0 = all, 1+ = level number
        RefreshGallery();
    }

    /// <summary>
    /// Handle show locked toggle
    /// </summary>
    private void OnShowLockedChanged(bool value)
    {
        showLocked = value;
        RefreshGallery();
    }

    /// <summary>
    /// Handle back button
    /// </summary>
    private void OnBackClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
