using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays card details when a card is selected/clicked
/// </summary>
public class CardSelectedDetailPanel : MonoBehaviour
{
    [Header("Detail Panel")]
    public GameObject detailPanel;
    public TextMeshProUGUI detailCardName;
    public TextMeshProUGUI detailCardDescription;
    public TextMeshProUGUI detailCardExample;
    // Note: Panel uses its own background - card background image is not displayed

    private void Awake()
    {
        // Hide panel initially
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[CardSelectedDetailPanel] Detail Panel GameObject is not assigned! Assign it in the inspector. The panel will not work without this.");
        }
    }

    private void Start()
    {
        // Check if all required fields are assigned
        if (detailPanel == null)
        {
            Debug.LogError("[CardSelectedDetailPanel] Detail Panel GameObject is not assigned in the inspector! Assign the GameObject that contains the panel UI.");
        }
        if (detailCardName == null)
        {
            Debug.LogError("[CardSelectedDetailPanel] Detail Card Name TextMeshProUGUI is not assigned in the inspector!");
        }
        if (detailCardDescription == null)
        {
            Debug.LogError("[CardSelectedDetailPanel] Detail Card Description TextMeshProUGUI is not assigned in the inspector!");
        }
        if (detailCardExample == null)
        {
            Debug.LogError("[CardSelectedDetailPanel] Detail Card Example TextMeshProUGUI is not assigned in the inspector!");
        }
    }

    /// <summary>
    /// Show card details in the panel
    /// </summary>
    public void ShowCardDetails(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogError($"[CardSelectedDetailPanel] Card data is null! Cannot show details. Component: '{gameObject.name}'");
            return;
        }

        if (detailPanel == null)
        {
            Debug.LogError($"[CardSelectedDetailPanel] ✗ CRITICAL: Detail panel GameObject is NULL on component '{gameObject.name}'! " +
                $"This component is attached to GameObject '{gameObject.name}'. " +
                "The 'Detail Panel' GameObject field must be assigned in the inspector for the panel to work. " +
                "Go to the inspector and assign the GameObject that contains the panel UI elements.");
            return;
        }
        
        // Log which component is being used and its hierarchy
        string componentRootNameForLog = gameObject.transform.root.name;
        Debug.Log($"[CardSelectedDetailPanel] Showing card details for '{cardData.cardName}' in panel '{detailPanel.name}' " +
            $"(Component: '{gameObject.name}', Component Root: '{componentRootNameForLog}')");

        // Update card name
        if (detailCardName != null)
        {
            detailCardName.text = string.IsNullOrEmpty(cardData.cardName) ? "No Name" : cardData.cardName;
            Debug.Log($"[CardSelectedDetailPanel] Updated card name: '{detailCardName.text}'");
        }
        else
        {
            Debug.LogError("[CardSelectedDetailPanel] Detail card name TextMeshProUGUI is not assigned! Assign it in the inspector.");
        }

        // Update card description
        if (detailCardDescription != null)
        {
            detailCardDescription.text = string.IsNullOrEmpty(cardData.cardDescription) ? "No Description" : cardData.cardDescription;
            Debug.Log($"[CardSelectedDetailPanel] Updated card description: '{detailCardDescription.text}'");
        }
        else
        {
            Debug.LogError("[CardSelectedDetailPanel] Detail card description TextMeshProUGUI is not assigned! Assign it in the inspector.");
        }

        // Update card example
        if (detailCardExample != null)
        {
            detailCardExample.text = string.IsNullOrEmpty(cardData.cardExample) ? "No Example" : cardData.cardExample;
            Debug.Log($"[CardSelectedDetailPanel] Updated card example: '{detailCardExample.text}'");
        }
        else
        {
            Debug.LogError("[CardSelectedDetailPanel] Detail card example TextMeshProUGUI is not assigned! Assign it in the inspector.");
        }

        // Note: Card background image is not displayed - panel uses its own background

        // Show panel - CRITICAL: Make sure the panel is active AND its parent hierarchy is enabled
        if (detailPanel != null)
        {
            bool wasActive = detailPanel.activeSelf;
            bool wasActiveInHierarchy = detailPanel.activeInHierarchy;
            
            // CRITICAL FIX: Enable only NECESSARY parent GameObjects in THIS panel's hierarchy
            // IMPORTANT: Only enable parents that are part of THIS detail panel's hierarchy
            // Don't enable unrelated parents (like CardsPanel1 when we're in CardsPanel2)
            // The issue: BG is active=True but activeInHierarchy=False, meaning BG's parent (or ancestor) is disabled
            // Solution: Walk up ONLY from this detail panel's hierarchy to enable disabled parents
            System.Collections.Generic.List<GameObject> enabledParents = new System.Collections.Generic.List<GameObject>();
            
            // Get the root of this detail panel's hierarchy to track what we're enabling
            GameObject detailPanelRoot = detailPanel.transform.root.gameObject;
            string detailPanelRootName = detailPanelRoot.name;
            string componentName = gameObject.name;
            
            // Get the component's root to see if it's different from the detail panel's root
            GameObject componentRootObj = gameObject.transform.root.gameObject;
            string componentRootName = componentRootObj.name;
            
            // CRITICAL: Check if CardsPanel1 is in the hierarchy path
            // If so, we need to be extra careful not to enable it
            bool cardsPanel1InPath = false;
            bool cardsPanel2InPath = false;
            Transform checkTransform = detailPanel.transform;
            while (checkTransform != null)
            {
                string checkName = checkTransform.name;
                if (checkName.Contains("CardsPanel1", System.StringComparison.OrdinalIgnoreCase))
                {
                    cardsPanel1InPath = true;
                }
                if (checkName.Contains("CardsPanel2", System.StringComparison.OrdinalIgnoreCase))
                {
                    cardsPanel2InPath = true;
                }
                checkTransform = checkTransform.parent;
            }
            
            Debug.Log($"[CardSelectedDetailPanel] Detail panel '{detailPanel.name}' is in hierarchy root '{detailPanelRootName}'. " +
                $"Component: '{componentName}' (root: '{componentRootName}'). " +
                $"CardsPanel1 in path: {cardsPanel1InPath}, CardsPanel2 in path: {cardsPanel2InPath}. " +
                $"Walking up to enable disabled parents (will skip CardsPanel1 if found)...");
            
            // Build a list of parent names in the hierarchy for reference
            System.Collections.Generic.List<string> hierarchyPath = new System.Collections.Generic.List<string>();
            Transform tempTransform = detailPanel.transform;
            while (tempTransform != null)
            {
                hierarchyPath.Add(tempTransform.name);
                tempTransform = tempTransform.parent;
            }
            string hierarchyPathStr = string.Join(" -> ", hierarchyPath);
            Debug.Log($"[CardSelectedDetailPanel] Detail panel hierarchy path: {hierarchyPathStr}");
            
            // Walk up from the detail panel all the way to the root, enabling any disabled parents
            // BUT: Only enable parents that are part of this detail panel's hierarchy
            // IMPORTANT: Skip any parent that looks like CardsPanel1 to prevent cross-panel activation
            Transform current = detailPanel.transform;
            int depth = 0;
            int maxDepth = 50; // Safety limit
            
            // First pass: Walk up and enable all disabled parents in THIS panel's hierarchy
            // Skip any parent that might be CardsPanel1 or a different panel
            while (current.parent != null && depth < maxDepth)
            {
                current = current.parent;
                string parentName = current.gameObject.name;
                
                // Check if this parent is disabled
                if (!current.gameObject.activeSelf)
                {
                    // CRITICAL CHECK: First, check if this parent is in the detail panel's DIRECT hierarchy path
                    // If it is, we MUST enable it (even if it "looks like a different panel")
                    // This is the MOST IMPORTANT check - do this FIRST before any other checks
                    bool isInDetailPanelPath = false;
                    
                    // Check if this parent is in the detail panel's hierarchy path
                    // (i.e., the detail panel is a descendant of this parent)
                    Transform checkParent = detailPanel.transform;
                    int pathDepth = 0;
                    while (checkParent != null && pathDepth < 20)
                    {
                        if (checkParent == current)
                        {
                            // This parent IS in the detail panel's path - we MUST enable it
                            isInDetailPanelPath = true;
                            Debug.Log($"[CardSelectedDetailPanel] ✓ Parent '{parentName}' (depth {depth}) is in detail panel's hierarchy path! " +
                                $"Detail panel: '{detailPanel.name}' is a descendant of '{parentName}'. " +
                                $"MUST enable it (even if it looks like a different panel) because the detail panel needs it to be visible.");
                            break;
                        }
                        checkParent = checkParent.parent;
                        pathDepth++;
                    }
                    
                    if (!isInDetailPanelPath)
                    {
                        // This parent is NOT in the detail panel's path
                        // Now check if it looks like a different panel - if so, skip it
                        bool isCardsPanelParent = parentName.Contains("CardsPanel", System.StringComparison.OrdinalIgnoreCase);
                        bool isDifferentPanel = isCardsPanelParent && 
                                               !parentName.Equals(detailPanelRootName, System.StringComparison.OrdinalIgnoreCase) &&
                                               !parentName.Equals(componentRootName, System.StringComparison.OrdinalIgnoreCase);
                        
                        if (isDifferentPanel)
                        {
                            Debug.LogWarning($"[CardSelectedDetailPanel] ⚠ SKIPPING parent '{parentName}' (depth {depth}) because it looks like a different panel " +
                                $"AND it's NOT in the detail panel's hierarchy path! " +
                                $"Detail panel root: '{detailPanelRootName}', Component root: '{componentRootName}'. " +
                                $"This prevents CardsPanel1 from activating when clicking CardsPanel2.");
                            // Skip this parent - don't enable it, but continue walking up
                            depth++;
                            continue;
                        }
                        
                        // Check if this parent has children that are different panels - if so, skip it
                        bool shouldSkipParent = false;
                        string conflictingChild = null;
                        
                        for (int i = 0; i < current.childCount; i++)
                        {
                            Transform child = current.GetChild(i);
                            string childName = child.name;
                            
                            // Check if this child is a CardsPanel GameObject that's different from ours
                            if (childName.Contains("CardsPanel", System.StringComparison.OrdinalIgnoreCase))
                            {
                                // If this child is NOT our detail panel root or component root, it's a different panel
                                if (!childName.Equals(detailPanelRootName, System.StringComparison.OrdinalIgnoreCase) &&
                                    !childName.Equals(componentRootName, System.StringComparison.OrdinalIgnoreCase))
                                {
                                    // This parent contains a different CardsPanel - DON'T enable it!
                                    shouldSkipParent = true;
                                    conflictingChild = childName;
                                    break;
                                }
                            }
                        }
                        
                        if (shouldSkipParent)
                        {
                            Debug.LogWarning($"[CardSelectedDetailPanel] ⚠ SKIPPING parent '{parentName}' (depth {depth}) because it contains child '{conflictingChild}' " +
                                $"which is a different panel AND it's NOT in the detail panel's hierarchy path! " +
                                $"Detail panel root: '{detailPanelRootName}', Component root: '{componentRootName}'. " +
                                $"Enabling this parent would activate '{conflictingChild}' unnecessarily.");
                            // Skip this parent - don't enable it, but continue walking up
                            depth++;
                            continue;
                        }
                    }
                    
                    // Safe to enable this parent
                    // Either it's in the detail panel's path (required), or it doesn't have conflicting children
                    current.gameObject.SetActive(true);
                    enabledParents.Add(current.gameObject);
                    
                    if (isInDetailPanelPath)
                    {
                        Debug.Log($"[CardSelectedDetailPanel] ✓ Enabled disabled parent '{parentName}' at depth {depth} " +
                            $"- REQUIRED because it's in the detail panel's hierarchy path. " +
                            $"Detail panel: '{detailPanel.name}' -> Parent: '{parentName}'");
                    }
                    else
                    {
                        Debug.Log($"[CardSelectedDetailPanel] ✓ Enabled disabled parent '{parentName}' at depth {depth} " +
                            $"(part of '{detailPanelRootName}' hierarchy, no conflicting panels found)");
                    }
                }
                depth++;
            }
            
            // Second pass: Check if immediate parent (BG) is enabled but not active in hierarchy
            // This means an ancestor of BG is disabled - walk up from BG to find and enable it
            // CRITICAL: Do this BEFORE activating the panel
            if (detailPanel.transform.parent != null)
            {
                Transform immediateParent = detailPanel.transform.parent;
                
                // Check if immediate parent is enabled but not active in hierarchy
                // This means one of its ancestors is disabled
                if (!immediateParent.gameObject.activeInHierarchy && immediateParent.gameObject.activeSelf)
                {
                    Debug.LogWarning($"[CardSelectedDetailPanel] Immediate parent '{immediateParent.name}' is enabled but NOT active in hierarchy. " +
                        $"Walking up from '{immediateParent.name}' to find and enable disabled ancestor...");
                    
                    // Walk up from immediate parent (BG) to root, enabling ALL disabled ancestors
                    // Start from BG and walk up the entire parent chain
                    current = immediateParent;
                    depth = 0;
                    bool foundAndEnabledAny = false;
                    
                    // Walk up the hierarchy, checking each parent
                    // IMPORTANT: Only enable parents that are part of THIS detail panel's hierarchy
                    // Don't enable parents that might be CardsPanel1 when we're in CardsPanel2
                    while (current != null && depth < maxDepth)
                    {
                        Transform parentOfCurrent = current.parent;
                        if (parentOfCurrent != null)
                        {
                            string parentName = parentOfCurrent.gameObject.name;
                            
                            // Check if this parent is disabled
                            if (!parentOfCurrent.gameObject.activeSelf)
                            {
                                // CRITICAL CHECK: First, check if this parent is in the detail panel's DIRECT hierarchy path
                                // If it is, we MUST enable it (even if it "looks like a different panel")
                                bool isParentInDetailPanelPath = false;
                                
                                // Check if this parent is in the detail panel's hierarchy path
                                Transform checkParentPath = detailPanel.transform;
                                while (checkParentPath != null)
                                {
                                    if (checkParentPath == parentOfCurrent)
                                    {
                                        // This parent IS in the detail panel's path - we MUST enable it
                                        isParentInDetailPanelPath = true;
                                        Debug.Log($"[CardSelectedDetailPanel] ✓ Parent '{parentName}' (depth {depth}) is in detail panel's hierarchy path. " +
                                            $"Must enable it for detail panel to be visible (even if it looks like a different panel).");
                                        break;
                                    }
                                    checkParentPath = checkParentPath.parent;
                                }
                                
                                // If this parent is NOT in the detail panel's path, check if it looks like a different panel
                                // Only skip it if it's NOT in the path AND looks like a different panel
                                if (!isParentInDetailPanelPath)
                                {
                                    bool mightBeDifferentPanel = parentName.Contains("CardsPanel", System.StringComparison.OrdinalIgnoreCase) && 
                                                                 !parentName.Equals(detailPanelRootName, System.StringComparison.OrdinalIgnoreCase) &&
                                                                 !parentName.Equals(componentRootName, System.StringComparison.OrdinalIgnoreCase);
                                    
                                    if (mightBeDifferentPanel)
                                    {
                                        Debug.LogWarning($"[CardSelectedDetailPanel] ⚠ SKIPPING parent '{parentName}' at depth {depth} " +
                                            $"because it looks like a different panel AND it's NOT in the detail panel's hierarchy path! " +
                                            $"Detail panel root: '{detailPanelRootName}', Component root: '{componentRootName}'. " +
                                            $"This prevents CardsPanel1 from activating when clicking CardsPanel2.");
                                        // Skip this parent - don't enable it, but continue walking up
                                        current = parentOfCurrent;
                                        depth++;
                                        continue;
                                    }
                                }
                                
                                // If this parent is NOT in the detail panel's path, check if it has conflicting children
                                // Only skip it if it's NOT in the path AND has conflicting children
                                if (!isParentInDetailPanelPath)
                                {
                                    // This parent is NOT in the detail panel's path
                                    // Check if it has children that are different panels - if so, skip it
                                    bool hasConflictingPanelChild = false;
                                    string conflictingChildName = null;
                                    
                                    for (int i = 0; i < parentOfCurrent.childCount; i++)
                                    {
                                        Transform child = parentOfCurrent.GetChild(i);
                                        string childName = child.name;
                                        
                                        // Check if this child is a CardsPanel that's different from ours
                                        if (childName.Contains("CardsPanel", System.StringComparison.OrdinalIgnoreCase))
                                        {
                                            // If this child is NOT our detail panel root or component root, it's a different panel
                                            if (!childName.Equals(detailPanelRootName, System.StringComparison.OrdinalIgnoreCase) &&
                                                !childName.Equals(componentRootName, System.StringComparison.OrdinalIgnoreCase))
                                            {
                                                hasConflictingPanelChild = true;
                                                conflictingChildName = childName;
                                                break;
                                            }
                                        }
                                    }
                                    
                                    if (hasConflictingPanelChild)
                                    {
                                        Debug.LogWarning($"[CardSelectedDetailPanel] ⚠ SKIPPING parent '{parentName}' at depth {depth} " +
                                            $"because it contains child '{conflictingChildName}' which is a different panel " +
                                            $"AND it's NOT in the detail panel's hierarchy path! " +
                                            $"Detail panel root: '{detailPanelRootName}', Component root: '{componentRootName}'. " +
                                            $"Enabling this parent would activate '{conflictingChildName}' unnecessarily.");
                                        // Skip this parent - don't enable it, but continue walking up
                                        current = parentOfCurrent;
                                        depth++;
                                        continue;
                                    }
                                }
                                
                                // Safe to enable this parent
                                // Either it's in the detail panel's path (required), or it doesn't have conflicting children
                                parentOfCurrent.gameObject.SetActive(true);
                                if (!enabledParents.Contains(parentOfCurrent.gameObject))
                                {
                                    enabledParents.Add(parentOfCurrent.gameObject);
                                    foundAndEnabledAny = true;
                                    
                                    if (isParentInDetailPanelPath)
                                    {
                                        Debug.Log($"[CardSelectedDetailPanel] ✓ Enabled disabled ancestor '{parentName}' at depth {depth} " +
                                            $"(parent of '{current.name}') - REQUIRED because it's in the detail panel's hierarchy path. " +
                                            $"Detail panel path: CardDescPanel -> BG -> CardsPanel1 -> Canvas");
                                    }
                                    else
                                    {
                                        Debug.Log($"[CardSelectedDetailPanel] ✓ Enabled disabled ancestor '{parentName}' at depth {depth} " +
                                            $"(parent of '{current.name}', part of '{detailPanelRootName}' hierarchy, no conflicting panels)");
                                    }
                                }
                            }
                            // Move to next parent in the chain
                            current = parentOfCurrent;
                        }
                        else
                        {
                            // Reached root (no more parents)
                            break;
                        }
                        depth++;
                    }
                    
                    if (foundAndEnabledAny)
                    {
                        Debug.Log($"[CardSelectedDetailPanel] Enabled {enabledParents.Count} disabled ancestor(s). " +
                            "Forcing Unity to update hierarchy...");
                        // Force Unity to update the hierarchy immediately
                        Canvas.ForceUpdateCanvases();
                        
                        // Re-check if immediate parent is now active in hierarchy
                        if (immediateParent.gameObject.activeInHierarchy)
                        {
                            Debug.Log($"[CardSelectedDetailPanel] ✓ Immediate parent '{immediateParent.name}' is now active in hierarchy!");
                        }
                        else
                        {
                            Debug.LogWarning($"[CardSelectedDetailPanel] ⚠ Immediate parent '{immediateParent.name}' is still not active in hierarchy. " +
                                "There might be another issue, or Unity needs more time to update.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[CardSelectedDetailPanel] ✗ No disabled ancestors found for '{immediateParent.name}', " +
                            $"but it's still not active in hierarchy! " +
                            $"BG active: {immediateParent.gameObject.activeSelf}, " +
                            $"BG activeInHierarchy: {immediateParent.gameObject.activeInHierarchy}. " +
                            $"This might indicate a deeper Unity hierarchy issue.");
                    }
                }
            }
            
            // Get parent hierarchy info for debugging
            string parentInfo = "";
            Transform parent = detailPanel.transform.parent;
            if (parent != null)
            {
                parentInfo = $" Parent: '{parent.name}' (active: {parent.gameObject.activeSelf}, activeInHierarchy: {parent.gameObject.activeInHierarchy})";
            }
            
            Debug.Log($"[CardSelectedDetailPanel] Activating panel '{detailPanel.name}' for card '{cardData.cardName}'. " +
                $"Before: active={wasActive}, activeInHierarchy={wasActiveInHierarchy}.{parentInfo}. Enabled {enabledParents.Count} parent(s).");
            
            // Activate the panel itself
            detailPanel.SetActive(true);
            
            // Wait a frame for Unity to update the hierarchy
            // Then check if activation was successful
            bool isNowActive = detailPanel.activeSelf;
            bool isNowActiveInHierarchy = detailPanel.activeInHierarchy;
            
            // If still not active in hierarchy after enabling parents, force a canvas update and check again
            if (!isNowActiveInHierarchy && enabledParents.Count > 0)
            {
                Canvas.ForceUpdateCanvases();
                // Re-check after canvas update
                isNowActiveInHierarchy = detailPanel.activeInHierarchy;
            }
            
            if (isNowActive)
            {
                if (isNowActiveInHierarchy)
                {
                    if (enabledParents.Count > 0)
                    {
                        Debug.Log($"[CardSelectedDetailPanel] ✓ Panel '{detailPanel.name}' activated successfully! " +
                            $"Enabled {enabledParents.Count} parent(s) to make it visible. " +
                            $"active={isNowActive}, activeInHierarchy={isNowActiveInHierarchy}");
                    }
                    else
                    {
                        Debug.Log($"[CardSelectedDetailPanel] ✓ Panel '{detailPanel.name}' activated successfully! " +
                            $"active={isNowActive}, activeInHierarchy={isNowActiveInHierarchy}");
                    }
                }
                else
                {
                    // Even after enabling parents, it's still not active in hierarchy
                    // This shouldn't happen, but log it anyway
                    string hierarchyInfo = BuildParentHierarchyString(detailPanel.transform);
                    Debug.LogError($"[CardSelectedDetailPanel] ✗ Panel '{detailPanel.name}' is active but STILL not active in hierarchy! " +
                        $"This is unusual. Parent hierarchy: {hierarchyInfo}");
                }
            }
            else
            {
                Debug.LogError($"[CardSelectedDetailPanel] ✗ FAILED to activate panel '{detailPanel.name}'! " +
                    $"Panel is still inactive (active={isNowActive}). This should never happen.");
            }
        }
        else
        {
            Debug.LogError($"[CardSelectedDetailPanel] Detail panel GameObject is null! Cannot activate panel for '{cardData.cardName}'");
        }
        
        // Force canvas update
        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// Build parent hierarchy string for debugging
    /// </summary>
    private string BuildParentHierarchyString(Transform transform)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        Transform current = transform;
        int depth = 0;
        while (current != null && depth < 10)
        {
            if (sb.Length > 0) sb.Append(" -> ");
            sb.Append($"{current.name}(active:{current.gameObject.activeSelf},activeInHierarchy:{current.gameObject.activeInHierarchy})");
            current = current.parent;
            depth++;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Hide card details
    /// </summary>
    public void HideCardDetails()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Clear card details
    /// </summary>
    public void ClearCardDetails()
    {
        if (detailCardName != null)
        {
            detailCardName.text = "";
        }

        if (detailCardDescription != null)
        {
            detailCardDescription.text = "";
        }

        if (detailCardExample != null)
        {
            detailCardExample.text = "";
        }
    }
}
