using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages hint buttons for all answers in a level.
/// Automatically links hint buttons to their corresponding answers and hides them when answers are correct.
/// </summary>
public class AnswerHintManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to OutputManager that contains the answer lists")]
    [SerializeField] private OutputManager outputManager;
    
    [Tooltip("Reference to PlayCardButton (optional - will auto-find if not assigned)")]
    [SerializeField] private PlayCardButton playCardButton;
    
    [Header("Hint Button Setup")]
    [Tooltip("Prefab for hint buttons. Should have a Button component and HintButton script.")]
    [SerializeField] private GameObject hintButtonPrefab;
    
    [Tooltip("Parent transform to spawn hint buttons under. If null, will use this GameObject's transform.")]
    [SerializeField] private Transform hintButtonParent;
    
    [Header("Hint Button Configuration")]
    [Tooltip("Default hint text template. Use {0} for output index, {1} for answer index.")]
    [SerializeField] private string defaultHintTextTemplate = "";
    
    [Tooltip("If true, use the default hint text template whenever no custom hint is provided. If false, buttons without custom hints will have no tooltip.")]
    [SerializeField] private bool useDefaultHintTemplate = false;
    
    [Tooltip("If true, hint buttons without hint text will be hidden. If false, buttons stay visible but won't show a tooltip.")]
    [SerializeField] private bool hideButtonsWithoutHints = false;
    
    [Tooltip("List of custom hint texts per answer. Index corresponds to [outputIndex][answerIndex]")]
    [SerializeField] private List<AnswerHintData> customHints = new List<AnswerHintData>();
    
    [System.Serializable]
    public class AnswerHintData
    {
        [Tooltip("Output index (which question/enemy)")]
        public int outputIndex;
        
        [Tooltip("Answer index (which answer within the question)")]
        public int answerIndex;
        
        [TextArea(2, 4)]
        [Tooltip("Hint text for this specific answer")]
        public string hintText;
    }
    
    [Header("Positioning")]
    [Tooltip("If true, hint buttons will be positioned on top of answer GameObjects")]
    [SerializeField] private bool positionOnAnswers = true;
    
    [Tooltip("Offset from answer position when positioning hint buttons")]
    [SerializeField] private Vector2 positionOffset = Vector2.zero;
    
    [Tooltip("If true, hint buttons will be positioned as children of answer GameObjects")]
    [SerializeField] private bool parentToAnswers = false;
    
    private Dictionary<string, HintButton> hintButtons = new Dictionary<string, HintButton>();
    private Dictionary<string, GameObject> answerObjects = new Dictionary<string, GameObject>();
    
    private void Start()
    {
        // Auto-find references if not assigned
        if (outputManager == null)
        {
            outputManager = FindObjectOfType<OutputManager>();
        }
        
        if (playCardButton == null)
        {
            playCardButton = FindObjectOfType<PlayCardButton>();
            if (playCardButton != null && outputManager == null)
            {
                outputManager = playCardButton.outputManager;
            }
        }
        
        if (outputManager == null)
        {
            Debug.LogError("[AnswerHintManager] OutputManager not found! Cannot set up hint buttons.");
            return;
        }
        
        if (hintButtonParent == null)
        {
            hintButtonParent = transform;
        }
        
        // Set up hint buttons for all answers
        SetupHintButtons();
    }
    
    /// <summary>
    /// Set up hint buttons for all answers in the OutputManager
    /// </summary>
    public void SetupHintButtons()
    {
        if (outputManager == null || outputManager.answerListContainer == null)
        {
            Debug.LogError("[AnswerHintManager] OutputManager or answerListContainer is null!");
            return;
        }
        
        // Clear existing hint buttons
        ClearHintButtons();
        
        // Create hint buttons for each answer
        for (int outputIndex = 0; outputIndex < outputManager.answerListContainer.Count; outputIndex++)
        {
            var answerList = outputManager.answerListContainer[outputIndex];
            
            if (answerList == null || answerList.answers == null) continue;
            
            for (int answerIndex = 0; answerIndex < answerList.answers.Count; answerIndex++)
            {
                GameObject answerObject = answerList.answers[answerIndex];
                
                if (answerObject == null) continue;
                
                // Create hint button for this answer
                CreateHintButtonForAnswer(outputIndex, answerIndex, answerObject);
            }
        }
        
        Debug.Log($"[AnswerHintManager] Set up {hintButtons.Count} hint buttons for answers.");
    }
    
    /// <summary>
    /// Create a hint button for a specific answer
    /// </summary>
    private void CreateHintButtonForAnswer(int outputIndex, int answerIndex, GameObject answerObject)
    {
        // Create hint button
        GameObject hintButtonObj;
        
        if (hintButtonPrefab != null)
        {
            hintButtonObj = Instantiate(hintButtonPrefab, hintButtonParent);
        }
        else
        {
            // Create default hint button
            hintButtonObj = CreateDefaultHintButton();
        }
        
        hintButtonObj.name = $"HintButton_Output{outputIndex}_Answer{answerIndex}";
        
        // Get or add HintButton component
        HintButton hintButton = hintButtonObj.GetComponent<HintButton>();
        if (hintButton == null)
        {
            hintButton = hintButtonObj.AddComponent<HintButton>();
        }
        
        // Set hint text
        string hintText = GetHintText(outputIndex, answerIndex);
        hintButton.SetHintText(hintText);
        bool hasHintText = !string.IsNullOrWhiteSpace(hintText);
        
        Button uiButton = hintButtonObj.GetComponent<Button>();
        if (uiButton != null)
        {
            uiButton.interactable = hasHintText || !hideButtonsWithoutHints;
        }
        
        if (!hasHintText && hideButtonsWithoutHints)
        {
            hintButtonObj.SetActive(false);
        }
        
        // Link to answer
        hintButton.LinkToAnswer(answerObject);
        
        // Position hint button
        if (positionOnAnswers)
        {
            PositionHintButton(hintButtonObj, answerObject);
        }
        
        // Store references
        string key = GetAnswerKey(outputIndex, answerIndex);
        hintButtons[key] = hintButton;
        answerObjects[key] = answerObject;
    }
    
    /// <summary>
    /// Create a default hint button if no prefab is provided
    /// </summary>
    private GameObject CreateDefaultHintButton()
    {
        GameObject buttonObj = new GameObject("HintButton");
        buttonObj.transform.SetParent(hintButtonParent, false);
        
        // Add RectTransform
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(40f, 40f);
        
        // Add Image for button background
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 1f, 0.8f); // Light blue
        
        // Add Button component
        Button button = buttonObj.AddComponent<Button>();
        
        // Add TextMeshProUGUI for "?" icon
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TMPro.TextMeshProUGUI text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = "?";
        text.color = Color.white;
        text.fontSize = 24;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        
        return buttonObj;
    }
    
    /// <summary>
    /// Position hint button relative to answer GameObject
    /// </summary>
    private void PositionHintButton(GameObject hintButton, GameObject answerObject)
    {
        RectTransform hintRect = hintButton.GetComponent<RectTransform>();
        if (hintRect == null) return;
        
        // Try to get RectTransform from answer
        RectTransform answerRect = answerObject.GetComponent<RectTransform>();
        
        if (answerRect != null)
        {
            // Position on top of answer (UI element)
            if (parentToAnswers)
            {
                hintButton.transform.SetParent(answerObject.transform, false);
            }
            
            // Position at answer's position with offset
            hintRect.position = answerRect.position + (Vector3)positionOffset;
            
            // Make sure hint button is on top (higher z-order)
            hintRect.SetAsLastSibling();
        }
        else
        {
            // Answer is a 3D object, try to position in world space
            // This is a simplified approach - you may need to adjust based on your setup
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Vector3 worldPos = answerObject.transform.position;
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(mainCam, worldPos);
                
                Canvas canvas = hintRect.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                    Vector2 localPos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out localPos);
                    hintRect.localPosition = localPos + positionOffset;
                }
            }
        }
    }
    
    /// <summary>
    /// Get hint text for a specific answer
    /// </summary>
    private string GetHintText(int outputIndex, int answerIndex)
    {
        // Check custom hints first
        foreach (var hintData in customHints)
        {
            if (hintData.outputIndex == outputIndex && hintData.answerIndex == answerIndex)
            {
                return hintData.hintText;
            }
        }
        
        // Use default template if enabled and populated
        if (useDefaultHintTemplate && !string.IsNullOrWhiteSpace(defaultHintTextTemplate))
        {
            return string.Format(defaultHintTextTemplate, outputIndex, answerIndex);
        }
        
        // No hint text
        return string.Empty;
    }
    
    /// <summary>
    /// Get unique key for an answer
    /// </summary>
    private string GetAnswerKey(int outputIndex, int answerIndex)
    {
        return $"Output{outputIndex}_Answer{answerIndex}";
    }
    
    /// <summary>
    /// Clear all hint buttons
    /// </summary>
    public void ClearHintButtons()
    {
        foreach (var hintButton in hintButtons.Values)
        {
            if (hintButton != null && hintButton.gameObject != null)
            {
                Destroy(hintButton.gameObject);
            }
        }
        
        hintButtons.Clear();
        answerObjects.Clear();
    }
    
    /// <summary>
    /// Get hint button for a specific answer
    /// </summary>
    public HintButton GetHintButton(int outputIndex, int answerIndex)
    {
        string key = GetAnswerKey(outputIndex, answerIndex);
        hintButtons.TryGetValue(key, out HintButton hintButton);
        return hintButton;
    }
    
    /// <summary>
    /// Manually hide hint button for a specific answer
    /// </summary>
    public void HideHintButton(int outputIndex, int answerIndex)
    {
        HintButton hintButton = GetHintButton(outputIndex, answerIndex);
        if (hintButton != null)
        {
            hintButton.HideButton();
        }
    }
    
    /// <summary>
    /// Manually show hint button for a specific answer
    /// </summary>
    public void ShowHintButton(int outputIndex, int answerIndex)
    {
        HintButton hintButton = GetHintButton(outputIndex, answerIndex);
        if (hintButton != null)
        {
            hintButton.ShowButton();
        }
    }
}

