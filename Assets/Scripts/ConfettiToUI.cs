using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Converts a SpriteAnimator GameObject to work with UI Canvas
/// Adds it as a child of a UI panel
/// </summary>
public class ConfettiToUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The panel/canvas element to place confetti on")]
    public RectTransform targetPanel;
    
    [Tooltip("The sprite animator GameObject (will be converted to UI)")]
    public GameObject spriteAnimatorObject;
    
    [Header("Settings")]
    [Tooltip("Position offset from panel center")]
    public Vector2 positionOffset = Vector2.zero;
    
    [Tooltip("Scale of the confetti")]
    public float scale = 1f;
    
    [Tooltip("Sorting order (higher = more on top)")]
    public int sortingOrder = 100;
    
    [ContextMenu("Convert to UI")]
    void Start()
    {
        ConvertToUI();
    }
  
    public void ConvertToUI()
    {
        if (targetPanel == null)
        {
  Debug.LogError("[ConfettiToUI] No target panel assigned!");
    return;
  }
     
        if (spriteAnimatorObject == null)
     {
            Debug.LogError("[ConfettiToUI] No sprite animator object assigned!");
      return;
        }
        
     // Get the SpriteAnimator component
        SpriteAnimator spriteAnim = spriteAnimatorObject.GetComponent<SpriteAnimator>();
        if (spriteAnim == null)
        {
    Debug.LogError("[ConfettiToUI] No SpriteAnimator component found!");
            return;
      }
        
        // Create UI GameObject under the panel
        GameObject uiConfetti = new GameObject("ConfettiUI");
        uiConfetti.transform.SetParent(targetPanel, false);
        
        // Add RectTransform
        RectTransform rectTransform = uiConfetti.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = positionOffset;
        rectTransform.sizeDelta = new Vector2(200, 200); // Adjust as needed
        rectTransform.localScale = Vector3.one * scale;
        
        // Add Image component
  Image image = uiConfetti.AddComponent<Image>();
        
        // Add Canvas for sorting
   Canvas canvas = uiConfetti.AddComponent<Canvas>();
 canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        
        // Add UIImageAnimator
        UIImageAnimator uiAnim = uiConfetti.AddComponent<UIImageAnimator>();
        uiAnim.sprites = spriteAnim.sprites;
        uiAnim.frameRate = spriteAnim.frameRate;
    uiAnim.playOnStart = spriteAnim.playOnStart;
        uiAnim.loop = spriteAnim.loop;
      uiAnim.pingPong = spriteAnim.pingPong;
        
   // Destroy original sprite object
    Destroy(spriteAnimatorObject);
   
        Debug.Log($"[ConfettiToUI] Converted to UI! Placed on panel: {targetPanel.name}");
    }
}
