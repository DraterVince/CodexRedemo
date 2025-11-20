using UnityEngine;

/// <summary>
/// Attach this to the Hint Button (or any UI element) to make its RectTransform
/// match the size (and optionally anchors) of a target RectTransform (e.g., CorrectAnswer).
/// - If both self and target use non-stretch anchors, copies the target's actual width/height.
/// - If either uses stretch anchors, copies target's anchors/pivot and offsets for a closer match.
/// Set runEveryFrame = true if the target's size changes frequently (e.g., dynamic text).
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class MatchRectSize : MonoBehaviour
{
    [Tooltip("The RectTransform to match (e.g., CorrectAnswer's RectTransform)")]
    public RectTransform target;

    [Tooltip("Copy width from target")]
    public bool matchWidth = true;

    [Tooltip("Copy height from target")]
    public bool matchHeight = true;

    [Tooltip("Re-apply every frame (useful if target size changes dynamically)")]
    public bool runEveryFrame = false;

    private RectTransform self;

    private void Awake()
    {
        CacheSelf();
        Apply();
    }

    private void OnEnable()
    {
        Apply();
    }

    private void Update()
    {
        if (runEveryFrame)
        {
            Apply();
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        // React to layout changes in editor and at runtime
        Apply();
    }

    public void Apply()
    {
        if (!CacheSelf() || target == null) return;

        bool selfStretching = IsStretching(self);
        bool targetStretching = IsStretching(target);

        if (!selfStretching && !targetStretching)
        {
            // Directly match sizes using SetSizeWithCurrentAnchors to respect current anchors
            if (matchWidth)
            {
                self.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, target.rect.width);
            }
            if (matchHeight)
            {
                self.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, target.rect.height);
            }
        }
        else
        {
            // Copy anchors/pivot for a proper stretch match
            self.anchorMin = target.anchorMin;
            self.anchorMax = target.anchorMax;
            self.pivot = target.pivot;

            // Copy offsets as needed
            Vector2 offMin = self.offsetMin;
            Vector2 offMax = self.offsetMax;

            if (matchWidth)
            {
                offMin.x = target.offsetMin.x;
                offMax.x = target.offsetMax.x;
            }
            if (matchHeight)
            {
                offMin.y = target.offsetMin.y;
                offMax.y = target.offsetMax.y;
            }

            self.offsetMin = offMin;
            self.offsetMax = offMax;
        }
    }

    private bool CacheSelf()
    {
        if (self == null)
        {
            self = GetComponent<RectTransform>();
        }
        return self != null;
    }

    private static bool IsStretching(RectTransform rt)
    {
        // Stretching if any axis has different min/max anchor
        return rt.anchorMin.x != rt.anchorMax.x || rt.anchorMin.y != rt.anchorMax.y;
    }
}
