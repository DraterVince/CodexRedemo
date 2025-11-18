using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sprite Animation Debugger - Add this to help diagnose animation issues
/// </summary>
public class SpriteAnimatorDebugger : MonoBehaviour
{
    private SpriteAnimator animator;
    private SpriteRenderer spriteRenderer;
 
    [Header("Debug Info")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private float lastCheckTime;
    
    private void Start()
    {
     animator = GetComponent<SpriteAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (animator == null)
  {
          Debug.LogError($"[{gameObject.name}] ? NO SpriteAnimator component found!");
            return;
     }
        
    if (spriteRenderer == null)
        {
            Debug.LogError($"[{gameObject.name}] ? NO SpriteRenderer component found!");
    return;
        }
     
        // Initial check
      Invoke("CheckSetup", 0.5f);
    }
    
    private void CheckSetup()
    {
        Debug.Log($"====== SPRITE ANIMATOR DEBUG: {gameObject.name} ======");
    
        // Check 1: Sprite List
        if (animator.sprites == null || animator.sprites.Count == 0)
      {
          Debug.LogError($"? PROBLEM: No sprites assigned!");
            Debug.LogError($"   SOLUTION: Drag sprites into the 'Sprites' list or use 'Import Sprite Sheet' button");
            return;
        }
        else
      {
   Debug.Log($"? Sprites: {animator.sprites.Count} sprites loaded");
         
       // Check for null sprites
            int nullCount = 0;
     for (int i = 0; i < animator.sprites.Count; i++)
   {
        if (animator.sprites[i] == null)
     {
       Debug.LogWarning($"   ? Sprite at index {i} is NULL!");
            nullCount++;
                }
      }
    
         if (nullCount > 0)
 {
    Debug.LogError($"? PROBLEM: {nullCount} sprites are NULL!");
       Debug.LogError($"   SOLUTION: Remove empty slots from the Sprites list");
            }
        }
 
      // Check 2: Frame Rate
        Debug.Log($"? Frame Rate: {animator.frameRate} FPS");
        if (animator.frameRate <= 0)
        {
            Debug.LogError($"? PROBLEM: Frame rate is {animator.frameRate}!");
       Debug.LogError($" SOLUTION: Set Frame Rate to at least 1");
        }
        
        // Check 3: Play On Start
        Debug.Log($"? Play On Start: {(animator.playOnStart ? "ENABLED" : "DISABLED")}");
        if (!animator.playOnStart)
        {
 Debug.LogWarning($"? WARNING: Play On Start is disabled!");
   Debug.LogWarning($"   You need to call animator.Play() manually");
        }
        
        // Check 4: Loop
        Debug.Log($"? Loop: {(animator.loop ? "ENABLED" : "DISABLED")}");
        
    // Check 5: Is Playing
        Debug.Log($"? Is Playing: {(animator.IsPlaying() ? "YES" : "NO")}");
      if (!animator.IsPlaying())
   {
        Debug.LogError($"? PROBLEM: Animation is NOT playing!");
            Debug.LogError($"   Trying to start it now...");
            animator.Play();
    
   // Check again after 0.1 seconds
   Invoke("CheckIfStarted", 0.1f);
        }
      
        // Check 6: Current Frame
        Debug.Log($"? Current Frame: {animator.GetCurrentFrame()} / {animator.GetFrameCount() - 1}");
        
        // Check 7: SpriteRenderer
        if (spriteRenderer.sprite == null)
   {
            Debug.LogWarning($"? SpriteRenderer has no sprite assigned!");
   }
  else
        {
            Debug.Log($"? Current Sprite: {spriteRenderer.sprite.name}");
    }
        
    // Check 8: GameObject Active
        Debug.Log($"? GameObject Active: {gameObject.activeInHierarchy}");
    
      // Check 9: Component Enabled
        Debug.Log($"? Component Enabled: {animator.enabled}");
        
        Debug.Log($"===============================================");
    }
    
    private void CheckIfStarted()
    {
        if (animator.IsPlaying())
        {
       Debug.Log($"? SUCCESS: Animation is now playing!");
        }
        else
        {
   Debug.LogError($"? FAILED: Animation still not playing after Play() call!");
        Debug.LogError($"   Check the Console for coroutine errors");
    }
    }
    
    private void Update()
    {
        if (!showDebugLogs) return;
        
     // Log every second
        if (Time.time - lastCheckTime > 1f)
        {
            lastCheckTime = Time.time;
 
      if (animator != null && animator.IsPlaying())
  {
    Debug.Log($"[{gameObject.name}] Frame: {animator.GetCurrentFrame()}/{animator.GetFrameCount()-1} | Playing: {animator.IsPlaying()}");
            }
   }
    }
    
    // Quick fix buttons (call from Inspector or code)
    [ContextMenu("Force Play Animation")]
    public void ForcePlay()
    {
        if (animator != null)
        {
    animator.Play();
            Debug.Log($"[{gameObject.name}] Forced animation to play!");
        }
    }
    
    [ContextMenu("Check Animation Setup")]
    public void CheckSetupNow()
 {
        CheckSetup();
    }
    
    [ContextMenu("Print All Sprite Names")]
    public void PrintSpriteNames()
    {
        if (animator == null || animator.sprites == null) return;
        
        Debug.Log($"=== All Sprites in {gameObject.name} ===");
        for (int i = 0; i < animator.sprites.Count; i++)
      {
      if (animator.sprites[i] != null)
        {
           Debug.Log($"  {i}: {animator.sprites[i].name}");
            }
    else
    {
            Debug.Log($"  {i}: NULL");
            }
        }
        Debug.Log($"=====================================");
  }
}
