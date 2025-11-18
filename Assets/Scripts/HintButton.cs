using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Hint button that shows a tooltip when clicked. The tooltip automatically disappears after a few seconds.
/// Can be linked to an answer GameObject - when the answer is marked as correct, the hint button will hide.
/// Attach this script to a Button GameObject and assign the hint text and tooltip prefab.
/// </summary>
public class HintButton : MonoBehaviour
{
    [Header("Hint Settings")]
    [Tooltip("The hint text to display in the tooltip")]
    [TextArea(3, 5)]
    [SerializeField] private string hintText = "This is a hint!";
    
    [Header("Answer Linking")]
    [Tooltip("Link this hint button to a specific answer GameObject. When the answer is marked as correct (SetActive(true)), this hint button will automatically hide.")]
    [SerializeField] private GameObject linkedAnswer;
    
    [Tooltip("If true, the hint button will check the linked answer's active state and hide when answer is correct")]
    [SerializeField] private bool hideWhenAnswerCorrect = true;
    
    [Tooltip("How often to check if the linked answer is correct (in seconds). Lower values = more responsive but more CPU usage.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float checkInterval = 0.2f;
    
    [Header("Tooltip Settings")]
    [Tooltip("Prefab for the tooltip panel. If null, will create a default tooltip at runtime.")]
    [SerializeField] private GameObject tooltipPrefab;
    
    [Tooltip("Parent canvas to spawn tooltip on. If null, will find Canvas automatically.")]
    [SerializeField] private Canvas parentCanvas;
    
    [Tooltip("How long the tooltip should be visible before auto-dismissing (in seconds)")]
    [Range(2f, 10f)]
    [SerializeField] private float tooltipDuration = 4f;
    
    [Header("Tooltip Position")]
    [Tooltip("Offset from the button position where tooltip should appear")]
    [SerializeField] private Vector2 tooltipOffset = new Vector2(0f, 100f);
    
    [Tooltip("If true, tooltip will appear above the button. If false, appears at offset position.")]
    [SerializeField] private bool appearAboveButton = true;
    
    [Header("Visual Settings")]
    [Tooltip("Background color for the tooltip")]
    [SerializeField] private Color tooltipBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    
    [Tooltip("Text color for the hint text")]
    [SerializeField] private Color tooltipTextColor = Color.yellow;
    
    [Tooltip("Font size for the hint text")]
    [SerializeField] private int fontSize = 24;
    
    [Tooltip("Padding around the text in the tooltip")]
    [SerializeField] private float padding = 15f;
    
    [Header("Panel Integration")]
    [Tooltip("If true, tooltip will dismiss when expected output panel is minimized")]
    [SerializeField] private bool dismissOnPanelMinimize = true;
    
    private Button button;
    private GameObject currentTooltip;
    private Coroutine dismissCoroutine;
    private Coroutine answerCheckCoroutine;
    private bool isAnswerCorrect = false;
    
    // Static reference to the currently active hint button (only one tooltip visible at a time)
    private static HintButton activeHintButton = null;

    private bool HasHintText => !string.IsNullOrWhiteSpace(hintText);
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("[HintButton] No Button component found on " + gameObject.name + "! HintButton requires a Button component.");
            enabled = false;
            return;
        }
        
        // Add click listener
        button.onClick.AddListener(OnHintButtonClicked);
    }
    
    private void OnEnable()
    {
        // Start checking answer status if linked to an answer
        if (hideWhenAnswerCorrect && linkedAnswer != null)
        {
            if (answerCheckCoroutine != null)
            {
                StopCoroutine(answerCheckCoroutine);
            }
            answerCheckCoroutine = StartCoroutine(CheckAnswerStatus());
        }
        
        // Subscribe to panel state changes if dismiss on minimize is enabled
        if (dismissOnPanelMinimize)
        {
            AnimatedPanel.OnPanelStateChanged += OnPanelStateChanged;
        }
    }
    
    private void OnDisable()
    {
        if (answerCheckCoroutine != null)
        {
            StopCoroutine(answerCheckCoroutine);
            answerCheckCoroutine = null;
        }
        
        // Unsubscribe from panel state changes
        if (dismissOnPanelMinimize)
        {
            AnimatedPanel.OnPanelStateChanged -= OnPanelStateChanged;
        }
    }
    
