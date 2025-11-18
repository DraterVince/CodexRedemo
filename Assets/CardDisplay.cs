using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public CardManager cardManager;

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
    private GameObject activeHintTooltip;
    private RectTransform activeHintTooltipRect;
    private TextMeshProUGUI activeHintTooltipText;

    private void Update()
    {
        gameObject.name = cardName;
    }

    /// <summary>
    /// Assigns the scriptable object backing this card display.
    /// </summary>
    /// <param name="item">Item data instance (can be null).</param>
    public void SetAssignedItem(Item item)
    {
        assignedItem = item;
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