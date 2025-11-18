using System.Collections.Generic;
using System.Linq; // Added for LINQ Select
using System.Threading.Tasks; // Added for async/await
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterSwitcher characterSwitcher;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private PlayerDataManager playerDataManager;

    [Header("UI Elements")]
    [SerializeField] private Image characterDisplayImage;
    [SerializeField] private Transform characterDisplayContainer;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button useCharacterButton;
    [SerializeField] private Button unlockButton;

    [Header("Lock UI")]
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private Image lockIcon;
    [SerializeField] private TextMeshProUGUI unlockCostText;
    [SerializeField] private TextMeshProUGUI lockedMessageText;

    [Header("Currency Display")]
    [Tooltip("Currency icon sprite (e.g., coin, gem icon)")]
    [SerializeField] private Sprite currencyIconSprite;
    [Tooltip("Currency icon image next to unlock cost text")]
    [SerializeField] private Image unlockCostCurrencyIcon;
    [Tooltip("Currency icon image on unlock button")]
    [SerializeField] private Image unlockButtonCurrencyIcon;

    [Header("Character Info")]
    [SerializeField] private TextMeshProUGUI characterDescriptionText;
    [SerializeField] private GameObject[] characterStatsUI;

    [Header("Visual Settings")]
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Sprite defaultLockSprite;
    [SerializeField] private float characterScale = 1f;

    [Header("Animation")]
    [SerializeField] private bool enableTransitionAnimation = true;
    [SerializeField] private float transitionSpeed = 5f;

    [Header("Audio")]
    [SerializeField] private AudioClip navigationSound;
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip lockedSound;

    private int currentDisplayIndex = 0;
    private GameObject currentCharacterInstance;
    private AudioSource audioSource;
    private bool isTransitioning = false;
    
    // Canvas groups for smooth transitions of character elements only
    private CanvasGroup characterImageCanvasGroup;
    private CanvasGroup characterNameCanvasGroup;
    private CanvasGroup characterDescCanvasGroup;

    private void Awake()
    {
        if (characterSwitcher == null)
            characterSwitcher = FindObjectOfType<CharacterSwitcher>();

        if (moneyManager == null)
          moneyManager = FindObjectOfType<MoneyManager>();

        if (playerDataManager == null)
         playerDataManager = PlayerDataManager.Instance;

        audioSource = GetComponent<AudioSource>();
    if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Register audio source with SettingsManager for volume control
        if (audioSource != null && SettingsManager.Instance != null)
        {
            SettingsManager.RegisterSFXSource(audioSource);
        }
 
        // Setup canvas groups for character display elements only
      SetupCanvasGroups();
    }
    
    private void OnEnable()
    {
        // Use a coroutine to ensure CharacterSwitcher is ready
        StartCoroutine(InitializeWithDelay());
    }
    
    /// <summary>
    /// Wait for CharacterSwitcher to be ready before initializing UI
    /// </summary>
    private System.Collections.IEnumerator InitializeWithDelay()
    {
        Debug.Log("[CharacterSelectionUI] Starting initialization...");
        
        // Wait one frame to ensure all Awake/Start methods have completed
        yield return null;
        
        // Try to find CharacterSwitcher multiple times with increasing delays
        int maxAttempts = 20; // Increased from 10
        int attempt = 0;
        
        while (attempt < maxAttempts)
        {
     attempt++;
            
      // Re-find references when panel is enabled
  if (characterSwitcher == null)
            {
    characterSwitcher = FindObjectOfType<CharacterSwitcher>();
Debug.Log($"[CharacterSelectionUI] Attempt {attempt}: CharacterSwitcher found = {characterSwitcher != null}");
            }
  
   if (moneyManager == null)
     {
      moneyManager = FindObjectOfType<MoneyManager>();
            }
            
            // Check if CharacterSwitcher is ready
        if (characterSwitcher != null && 
         characterSwitcher.characters != null && 
  characterSwitcher.characters.Count > 0)
            {
Debug.Log($"[CharacterSelectionUI] CharacterSwitcher ready with {characterSwitcher.characters.Count} characters!");
     
 // Load unlock states and update display
  LoadCharacterUnlockStates();
       
    // Force a clean state before displaying
         StopAllCoroutines();
      isTransitioning = false;
    EnsureCanvasGroupsVisible();
      
       // Update the display
                UpdateCharacterDisplay();
       
  yield break; // Success, exit coroutine
      }
          
   // Wait before retrying
            yield return new WaitForSeconds(0.1f);
 }
        
        // If we got here, we failed to find CharacterSwitcher
 Debug.LogError("[CharacterSelectionUI] Failed to find CharacterSwitcher after " + maxAttempts + " attempts!");
     Debug.LogError("[CharacterSelectionUI] Make sure you have a GameObject with CharacterSwitcher component in the scene!");
        Debug.LogError("[CharacterSelectionUI] And make sure it has characters added to the list!");
    }

    private void SetupCanvasGroups()
    {
        // Add CanvasGroup to character image if it doesn't have one
  if (characterDisplayImage != null)
        {
 characterImageCanvasGroup = characterDisplayImage.GetComponent<CanvasGroup>();
       if (characterImageCanvasGroup == null)
    {
   characterImageCanvasGroup = characterDisplayImage.gameObject.AddComponent<CanvasGroup>();
            }
   // Ensure it starts visible
  characterImageCanvasGroup.alpha = 1f;
    }
        
        // Add CanvasGroup to character name text if it doesn't have one
   if (characterNameText != null)
        {
      characterNameCanvasGroup = characterNameText.GetComponent<CanvasGroup>();
     if (characterNameCanvasGroup == null)
      {
  characterNameCanvasGroup = characterNameText.gameObject.AddComponent<CanvasGroup>();
 }
     // Ensure it starts visible
       characterNameCanvasGroup.alpha = 1f;
}
    
        // Add CanvasGroup to character description text if it doesn't have one
   if (characterDescriptionText != null)
 {
    characterDescCanvasGroup = characterDescriptionText.GetComponent<CanvasGroup>();
    if (characterDescCanvasGroup == null)
      {
   characterDescCanvasGroup = characterDescriptionText.gameObject.AddComponent<CanvasGroup>();
   }
 // Ensure it starts visible
       characterDescCanvasGroup.alpha = 1f;
        }
    }

    private void Start()
    {
    Debug.Log("[CharacterSelectionUI] Start() called");
   
     // Setup button listeners
      if (previousButton != null)
            previousButton.onClick.AddListener(OnPreviousCharacter);

        if (nextButton != null)
nextButton.onClick.AddListener(OnNextCharacter);

   if (useCharacterButton != null)
            useCharacterButton.onClick.AddListener(OnUseCharacter);

      if (unlockButton != null)
     unlockButton.onClick.AddListener(OnUnlockCharacter);

        // Only initialize if the panel is active
 // Otherwise, OnEnable will handle it
    if (gameObject.activeInHierarchy)
     {
Debug.Log("[CharacterSelectionUI] Panel is active, initializing in Start()");
  
     if (characterSwitcher != null && characterSwitcher.autoSave)
     {
   currentDisplayIndex = PlayerPrefs.GetInt(characterSwitcher.saveKey, 0);
  }

    LoadCharacterUnlockStates();
  UpdateCharacterDisplay();
 }
   else
        {
    Debug.Log("[CharacterSelectionUI] Panel is inactive, skipping Start() initialization (OnEnable will handle it)");
    }
    }

    private void OnDestroy()
    {
    if (previousButton != null)
        previousButton.onClick.RemoveListener(OnPreviousCharacter);

        if (nextButton != null)
    nextButton.onClick.RemoveListener(OnNextCharacter);

        if (useCharacterButton != null)
            useCharacterButton.onClick.RemoveListener(OnUseCharacter);

  if (unlockButton != null)
   unlockButton.onClick.RemoveListener(OnUnlockCharacter);
    }

    private void LoadCharacterUnlockStates()
    {
  if (characterSwitcher == null)
      {
        Debug.LogWarning("[CharacterSelectionUI] Cannot load unlock states - CharacterSwitcher is null!");
   return;
     }
   
        if (characterSwitcher.characters == null || characterSwitcher.characters.Count == 0)
    {
       Debug.LogWarning("[CharacterSelectionUI] Cannot load unlock states - no characters in CharacterSwitcher!");
    return;
   }

     // First, try to load from Supabase (PlayerDataManager)
        if (playerDataManager != null && playerDataManager.GetCurrentPlayerData() != null)
   {
            var playerData = playerDataManager.GetCurrentPlayerData();

   if (!string.IsNullOrEmpty(playerData.unlocked_cosmetics))
  {
     try
     {
   string json = playerData.unlocked_cosmetics;
  json = json.Replace("[", "").Replace("]", "").Replace("\"", "");
   
    if (string.IsNullOrEmpty(json))
         {
          Debug.Log("[CharacterSelectionUI] Unlocked cosmetics is empty, using default unlock states");
         characterSwitcher.LoadUnlockStates();
       return;
    }
     
            string[] unlockedNames = json.Split(',').Select(s => s.Trim()).ToArray();

   // Update all characters based on Supabase data
  foreach (var characterData in characterSwitcher.characters)
  {
     bool isUnlocked = System.Array.Exists(unlockedNames, name =>
          name.Equals(characterData.characterName, System.StringComparison.OrdinalIgnoreCase));

    if (isUnlocked && !characterData.isUnlocked)
       {
    characterData.isUnlocked = true;
     Debug.Log($"[CharacterSelectionUI] Unlocked {characterData.characterName} from Supabase");
   }
    }

   // Save the updated unlock states to PlayerPrefs (CharacterSwitcher saves when loading, so no need to call SaveUnlockStates)
        // Just save directly to PlayerPrefs
      foreach (var characterData in characterSwitcher.characters)
    {
      if (characterData.isUnlocked)
            {
                PlayerPrefs.SetInt($"Character_{characterData.characterName}_Unlocked", 1);
   }
        }
        PlayerPrefs.Save();
      
     Debug.Log($"[CharacterSelectionUI] ? Loaded {unlockedNames.Length} unlocked characters from Supabase");
        Debug.Log($"[CharacterSelectionUI] Unlocked characters: {string.Join(", ", unlockedNames)}");
    return;
}
          catch (System.Exception e)
     {
    Debug.LogWarning($"[CharacterSelectionUI] Failed to parse unlocked cosmetics from Supabase: {e.Message}");
   }
   }
     else
          {
 Debug.Log("[CharacterSelectionUI] No unlocked cosmetics in Supabase player data");
     }
     }
        else
     {
  Debug.Log("[CharacterSelectionUI] PlayerDataManager not available, loading from PlayerPrefs");
   }

     // Fallback: Load from PlayerPrefs
     Debug.Log("[CharacterSelectionUI] Loading character unlock states from PlayerPrefs");
      characterSwitcher.LoadUnlockStates();
    }

    private void UpdateCharacterDisplay()
    {
        if (characterSwitcher == null)
     {
            Debug.LogError("[CharacterSelectionUI] CharacterSwitcher is null! Make sure there's a CharacterSwitcher in the scene.");
   return;
        }
        
        if (characterSwitcher.characters == null)
  {
       Debug.LogError("[CharacterSelectionUI] CharacterSwitcher.characters list is null!");
    return;
  }
        
        if (characterSwitcher.characters.Count == 0)
        {
 Debug.LogWarning("[CharacterSelectionUI] No characters available to display. Add characters to CharacterSwitcher!");
       return;
 }

        currentDisplayIndex = Mathf.Clamp(currentDisplayIndex, 0, characterSwitcher.characters.Count - 1);

        var character = characterSwitcher.characters[currentDisplayIndex];
        
  if (character == null)
      {
  Debug.LogError($"[CharacterSelectionUI] Character at index {currentDisplayIndex} is null!");
    return;
     }

        if (characterNameText != null)
        {
            characterNameText.text = character.characterName;
     characterNameText.color = character.isUnlocked ? unlockedColor : lockedColor;
   }

        if (characterDescriptionText != null)
        {
         // Always show the character's custom description (whether locked or unlocked)
       string description = string.IsNullOrEmpty(character.characterDescription) 
          ? "A mysterious warrior awaiting to be unlocked..." 
  : character.characterDescription;
            
 characterDescriptionText.text = description;
     
       // Change color based on lock state
    characterDescriptionText.color = character.isUnlocked ? unlockedColor : lockedColor;
   }

        UpdateCharacterVisual(character);
      UpdateLockState(character);
        UpdateButtonStates(character);
        UpdateNavigationButtons();
   
        Debug.Log($"[CharacterSelectionUI] Displaying character: {character.characterName} (Index: {currentDisplayIndex})");
    }

    private void UpdateCharacterVisual(CharacterSwitcher.CharacterData character)
    {
        // Clean up previous character instance
      if (currentCharacterInstance != null)
        {
  Destroy(currentCharacterInstance);
            currentCharacterInstance = null;
     }

        // Update character sprite
        if (characterDisplayImage != null)
        {
            if (character.characterSprite != null)
        {
          characterDisplayImage.sprite = character.characterSprite;
  characterDisplayImage.color = character.isUnlocked ? unlockedColor : lockedColor;
         characterDisplayImage.gameObject.SetActive(true);
   
   // Ensure the canvas group alpha is set correctly
       if (characterImageCanvasGroup != null)
                {
        characterImageCanvasGroup.alpha = 1f;
        }
                
      Debug.Log($"[CharacterSelectionUI] Updated sprite to: {character.characterName}");
            }
  else
   {
        characterDisplayImage.gameObject.SetActive(false);
 }
        }

        // Instantiate character prefab if available
        if (character.characterPrefab != null && characterDisplayContainer != null)
        {
       currentCharacterInstance = Instantiate(character.characterPrefab, characterDisplayContainer);
     currentCharacterInstance.transform.localPosition = Vector3.zero;
            currentCharacterInstance.transform.localRotation = Quaternion.identity;
     currentCharacterInstance.transform.localScale = Vector3.one * characterScale;

            if (!character.isUnlocked)
        {
                Renderer[] renderers = currentCharacterInstance.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
     {
foreach (var material in renderer.materials)
 {
     material.color = lockedColor;
   }
                }
}
            
     Debug.Log($"[CharacterSelectionUI] Instantiated prefab for: {character.characterName}");
        }
        
      // Force canvas groups to be visible
     EnsureCanvasGroupsVisible();
  }
    
    /// <summary>
    /// Ensure all canvas groups are fully visible (alpha = 1)
    /// </summary>
    private void EnsureCanvasGroupsVisible()
    {
      if (characterImageCanvasGroup != null)
            characterImageCanvasGroup.alpha = 1f;
            
        if (characterNameCanvasGroup != null)
        characterNameCanvasGroup.alpha = 1f;
            
        if (characterDescCanvasGroup != null)
            characterDescCanvasGroup.alpha = 1f;
    }

    private void UpdateLockState(CharacterSwitcher.CharacterData character)
    {
        bool isLocked = !character.isUnlocked;

        if (lockOverlay != null)
        {
        lockOverlay.SetActive(isLocked);
        }

        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(isLocked);
          if (defaultLockSprite != null)
            {
     lockIcon.sprite = defaultLockSprite;
     }
        }

        if (unlockCostText != null)
    {
        if (isLocked && character.unlockCost > 0)
      {
     // Remove $ sign - just show the number
  unlockCostText.text = character.unlockCost.ToString();
  unlockCostText.gameObject.SetActive(true);
        
 // Show currency icon if available
                if (unlockCostCurrencyIcon != null)
       {
   unlockCostCurrencyIcon.gameObject.SetActive(true);
         if (currencyIconSprite != null)
       {
            unlockCostCurrencyIcon.sprite = currencyIconSprite;
              }
 }
 }
            else
   {
 unlockCostText.gameObject.SetActive(false);
  
      // Hide currency icon when not needed
       if (unlockCostCurrencyIcon != null)
           {
         unlockCostCurrencyIcon.gameObject.SetActive(false);
                }
          }
        }

      if (lockedMessageText != null)
        {
   if (isLocked)
        {
                lockedMessageText.text = "LOCKED";
             lockedMessageText.gameObject.SetActive(true);
      }
       else
  {
                lockedMessageText.gameObject.SetActive(false);
            }
  }
    }

    private void UpdateButtonStates(CharacterSwitcher.CharacterData character)
    {
        bool isLocked = !character.isUnlocked;

        if (useCharacterButton != null)
        {
            // Make button invisible if character is locked
            useCharacterButton.gameObject.SetActive(!isLocked);
            useCharacterButton.interactable = !isLocked;

        Text buttonText = useCharacterButton.GetComponentInChildren<Text>();
       if (buttonText != null)
            {
        bool isCurrentlyUsed = characterSwitcher.currentCharacterIndex == currentDisplayIndex;
   buttonText.text = isCurrentlyUsed ? "SELECTED" : "SELECT";
         }
    }

        if (unlockButton != null)
        {
  bool canAfford = true;
 if (moneyManager != null && character.unlockCost > 0)
          {
 canAfford = moneyManager.moneyCount >= character.unlockCost;
            }

        unlockButton.interactable = isLocked && canAfford;
  unlockButton.gameObject.SetActive(isLocked);

 Text buttonText = unlockButton.GetComponentInChildren<Text>();
 TextMeshProUGUI buttonTextTMP = unlockButton.GetComponentInChildren<TextMeshProUGUI>();
      
      if (buttonText != null)
  {
       // Remove $ sign from button text
  buttonText.text = canAfford ? "UNLOCK (" + character.unlockCost + ")" : "NOT ENOUGH MONEY";
  }
  else if (buttonTextTMP != null)
        {
  // Support TextMeshPro as well
    buttonTextTMP.text = canAfford ? "UNLOCK (" + character.unlockCost + ")" : "NOT ENOUGH MONEY";
    }
     
 // Show/hide currency icon on button
   if (unlockButtonCurrencyIcon != null)
   {
     if (isLocked && canAfford && currencyIconSprite != null)
       {
     unlockButtonCurrencyIcon.gameObject.SetActive(true);
        unlockButtonCurrencyIcon.sprite = currencyIconSprite;
   }
       else
    {
 unlockButtonCurrencyIcon.gameObject.SetActive(false);
         }
 }
    }
    }

    private void UpdateNavigationButtons()
    {
        if (characterSwitcher == null) return;

     if (previousButton != null)
        {
        previousButton.interactable = characterSwitcher.characters.Count > 1;
}

        if (nextButton != null)
        {
            nextButton.interactable = characterSwitcher.characters.Count > 1;
        }
    }

    private void OnPreviousCharacter()
    {
        if (isTransitioning || characterSwitcher == null) return;

        PlaySound(navigationSound);

     currentDisplayIndex--;
        if (currentDisplayIndex < 0)
        {
            currentDisplayIndex = characterSwitcher.characters.Count - 1;
        }

  if (enableTransitionAnimation)
        {
 StartCoroutine(TransitionToCharacter());
        }
  else
        {
   UpdateCharacterDisplay();
        }
}

    private void OnNextCharacter()
    {
     if (isTransitioning || characterSwitcher == null) return;

        PlaySound(navigationSound);

        currentDisplayIndex++;
        if (currentDisplayIndex >= characterSwitcher.characters.Count)
  {
       currentDisplayIndex = 0;
        }

     if (enableTransitionAnimation)
        {
            StartCoroutine(TransitionToCharacter());
    }
        else
        {
            UpdateCharacterDisplay();
        }
    }

    private void OnUseCharacter()
    {
        if (characterSwitcher == null) return;

      var character = characterSwitcher.characters[currentDisplayIndex];

    if (!character.isUnlocked)
        {
       PlaySound(lockedSound);
   Debug.Log("Cannot use locked character!");
          return;
        }

        PlaySound(selectSound);

        characterSwitcher.SwitchToCharacter(currentDisplayIndex);

 UpdateButtonStates(character);

        Debug.Log("Character selected: " + character.characterName);
        
 // Notify CharacterSelectionManager that character is confirmed
        CharacterSelectionManager manager = FindObjectOfType<CharacterSelectionManager>();
 if (manager != null)
        {
            manager.OnCharacterConfirmed();
      }
    }

  private async void OnUnlockCharacter()
  {
        if (characterSwitcher == null) return;

      var character = characterSwitcher.characters[currentDisplayIndex];

        if (character.isUnlocked)
      {
      Debug.Log("Character already unlocked!");
        return;
     }

   int currentMoney = moneyManager != null ? moneyManager.moneyCount : 0;

    if (currentMoney < character.unlockCost)
      {
       PlaySound(lockedSound);
            Debug.Log("Not enough money! Need " + character.unlockCost + ", have " + currentMoney);
     return;
 }

        // Deduct money
        if (moneyManager != null && character.unlockCost > 0)
        {
  moneyManager.SpendMoney(character.unlockCost);
        }

        // Unlock character locally
        characterSwitcher.UnlockCharacter(currentDisplayIndex);

    // Save to Supabase and update PlayerDataManager
        await SaveUnlockedCharacterToSupabase(character.characterName);

   // Also sync the money change to Supabase
        if (playerDataManager != null && moneyManager != null)
        {
   await playerDataManager.UpdateMoney(moneyManager.moneyCount);
        }

    PlaySound(unlockSound);
  UpdateCharacterDisplay();

        Debug.Log($"[CharacterSelectionUI] Character unlocked and saved: {character.characterName}");
    }

    private async Task SaveUnlockedCharacterToSupabase(string characterName)
    {
        if (playerDataManager == null || playerDataManager.GetCurrentPlayerData() == null)
      {
        Debug.LogWarning("[CharacterSelectionUI] Cannot save to Supabase - PlayerDataManager or PlayerData is null");
   return;
}

    try
        {
 var playerData = playerDataManager.GetCurrentPlayerData();

         // Parse existing unlocked cosmetics
 List<string> unlockedList = new List<string>();
            if (!string.IsNullOrEmpty(playerData.unlocked_cosmetics))
        {
      string json = playerData.unlocked_cosmetics;
           json = json.Replace("[", "").Replace("]", "").Replace("\"", "");
  if (!string.IsNullOrEmpty(json))
    {
       unlockedList.AddRange(json.Split(',').Select(s => s.Trim()));
          }
}

    // Add new character if not already in list
  if (!unlockedList.Contains(characterName))
     {
       unlockedList.Add(characterName);
 Debug.Log($"[CharacterSelectionUI] Adding {characterName} to unlocked list");
         }

        // Update player data
          playerData.unlocked_cosmetics = "[\"" + string.Join("\",\"", unlockedList) + "\"]";
  playerData.updated_at = System.DateTime.UtcNow.ToString("o");

    // Save to Supabase
bool success = await playerDataManager.UpdatePlayerData(playerData);
 
     if (success)
  {
  Debug.Log($"[CharacterSelectionUI] ? Successfully saved {characterName} unlock to Supabase");
          Debug.Log($"[CharacterSelectionUI] Total unlocked cosmetics: {string.Join(", ", unlockedList)}");
 
 // Also save to PlayerPrefs for persistence
   PlayerPrefs.SetString("unlockedCosmetics", playerData.unlocked_cosmetics);
  PlayerPrefs.Save();
  }
   else
       {
    Debug.LogError($"[CharacterSelectionUI] ? Failed to save {characterName} unlock to Supabase");
            }
        }
        catch (System.Exception ex)
   {
     Debug.LogError($"[CharacterSelectionUI] Error saving unlocked character: {ex.Message}");
      }
 }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            float volume = 1f;
            if (SettingsManager.Instance != null)
            {
                volume = SettingsManager.GetEffectiveVolume(false, 1f);
            }
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private System.Collections.IEnumerator TransitionToCharacter()
    {
        isTransitioning = true;

        float elapsed = 0f;
   float duration = 1f / transitionSpeed;

    // Fade out
        while (elapsed < duration / 2f)
    {
            elapsed += Time.deltaTime;
          float alpha = 1f - (elapsed / (duration / 2f));
            
            if (characterImageCanvasGroup != null)
    characterImageCanvasGroup.alpha = alpha;
            if (characterNameCanvasGroup != null)
      characterNameCanvasGroup.alpha = alpha;
          if (characterDescCanvasGroup != null)
                characterDescCanvasGroup.alpha = alpha;
        
      yield return null;
        }
        
        // Update display
     UpdateCharacterDisplay();
     
        // Fade in
        elapsed = 0f;
     while (elapsed < duration / 2f)
  {
            elapsed += Time.deltaTime;
         float alpha = elapsed / (duration / 2f);
            
   if (characterImageCanvasGroup != null)
             characterImageCanvasGroup.alpha = alpha;
  if (characterNameCanvasGroup != null)
     characterNameCanvasGroup.alpha = alpha;
            if (characterDescCanvasGroup != null)
           characterDescCanvasGroup.alpha = alpha;
  
          yield return null;
    }

        EnsureCanvasGroupsVisible();
        isTransitioning = false;
    }

    public void OpenCharacterSelection()
    {
        Debug.Log("[CharacterSelectionUI] OpenCharacterSelection() called");
        gameObject.SetActive(true);
        StopAllCoroutines();
        isTransitioning = false;
        StartCoroutine(InitializeAndDisplay());
    }

