using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages loading screen animations and transitions
/// </summary>
public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }
    
    [Header("Loading Screen UI")]
    [Tooltip("Main loading screen panel")]
    public GameObject loadingPanel;
    
    [Tooltip("Loading text (optional)")]
    public TextMeshProUGUI loadingText;
    
    [Tooltip("Loading spinner/animation (optional)")]
    public GameObject loadingSpinner;
    
    [Tooltip("Fade overlay for transitions")]
    public Image fadeOverlay;
    
    [Header("Settings")]
    [Tooltip("Duration of loading screen display (minimum)")]
    public float minLoadingDuration = 1.5f;
    
    [Tooltip("Fade in/out duration")]
    public float fadeDuration = 0.5f;
    
    [Tooltip("Use this as a singleton instance")]
    public bool useSingleton = true;
    
    private bool isLoadingActive = false;
    private CanvasGroup canvasGroup;
    private Animator spinnerAnimator;
    private Animation spinnerAnimation;
    
    private void Awake()
    {
        if (useSingleton)
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[LoadingScreenManager] Instance created and set to DontDestroyOnLoad");
            }
            else if (Instance != this)
            {
                Debug.LogWarning("[LoadingScreenManager] Duplicate instance detected, destroying duplicate");
                Destroy(gameObject);
                return;
            }
        }
        
        // Setup canvas group if not exists
        if (fadeOverlay != null)
        {
            canvasGroup = fadeOverlay.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = fadeOverlay.gameObject.AddComponent<CanvasGroup>();
            }
            
            // Ensure fade overlay is black
            fadeOverlay.color = Color.black;
            
            // Ensure it's on top (high sort order)
            Canvas canvas = fadeOverlay.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                // Make sure the canvas is set to Screen Space - Overlay or has proper sorting
                CanvasGroup canvasCanvasGroup = canvas.GetComponent<CanvasGroup>();
                if (canvasCanvasGroup == null)
                {
                    canvasCanvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            // Start hidden
            fadeOverlay.gameObject.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }
        
        // Start loading panel hidden
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
        
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(false);
            
            // Get animation components
            spinnerAnimator = loadingSpinner.GetComponent<Animator>();
            spinnerAnimation = loadingSpinner.GetComponent<Animation>();
        }
    }
    
    private void OnDestroy()
    {
        // Prevent accidental destruction - only destroy if this is not the instance
        if (useSingleton && Instance == this)
        {
            Debug.LogWarning("[LoadingScreenManager] Instance is being destroyed! This should not happen with DontDestroyOnLoad.");
        }
    }
    
    /// <summary>
    /// Show loading screen with animation
    /// </summary>
    public void ShowLoadingScreen(string message = "Loading...")
    {
        // Always allow showing loading screen (reset state if needed)
        isLoadingActive = true;
        
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        
        if (loadingText != null)
        {
            loadingText.text = message;
        }
        
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(true);
            
            // Refresh animation components to ensure they're current
            spinnerAnimator = null;
            spinnerAnimation = null;
            
            // Play spinner animation
            PlaySpinnerAnimation();
        }
        
        // Setup fade overlay to be black and fully opaque
        if (fadeOverlay != null)
        {
            // Ensure fade overlay is black
            fadeOverlay.color = Color.black;
            
            // Ensure canvas group exists
            if (canvasGroup == null)
            {
                canvasGroup = fadeOverlay.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = fadeOverlay.gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            // Set to fully opaque (black screen)
            canvasGroup.alpha = 1f;
            fadeOverlay.gameObject.SetActive(true);
        }
        else if (loadingPanel != null)
        {
            // If no fade overlay, try to fade the panel itself
            CanvasGroup panelGroup = loadingPanel.GetComponent<CanvasGroup>();
            if (panelGroup == null)
            {
                panelGroup = loadingPanel.AddComponent<CanvasGroup>();
            }
            panelGroup.alpha = 1f; // Start fully visible
        }
    }
    
    /// <summary>
    /// Hide loading screen with fade out animation (fades from black screen to reveal scene)
    /// </summary>
    public void HideLoadingScreen()
    {
        if (!isLoadingActive) return;
        if (!gameObject.activeInHierarchy) return;
        
        StartCoroutine(FadeOutAndHide());
    }
    
    /// <summary>
    /// Show loading screen, wait for minimum duration, then load scene
    /// </summary>
    public IEnumerator ShowLoadingAndLoadScene(string sceneName, string message = "Loading...")
    {
        ShowLoadingScreen(message);
        
        // Wait for minimum loading duration
        yield return new WaitForSeconds(minLoadingDuration);
        
        // Load scene asynchronously
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        // Wait until scene is loaded
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        // Wait a bit more for smooth transition
        yield return new WaitForSeconds(0.2f);
        
        // Activate scene
        asyncLoad.allowSceneActivation = true;
        
        // Wait for scene to fully activate
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Hide loading screen after scene is loaded
        yield return new WaitForSeconds(0.1f);
        HideLoadingScreen();
    }
    
    private IEnumerator FadeIn()
    {
        if (fadeOverlay == null || canvasGroup == null) yield break;
        
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeInPanel(CanvasGroup group)
    {
        if (group == null) yield break;
        
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            group.alpha = alpha;
            yield return null;
        }
        
        group.alpha = 1f;
    }
    
    private IEnumerator FadeOutAndHide()
    {
        CanvasGroup groupToFade = null;
        
        // Prioritize fade overlay for black screen fade out
        if (fadeOverlay != null && fadeOverlay.gameObject != null)
        {
            if (canvasGroup == null)
            {
                canvasGroup = fadeOverlay.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = fadeOverlay.gameObject.AddComponent<CanvasGroup>();
                }
            }
            groupToFade = canvasGroup;
            // Ensure it starts at full black
            if (groupToFade != null)
            {
                groupToFade.alpha = 1f;
            }
        }
        else if (loadingPanel != null && loadingPanel.gameObject != null)
        {
            groupToFade = loadingPanel.GetComponent<CanvasGroup>();
        }
        
        // Fade out from black (alpha 1) to transparent (alpha 0)
        if (groupToFade != null)
        {
            float elapsed = 0f;
            float startAlpha = groupToFade.alpha;
            
            while (elapsed < fadeDuration)
            {
                // Check if objects still exist
                if (groupToFade == null || !gameObject.activeInHierarchy)
                {
                    yield break;
                }
                
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
                groupToFade.alpha = alpha;
                yield return null;
            }
            
            if (groupToFade != null)
            {
                groupToFade.alpha = 0f;
            }
        }
        
        // Hide UI elements after fade (only if still valid)
        if (loadingPanel != null && loadingPanel.gameObject != null)
        {
            loadingPanel.SetActive(false);
        }
        
        if (loadingSpinner != null && loadingSpinner.gameObject != null)
        {
            // Stop spinner animation
            StopSpinnerAnimation();
            
            loadingSpinner.SetActive(false);
        }
        
        if (fadeOverlay != null && fadeOverlay.gameObject != null)
        {
            fadeOverlay.gameObject.SetActive(false);
        }
        
        isLoadingActive = false;
    }
    
    /// <summary>
    /// Play the spinner animation
    /// </summary>
    private void PlaySpinnerAnimation()
    {
        if (loadingSpinner == null || !loadingSpinner.activeInHierarchy) return;
        
        // Try Animator first (most common)
        if (spinnerAnimator == null)
        {
            spinnerAnimator = loadingSpinner.GetComponent<Animator>();
        }
        
        if (spinnerAnimator != null)
        {
            spinnerAnimator.enabled = true;
            
            // Try to play "LoadingAnimation" by name first
            if (HasAnimationState(spinnerAnimator, "LoadingAnimation"))
            {
                spinnerAnimator.Play("LoadingAnimation", 0, 0f); // Play from start
            }
            // If there's a "Play" trigger, use it
            else if (spinnerAnimator.parameters.Length > 0)
            {
                foreach (AnimatorControllerParameter param in spinnerAnimator.parameters)
                {
                    if (param.name == "Play" && param.type == AnimatorControllerParameterType.Trigger)
                    {
                        spinnerAnimator.SetTrigger("Play");
                        break;
                    }
                }
            }
            // Otherwise, play the first state (for looping animations in default state)
            else
            {
                spinnerAnimator.Play(0, 0, 0f); // Play first state from start
            }
        }
        
        // Try Animation component as fallback
        if (spinnerAnimation == null)
        {
            spinnerAnimation = loadingSpinner.GetComponent<Animation>();
        }
        
        if (spinnerAnimation != null)
        {
            // Try to play "LoadingAnimation" by name
            if (spinnerAnimation.GetClip("LoadingAnimation") != null)
            {
                spinnerAnimation.Play("LoadingAnimation");
            }
            // Play default clip if available
            else if (spinnerAnimation.clip != null)
            {
                spinnerAnimation.Play(spinnerAnimation.clip.name);
            }
            // Play first available clip
            else
            {
                foreach (AnimationState state in spinnerAnimation)
                {
                    spinnerAnimation.Play(state.name);
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Check if animator has a specific animation state
    /// </summary>
    private bool HasAnimationState(Animator animator, string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;
        
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == stateName)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Stop the spinner animation
    /// </summary>
    private void StopSpinnerAnimation()
    {
        if (spinnerAnimator != null)
        {
            spinnerAnimator.SetTrigger("Stop");
            // Or disable if needed
            // spinnerAnimator.enabled = false;
        }
        
        if (spinnerAnimation != null && spinnerAnimation.isPlaying)
        {
            spinnerAnimation.Stop();
        }
    }
}