    private void OnDestroy()
    {
        // Clean up
        if (button != null)
        {
            button.onClick.RemoveListener(OnHintButtonClicked);
        }
        
        // Unsubscribe from panel state changes
        if (dismissOnPanelMinimize)
        {
            AnimatedPanel.OnPanelStateChanged -= OnPanelStateChanged;
        }
        
        // Dismiss tooltip and clear active reference
        DismissTooltip();
        
        // Clear active reference if this was destroyed
        if (activeHintButton == this)
        {
            activeHintButton = null;
        }
    }
    
    /// <summary>
    /// Called when the hint button is clicked
    /// </summary>
    private void OnHintButtonClicked()
    {
        if (!HasHintText)
        {
            Debug.LogWarning($"[HintButton] Hint text is empty on '{gameObject.name}'. Tooltip will not be shown.");
            return;
        }
        
        // If this button already has a tooltip showing, dismiss it
        if (currentTooltip != null)
        {
            DismissTooltip();
            return;
        }
        
        // Dismiss any other active tooltip from another hint button
        if (activeHintButton != null && activeHintButton != this)
        {
            activeHintButton.DismissTooltip();
        }
        
        // Show this button's tooltip
        ShowTooltip();
    }
    
    /// <summary>
    /// Shows the tooltip with the hint text
    /// </summary>
    private void ShowTooltip()
    {
        if (!HasHintText)
        {
            return;
        }
        
        // Find or create canvas
        Canvas canvas = parentCanvas;
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[HintButton] No Canvas found in scene! Cannot display tooltip.");
                return;
            }
        }
        
        // Create tooltip
        if (tooltipPrefab != null)
        {
            currentTooltip = Instantiate(tooltipPrefab, canvas.transform);
        }
        else
        {
            currentTooltip = CreateDefaultTooltip(canvas);
        }
        
        // Position tooltip
        PositionTooltip();
        
        // Set text
        TextMeshProUGUI textComponent = currentTooltip.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = hintText;
        }
        
        // Set this as the active hint button
        activeHintButton = this;
        
        // Start auto-dismiss coroutine
        if (dismissCoroutine != null)
        {
            StopCoroutine(dismissCoroutine);
        }
        dismissCoroutine = StartCoroutine(DismissAfterDelay());
    }
    
    /// <summary>
    /// Creates a default tooltip panel if no prefab is assigned
    /// </summary>
    private GameObject CreateDefaultTooltip(Canvas parent)
    {
        // Create panel
        GameObject panel = new GameObject("HintTooltip");
        panel.transform.SetParent(parent.transform, false);
        
        // Add RectTransform
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(300f, 150f);
        
        // Add Image for background
        Image background = panel.AddComponent<Image>();
        background.color = tooltipBackgroundColor;
        
        // Add TextMeshProUGUI for text
        GameObject textObj = new GameObject("HintText");
        textObj.transform.SetParent(panel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(padding, padding);
        textRect.offsetMax = new Vector2(-padding, -padding);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = hintText;
        text.color = tooltipTextColor;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        
        // Make sure it's on top
        panel.transform.SetAsLastSibling();
        
        return panel;
    }
    
    /// <summary>
    /// Positions the tooltip relative to the button
    /// </summary>
    private void PositionTooltip()
    {
        if (currentTooltip == null) return;
        
        RectTransform tooltipRect = currentTooltip.GetComponent<RectTransform>();
        RectTransform buttonRect = GetComponent<RectTransform>();
        
        if (tooltipRect == null || buttonRect == null) return;
        
        // Get button position in canvas space
        Vector2 buttonPosition = buttonRect.position;
        
        // Calculate tooltip position
        Vector2 tooltipPosition;
        if (appearAboveButton)
        {
            // Position above button
            float buttonHeight = buttonRect.rect.height;
            tooltipPosition = buttonPosition + new Vector2(0f, buttonHeight * 0.5f + tooltipRect.rect.height * 0.5f + tooltipOffset.y);
        }
        else
        {
            // Use offset position
            tooltipPosition = buttonPosition + tooltipOffset;
        }
        
        tooltipRect.position = tooltipPosition;
        
        // Keep tooltip within screen bounds
        Canvas canvas = tooltipRect.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                Vector3[] corners = new Vector3[4];
                tooltipRect.GetWorldCorners(corners);
                
                float minX = float.MaxValue, minY = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue;
                
                foreach (Vector3 corner in corners)
                {
                    Vector2 localCorner;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, corner, canvas.worldCamera, out localCorner);
                    minX = Mathf.Min(minX, localCorner.x);
                    minY = Mathf.Min(minY, localCorner.y);
                    maxX = Mathf.Max(maxX, localCorner.x);
                    maxY = Mathf.Max(maxY, localCorner.y);
                }
                
                // Adjust if out of bounds
                if (minX < canvasRect.rect.xMin)
                {
                    tooltipPosition.x += canvasRect.rect.xMin - minX;
                }
                if (maxX > canvasRect.rect.xMax)
                {
                    tooltipPosition.x -= maxX - canvasRect.rect.xMax;
                }
                if (minY < canvasRect.rect.yMin)
                {
                    tooltipPosition.y += canvasRect.rect.yMin - minY;
                }
                if (maxY > canvasRect.rect.yMax)
                {
                    tooltipPosition.y -= maxY - canvasRect.rect.yMax;
                }
                
                tooltipRect.position = tooltipPosition;
            }
        }
    }
    
    /// <summary>
    /// Coroutine to dismiss tooltip after delay
    /// </summary>
    private System.Collections.IEnumerator DismissAfterDelay()
    {
        yield return new WaitForSeconds(tooltipDuration);
        DismissTooltip();
    }
    
    /// <summary>
    /// Dismisses the current tooltip
    /// </summary>
    public void DismissTooltip()
    {
        if (dismissCoroutine != null)
        {
            StopCoroutine(dismissCoroutine);
            dismissCoroutine = null;
        }
        
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
            currentTooltip = null;
        }
        
        // Clear active reference if this was the active button
        if (activeHintButton == this)
        {
            activeHintButton = null;
        }
    }
    
    /// <summary>
    /// Set the hint text programmatically
    /// </summary>
    public void SetHintText(string text)
    {
        hintText = text;
    }
    
    /// <summary>
    /// Set the tooltip duration programmatically
    /// </summary>
    public void SetTooltipDuration(float duration)
    {
        tooltipDuration = Mathf.Clamp(duration, 2f, 10f);
    }
    
    /// <summary>
    /// Link this hint button to an answer GameObject
    /// </summary>
    public void LinkToAnswer(GameObject answerObject)
    {
        linkedAnswer = answerObject;
        isAnswerCorrect = false;
        
        // Restart checking if enabled
        if (hideWhenAnswerCorrect && gameObject.activeInHierarchy)
        {
            if (answerCheckCoroutine != null)
            {
                StopCoroutine(answerCheckCoroutine);
            }
            answerCheckCoroutine = StartCoroutine(CheckAnswerStatus());
        }
    }
    
    /// <summary>
    /// Manually hide the hint button (can be called when answer is correct)
    /// </summary>
    public void HideButton()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Manually show the hint button
    /// </summary>
    public void ShowButton()
    {
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Check if the linked answer is correct
    /// </summary>
    public bool IsAnswerCorrect()
    {
        return isAnswerCorrect;
    }
    
    /// <summary>
    /// Coroutine to check if the linked answer has been marked as correct
    /// </summary>
    private IEnumerator CheckAnswerStatus()
    {
        while (hideWhenAnswerCorrect && linkedAnswer != null && !isAnswerCorrect)
        {
            // Check if the answer GameObject is active (marked as correct)
            if (linkedAnswer.activeSelf)
            {
                isAnswerCorrect = true;
                // Hide the hint button
                gameObject.SetActive(false);
                // Dismiss any open tooltip
                DismissTooltip();
                yield break;
            }
            
            yield return new WaitForSeconds(checkInterval);
        }
    }
    
    /// <summary>
    /// Called when the expected output panel state changes
    /// </summary>
    private void OnPanelStateChanged(bool isExpanded, AnimatedPanel panel)
    {
        // If panel is minimized (not expanded), dismiss the tooltip
        if (!isExpanded && dismissOnPanelMinimize)
        {
            DismissTooltip();
        }
    }
}

