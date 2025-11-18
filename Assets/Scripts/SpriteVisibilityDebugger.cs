using UnityEngine;

/// <summary>
/// Diagnose sprite visibility issues - why sprite changes but isn't visible
/// </summary>
public class SpriteVisibilityDebugger : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    
    [Header("Debug Settings")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.green;
    
    void Start()
    {
    spriteRenderer = GetComponent<SpriteRenderer>();
 mainCamera = Camera.main;
        
        Debug.Log("========== SPRITE VISIBILITY DEBUG ==========");
 
        // Check 1: SpriteRenderer exists
      if (spriteRenderer == null)
        {
  Debug.LogError($"? [{gameObject.name}] NO SpriteRenderer found!");
         return;
        }
        else
        {
 Debug.Log($"? SpriteRenderer found on {gameObject.name}");
 }
        
        // Check 2: Sprite assigned
        if (spriteRenderer.sprite == null)
     {
 Debug.LogWarning($"? [{gameObject.name}] SpriteRenderer has NO sprite assigned!");
  }
        else
        {
            Debug.Log($"? Sprite assigned: {spriteRenderer.sprite.name}");
    }
        
        // Check 3: Sprite size
        if (spriteRenderer.sprite != null)
        {
          Vector2 size = spriteRenderer.sprite.bounds.size;
         Debug.Log($"? Sprite size: {size.x:F2} x {size.y:F2} units");
            
  if (size.x < 0.01f || size.y < 0.01f)
     {
                Debug.LogError($"? Sprite is TINY! Size: {size}");
   Debug.LogError($"   This might be invisible on screen!");
          }
        }

        // Check 4: Position
        Vector3 pos = transform.position;
    Debug.Log($"? Position: ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
        
        // Check 5: Scale
        Vector3 scale = transform.lossyScale;
     Debug.Log($"? Scale: ({scale.x:F2}, {scale.y:F2}, {scale.z:F2})");
  
   if (scale.x < 0.01f || scale.y < 0.01f)
        {
            Debug.LogError($"? Scale is TINY! Scale: {scale}");
 Debug.LogError($"   Increase scale to at least (1, 1, 1)");
        }
     
        // Check 6: Sorting Layer
        Debug.Log($"? Sorting Layer: {spriteRenderer.sortingLayerName}");
        Debug.Log($"? Order in Layer: {spriteRenderer.sortingOrder}");
        
        // Check 7: Color/Alpha
        Color color = spriteRenderer.color;
     Debug.Log($"? Color: R={color.r:F2} G={color.g:F2} B={color.b:F2} A={color.a:F2}");
  
        if (color.a < 0.1f)
 {
       Debug.LogError($"? Alpha is {color.a:F2} - sprite is nearly INVISIBLE!");
      Debug.LogError($"   Set alpha to 1.0 in SpriteRenderer color");
   }
        
        // Check 8: Enabled
        if (!spriteRenderer.enabled)
    {
 Debug.LogError($"? SpriteRenderer is DISABLED!");
        }
        else
  {
            Debug.Log($"? SpriteRenderer is enabled");
        }
        
        // Check 9: GameObject active
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError($"? GameObject is INACTIVE in hierarchy!");
     }
      else
      {
       Debug.Log($"? GameObject is active");
        }
        
        // Check 10: Camera can see it
        if (mainCamera != null)
        {
            Debug.Log($"? Main Camera found: {mainCamera.name}");
            Debug.Log($"   Camera position: {mainCamera.transform.position}");
            Debug.Log($"   Camera size: {mainCamera.orthographicSize}");

 // Check if sprite is in camera view
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
            Debug.Log($"   Sprite viewport position: ({viewportPos.x:F2}, {viewportPos.y:F2}, {viewportPos.z:F2})");
            
            bool inView = viewportPos.x >= 0 && viewportPos.x <= 1 && 
    viewportPos.y >= 0 && viewportPos.y <= 1 && 
        viewportPos.z > 0;
    
    if (!inView)
            {
    Debug.LogError($"? Sprite is OUTSIDE camera view!");
      Debug.LogError($"   Viewport coords should be 0-1, currently: ({viewportPos.x:F2}, {viewportPos.y:F2})");
     Debug.LogError($"   SOLUTION: Move sprite to (0, 0, 0) or adjust camera position");
            }
            else
            {
           Debug.Log($"? Sprite IS in camera view!");
      }
   
    // Check Z position relative to camera
          float zDistance = transform.position.z - mainCamera.transform.position.z;
         Debug.Log($"   Z distance from camera: {zDistance:F2}");
 
      if (zDistance < mainCamera.nearClipPlane || zDistance > mainCamera.farClipPlane)
            {
     Debug.LogError($"? Sprite is outside camera clip planes!");
                Debug.LogError($"   Near: {mainCamera.nearClipPlane}, Far: {mainCamera.farClipPlane}, Sprite Z: {zDistance}");
            }
   }
        else
        {
      Debug.LogError($"? NO Main Camera found!");
        }
  
        Debug.Log("=============================================");
        
        // Suggest fixes
 InvokeRepeating("CheckVisibilityIssues", 1f, 2f);
    }
    
    void CheckVisibilityIssues()
    {
        if (spriteRenderer == null) return;
        
        // Quick checks every 2 seconds
   if (spriteRenderer.color.a < 0.1f)
        {
      Debug.LogWarning($"[{gameObject.name}] Still nearly invisible! Alpha={spriteRenderer.color.a:F2}");
        }
        
        if (mainCamera != null)
        {
    Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
            bool inView = viewportPos.x >= 0 && viewportPos.x <= 1 && 
 viewportPos.y >= 0 && viewportPos.y <= 1 && 
   viewportPos.z > 0;
   
            if (!inView)
            {
      Debug.LogWarning($"[{gameObject.name}] Still outside camera! Viewport: ({viewportPos.x:F2}, {viewportPos.y:F2})");
            }
      }
    }
    
 void OnDrawGizmos()
    {
        if (!showGizmos || spriteRenderer == null) return;
        
        // Draw sprite bounds
    Gizmos.color = gizmoColor;
      Gizmos.DrawWireCube(transform.position, spriteRenderer.bounds.size);
        
        // Draw center point
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        
    // Draw line to camera
        if (mainCamera != null)
  {
     Gizmos.color = Color.yellow;
Gizmos.DrawLine(transform.position, mainCamera.transform.position);
        }
    }
    
    [ContextMenu("Move to Camera Center")]
    void MoveToCameraCenter()
    {
        if (mainCamera != null)
    {
            Vector3 newPos = mainCamera.transform.position;
            newPos.z = 0;
         transform.position = newPos;
        Debug.Log($"Moved {gameObject.name} to camera center: {newPos}");
        }
  }
    
    [ContextMenu("Set Full Opacity")]
    void SetFullOpacity()
    {
     if (spriteRenderer != null)
        {
    Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
    Debug.Log($"Set {gameObject.name} alpha to 1.0");
        }
    }
    
    [ContextMenu("Reset Scale to 1,1,1")]
    void ResetScale()
    {
        transform.localScale = Vector3.one;
     Debug.Log($"Reset {gameObject.name} scale to (1,1,1)");
    }
}