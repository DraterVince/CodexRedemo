using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Match this RectTransform's size to a target resolved by name or tag.
/// Useful when you have multiple Hint Buttons and Correct Answers that follow
/// a naming convention (e.g., "Hint_01" pairs with "CorrectAnswer_01").
///
/// Features:
/// - Resolve target by exact name, name contains, or tag
/// - Derive target name from this object's name by replacing prefix/suffix
/// - Choose search scope: same parent, in container, or whole scene
/// - Matches width/height, and handles stretch anchors by copying anchors/offsets
/// - Optional: runEveryFrame for dynamic UIs
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class MatchRectSizeByNameOrTag : MonoBehaviour
{
    public enum ResolveMode
    {
        ExactName,
        NameContains,
        Tag
    }

    public enum SearchScope
    {
        SameParent,
        InContainer,
        WholeScene
    }

    [Header("Resolution")]
    public ResolveMode resolveMode = ResolveMode.ExactName;
    public SearchScope searchScope = SearchScope.SameParent;

    [Tooltip("Used for ExactName resolution. If 'deriveFromSelfName' is true, this will be computed at runtime.")]
    public string targetExactName = "CorrectAnswer_01";

    [Tooltip("Used for NameContains resolution. We'll pick the closest match by distance.")]
    public string targetNameContains = "CorrectAnswer";

    [Tooltip("Used for Tag resolution. All potential matches must share this tag.")]
    public string targetTag = "CorrectAnswer";

    [Header("Derive target name from this object's name")]
    public bool deriveFromSelfName = true;
    [Tooltip("If true, replaces this object's prefix with targetPrefix. Example: Hint_ -> CorrectAnswer_")]
    public bool replacePrefix = true;
    public string selfPrefix = "Hint_";
    public string targetPrefix = "CorrectAnswer_";

    [Tooltip("If true, replaces this object's suffix with targetSuffix. Example: _Hint -> _Correct")]
    public bool replaceSuffix = false;
    public string selfSuffix = "";
    public string targetSuffix = "";

    [Tooltip("Optional container to limit the search.")]
    public Transform container;

    [Header("Match Options")]
    public bool matchWidth = true;
    public bool matchHeight = true;
    public bool runEveryFrame = false;

    private RectTransform self;
    private RectTransform target;

    private void Awake()
    {
        CacheSelf();
        TryResolveTarget();
        Apply();
    }

    private void OnEnable()
    {
        TryResolveTarget();
        Apply();
    }

    private void Update()
    {
        if (runEveryFrame)
        {
            TryResolveTarget();
            Apply();
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        Apply();
    }

    private bool CacheSelf()
    {
        if (self == null)
        {
            self = GetComponent<RectTransform>();
        }
        return self != null;
    }

    private void TryResolveTarget()
    {
        if (!CacheSelf()) return;

        string derivedName = self.name;
        if (deriveFromSelfName)
        {
            derivedName = self.name;
            if (replacePrefix && !string.IsNullOrEmpty(selfPrefix) && derivedName.StartsWith(selfPrefix))
            {
                derivedName = targetPrefix + derivedName.Substring(selfPrefix.Length);
            }
            if (replaceSuffix && !string.IsNullOrEmpty(selfSuffix) && derivedName.EndsWith(selfSuffix))
            {
                derivedName = derivedName.Substring(0, derivedName.Length - selfSuffix.Length) + targetSuffix;
            }
        }

        // If we derive, use the result as the exact name when in ExactName mode
        string exactNameToFind = resolveMode == ResolveMode.ExactName
            ? (deriveFromSelfName ? derivedName : targetExactName)
            : targetExactName;

        // Resolve candidates based on scope
        RectTransform[] candidates = null;
        switch (searchScope)
        {
            case SearchScope.SameParent:
                if (self.parent != null)
                {
                    candidates = self.parent.GetComponentsInChildren<RectTransform>(true);
                }
                break;
            case SearchScope.InContainer:
                if (container != null)
                {
                    candidates = container.GetComponentsInChildren<RectTransform>(true);
                }
                break;
            case SearchScope.WholeScene:
                candidates = GameObject.FindObjectsOfType<RectTransform>(true);
                break;
        }

        if (candidates == null || candidates.Length == 0)
        {
            target = null;
            return;
        }

        // Filter according to mode
        switch (resolveMode)
        {
            case ResolveMode.ExactName:
                target = candidates.FirstOrDefault(rt => rt != self && rt.name == exactNameToFind);
                break;
            case ResolveMode.NameContains:
                var list = candidates.Where(rt => rt != self && rt.name.IndexOf(targetNameContains, System.StringComparison.OrdinalIgnoreCase) >= 0);
                // choose nearest by distance for stability
                target = list.OrderBy(rt => Vector3.SqrMagnitude(rt.position - self.position)).FirstOrDefault();
                break;
            case ResolveMode.Tag:
                var tagged = candidates.Where(rt => rt != self && rt.CompareTag(targetTag));
                target = tagged.OrderBy(rt => Vector3.SqrMagnitude(rt.position - self.position)).FirstOrDefault();
                break;
        }
    }

    public void Apply()
    {
        if (!CacheSelf() || target == null) return;

        // Try to use a LayoutElement from the target first (more accurate than rect in many dynamic layouts)
        var targetLE = target.GetComponent<LayoutElement>();
        var selfLE = GetComponent<LayoutElement>();

        bool selfStretching = IsStretching(self);
        bool targetStretching = IsStretching(target);

        if (targetLE != null && (targetLE.preferredWidth > 0 || targetLE.preferredHeight > 0))
        {
            if (selfLE == null) selfLE = gameObject.AddComponent<LayoutElement>();

            if (matchWidth && targetLE.preferredWidth > 0)
            {
                selfLE.preferredWidth = targetLE.preferredWidth;
            }
            if (matchHeight && targetLE.preferredHeight > 0)
            {
                selfLE.preferredHeight = targetLE.preferredHeight;
            }
        }
        else if (!selfStretching && !targetStretching)
        {
            // Directly match sizes using SetSizeWithCurrentAnchors to respect current anchors
            if (matchWidth)
                self.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, target.rect.width);
            if (matchHeight)
                self.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, target.rect.height);
        }
        else
        {
            // Copy anchors/pivot for a proper stretch match
            self.anchorMin = target.anchorMin;
            self.anchorMax = target.anchorMax;
            self.pivot = target.pivot;

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

        // If ContentSizeFitter on target drives size, ensure we re-apply after layout rebuilds
        var csf = target.GetComponent<ContentSizeFitter>();
        if (csf != null && !runEveryFrame)
        {
            // Trigger a delayed apply on next frame to pick up final sizes
            StartCoroutine(DelayedApply());
        }
    }

    private System.Collections.IEnumerator DelayedApply()
    {
        yield return null; // wait 1 frame for layout to settle
        Apply();
    }

    private static bool IsStretching(RectTransform rt)
    {
        return rt.anchorMin.x != rt.anchorMax.x || rt.anchorMin.y != rt.anchorMax.y;
    }
}
