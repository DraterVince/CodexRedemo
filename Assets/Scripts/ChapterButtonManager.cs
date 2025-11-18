using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the locking/unlocking of chapter buttons based on player level progression.
/// Each button unlocks after completing 10 levels (button 1 = level 10, button 2 = level 20, etc.)
/// </summary>
public class ChapterButtonManager : MonoBehaviour
{
    [Header("Chapter Buttons (in order)")]
    [Tooltip("Assign 6 buttons in order. Button 1 unlocks at level 10, button 2 at level 20, etc.")]
    public Button[] chapterButtons = new Button[6];
    
    [Header("Visual Settings")]
    [Tooltip("Color multiplier for locked buttons (0-1, lower = darker)")]
    [Range(0f, 1f)]
    public float lockedColorMultiplier = 0.6f;
    
    [Header("Lock Overlay (Optional)")]
    [Tooltip("Optional: GameObjects to show/hide as lock icons for each button")]
    public GameObject[] lockOverlays = new GameObject[6];
    
    [Header("Level Display (Optional)")]
    [Tooltip("Optional: Text elements to display required level on each button")]
    public TextMeshProUGUI[] requiredLevelTexts = new TextMeshProUGUI[6];
    
    private int currentPlayerLevel = 0;
    
    private void Start()
    {
        UpdateButtonStates();
    }
    
    private void OnEnable()
    {
        UpdateButtonStates();
    }
    
    /// <summary>
    /// Update all button states based on current player progress
    /// </summary>
    public void UpdateButtonStates()
    {
        // Get current level from PlayerDataManager first, fallback to PlayerPrefs
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
        {
            currentPlayerLevel = PlayerDataManager.Instance.GetCurrentPlayerData().levels_unlocked;
        }
        else
        {
            currentPlayerLevel = PlayerPrefs.GetInt("levelAt", 6); // Default to 6 (Level 1 unlocked)
        }
        
        Debug.Log($"[ChapterButtonManager] Current player level: {currentPlayerLevel}");
        
        // Update each button (1-6 correspond to levels 10, 20, 30, 40, 50, 60)
        for (int i = 0; i < chapterButtons.Length; i++)
        {
            if (chapterButtons[i] == null) continue;
            
            int requiredLevel = (i + 1) * 10; // Button 0 = level 10, Button 1 = level 20, etc.
            
            // Always unlock buttons 0 and 1 (first two buttons for CardsPanel1 and CardsPanel2)
            bool isUnlocked;
            if (i == 0 || i == 1)
            {
                isUnlocked = true; // Always unlocked
            }
            else
            {
                isUnlocked = currentPlayerLevel >= requiredLevel;
            }
            
            UpdateButtonVisual(i, isUnlocked, requiredLevel);
        }
    }
    
    /// <summary>
    /// Update visual state of a specific button
    /// </summary>
    private void UpdateButtonVisual(int buttonIndex, bool isUnlocked, int requiredLevel)
    {
        Button button = chapterButtons[buttonIndex];
        
        // Set button interactability
        button.interactable = isUnlocked;
        
        // Use Unity's ColorBlock system to handle disabled state
        ColorBlock colors = button.colors;
        colors.disabledColor = colors.normalColor * lockedColorMultiplier;
        button.colors = colors;
        
        // Update lock overlay visibility
        if (buttonIndex < lockOverlays.Length && lockOverlays[buttonIndex] != null)
        {
            lockOverlays[buttonIndex].SetActive(!isUnlocked);
        }
        
        // Update required level text
        if (buttonIndex < requiredLevelTexts.Length && requiredLevelTexts[buttonIndex] != null)
        {
            if (isUnlocked)
            {
                requiredLevelTexts[buttonIndex].text = $"Chapter {buttonIndex + 1}";
            }
            else
            {
                requiredLevelTexts[buttonIndex].text = $"Locked\nLevel {requiredLevel} Required";
            }
        }
        
        Debug.Log($"[ChapterButtonManager] Button {buttonIndex + 1} - Required Level: {requiredLevel}, Unlocked: {isUnlocked}");
    }
    
    /// <summary>
    /// Call this when player levels up to refresh button states
    /// </summary>
    public void RefreshButtons()
    {
        UpdateButtonStates();
    }
    
    /// <summary>
    /// Check if a specific chapter is unlocked
    /// </summary>
    public bool IsChapterUnlocked(int chapterIndex)
    {
        // Always unlock chapters 0 and 1 (first two buttons)
        if (chapterIndex == 0 || chapterIndex == 1)
        {
            return true;
        }
        
        int requiredLevel = (chapterIndex + 1) * 10;
        return currentPlayerLevel >= requiredLevel;
    }
}