private System.Collections.IEnumerator InitializeAndDisplay()
    {
        yield return new WaitForSeconds(0.2f);
        
        if (characterSwitcher == null)
        {
            characterSwitcher = FindObjectOfType<CharacterSwitcher>();
        }
        
        if (moneyManager == null)
        {
  moneyManager = FindObjectOfType<MoneyManager>();
        }
        
     EnsureCanvasGroupsVisible();
        
     if (characterSwitcher != null && characterSwitcher.characters != null && characterSwitcher.characters.Count > 0)
        {
       LoadCharacterUnlockStates();
            UpdateCharacterDisplay();
        }
        else
  {
StartCoroutine(InitializeWithDelay());
        }
    }

    public void RefreshDisplay()
    {
        Debug.Log("[CharacterSelectionUI] RefreshDisplay() called");
        
        StopAllCoroutines();
        isTransitioning = false;
        
        if (characterSwitcher == null)
        {
            characterSwitcher = FindObjectOfType<CharacterSwitcher>();
}
      
        if (moneyManager == null)
        {
            moneyManager = FindObjectOfType<MoneyManager>();
        }
        
  EnsureCanvasGroupsVisible();
        
        if (characterSwitcher != null && characterSwitcher.autoSave)
  {
            currentDisplayIndex = PlayerPrefs.GetInt(characterSwitcher.saveKey, 0);
        }
        
   if (characterSwitcher != null && characterSwitcher.characters != null && characterSwitcher.characters.Count > 0)
        {
    LoadCharacterUnlockStates();
   UpdateCharacterDisplay();
        }
        else
        {
            Debug.LogWarning("[CharacterSelectionUI] Cannot refresh - CharacterSwitcher not ready");
            StartCoroutine(InitializeWithDelay());
        }
    }

    public void CloseCharacterSelection()
    {
     Debug.Log("[CharacterSelectionUI] CloseCharacterSelection() called");
   gameObject.SetActive(false);
    }
}
