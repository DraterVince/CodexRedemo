using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public CardManager cardManager;

    private void Awake()
    {
        // Force consistent damage layout/color regardless of scene/prefab overrides
        ForceDamageTopRightDefaults();
        EnsureSubtitleText();
        EnsureDamageText();
        // Apply layout once more in case a preassigned text existed
        if (enforceDamageTextLayout)
        {
            ApplyDamageTextLayout();
        }
    }

    private void ForceDamageTopRightDefaults()
    {
        // Ensure damage label sits at top-right and is red
        damageTextAnchorMin = new Vector2(1f, 1f);
        damageTextAnchorMax = new Vector2(1f, 1f);
        damageTextPivot = new Vector2(1f, 1f);
        damageTextOffsetMin = new Vector2(-160f, -72f);
        damageTextOffsetMax = new Vector2(-16f, -16f);
        damageTextAlignment = TextAlignmentOptions.Right;
        damageTextFontSize = 40;
        attackDamageColor = Color.red;
        enforceDamageTextLayout = true;
    }

    public string cardName;
    public Image cardDesign;

    [SerializeField] GameObject Card;

    [Header("Hint Tooltip Settings")]
    [Tooltip("Enable or disable hover hints for this card display.")]
    [SerializeField] private bool enableHoverHints = true;
    [Tooltip("Optional custom tooltip prefab. Leave empty to use the default tooltip style.")]
    [SerializeField] private GameObject hintTooltipPrefab;
    [Tooltip("Canvas to parent the tooltip under. If null, the nearest parent canvas will be used.")]
    [SerializeField] private Canvas hintTooltipCanvas;
    [Tooltip("Offset (in pixels) from the pointer position when showing the tooltip.")]
    [SerializeField] private Vector2 hintTooltipOffset = new Vector2(0f, 90f);
    [Tooltip("If true, the tooltip follows the pointer while hovering.")]
    [SerializeField] private bool followPointer = true;

    private Item assignedItem;

    [Header("Subtitle Display")]
    [SerializeField] private bool showSubtitle = false;
    [SerializeField] private TextMeshProUGUI subtitleText; // optional manual assign
    [SerializeField] private bool autoCreateSubtitleText = true;
    [SerializeField] private Vector2 subtitleAnchorMin = new Vector2(1f, 1f);   // top-right
    [SerializeField] private Vector2 subtitleAnchorMax = new Vector2(1f, 1f);   // top-right
    [SerializeField] private Vector2 subtitlePivot = new Vector2(1f, 1f);       // top-right
    [SerializeField] private Vector2 subtitleOffsetMin = new Vector2(-300f, -96f); // width ~284, height ~64
    [SerializeField] private Vector2 subtitleOffsetMax = new Vector2(-16f, -16f);  // inset 16px from top-right
    [SerializeField] private int subtitleFontSize = 40;
    [SerializeField] private TextAlignmentOptions subtitleAlignment = TextAlignmentOptions.Right;
    [SerializeField] private Color subtitleColor = Color.white;
    [Tooltip("If empty, defaults to assigned item's cardName.")]
    [SerializeField] private string subtitleFallback = string.Empty;

    [Header("Attack Damage Display")]
    [SerializeField] private bool showAttackDamage = true;
    [SerializeField] private TextMeshProUGUI attackDamageText; // optional manual assign
    [SerializeField] private bool autoCreateDamageText = true;
    [SerializeField] private Vector2 damageTextAnchorMin = new Vector2(1f, 1f);   // top-right
    [SerializeField] private Vector2 damageTextAnchorMax = new Vector2(1f, 1f);   // top-right
    [SerializeField] private Vector2 damageTextPivot = new Vector2(1f, 1f);       // top-right
    [SerializeField] private Vector2 damageTextOffsetMin = new Vector2(-160f, -72f); // wider and taller box
    [SerializeField] private Vector2 damageTextOffsetMax = new Vector2(-16f, -16f);  // inset 16px from top-right
    [SerializeField] private int damageTextFontSize = 40;
    [SerializeField] private TextAlignmentOptions damageTextAlignment = TextAlignmentOptions.Right;
    [SerializeField] private bool enforceDamageTextLayout = true; // reapply anchors/offsets even if a manual reference exists
    [Tooltip("Optional: A prefix/suffix for the displayed damage, e.g., 'DMG: ' or 'x'. Leave empty for raw number.")]
    [SerializeField] private string attackDamagePrefix = "DMG: ";
    [SerializeField] private string attackDamageSuffix = "";
    [Tooltip("Color of the damage text.")]
    [SerializeField] private Color attackDamageColor = Color.red; // red for damage

    private GameObject activeHintTooltip;
    private RectTransform activeHintTooltipRect;
    private TextMeshProUGUI activeHintTooltipText;

    private void Update()
    {
        gameObject.name = cardName;
        if (showSubtitle)
        {
            UpdateSubtitleDisplay();
        }
        if (showAttackDamage)
        {
            UpdateAttackDamageDisplay();
        }
    }

    /// <summary>
    /// Assigns the scriptable object backing this card display.
    /// </summary>
    /// <param name="item">Item data instance (can be null).</param>
    public void SetAssignedItem(Item item)
    {
        assignedItem = item;
        UpdateSubtitleDisplay();
        UpdateAttackDamageDisplay();
    }

    /// <summary>
    /// Returns the currently assigned item (may be null).
    /// </summary>
    public Item GetAssignedItem()
    {
        return assignedItem;
    }

    /// <summary>
    /// Returns true if the assigned item has a hint.
    /// </summary>
    public bool HasHint()
    {
        return assignedItem != null && !string.IsNullOrWhiteSpace(assignedItem.hintText);
    }

    /// <summary>
    /// Returns the hint text for the assigned item (empty string if none).
    /// </summary>
    public string GetHintText()
    {
        return assignedItem != null ? assignedItem.hintText : string.Empty;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enableHoverHints || !HasHint())
        {
            return;
        }

        Vector2 pointerPos = eventData != null ? eventData.position : (Vector2)Input.mousePosition;
        ShowHintTooltip(GetHintText(), pointerPos);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!enableHoverHints || !followPointer || activeHintTooltipRect == null)
        {
            return;
        }

        Vector2 pointerPos = eventData != null ? eventData.position : (Vector2)Input.mousePosition;
        UpdateTooltipPosition(pointerPos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideHintTooltip();
    }

    private void OnDisable()
    {
        HideHintTooltip();
    }

    private void OnDestroy()
    {
        HideHintTooltip();
    }

    private void ShowHintTooltip(string hint, Vector2 pointerPosition)
    {
        Canvas canvas = ResolveHintCanvas();
        if (canvas == null)
        {
            Debug.LogWarning($"[CardDisplay] Cannot show hint tooltip because no Canvas was found for card '{gameObject.name}'.");
            return;
        }

        HideHintTooltip();

        if (hintTooltipPrefab != null)
        {
            activeHintTooltip = Instantiate(hintTooltipPrefab, canvas.transform);
        }
        else
        {
            activeHintTooltip = CreateDefaultHintTooltip(canvas);
        }

        if (activeHintTooltip == null)
        {
            return;
        }

        activeHintTooltipRect = activeHintTooltip.GetComponent<RectTransform>();
        if (activeHintTooltipRect == null)
        {
            activeHintTooltipRect = activeHintTooltip.AddComponent<RectTransform>();
        }

        activeHintTooltip.transform.SetAsLastSibling();

        activeHintTooltipText = activeHintTooltip.GetComponentInChildren<TextMeshProUGUI>();
        if (activeHintTooltipText == null)
        {
            Debug.LogWarning($"[CardDisplay] Hint tooltip prefab '{activeHintTooltip.name}' for card '{gameObject.name}' is missing a TextMeshProUGUI component.");
            Destroy(activeHintTooltip);
            activeHintTooltip = null;
            activeHintTooltipRect = null;
            return;
        }

        activeHintTooltipText.text = hint;
        UpdateTooltipPosition(pointerPosition);
    }

    private void UpdateTooltipPosition(Vector2 pointerPosition)
    {
        if (activeHintTooltipRect == null)
        {
            return;
        }

        Canvas canvas = hintTooltipCanvas;
        if (canvas == null)
        {
            canvas = ResolveHintCanvas();
            if (canvas == null)
            {
                return;
            }
        }

        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null)
        {
            activeHintTooltipRect.position = pointerPosition + hintTooltipOffset;
            return;
        }

        Vector2 anchoredPos;
        Vector2 adjustedPointer = pointerPosition + hintTooltipOffset;

        Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, adjustedPointer, canvasCamera, out anchoredPos))
        {
            activeHintTooltipRect.anchoredPosition = anchoredPos;
        }
        else
        {
            activeHintTooltipRect.position = pointerPosition + hintTooltipOffset;
        }
    }

    private void HideHintTooltip()
    {
        if (activeHintTooltip != null)
        {
            Destroy(activeHintTooltip);
            activeHintTooltip = null;
        }

        activeHintTooltipRect = null;
        activeHintTooltipText = null;
    }

    private Canvas ResolveHintCanvas()
    {
        if (hintTooltipCanvas != null)
        {
            return hintTooltipCanvas;
        }

        hintTooltipCanvas = GetComponentInParent<Canvas>();
        if (hintTooltipCanvas == null)
        {
            hintTooltipCanvas = FindObjectOfType<Canvas>();
        }

        return hintTooltipCanvas;
    }

    private void EnsureSubtitleText()
    {
        if (!showSubtitle) return;
        if (subtitleText != null) return;
        if (!autoCreateSubtitleText) return;

        var go = new GameObject("SubtitleText");
        go.transform.SetParent(transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = subtitleAnchorMin;
        rt.anchorMax = subtitleAnchorMax;
        rt.pivot = subtitlePivot;
        rt.offsetMin = subtitleOffsetMin;
        rt.offsetMax = subtitleOffsetMax;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = string.Empty;
        tmp.fontSize = subtitleFontSize;
        tmp.alignment = subtitleAlignment;
        tmp.color = subtitleColor;
        tmp.raycastTarget = false;

        subtitleText = tmp;
    }

    private void UpdateSubtitleDisplay()
    {
        if (!showSubtitle || subtitleText == null) return;
        string value = subtitleFallback;
        if (assignedItem != null && !string.IsNullOrWhiteSpace(assignedItem.cardName))
        {
            value = assignedItem.cardName;
        }
        subtitleText.text = value ?? string.Empty;
    }

    private void EnsureDamageText()
    {
        if (!showAttackDamage) return;

        // If text already exists (assigned via inspector), still optionally enforce layout
        if (attackDamageText != null)
        {
            if (enforceDamageTextLayout)
            {
                ApplyDamageTextLayout();
            }
            return;
        }

        // If no text exists and we shouldn't auto-create, still optionally enforce (noop if null)
        if (!autoCreateDamageText)
        {
            if (enforceDamageTextLayout)
            {
                ApplyDamageTextLayout();
            }
            return;
        }

        // Build a child TMP text under this card
        var go = new GameObject("AttackDamageText");
        go.transform.SetParent(transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = damageTextAnchorMin;
        rt.anchorMax = damageTextAnchorMax;
        rt.pivot = damageTextPivot;
        rt.offsetMin = damageTextOffsetMin;
        rt.offsetMax = damageTextOffsetMax;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = string.Empty;
        tmp.fontSize = damageTextFontSize;
        tmp.alignment = damageTextAlignment;
        tmp.color = attackDamageColor;
        tmp.raycastTarget = false;

        attackDamageText = tmp;

        // Always enforce layout so prefabs/scenes with overridden anchors are corrected
        if (enforceDamageTextLayout)
        {
            ApplyDamageTextLayout();
        }
    }

    private void ApplyDamageTextLayout()
    {
        if (attackDamageText == null) return;
        var rt = attackDamageText.rectTransform;
        rt.anchorMin = damageTextAnchorMin;
        rt.anchorMax = damageTextAnchorMax;
        rt.pivot = damageTextPivot;
        rt.offsetMin = damageTextOffsetMin;
        rt.offsetMax = damageTextOffsetMax;
        attackDamageText.alignment = damageTextAlignment;
        attackDamageText.fontSize = damageTextFontSize;
        attackDamageText.color = attackDamageColor;
    }

    private void UpdateAttackDamageDisplay()
    {
        if (!showAttackDamage || attackDamageText == null) return;

        // Find a PlayCardButton to obtain damage info and indices
        var pcb = FindObjectOfType<PlayCardButton>();
        var om = FindObjectOfType<OutputManager>();
        if (pcb == null || om == null)
        {
            attackDamageText.text = string.Empty;
            if (enforceDamageTextLayout) ApplyDamageTextLayout();
            return;
        }

        int questionIndex = om.counter;

        // Determine this card's answer index within the current set by matching name
        int answerIndex = -1;
        var cm = cardManager != null ? cardManager : FindObjectOfType<CardManager>();
        if (cm != null && cm.cardDisplayContainer != null && cm.counter >= 0 && cm.counter < cm.cardDisplayContainer.Count)
        {
            var displays = cm.cardDisplayContainer[cm.counter].cardDisplay;
            for (int i = 0; i < displays.Count; i++)
            {
                if (displays[i] == this)
                {
                    answerIndex = i;
                    break;
                }
            }
        }

        if (answerIndex < 0)
        {
            attackDamageText.text = string.Empty;
            return;
        }

        float damage = pcb.GetCorrectAnswerDamage(questionIndex, answerIndex);
        if (damage <= 0.01f)
        {
            attackDamageText.text = string.Empty;
            return;
        }

        attackDamageText.color = attackDamageColor;
        attackDamageText.text = string.Concat(attackDamagePrefix, damage.ToString("0.#"), attackDamageSuffix);
    }

    private GameObject CreateDefaultHintTooltip(Canvas parentCanvas)
    {
        GameObject tooltip = new GameObject("CardHintTooltip");
        tooltip.transform.SetParent(parentCanvas.transform, false);

        RectTransform rectTransform = tooltip.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(320f, 140f);

        Image background = tooltip.AddComponent<Image>();
        background.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);

        GameObject textObj = new GameObject("HintText");
        textObj.transform.SetParent(tooltip.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 18f);
        textRect.offsetMax = new Vector2(-18f, -18f);

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.enableWordWrapping = true;
        textComponent.fontSize = 24f;
        textComponent.color = Color.yellow;
        textComponent.text = string.Empty;

        return tooltip;
    }
}