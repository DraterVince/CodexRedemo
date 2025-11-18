using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelection : MonoBehaviour
{
    public Button[] lvlButtons;
    public Button playButton;
    
    [Header("Username Display")]
    public TextMeshProUGUI usernameText;
    
    [Header("Character Selection")]
    [Tooltip("Enable character selection before starting level")]
    public bool enableCharacterSelection = true;
    [Tooltip("Reference to Character Selection Manager")]
    public CharacterSelectionManager characterSelectionManager;
    
    [Header("Fade In Animation")]
    [Tooltip("Main panel to fade in (if null, will try to find Canvas or create fade overlay)")]
    public GameObject mainPanel;
    [Tooltip("Fade in duration")]
    public float fadeInDuration = 0.8f;
    [Tooltip("Delay before fade in starts")]
    public float fadeInDelay = 0.1f;

    private int selectedLevel = -1;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        // Setup fade in
        SetupFadeIn();
        
        // Auto-find CharacterSelectionManager if not assigned
        if (characterSelectionManager == null)
        {
            characterSelectionManager = FindObjectOfType<CharacterSelectionManager>();
        }
   
        // Display username
        if (usernameText != null)
        {
            string username = PlayerPrefs.GetString("username", "Player");
            usernameText.text = "Welcome, " + username + "!";
        }
        
        // Tutorial level is at index 5, Level 1 is at index 6
        int levelAt = PlayerPrefs.GetInt("levelAt", 6);

        for (int i = 0; i < lvlButtons.Length; i++)
        {
            // Level button 0 = scene index 6 (Level 1)
            if (i + 6 > levelAt)
                lvlButtons[i].interactable = false;
            lvlButtons[i].onClick.AddListener(() => SelectLevel(System.Array.IndexOf(lvlButtons, UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>())));
        }
        playButton.onClick.AddListener(PlaySelectedLevel);

        playButton.interactable = false;
    }
    
    private void SetupFadeIn()
    {
        // Find or create canvas group for fade effect
        if (mainPanel != null)
        {
            canvasGroup = mainPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = mainPanel.AddComponent<CanvasGroup>();
            }
        }
        else
        {
            // Try to find Canvas in scene
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                canvasGroup = canvas.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
                }
            }
        }
        
        // Start fade in animation
        if (canvasGroup != null)
        {
            StartCoroutine(FadeInCoroutine());
        }
    }
    
    private IEnumerator FadeInCoroutine()
    {
        // Set initial alpha to 0
        canvasGroup.alpha = 0f;
        
        // Wait for delay
        if (fadeInDelay > 0f)
        {
            yield return new WaitForSeconds(fadeInDelay);
        }
        
        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        
        // Ensure fully visible
        canvasGroup.alpha = 1f;
    }
    
    void SelectLevel(int levelAt)
    {
        selectedLevel = levelAt;

        foreach (var btn in lvlButtons)
            btn.GetComponent<Image>().color = Color.white;
            lvlButtons[levelAt].GetComponent<Image>().color = Color.green;
            playButton.interactable = true;
    }
    
    void PlaySelectedLevel()
    {
        if (selectedLevel >= 0)
        {
            // Level button 0 = scene index 6 (Level 1)
            int sceneIndex = selectedLevel + 6;
       
            // Use character selection if enabled
            if (enableCharacterSelection && characterSelectionManager != null)
            {
                characterSelectionManager.PrepareToStartLevel(sceneIndex);
            }
            else
            {
                // Direct load without character selection
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
            }
        }
    }
}
