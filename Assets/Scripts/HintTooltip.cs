using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Standalone tooltip component that can be used with HintButton or independently.
/// Handles fade in/out animations and auto-dismiss functionality.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class HintTooltip : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Duration of fade in animation (seconds)")]
    [SerializeField] private float fadeInDuration = 0.3f;
    
    [Tooltip("Duration of fade out animation (seconds)")]
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    [Tooltip("How long the tooltip should be visible before auto-dismissing (seconds)")]
    [Range(2f, 10f)]
    [SerializeField] private float displayDuration = 4f;
    
    [Header("Auto Dismiss")]
    [Tooltip("If true, tooltip will automatically dismiss after displayDuration")]
    [SerializeField] private bool autoDismiss = true;
    
    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    private Coroutine dismissCoroutine;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Start invisible
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
    
    private void OnEnable()
    {
        // Fade in when enabled
        Show();
    }
    
    /// <summary>
    /// Show the tooltip with fade in animation
    /// </summary>
    public void Show()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeIn());
        
        // Start auto-dismiss if enabled
        if (autoDismiss)
        {
            if (dismissCoroutine != null)
            {
                StopCoroutine(dismissCoroutine);
            }
            dismissCoroutine = StartCoroutine(AutoDismiss());
        }
    }
    
    /// <summary>
    /// Hide the tooltip with fade out animation
    /// </summary>
    public void Hide()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        if (dismissCoroutine != null)
        {
            StopCoroutine(dismissCoroutine);
            dismissCoroutine = null;
        }
        
        fadeCoroutine = StartCoroutine(FadeOut());
    }
    
    /// <summary>
    /// Fade in animation coroutine
    /// </summary>
    private IEnumerator FadeIn()
    {
        canvasGroup.blocksRaycasts = true;
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        fadeCoroutine = null;
    }
    
    /// <summary>
    /// Fade out animation coroutine
    /// </summary>
    private IEnumerator FadeOut()
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        fadeCoroutine = null;
        
        // Deactivate after fade out
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Auto-dismiss coroutine
    /// </summary>
    private IEnumerator AutoDismiss()
    {
        yield return new WaitForSeconds(displayDuration);
        Hide();
    }
    
    /// <summary>
    /// Set the display duration
    /// </summary>
    public void SetDisplayDuration(float duration)
    {
        displayDuration = Mathf.Clamp(duration, 2f, 10f);
    }
    
    /// <summary>
    /// Set whether to auto-dismiss
    /// </summary>
    public void SetAutoDismiss(bool auto)
    {
        autoDismiss = auto;
    }
    
    /// <summary>
    /// Manually dismiss the tooltip (can be called from button click, etc.)
    /// </summary>
    public void Dismiss()
    {
        Hide();
    }
}

